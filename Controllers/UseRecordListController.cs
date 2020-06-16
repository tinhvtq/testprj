using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;

namespace ReservationSystem.Controllers
{
    public class UseRecordListController : Controller
    {
        private readonly DbConnectionClass _db;

        public UseRecordListController(DbConnectionClass db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns a list data categories into view
        /// </summary>
        public IActionResult Index()
        {
            // check login
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }
            var record_lists = _db.Categories.Where(c => c.Status == 1).Include(p => p.Machines).ToList();
            ViewBag.ListRecord = record_lists;
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();
            ViewBag.CanUseMachine = machines;

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return View();
        }

        /// <summary>
        /// Get list record machines
        /// </summary>
        /// <returns></returns>
        public IActionResult GetRecordListAll()
        {
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();
            ViewBag.CanUseMachine = machines;

            var merchines = _db.Machines.Where(p => p.Status == 1).ToList();

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();

            return PartialView("_GetRecordList", merchines);
        }

        /// <summary>
        /// Returns list Reservation by machineId
        /// </summary>
        /// <param name="id">MachineId</param>
        /// <returns>Affiliation data (n records)</returns>
        public IActionResult GetReservationByMachineId(int id)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            string format_Day = (_lang == "en-US") ? "MM/dd/yyyy HH:mm:ss" : "yyyy/MM/dd HH:mm:ss";

            var reservations = (_db.Reservations.Where(p => p.MachineId == id).Join(_db.Users.Where(u => u.Status == 1),
                                                p => p.UserId,
                                                a => a.UserId,
                                                (a, p) => new TimeLogResult
                                                {
                                                    StartTime = a.StartTime.ToString(format_Day),
                                                    EndTime = a.EndTime.ToString(format_Day),
                                                    UseTime = ((a.EndTime - a.StartTime).TotalHours).ToString(),
                                                    UserNameEn = (_lang == "en-US") ? p.UserNameEn : p.UserNameJp
                                                })).ToList();
            return PartialView("_GetReservation", reservations);
        }

        /// <summary>
        /// Load view detail Machine
        /// </summary>
        /// <param name="id">MachineId</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IActionResult GetViewDetailMachine(int id, string startDate, string endDate)
        {
            ParamaterSearch paramater = new ParamaterSearch
            {
                id = id,
                startDate = startDate,
                endDate = endDate
            };

            var machine = _db.Machines.Include(m => m.CustomFields).Where(m => m.MachineId == id).FirstOrDefault();
            var customFieds = machine.CustomFields.OrderByDescending(c => c.FieldId).ToList();
            ViewBag.customFieds = customFieds;

            ViewBag.Parameter = paramater;
            return PartialView("_GetDetailMachine", machine);
        }

        /// <summary>
        /// Get a list of machine items by machineId and startDate, endDate
        /// </summary>
        /// <param name="id">MachineId</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IActionResult GetDetailMachine(int id, string startDate, string endDate)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            string format_Day = (_lang == "en-US") ? "MM/dd/yyyy HH:mm:ss" : "yyyy/MM/dd HH:mm:ss";

