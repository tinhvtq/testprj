using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Text;

namespace ReservationSystem.Controllers
{
    public class ReturnCategory
    {
        public int categoryId;
        public string categoryNameEn;
        public string categoryNameJp;
        public int machines;
    }
    public class MachineController : Controller
    {
        private readonly DbConnectionClass _db;
        private readonly IWebHostEnvironment _env;

        public MachineController(DbConnectionClass db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        /*
         * Get list machine for poup can use machine
         * params: string machine id
         * return partial_view for ajax
         */
        [HttpPost]
        public IActionResult ListCanUseMachine(string machineId)
        {
            var categories = _db.Categories.Where(c => c.Status == 1).Include(c => c.Machines).ToList();
            int[] arrMachineId = null;
            if (machineId != null && machineId.Length > 0)
            {
                arrMachineId = machineId.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }

            ViewBag.arrMachineId = arrMachineId;
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return PartialView("_ListCanUseMachine", categories);
        }

        /**
         * 
         * Create new machine form
         * *
         */
         public IActionResult CreateMachineForm(int cate_id)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            ViewBag.Lang = _lang;
            var categories = _db.Categories.Where(c => c.Status == 1).Include(c => c.Machines).ToList();
            ViewBag.Category = categories;
            ViewBag.CategoryId = cate_id;
            return PartialView("_CreateMachineForm");
        }
        /*
         * Get machine machine
         * params: int machine id
         * return partial_view for ajax
         */
        public IActionResult GetMachineDetail(int id)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            ViewBag.Lang = _lang;

            var categories = _db.Categories.Where(c => c.Status == 1).Include(c => c.Machines).ToList();
            ViewBag.Category = categories;

            var machine = _db.Machines.Include(m => m.CustomFields).Include(m => m.ManualFiles).Where(m => m.MachineId == id).FirstOrDefault();
            var can_users = _db.Users.Where(u => u.Status == 1).Join(
                                    _db.CanUseMachines.Where(can => can.MachineId == id),
                                    user => user.UserId,
                                    can => can.UserId,
                                    (user, can) => new
                                    {
                                        userId = user.UserId,
                                        userNameJp = user.UserNameJp,
                                        userNameEn = user.UserNameEn,
                                        userMail = user.Mail
                                    }
                                ).ToList();
            List<int> idList = new List<int>();
            List<string> nameList = new List<string>();
            List<string> mailList = new List<string>();

            foreach (var item in can_users)
            {
                idList.Add(item.userId);
                if(_lang == "en-US")
                {
                    nameList.Add(item.userNameEn);
                }
                else
                {
                    nameList.Add(item.userNameJp);
                }
                
                mailList.Add(item.userMail);
            }

            ViewBag.idList = idList;
            ViewBag.nameList = nameList;
            ViewBag.mailList = mailList;

            var admin_users = _db.Users.Where(u => u.Status == 1).Join(
                                    _db.MachineAdmins.Where(ma => ma.MachineId == id),
                                    user => user.UserId,
                                    ma => ma.UserId,
                                    (user, ma) => new
                                    {
                                        userId = user.UserId,
                                        userNameJp = user.UserNameJp,
                                        userNameEn = user.UserNameEn,
                                        userMail = user.Mail
                                    }
                                ).ToList();
            List<int> idListAdmin = new List<int>();
            List<string> nameListAdmin = new List<string>();
            List<string> mailListAdmin = new List<string>();

            foreach (var user in admin_users)
            {
                idListAdmin.Add(user.userId);
                if (_lang == "en-US")
                {
                    nameListAdmin.Add(user.userNameEn);
                }
                else
                {
                    nameListAdmin.Add(user.userNameJp);
                }
                mailListAdmin.Add(user.userMail);
            }

            ViewBag.idListAdmin = idListAdmin;
            ViewBag.nameListAdmin = nameListAdmin;
            ViewBag.mailListAdmin = mailListAdmin;

            var customFieds = machine.CustomFields.OrderByDescending(c => c.FieldId).ToList();
            ViewBag.customFieds = customFieds;
            return PartialView("_GetMachineDetail", machine);
        }

        /*
         * Machine master page
         * **/
        public IActionResult Index()
        {
            // check login
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }
            else
            {
                if (HttpContext.Session.GetInt32("UserType") == 1 && HttpContext.Session.GetInt32("BuiltinUser") != 0)
                {
                    var categories = _db.Categories.Where(x => x.Status == 1).ToList();
                    var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
                    var culture = langRequest.RequestCulture.Culture;
                    ViewBag.Lang = culture.ToString();
                    return View("Index", categories);
                }
            }

            return NotFound();


        }
        /*
         * Get list machine for tableview
         * params: int machine id
         * return json for ajax
         */
        public IActionResult GetAllMachine(int id)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            if (id == 0)
            {
                var machines = _db.Machines.Where(m => m.Status == 1).Join(
                                    _db.Categories,
                                    machine => machine.CategoryId,
                                    cate => cate.CategoryId,
                                    (machine, cate) => new
                                    {
                                        machineId = machine.MachineId,
                                        machineName = (_lang == "en-US") ? machine.MachineNameEn : machine.MachineNameJp,
                                        categoryName = (_lang == "en-US") ? cate.CategoryNameEn : cate.CategoryNameJp,
                                        categoryId = machine.CategoryId
                                    }
                                ).OrderBy(x => x.categoryId).ToList();
                return Json(machines);
            }
            else
            {
                var machines = _db.Machines.Where(m => m.CategoryId == id).Where(m => m.Status == 1).Join(
                                    _db.Categories,
                                    machine => machine.CategoryId,
                                    cate => cate.CategoryId,
                                    (machine, cate) => new
                                    {
                                        machineId = machine.MachineId,
                                        machineName = (_lang == "en-US") ? machine.MachineNameEn : machine.MachineNameJp,
                                        categoryName = (_lang == "en-US") ? cate.CategoryNameEn : cate.CategoryNameJp,
                                        categoryId = machine.CategoryId
                                    }
                                ).OrderBy(x => x.categoryId).ToList();
                return Json(machines);
            }
            
        }
        public IActionResult GetAllCategory()
        {
            var categories = _db.Categories.Where(c => c.Status == 1).ToList();

            List<ReturnCategory> return_collection = new List<ReturnCategory>();
            foreach (Category cate in categories)
            {
                var machines = _db.Machines.Where(m => m.Status == 1).Where(m => m.CategoryId == cate.CategoryId).Count();
                var objCate = new ReturnCategory();
                objCate.categoryId = cate.CategoryId;
                objCate.categoryNameEn = cate.CategoryNameEn;
                objCate.categoryNameJp = cate.CategoryNameJp;
                objCate.machines = machines;
                return_collection.Add(objCate);
            }
            return Json(return_collection);
        }

        /*
         * Create new category
         * **/
        public IActionResult CreateCategory(string categoryNameEn, string categoryNameJp)
        {
            Category cate = new Category();
            cate.CategoryNameEn = categoryNameEn;
            cate.CategoryNameJp = categoryNameJp;
            cate.Status = 1;
            _db.Add(cate);
            _db.SaveChanges();
            var result = new
            {
                status = 200,
                message = "Create Category Success",
                data = cate
            };
            return Json(result);
        }

        /*
         * Update category
         * **/
        public IActionResult UpdateCategory(int id, string categoryNameEn, string categoryNameJp)
        {
            Category cate = _db.Categories.Find(id);
            if (cate == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Update Fail! CategoryId not exist"
                };
                return Json(result);
            }
            else
            {
                cate.CategoryNameEn = categoryNameEn;
                cate.CategoryNameJp = categoryNameJp;
                _db.Categories.Update(cate);
                _db.SaveChanges();

                var count = _db.Categories.Where(c => c.CategoryId <= id).Where(c => c.Status == 1).OrderBy(c => c.CategoryId).Count();
                var result = new
                {
                    status = 200,
                    message = "Update Success",
                    data = cate,
                    count = count
                };
                return Json(result);
            }

        }
        /*
         * Delete by change status category
         * **/
        public IActionResult DeleteCategoryById(int id)
        {
            Category cate = _db.Categories.Find(id);
            if (cate == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Delete Fail! CategoryId not exist"
                };
                return Json(result);
            }
            else
            {
                cate.Status = 0;
                _db.Categories.Update(cate);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = "Delete Success"
                };
                return Json(result);
            }
        }

        /*
         * Create new machine
         * **/
        public IActionResult CreateMachine(MachineFormData formData)
        {
            var cate = _db.Categories.Include(c => c.Machines).Where(c => c.CategoryId == formData.CategoryId).FirstOrDefault();
            if (cate == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Create Fail! CategoryId not exist"
                };
                return Json(result);
            }
            else
            {
                string original_filename = string.Empty;
                string filename = string.Empty;
                string imageFile = string.Empty;
                string manualFile = string.Empty;
                var files = Request.Form.Files;
                string manual_file_name = string.Empty;
                if (files.Count() > 0)
                {
                    foreach(IFormFile file in files)
                    {
                        //IFormFile file = Request.Form.Files[0];
                        var formInputName = file.Name;

                        //string extention = Path.GetExtension(file.FileName);

                        original_filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        string str_random = RandomFilename();

                        filename = str_random + "_" + original_filename;
                        filename = EnsureCorrectFilename(filename);

                        FileStream fs = System.IO.File.Create(GetPathAndFilename(filename));
                        file.CopyTo(fs);
                        fs.Close();
                        if (formInputName == "ImageFile")
                        {
                            imageFile = "files/" + filename;
                        }
                        else
                        {
                            manualFile = "files/" + filename;
                            manual_file_name = original_filename;
                        }
                    }
                    
                }

                Machine m = new Machine();
                m.MachineNameEn = formData.MachineNameEn;
                m.MachineNameJp = formData.MachineNameJp;
                m.UnitPrice = formData.UnitPrice;
                m.ReferenceAuth = formData.ReferenceAuth;
                m.MakerNameEn = formData.MakerNameEn;
                m.MakerNameJp = formData.MakerNameJp;
                m.ModelEn = formData.ModelEn;
                m.ModelJp = formData.ModelJp;
                m.EquipmentNumber = formData.EquipmentNumber;
                m.BusinessPerson = formData.BusinessPerson;
                m.BusinessAddress = formData.BusinessAddress;
                m.BusinessTel = formData.BusinessTel;
                m.BusinessFax = formData.BusinessFax;
                m.BusinessMail = formData.BusinessMail;
                m.TechnicalAddress = formData.TechnicalAddress;
                m.TechnicalFax = formData.TechnicalFax;
                m.TechnicalPersion = formData.TechnicalPersion;
                m.TechnicalMail = formData.TechnicalMail;
                m.TechnicalTel = formData.TechnicalTel;
                m.PurchaseDate = formData.PurchaseDate;
                m.PurchaseCampany = formData.PurchaseCampany;
                m.ExplanationEn = formData.ExplanationEn;
                m.ExplanationJp = formData.ExplanationJp;
                m.SpecEn = formData.SpecEn;
                m.SpecJp = formData.SpecJp;
                m.PlaceEn = formData.PlaceEn;
                m.PlaceJp = formData.PlaceJp;
                m.ExpendableSupplies = formData.ExpendableSupplies;
                m.URL = formData.URL;
                m.URL2 = formData.URL2;
                m.URL3 = formData.URL3;
                m.Status = 1;
                m.ShowOrder = 0;
                m.ImageFile = imageFile;
                //m.ManualFile = manualFile;
                cate.Machines.Add(m);
                _db.SaveChanges();

                Machine machine = _db.Machines.Include(u => u.CanUseMachines).Include(u => u.MachineAdmins).Include(u => u.CustomFields).Include(u => u.ManualFiles).Where(u => u.MachineId == m.MachineId).FirstOrDefault();
                if(manualFile.Length > 0)
                {
                    ManualFile mfile = new ManualFile();
                    mfile.MachineId = m.MachineId;
                    mfile.ManualFileName = manualFile;
                    mfile.DisplayName = manual_file_name;
                    machine.ManualFiles.Add(mfile);
                    _db.SaveChanges();
                }

                int[] arrUserAdminId = null;
                if (formData.charge_user != null && formData.charge_user.Length > 0)
                {
                    arrUserAdminId = formData.charge_user.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    foreach (int i in arrUserAdminId)
                    {
                        var machineAdmin = new MachineAdmin();
                        machineAdmin.MachineId = machine.MachineId;
                        machineAdmin.UserId = i;
                        machine.MachineAdmins.Add(machineAdmin);
                    }
                    _db.SaveChanges();
                }

                int[] arrUserId = null;
                if (formData.use_user != null && formData.use_user.Length > 0)
                {
                    arrUserId = formData.use_user.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    foreach (int i in arrUserId)
                    {
                        var canUseMachine = new CanUseMachine();
                        canUseMachine.MachineId = machine.MachineId;
                        canUseMachine.UserId = i;
                        machine.CanUseMachines.Add(canUseMachine);
                    }
                    _db.SaveChanges();
                }

                int[] arr_Check = null;
                string[] arr_item_name_jp = null;
                string[] arr_item_name_en = null;
                if (formData.item_name_jp != null && formData.item_name_jp.Length > 0)
                {
                    arr_Check = formData.arrCheck.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    arr_item_name_jp = formData.item_name_jp.Split(',').ToArray();
                    arr_item_name_en = formData.item_name_en.Split(',').ToArray();
                    for (int c = 0; c < arr_item_name_jp.Length; c++)
                    {
                        var custome = new CustomField();
                        custome.FieldNameEn = arr_item_name_en[c];
                        custome.FieldNameJp = arr_item_name_jp[c];
                        custome.MachineId = machine.MachineId;
                        custome.Required = (arr_Check[c] == 1) ? true : false;
                        machine.CustomFields.Add(custome);
                    }
                    _db.SaveChanges();
                }

                var result = new
                {
                    status = 200,
                    message = "Create Success",
                    id = m.MachineId
                };
                return Json(result);
            }
        }
        /*
         * Update machine
         * **/
        public IActionResult UpdateMachine(MachineFormData formData)
        {
            Machine m = _db.Machines.Include(u => u.CanUseMachines).Include(u => u.MachineAdmins).Include(u => u.CustomFields).Include(u=>u.ManualFiles).Where(u => u.MachineId == formData.MachineId).FirstOrDefault();
            if (m == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Machine not exist"
                };
                return Json(result);
            }
            else
            {
                string origin_name = string.Empty;
                string manual_file_name = string.Empty;
                string manualFile = string.Empty;
                string imageFile = m.ImageFile;
                if (formData.StatusRemoveImage != 0)
                {
                    string _fileToBeDeleted = _env.WebRootPath + m.ImageFile;
                    // Check if file exists with its full path    
                    if (System.IO.File.Exists(_fileToBeDeleted))
                    {
                        // If file found, delete it    
                        System.IO.File.Delete(_fileToBeDeleted);

                        //Update name infomation file
                        imageFile = string.Empty;
                    }
                }

                string filename = string.Empty;               
                var files = Request.Form.Files;
                if (files.Count() > 0)
                {
                    foreach (IFormFile file in files)
                    {
                        //IFormFile file = Request.Form.Files[0];
                        var formInputName = file.Name;

                        //string extention = Path.GetExtension(file.FileName);
                        origin_name = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        string str_random = RandomFilename();

                        filename = str_random + "_" + origin_name;
                        filename = EnsureCorrectFilename(filename);

                        FileStream fs = System.IO.File.Create(GetPathAndFilename(filename));
                        file.CopyTo(fs);
                        fs.Close();
                        if (formInputName == "ImageFile")
                        {
                            imageFile = "files/" + filename;
                        }
                        else
                        {
                            manualFile =  "files/" + filename;
                            manual_file_name = origin_name;
                        }
                    }

                }
                m.CategoryId = formData.CategoryId;
                m.MachineNameEn = formData.MachineNameEn;
                m.MachineNameJp = formData.MachineNameJp;
                m.UnitPrice = formData.UnitPrice;
                m.ReferenceAuth = formData.ReferenceAuth;
                m.MakerNameEn = formData.MakerNameEn;
                m.MakerNameJp = formData.MakerNameJp;
                m.ModelEn = formData.ModelEn;
                m.ModelJp = formData.ModelJp;
                m.EquipmentNumber = formData.EquipmentNumber;
                m.BusinessPerson = formData.BusinessPerson;
                m.BusinessAddress = formData.BusinessAddress;
                m.BusinessTel = formData.BusinessTel;
                m.BusinessFax = formData.BusinessFax;
                m.BusinessMail = formData.BusinessMail;
                m.TechnicalAddress = formData.TechnicalAddress;
                m.TechnicalFax = formData.TechnicalFax;
                m.TechnicalPersion = formData.TechnicalPersion;
                m.TechnicalMail = formData.TechnicalMail;
                m.TechnicalTel = formData.TechnicalTel;
                m.PurchaseDate = formData.PurchaseDate;
                m.PurchaseCampany = formData.PurchaseCampany;
                m.ExplanationEn = formData.ExplanationEn;
                m.ExplanationJp = formData.ExplanationJp;
                m.SpecEn = formData.SpecEn;
                m.SpecJp = formData.SpecJp;
                m.PlaceEn = formData.PlaceEn;
                m.PlaceJp = formData.PlaceJp;
                m.ExpendableSupplies = formData.ExpendableSupplies;
                m.URL = formData.URL;
                m.URL2 = formData.URL2;
                m.URL3 = formData.URL3;
                m.Status = 1;
                m.ShowOrder = 0;
                m.ImageFile = imageFile;
                //m.ManualFile = manualFile;
                _db.Machines.Update(m);

                if (manualFile.Length > 0)
                {
                    ManualFile mfile = new ManualFile();
                    mfile.MachineId = m.MachineId;
                    mfile.ManualFileName = manualFile;
                    mfile.DisplayName = manual_file_name;
                    m.ManualFiles.Add(mfile);
                    _db.SaveChanges();
                }

                m.MachineAdmins.Clear();
                _db.SaveChanges();
                int[] arrUserAdminId = null;
                if (formData.charge_user != null && formData.charge_user.Length > 0)
                {
                    arrUserAdminId = formData.charge_user.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    foreach (int i in arrUserAdminId)
                    {
                        var machineAdmin = new MachineAdmin();
                        machineAdmin.MachineId = m.MachineId;
                        machineAdmin.UserId = i;
                        m.MachineAdmins.Add(machineAdmin);
                    }
                    _db.SaveChanges();
                }
                m.CanUseMachines.Clear();
                _db.SaveChanges();
                int[] arrUserId = null;
                if (formData.use_user != null && formData.use_user.Length > 0)
                {
                    arrUserId = formData.use_user.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    foreach (int i in arrUserId)
                    {
                        var canUseMachine = new CanUseMachine();
                        canUseMachine.MachineId = m.MachineId;
                        canUseMachine.UserId = i;
                        m.CanUseMachines.Add(canUseMachine);
                    }
                    _db.SaveChanges();
                }

                //m.CustomFields.Clear();
                //_db.SaveChanges();
                int[] arr_Check = null;
                string[] arr_item_name_jp = null;
                string[] arr_item_name_en = null;
                int[] arr_item = null;
                if (formData.item_name_jp != null && formData.item_name_jp.Length > 0)
                {
                    arr_item = formData.ItemId.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    arr_Check = formData.arrCheck.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    arr_item_name_jp = formData.item_name_jp.Split(',').ToArray();
                    arr_item_name_en = formData.item_name_en.Split(',').ToArray();

                    for (int c = 0; c < arr_item_name_jp.Length; c++)
                    {
                        if (arr_item[c] > 0)
                        {
                            CustomField cust = _db.CustomFields.Find(arr_item[c]);
                            if(cust != null)
                            {
                                cust.FieldNameEn = arr_item_name_en[c];
                                cust.FieldNameJp = arr_item_name_jp[c];
                                cust.Required = (arr_Check[c] == 1) ? true : false;
                                _db.CustomFields.Update(cust);
                            }
                        }
                        else
                        {
                            var custome = new CustomField();
                            custome.FieldNameEn = arr_item_name_en[c];
                            custome.FieldNameJp = arr_item_name_jp[c];
                            custome.MachineId = m.MachineId;
                            custome.Required = (arr_Check[c] == 1) ? true : false;
                            m.CustomFields.Add(custome);
                        }
                    }
                    _db.SaveChanges();
                }

                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = "Update Success"
                };
                return Json(result);
            }

        }

        /*
         * Delete machine by change status
         * **/
        public IActionResult deleteMachineById(int id)
        {
            Machine machine = _db.Machines.Find(id);
            if (machine == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Delete Fail! MachineId not exist"
                };
                return Json(result);
            }
            else
            {
                machine.Status = 0;
                _db.Machines.Update(machine);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = "Delete Success"
                };
                return Json(result);
            }
        }


        private static readonly Random random = new Random();
        /*
         * Create new random string
         * **/
        public static string RandomFilename()
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray()) + "-" +
                   new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray()) + "-" +
                   new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        /*
         * for upload file
         * **/
        private string EnsureCorrectFilename(string filename)
        {
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);

            return filename;
        }
        /*
         * for upload file
         * **/
        private string GetPathAndFilename(string filename)
        {
            return _env.WebRootPath + "\\files\\" + filename;
        }
        /*
         * calc string for checkvalidate
         * **/
        private int countJapaneseStringLength(String str)
        {
            int iTotal = 0;
            str.ToList().ForEach(ch => {
                int iCode = ch;
                if (iCode >= 65 && iCode <= 122)
                {
                    iTotal++;
                }
                else if (iCode >= 48 && iCode <= 57)
                {
                    iTotal++;
                }
                else if (iCode == 32)
                {
                    iTotal++;
                }
                else
                {
                    iTotal += 2;
                }
            });
            return iTotal;
        }
        /*
         * Writelog when import csv
         * **/
        public void WriteLog(string strLog)
        {
            DateTime time = DateTime.Now;
            string logFilePath =  _env.WebRootPath + "\\logs\\machine_import_error_log_" + time.ToString("yyyy-MM-dd") + ".txt";
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
            {
                using (StreamWriter log = new StreamWriter(fileStream))
                {
                    log.WriteLine("================================"+ time.ToString("yyyy-MM-dd HH:mm:ss") + "======================================");
                    log.WriteLine(strLog);
                }
            }
        }

        /*
         * CSV import
         * **/
        public IActionResult CsvImportProccess(CsvFormData data)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var attachedFile = Request.Form.Files["CsvFile"];
            if (attachedFile == null || attachedFile.Length <= 0) return Json(null);
            var csvReader = new StreamReader(attachedFile.OpenReadStream(), Encoding.GetEncoding(932), false);
            var uploadModelList = new List<CsvData>();
            string inputDataRead;
            var values = new List<string>();
            while ((inputDataRead = csvReader.ReadLine()) != null)
            {
                values.Add(inputDataRead.Trim());

            }
            /*values.Remove(values[0]);
            values.Remove(values[values.Count - 1]);*/
            Boolean err = false;
            List<string> cate_name_list = new List<string>();
            int row = 1;
            foreach (var value in values)
            {
                var uploadModelRecord = new CsvData();
                var eachValue = value.Split(',');

                if (eachValue[0] == "" || countJapaneseStringLength(eachValue[0]) > 50)
                {
                    err = true;
                    break;
                }
                else
                {
                    var cate_name = eachValue[0];
                    var category = _db.Categories.Include(c => c.Machines).Where(c => c.CategoryNameJp == cate_name && c.Status != 0).FirstOrDefault();
                    if (category == null)
                    {
                        WriteLog("Error at rows: "+ row + " - 分類名(Category): [" + cate_name  + "] not exist.");
                        err = true;
                        break;
                    }
                    else
                    {
                        uploadModelRecord.CategoryName = cate_name;
                        if (eachValue[1] == "" || countJapaneseStringLength(eachValue[1]) > 100)
                        {
                            WriteLog("Error at rows: " + row + " - 機器名(MachineNameJp): [" + eachValue[1] + "] too long.");
                            err = true;
                            break;
                        }
                        else
                        {
                            uploadModelRecord.MachineNameJp = eachValue[1];
                        }

                        if (eachValue[2] == "" || countJapaneseStringLength(eachValue[2]) > 100)
                        {
                            WriteLog("Error at rows: " + row + " - 機器名(MachineNameEn): [" + eachValue[2] + "] too long.");
                            err = true;
                            break;
                        }
                        else
                        {
                            uploadModelRecord.MachineNameEn = eachValue[2];
                        }

                        if (eachValue[3] == "" || countJapaneseStringLength(eachValue[3]) > 100)
                        {
                            WriteLog("Error at rows: " + row + " - メーカー名(MakerNameJp): [" + eachValue[3] + "] too long.");
                            err = true;
                            break;
                        }
                        else
                        {
                            uploadModelRecord.MakerNameJp = eachValue[3];
                        }

                        if (eachValue[4] == "" || countJapaneseStringLength(eachValue[4]) > 50)
                        {
                            WriteLog("Error at rows: " + row + " - 型式(ModelJp): [" + eachValue[4] + "] too long.");
                            err = true;
                            break;
                        }
                        else
                        {
                            uploadModelRecord.ModelJp = eachValue[4];
                        }

                        if (eachValue[5] == "" || countJapaneseStringLength(eachValue[5]) > 50)
                        {
                            WriteLog("Error at rows: " + row + " - 型式(ModelEn): [" + eachValue[5] + "] too long.");
                            err = true;
                            break;
                        }
                        else
                        {
                            uploadModelRecord.ModelEn = eachValue[5];
                        }

                        if (eachValue[6] == "" || countJapaneseStringLength(eachValue[6]) > 100)
                        {
                            WriteLog("Error at rows: " + row + " - 設置場所(PlaceJp): [" + eachValue[6] + "] too long.");
                            err = true;
                            break;
                        }
                        else
                        {
                            uploadModelRecord.PlaceJp = eachValue[6];
                        }

                        if (eachValue[7] == "" || countJapaneseStringLength(eachValue[7]) > 100)
                        {
                            WriteLog("Error at rows: " + row + " - 設置場所(PlaceEn): [" + eachValue[7] + "] too long.");
                            err = true;
                            break;
                        }
                        else
                        {
                            uploadModelRecord.PlaceEn = eachValue[7];
                        }

                        if (!err)
                        {
                            var machine_query = _db.Machines.Where(m => m.MachineNameEn == uploadModelRecord.MachineNameEn && m.MachineNameJp == uploadModelRecord.MachineNameJp && m.Status == 1 && m.CategoryId == category.CategoryId).FirstOrDefault();

                            if (machine_query != null)
                            {
                                machine_query.MachineNameJp = uploadModelRecord.MachineNameJp;
                                machine_query.MachineNameEn = uploadModelRecord.MachineNameEn;
                                machine_query.MakerNameJp = uploadModelRecord.MakerNameJp;
                                machine_query.ModelJp = uploadModelRecord.ModelJp;
                                machine_query.ModelEn = uploadModelRecord.ModelEn;
                                machine_query.PlaceJp = uploadModelRecord.PlaceJp;
                                machine_query.PlaceEn = uploadModelRecord.PlaceEn;
                                machine_query.Status = 1;
                                _db.Machines.Update(machine_query);
                                _db.SaveChanges();
                            }
                            else
                            {
                                Machine machine = new Machine();
                                machine.MachineNameJp = uploadModelRecord.MachineNameJp;
                                machine.MachineNameEn = uploadModelRecord.MachineNameEn;
                                machine.MakerNameJp = uploadModelRecord.MakerNameJp;
                                machine.ModelJp = uploadModelRecord.ModelJp;
                                machine.ModelEn = uploadModelRecord.ModelEn;
                                machine.PlaceJp = uploadModelRecord.PlaceJp;
                                machine.PlaceEn = uploadModelRecord.PlaceEn;
                                machine.Status = 1;
                                category.Machines.Add(machine);
                            }
                        }
                        
                    }
                }

                uploadModelList.Add(uploadModelRecord);// newModel needs to be an object of type ContextTables.  
                row++;
            }


            if (err)
            {
                var result = new
                {
                    status = 400,
                    message = (_lang == "en-US") ? "The content of the file is not correct." : "ファイルの内容が正しくありません。"
                };
                return Json(result);
            }
            else
            {
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = "Import success",
                    data = uploadModelList
                };
                return Json(result);
            }
        }
    }
}