            // List<TimeLogResult> query = new List<TimeLogResult>();
            List<IDictionary<string, string>> query = new List<IDictionary<string, string>>();
            List<Reservation> linqQuery = new List<Reservation>();
            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                linqQuery = _db.Reservations.Include(p => p.CustomFieldValues)
                                            .Include(p => p.User)
                                            .ThenInclude(u => u.Group)
                                            .Where(p => p.MachineId == id && p.StartTime.Date >= Convert.ToDateTime(startDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                linqQuery = _db.Reservations.Include(p => p.CustomFieldValues)
                                            .Include(p => p.User)
                                            .ThenInclude(u => u.Group)
                                            .Where(p => p.MachineId == id && p.StartTime.Date >= Convert.ToDateTime(startDate) && p.EndTime.Date <= Convert.ToDateTime(endDate)).ToList();
            }
            else
            {
                linqQuery = _db.Reservations.Include(p => p.CustomFieldValues)
                                            .Include(p => p.User)
                                            .ThenInclude(u => u.Group)
                                            .Where(p => p.MachineId == id).ToList();
            }

            if (linqQuery != null)
            {
                foreach (var item in linqQuery)
                {
                    // dynamic timelog = new ExpandoObject();
                    var timelog = new Dictionary<string, string>();
                    // TimeLogResult timelog = new TimeLogResult();
                    timelog["startTime"] = item.StartTime.ToString(format_Day);
                    timelog["endTime"] = item.EndTime.ToString(format_Day);
                    timelog["purpose_Remark"] = item.Note;
                    timelog["useTime"] = ((item.EndTime - item.StartTime).TotalHours).ToString("0.00", CultureInfo.InvariantCulture) + ((_lang == "en-US") ? "Hours" : "時間");
                    timelog["userNameEn"] = (_lang == "en-US") ? item.User.UserNameEn : item.User.UserNameJp;
                    timelog["groupName"] = (_lang == "en-US") ? item.User.Group.GroupNameEn : item.User.Group.GroupNameJp;

                    var arr = item.CustomFieldValues.ToArray();

                    for (var i = 0; i < arr.Length; i++) {
                        timelog["fieldNameEn" + i] = arr[i].FieldValue;
                    }

                    // if (arr.Length == 1)
                    // {
                    //     timelog.fieldNameEn0 = arr[0].FieldValue;
                    // }

                    // if (arr.Length == 2)
                    // {
                    //     timelog.fieldNameEn0 = arr[0].FieldValue;
                    //     timelog.fieldNameEn1 = arr[1].FieldValue;
                    // }

                    // if (arr.Length == 3)
                    // {
                    //     timelog.fieldNameEn0 = arr[0].FieldValue;
                    //     timelog.fieldNameEn1 = arr[1].FieldValue;
                    //     timelog.fieldNameEn2 = arr[2].FieldValue;
                    // }

                    // if (arr.Length == 4)
                    // {
                    //     timelog.fieldNameEn0 = arr[0].FieldValue;
                    //     timelog.fieldNameEn1 = arr[1].FieldValue;
                    //     timelog.fieldNameEn2 = arr[2].FieldValue;
                    //     timelog.fieldNameEn3 = arr[3].FieldValue;
                    // }

                    query.Add(timelog);
                }
            }

            return Json(query);
        }

        /// <summary>
        /// Returns the data of machine entries with machineId displaying them on popup 
        /// </summary>
        /// <param name="id">MachineId</param>
        /// <returns></returns>
        public IActionResult GetMachineById(int id)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            ViewBag.Lang = _lang;

            DetailedMachineInfo info = new DetailedMachineInfo();
            List<ModalFileRecord> list_file = new List<ModalFileRecord>();
            List<ModalUseUser> use_user = new List<ModalUseUser>();

            var query = _db.Machines.Include(p => p.Category).FirstOrDefault(p => p.MachineId == id);

            if (query != null)
            {
                var manual_files = _db.ManualFiles.Where(p => p.MachineId == query.MachineId).ToList();
                ViewBag.ManualFiles = manual_files;

                var list_us = _db.MachineAdmins.Where(p => p.MachineId == query.MachineId).ToList();

                if (list_us.Count() > 0)
                {
                    foreach (var item in list_us)
                    {
                        var user = _db.Users.FirstOrDefault(p => p.UserId == item.UserId);

                        use_user.Add(new ModalUseUser
                        {
                            userId = user.UserId,
                            userNameEn = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp
                        });
                    }
                }
                else
                {
                    use_user = null;
                }

                info.image = query.ImageFile ?? string.Empty;
                info.machine_name = (_lang == "en-US") ? query.MachineNameEn : query.MachineNameJp ?? string.Empty;
                info.unit_price = query.UnitPrice;
                info.marker_name = (_lang == "en-US") ? query.MakerNameEn : query.MakerNameJp ?? string.Empty;
                info.model = (_lang == "en-US") ? query.ModelEn : query.ModelJp ?? string.Empty;
                info.performance = (_lang == "en-US") ? query.SpecEn : query.SpecJp ?? string.Empty;
                info.categorry = (_lang == "en-US") ? query.Category.CategoryNameEn : query.Category.CategoryNameJp ?? string.Empty;
                info.set_up_area = (_lang == "en-US") ? query.PlaceEn : query.PlaceJp ?? string.Empty;
                info.explanation = (_lang == "en-US") ? query.ExplanationEn : query.ExplanationJp ?? string.Empty;

                info.url = query.URL;
                info.url2 = query.URL2;
                info.url3 = query.URL3;
                //info.manual_file = list_file;
                info.users = use_user;
                info.purchaseDate = query.PurchaseDate;
                info.storePurchased = query.PurchaseCampany;
                info.vesselNumber = query.EquipmentNumber;
                info.businessPerson = query.BusinessPerson;
                info.businessAddress = query.BusinessAddress;
                info.businessTel = query.BusinessTel;
                info.businessFax = query.BusinessFax;
                info.businessMail = query.BusinessMail;
                info.technicalPersion = query.TechnicalPersion;
                info.technicalAddress = query.TechnicalAddress;
                info.technicalTel = query.TechnicalTel;
                info.technicalFax = query.TechnicalFax;
                info.technicalMail = query.TechnicalMail;
                info.expendableSupplies = query.ExpendableSupplies;
            }


            ViewBag.List = info;

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

            List<string> nameList = new List<string>();

            foreach (var item in can_users)
            {
                if (_lang == "en-US")
                {
                    nameList.Add(item.userNameEn);
                }
                else
                {
                    nameList.Add(item.userNameJp);
                }

            }

            ViewBag.nameList = nameList;

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

            List<string> nameListAdmin = new List<string>();

            foreach (var user in admin_users)
            {
                if (_lang == "en-US")
                {
                    nameListAdmin.Add(user.userNameEn);
                }
                else
                {
                    nameListAdmin.Add(user.userNameJp);
                }
            }

            ViewBag.nameListAdmin = nameListAdmin;

            return PartialView("_GetModalDetail", query);

        }

        /// <summary>
        /// Get user by userId
        /// </summary>
        /// <param name="id">userId</param>
        /// <returns></returns>
        public IActionResult GetUserById(int? id)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            if (id == null)
            {
                return NotFound();
            }

            var user = _db.Users.Where(u => u.Status == 1).FirstOrDefault(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var group = _db.Groups.Where(g => g.Status == 1).FirstOrDefault(p => p.GroupId == user.GroupId);

            if (group == null)
            {
                return NotFound();
            }

            ModalUserDetail detail = new ModalUserDetail
            {
                username = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                email = user.Mail,
                groupname = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp
            };

            return Json(detail);
        }

        /// <summary>
        /// Returns search results based on the start date and end date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IActionResult SearchRecordList(string startDate, string endDate)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            string format_Date = (_lang == "en-US") ? "MM/dd/yyyy HH:mm:ss" : "yyyy/MM/dd HH:mm:ss";
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();
            ViewBag.CanUseMachine = machines;
            List<Reservation> list_reser = new List<Reservation>();
            List<ResultSearch> resultSearches = new List<ResultSearch>();
            List<TimeLogResult> timeLogResults = new List<TimeLogResult>();

            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                list_reser = _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate)).OrderBy(p => p.MachineId).ToList();
            }

            if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                list_reser = _db.Reservations.Where(a => a.EndTime.Date <= Convert.ToDateTime(endDate)).OrderBy(p => p.MachineId).ToList();
            }

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                list_reser = _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate) && a.EndTime.Date <= Convert.ToDateTime(endDate)).OrderBy(p => p.MachineId).ToList();
            }

            if (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
               return GetRecordListAll();
            }

            if (user_id != 1)
            {
                for (int i = list_reser.Count() - 1; i >= 0; i--)
                {
                    if (machines.IndexOf(list_reser[i].MachineId) > -1)
                    {

                    }
                    else
                    {
                        list_reser.RemoveAt(i);
                    }
                }
            }

            if (list_reser.Count() > 0)
            {
                var id_temp = 0;
                foreach (var item in list_reser)
                {
                    if (id_temp == item.MachineId)
                    {
                        timeLogResults.Add(new TimeLogResult
                        {
                            StartTime = item.StartTime.ToString(format_Date),
                            EndTime = item.EndTime.ToString(format_Date),
                            UseTime = ((item.EndTime - item.StartTime).TotalHours).ToString(),
                            UserNameEn = (_lang == "en-US") ? _db.Users.Find(item.UserId).UserNameEn : _db.Users.Find(item.UserId).UserNameJp ?? ""
                        });
                    }
                    else
                    {
                        id_temp = item.MachineId;

                        timeLogResults = new List<TimeLogResult>
                        {
                            new TimeLogResult
                            {
                                StartTime = item.StartTime.ToString(format_Date),
                                EndTime = item.EndTime.ToString(format_Date),
                                UseTime = ((item.EndTime - item.StartTime).TotalHours).ToString(),
                                UserNameEn = (_lang == "en-US") ? _db.Users.Find(item.UserId).UserNameEn : _db.Users.Find(item.UserId).UserNameJp ?? ""
                            }
                        };

                        if (timeLogResults.Count() > 0)
                        {
                            var machine = _db.Machines.Where(p=>p.Status == 1 && p.MachineId == id_temp).FirstOrDefault();
                            if (machine != null)
                            {
                                resultSearches.Add(new ResultSearch
                                {
                                    MachineId = machine.MachineId,
                                    MachineNameEn = (_lang == "en-US") ? machine.MachineNameEn : machine.MachineNameJp,
                                    TimeLog = timeLogResults
                                });
                            }
                        }
                    }
                }
            }

            ViewBag.Result = resultSearches;
            return PartialView("_SearchRecord", ViewBag.Result);
        }

        /// <summary>
        /// Export use record list CSV by start date and end date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ExportUseRecordListCSV(string startDate, string endDate)
        {
            // Set cocolumn Headers
            var columnHeaders = new string[]
            {
                "機器名(日)",
                "機器名(英)",
                "開始時間",
                "終了時間",
                "使用時間",
                "所属（日）",
                "所属（英）",
                "使用者（日）",
                "使用者（英）",
                "使用用途・備考",
                "Run数",
                "Walk",
                "Long text to test"

            };

            // Set file name
            var filenameCSV = $"UseRecordList.csv";

            // Query retrieves data from database

            List<object[]> query = new List<object[]>();

            if (HttpContext.Session.GetInt32("UserId") != 1)
            {
                query = List_Data_Obj(startDate, endDate);
            }
            else
            {
                if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
                {
                    query = (from reser in _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate)).OrderBy(p => p.MachineId)
                             join machine in _db.Machines on reser.MachineId equals machine.MachineId
                             join user in _db.Users on reser.UserId equals user.UserId
                             join gr in _db.Groups on user.GroupId equals gr.GroupId
                             select new object[]
                             {
                             // In the case of [,], the data is surrounded by ["]
                             !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                             !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                             reser.StartTime.ToString("yyyy/MM/dd H:mm:ss"),
                             reser.EndTime.ToString("yyyy/MM/dd H:mm:ss"),
                             (reser.EndTime.Subtract(reser.StartTime)).TotalHours,
                             !string.IsNullOrEmpty(gr.GroupNameJp) ? gr.GroupNameJp.IndexOf(",") > -1 ? $"\"{gr.GroupNameJp}\"" : $"{gr.GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(gr.GroupNameEn) ? gr.GroupNameEn.IndexOf(",") > -1 ? $"\"{gr.GroupNameEn}\"" : $"{gr.GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(user.UserNameJp) ? user.UserNameJp.IndexOf(",") > -1 ? $"\"{user.UserNameJp}\"" : $"{user.UserNameJp}" : $"",
                             !string.IsNullOrEmpty(user.UserNameEn) ? user.UserNameEn.IndexOf(",") > -1 ? $"\"{user.UserNameEn}\"" : $"{user.UserNameEn}" : $"",
                             !string.IsNullOrEmpty(reser.Note) ? reser.Note.IndexOf(",") > -1 ? $"\"{reser.Note}\"" : $"{reser.Note}" : $"",
                             $"",
                             $"",
                             $""
                             }).ToList();
                }
                else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    query = (from reser in _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate) && a.StartTime.Date <= Convert.ToDateTime(endDate)).OrderBy(a => a.MachineId)
                             join machine in _db.Machines on reser.MachineId equals machine.MachineId
                             join user in _db.Users on reser.UserId equals user.UserId
                             join gr in _db.Groups on user.GroupId equals gr.GroupId
                             select new object[]
                             {
                             // In the case of [,], the data is surrounded by ["]
                             !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                             !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                             reser.StartTime.ToString("yyyy/MM/dd H:mm:ss"),
                             reser.EndTime.ToString("yyyy/MM/dd H:mm:ss"),
                             (reser.EndTime.Subtract(reser.StartTime)).TotalHours,
                             !string.IsNullOrEmpty(gr.GroupNameJp) ? gr.GroupNameJp.IndexOf(",") > -1 ? $"\"{gr.GroupNameJp}\"" : $"{gr.GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(gr.GroupNameEn) ? gr.GroupNameEn.IndexOf(",") > -1 ? $"\"{gr.GroupNameEn}\"" : $"{gr.GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(user.UserNameJp) ? user.UserNameJp.IndexOf(",") > -1 ? $"\"{user.UserNameJp}\"" : $"{user.UserNameJp}" : $"",
                             !string.IsNullOrEmpty(user.UserNameEn) ? user.UserNameEn.IndexOf(",") > -1 ? $"\"{user.UserNameEn}\"" : $"{user.UserNameEn}" : $"",
                             !string.IsNullOrEmpty(reser.Note) ? reser.Note.IndexOf(",") > -1 ? $"\"{reser.Note}\"" : $"{reser.Note}" : $"",
                             $"",
                             $"",
                             $""
                             }).ToList();
                }
                else
                {
                    query = (from reser in _db.Reservations.OrderBy(a => a.MachineId)
                             join machine in _db.Machines on reser.MachineId equals machine.MachineId
                             join user in _db.Users on reser.UserId equals user.UserId
                             join gr in _db.Groups on user.GroupId equals gr.GroupId
                             select new object[]
                             {
                             // In the case of [,], the data is surrounded by ["]
                             !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                             !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                             reser.StartTime.ToString("yyyy/MM/dd H:mm:ss"),
                             reser.EndTime.ToString("yyyy/MM/dd H:mm:ss"),
                             (reser.EndTime.Subtract(reser.StartTime)).TotalHours,
                             !string.IsNullOrEmpty(gr.GroupNameJp) ? gr.GroupNameJp.IndexOf(",") > -1 ? $"\"{gr.GroupNameJp}\"" : $"{gr.GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(gr.GroupNameEn) ? gr.GroupNameEn.IndexOf(",") > -1 ? $"\"{gr.GroupNameEn}\"" : $"{gr.GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(user.UserNameJp) ? user.UserNameJp.IndexOf(",") > -1 ? $"\"{user.UserNameJp}\"" : $"{user.UserNameJp}" : $"",
                             !string.IsNullOrEmpty(user.UserNameEn) ? user.UserNameEn.IndexOf(",") > -1 ? $"\"{user.UserNameEn}\"" : $"{user.UserNameEn}" : $"",
                             !string.IsNullOrEmpty(reser.Note) ? reser.Note.IndexOf(",") > -1 ? $"\"{reser.Note}\"" : $"{reser.Note}" : $"",
                             $"",
                             $"",
                             $""
                             }).ToList();
                }
            }

            if (query.Count() != 0)
            {
                // Build the file content
                var useRecordCSV = new StringBuilder();
                query.ForEach(line =>
                {
                    useRecordCSV.AppendLine(string.Join(",", line));
                });

                byte[] buffer = Encoding.GetEncoding("Shift-JIS").GetBytes($"{string.Join(",", columnHeaders)}\r\n{useRecordCSV}");

                // use application/octet-stream for file downloads, saves us needing to
                // infer MIME type
                return File(buffer, "application/octet-stream", filenameCSV);
            }
            else
            {
                return StatusCode(206);
            }
        }

        /// <summary>
        /// Get data export if user not admin
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<object[]> List_Data_Obj(string startDate, string endDate)
        {
            List<object[]> obj = new List<object[]>();
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();

            var sqlQuery = _db.Reservations.OrderBy(p => p.MachineId);


            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate)).OrderBy(p => p.MachineId);
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate) && a.EndTime.Date <= Convert.ToDateTime(endDate)).OrderBy(p => p.MachineId);
            }

            for (int i = 0; i < machines.Count(); i++)
            {
                var query = sqlQuery.Join(
                                    _db.Machines.Where(m => m.MachineId == machines[i] && m.Status == 1),
                                    res => res.MachineId,
                                    mac => mac.MachineId,
                                    (res, mac) => new
                                    {
                                        MachineNameEn = mac.MachineNameEn,
                                        MachineNameJp = mac.MachineNameJp,
                                        StartTime = res.StartTime,
                                        EndTime = res.EndTime,
                                        Note = res.Note,
                                        UserId = res.UserId

                                    }
                               )
                             .Join(
                                    _db.Users.Where(u => u.Status == 1),
                                    rs => rs.UserId,
                                    us => us.UserId,
                                    (rs, us) => new
                                    {
                                        MachineNameEn = rs.MachineNameEn,
                                        MachineNameJp = rs.MachineNameJp,
                                        StartTime = rs.StartTime,
                                        EndTime = rs.EndTime,
                                        Note = rs.Note,
                                        UserId = rs.UserId,
                                        UserNameEn = us.UserNameEn,
                                        UserNameJp = us.UserNameJp,
                                        GroupId = us.GroupId

                                    }
                                )
                              .Join(
                                    _db.Groups.Where(g => g.Status == 1),
                                    r => r.GroupId,
                                    gr => gr.GroupId,
                                    (r, gr) => new
                                    {
                                        MachineNameEn = r.MachineNameEn,
                                        MachineNameJp = r.MachineNameJp,
                                        StartTime = r.StartTime,
                                        EndTime = r.EndTime,
                                        Note = r.Note,
                                        UserId = r.UserId,
                                        UserNameEn = r.UserNameEn,
                                        UserNameJp = r.UserNameJp,
                                        GroupId = r.GroupId,
                                        GroupNameEn = gr.GroupNameEn,
                                        GroupNameJp = gr.GroupNameJp
                                    }
                                ).ToList();



                foreach (var item in query)
                {
                    obj.Add(new object[]
                    {
                             !string.IsNullOrEmpty(item.MachineNameJp) ? item.MachineNameJp.IndexOf(",") > -1 ? $"\"{item.MachineNameJp}\"" : $"{item.MachineNameJp}" : $"",
                             !string.IsNullOrEmpty(item.MachineNameEn) ? item.MachineNameEn.IndexOf(",") > -1 ? $"\"{item.MachineNameEn}\"" : $"{item.MachineNameEn}" : $"",
                             item.StartTime.ToString("yyyy/MM/dd H:mm:ss"),
                             item.EndTime.ToString("yyyy/MM/dd H:mm:ss"),
                             (item.EndTime.Subtract(item.StartTime)).TotalHours,
                             !string.IsNullOrEmpty(item.GroupNameJp) ? item.GroupNameJp.IndexOf(",") > -1 ? $"\"{item.GroupNameJp}\"" : $"{item.GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(item.GroupNameEn) ? item.GroupNameEn.IndexOf(",") > -1 ? $"\"{item.GroupNameEn}\"" : $"{item.GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(item.UserNameJp) ? item.UserNameJp.IndexOf(",") > -1 ? $"\"{item.UserNameJp}\"" : $"{item.UserNameJp}" : $"",
                             !string.IsNullOrEmpty(item.UserNameEn) ? item.UserNameEn.IndexOf(",") > -1 ? $"\"{item.UserNameEn}\"" : $"{item.UserNameEn}" : $"",
                             !string.IsNullOrEmpty(item.Note) ? item.Note.IndexOf(",") > -1 ? $"\"{item.Note}\"" : $"{item.Note}" : $"",
                             $"",
                             $"",
                             $""
                    });
                }
            }

            return obj;
        }

        public JsonResult GetCustomFields(int machinelID)
        {
            var getListCustomFields = _db.CustomFields.Where(p => p.MachineId == machinelID).ToList().OrderBy(p => p.FieldId);

            return Json(getListCustomFields);
        }
    }
}