using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Text;

namespace ReservationSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly DbConnectionClass _db;
        private readonly IWebHostEnvironment _env;

        public UserController(DbConnectionClass db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        /*
         * Login page
         * **/
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserType") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("ja-JP")),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) }
            );

            //Load list infomation
            var list_info = _db.Infomations.OrderByDescending(p => p.InfomationId);
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            ViewBag.Lang = "ja-JP";
            return View(list_info);

        }
        /*
         * Login action
         * **/
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var find_User = _db.Users.Where(u => u.LoginAccount.Equals(username) && u.LoginPassword.Equals(password) && u.Status == 1).FirstOrDefault();

            if (find_User != null)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetInt32("UserType", find_User.UserType);
                HttpContext.Session.SetInt32("UserId", find_User.UserId);
                HttpContext.Session.SetInt32("BuiltinUser", find_User.BuiltinUser ? 1 : 0);
                HttpContext.Session.SetInt32("GroupId", find_User.GroupId);
                var result = new
                {
                    status = 200,
                    message = "Login success"
                };
                return Json(result);
            }
            else
            {
                var result = new
                {
                    status = 404,
                    message = "Login success"
                };
                return Json(result);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserType");
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("BuiltinUser");
            HttpContext.Session.Remove("GroupId");
            HttpContext.Session.Clear();
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("ja-JP")),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) }
            );

            return RedirectToAction("Login");
        }


        private List<Group> getAllChildGroups(int? groupId) {
            var groups = new List<Group>();
            if (!groupId.HasValue) return groups;
            var childgroups = _db.Groups.Where(x => x.Status == 1 && x.ParentGroupId == groupId).OrderBy(x => x.ParentGroupId).ToList();
            foreach(var childgroup in childgroups)
            {
                groups.Add(childgroup);
                var subchilds = getAllChildGroups(childgroup.GroupId);
                foreach(var item in subchilds)
                {
                    groups.Add(item);
                }
            }
            return groups;
        }

        /*
         * User Master page
         * **/
        public IActionResult UserMaster()
        {
            //check login
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }
            var groupId = Convert.ToInt32(HttpContext.Session.GetInt32("GroupId"));
            ViewBag.GroupId = groupId;
            if (groupId > 1)
            {
                var groups = new List<Group>();
                var group = _db.Groups.Where(x => x.Status == 1 && x.GroupId == groupId).FirstOrDefault();
                groups.Add(group);

                var childgroups = getAllChildGroups(groupId);
                foreach(var childgroup in childgroups)
                {
                    groups.Add(childgroup);
                }

                ViewBag.ListGroups = groups;

            }
            else
            {
                var groups = _db.Groups.Where(x => x.Status == 1).OrderBy(x => x.ParentGroupId).ToList();
                ViewBag.ListGroups = groups;

            }

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return View();
        }

        /*
         * Load user group
         * **/
        public IActionResult LoadUserGroup()
        {
            var groupId = HttpContext.Session.GetInt32("GroupId");
            if (groupId > 1)
            {
                var groups = new List<Group>();
                var group = _db.Groups.Where(x => x.Status == 1 && x.GroupId == groupId).FirstOrDefault();
                groups.Add(group);

                var childgroups = getAllChildGroups(groupId);
                foreach(var childgroup in childgroups)
                {
                    groups.Add(childgroup);
                }

                ViewBag.ListGroups = groups;
            }
            else
            {
                var groups = _db.Groups.Where(x => x.Status == 1).OrderBy(x => x.ParentGroupId).ToList();
                ViewBag.ListGroups = groups;
            }
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return PartialView("_LoadUserGroup");
        }

        /*
         * Load user group to sorting
         * **/
        public IActionResult LoadUserGroupToSort()
        {
            var groupId = HttpContext.Session.GetInt32("GroupId");
            if (groupId > 1)
            {
                var groups = new List<Group>();
                var group = _db.Groups.Where(x => x.Status == 1 && x.GroupId == groupId).FirstOrDefault();
                groups.Add(group);

                var childgroups = getAllChildGroups(groupId);
                foreach(var childgroup in childgroups)
                {
                    groups.Add(childgroup);
                }

                ViewBag.ListGroups = groups;
            }
            else
            {
                var groups = _db.Groups.Where(x => x.Status == 1).OrderBy(x => x.ParentGroupId).ToList();
                ViewBag.ListGroups = groups;
            }
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return PartialView("_LoadUserGroupToSort");
        }

        /*
         * 
         *Update group when moving
         *
         */
        public IActionResult MoveGroupUpdate(int childId, int parentId)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            if(parentId == 1)
            {
                var result = new
                {
                    status = 301,
                    message = (_lang == "en-US") ? "It is not possible to move." : "移動できません。"
                };
                return Json(result);
            }

            var group = _db.Groups.Find(childId);
            if(group.ParentGroupId == parentId || group.ParentGroupId == null && parentId == 0)
            {
                var result = new
                {
                    status = 300,
                    message = (_lang == "en-US") ? "It is the same group. It is not possible to move." : "同じ所属です。移動できません。"
                };
                return Json(result);
            }
            else
            {
                if(parentId == 0)
                {
                    group.ParentGroupId = null;
                }
                else
                {
                    group.ParentGroupId = parentId;
                }
                
                _db.Groups.Update(group);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = (_lang == "en-US") ? "Group information was registed": "所属情報を登録しました。"
                };
                return Json(result);
            }
            
        }

        /*
         * Get user in group
         * **/
        public IActionResult GetUserInGroup(int id, int status)
        {
            var user_id = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            try
            {
                if (status == 2)
                {
                    var users = _db.Users.Where(x => x.GroupId == id)
                                     .Join(
                                         _db.Groups,
                                         user => user.GroupId,
                                         group => group.GroupId,
                                         (user, group) => new
                                         {
                                             userId = user.UserId,
                                             userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                             groupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                             groupId = group.GroupId,
                                             userClass = (_lang == "en-US") ? ((user.UserType == 1) ? "Admin" : "Normal") : ((user.UserType == 1) ? "管理者" : "一般"),
                                             status = (user_id == user.UserId) ? 3 : user.Status
                                         }
                                     ).ToList();
                    return Json(users);
                }
                else
                {
                    var users = _db.Users.Where(x => x.GroupId == id).Where(x => x.Status == status)
                                    .Join(
                                        _db.Groups,
                                        user => user.GroupId,
                                        group => group.GroupId,
                                        (user, group) => new
                                        {
                                            userId = user.UserId,
                                            userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                            groupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                            groupId = group.GroupId,
                                            userClass = (_lang == "en-US") ? ((user.UserType == 1) ? "Admin" : "Normal" ) : ((user.UserType == 1) ? "管理者" : "一般"),
                                            status = (user_id == user.UserId) ? 3 : user.Status
                                        }
                                    ).ToList();
                    return Json(users);
                }
            }catch (Exception){
                return Ok(400);
            }


        }

        /*
         * Get group detail by group id
         * **/
        public IActionResult GetGroupDetail(int id)
        {
            var group = _db.Groups.Find(id);
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return PartialView("_GetGroupDetail", group);
        }

        public IActionResult SaveGroup(int id, string groupNameEn, string groupNameJp)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var group = _db.Groups.Find(id);
            if (group == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Update Fail! GroupId not exist"
                };
                return Json(result);
            }
            else
            {
                group.GroupNameEn = groupNameEn;
                group.GroupNameJp = groupNameJp;
                _db.Groups.Update(group);
                _db.SaveChanges();

                var result = new
                {
                    status = 200,
                    message = "Update Success",
                    data = group,
                    groupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp
                };
                return Json(result);
            }
            
        }

        public IActionResult CreateGroup(int parent_id, string groupNameEn, string groupNameJp)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var group = new Group();
            group.GroupNameEn = groupNameEn;
            group.GroupNameJp = groupNameJp;
            group.Status = 1;
            if (parent_id > 0)
            {
                group.ParentGroupId = parent_id;
            }               
            _db.Add(group);
            _db.SaveChanges();
            var result = new
            {
                status = 200,
                message = "Create Group Success",
                data = group,
                groupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp
            };
            return Json(result);
        }

        public IActionResult DeleteGroup(int id)
        {
            var group = _db.Groups.Where(x=>x.GroupId == id).First();
            if (group == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Delete Fail! GroupId not exist"
                };
                return Json(result);
            }
            else
            {
                var groupsID = _db.Groups.Where(x => x.GroupId == id || x.ParentGroupId == id).Select(g=>g.GroupId).ToArray();

                var getUserByGroup = _db.Users.Where(u=> groupsID.Contains(u.GroupId)).ToList();

                if (getUserByGroup == null)
                {
                    var result1 = new
                    {
                        status = 400,
                        message = "Delete Fail! GroupId not exist"
                    };
                    return Json(result1);
                }
                else
                {
                    foreach (var item in getUserByGroup)
                    {
                        item.Status = 0;
                        _db.Users.Update(item);
                    }
                }

                group.Status = 0;
                _db.Groups.Update(group);
                _db.SaveChanges();

                var result = new
                {
                    status = 200,
                    message = "Delete Group Success",
                    //data = group
                };
                return Json(result);
            }
        }

        /*
         * Get user detail by group id
         * **/
        public IActionResult GetUserById(int id)
        {
            var userType = HttpContext.Session.GetInt32("UserType");
            ViewBag.userType = userType;
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            ViewBag.Lang = _lang;

            var machines = _db.Machines.Where(m => m.Status == 1).Join(
                                    _db.CanUseMachines.Where(can => can.UserId == id),
                                    machine => machine.MachineId,
                                    can => can.MachineId,
                                    (machine, can) => new
                                    {
                                        machineId = machine.MachineId,
                                        machineNameJp = machine.MachineNameJp,
                                        machineNameEn = machine.MachineNameEn
                                    }
                                ).ToList();
            var user = _db.Users.Include(u => u.Group).Include(u => u.CanUseMachines).Where(u => u.UserId == id).First();

            List<int> idList = new List<int>();
            List<string> nameList = new List<string>();

            foreach (var item in machines)
            {
                idList.Add(item.machineId);
                var name = (_lang == "en-US") ? item.machineNameEn : item.machineNameJp;
                nameList.Add(name);
            }

            ViewBag.idList = idList;
            ViewBag.nameList = nameList;
            
            return PartialView("_GetUserDetail", user);
        }

        public IActionResult DeleteUserById(int id)
        {
            var user = _db.Users.Include(u => u.CanUseMachines).Where(u => u.UserId == id).First();
            if (user == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Delete Fail! User not exist"
                };
                return Json(result);
            }
            else
            {
                user.Status = 0;
                _db.Users.Update(user);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = "Delete User Success"
                };
                return Json(result);
            }
        }

        public IActionResult RestoreUserById(int id)
        {
            var user = _db.Users.Include(u => u.CanUseMachines).Where(u => u.UserId == id).First();
            if (user == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Restore Fail! User not exist"
                };
                return Json(result);
            }
            else
            {
                user.Status = 1;
                _db.Users.Update(user);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = "Restore User Success"
                };
                return Json(result);
            }
        }

        [HttpPost]
        public IActionResult SaveCreateUser(string userNameJp, string userNameEn, string tel,string mail,
                string loginAccount,string loginPassword,int userType, int groupId, string machineId)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var checkuser = _db.Users.Where(u => u.LoginAccount == loginAccount).FirstOrDefault();
            if (checkuser == null)
            {
                int[] arrMachineId = null;
                if (machineId != null && machineId.Length > 0)
                {
                    arrMachineId = machineId.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                }

                var group = _db.Groups.Include(p => p.Users).Where(x => x.GroupId == groupId).FirstOrDefault();

                var user = new User();
                user.UserNameJp = userNameJp;
                user.UserNameEn = userNameEn;
                user.Tel = tel;
                user.Mail = mail;
                user.LoginAccount = loginAccount;
                user.LoginPassword = loginPassword;
                user.UserType = userType;
                user.Status = 1;
                user.GroupId = groupId;

                group.Users.Add(user);
                _db.SaveChanges();

                var newUser = _db.Users.Include(u => u.CanUseMachines).Where(u => u.UserId == user.UserId).FirstOrDefault();
                if (arrMachineId != null)
                {
                    foreach (int i in arrMachineId)
                    {
                        var canUseMachine = new CanUseMachine();
                        canUseMachine.MachineId = i;
                        canUseMachine.UserId = user.UserId;
                        newUser.CanUseMachines.Add(canUseMachine);
                    }
                    _db.SaveChanges();
                }
                var result = new
                {
                    status = 200,
                    message = "Create User Success"
                };
                return Json(result);
            }
            else
            {
                var result = new
                {
                    status = 400,
                    message = (_lang == "en-US") ? "This login ID is already used." : "このログインIDは既に使用されています"
                };
                return Json(result);
            }

        }
        [HttpPost]
        public IActionResult SaveUpdateUser(int id, string userNameJp, string userNameEn, string tel, string mail,
                string loginAccount, string loginPassword, int userType, string machineId)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            User user = _db.Users.Include(u => u.CanUseMachines).Where(u => u.UserId == id).FirstOrDefault();
            if (user == null)
            {
                var result = new
                {
                    status = 400,
                    message = (_lang == "en-US") ? "User information does not exist" : "指定されたユーザーは存在しません。"
                };
                return Json(result);
            }
            else
            {
                var checkuser = _db.Users.Where(u => u.LoginAccount == loginAccount).FirstOrDefault();
                if (checkuser != null && user.LoginAccount != loginAccount)
                {
                    var result = new
                    {
                        status = 400,
                        message = (_lang == "en-US") ? "This login ID is already used." : "このログインIDは既に使用されています"
                    };
                    return Json(result);

                }
                else { 
                    user.UserNameJp = userNameJp;
                    user.UserNameEn = userNameEn;
                    user.Tel = tel;
                    user.Mail = mail;
                    user.LoginAccount = loginAccount;
                    user.LoginPassword = loginPassword;
                    if (HttpContext.Session.GetInt32("UserType") == 1)
                    {
                        user.UserType = userType;
                    }
                    _db.Users.Update(user);
                    user.CanUseMachines.Clear();
                    try
                    {
                        _db.SaveChanges();
                    }
                    catch (Exception) { }

                    int[] arrMachineId = null;
                    if (machineId != null && machineId.Length > 0)
                    {
                        arrMachineId = machineId.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                        foreach (int i in arrMachineId)
                        {
                            var canUseMachine = new CanUseMachine();
                            canUseMachine.MachineId = i;
                            canUseMachine.UserId = user.UserId;
                            user.CanUseMachines.Add(canUseMachine);
                        }
                        try
                        {
                            _db.SaveChanges();
                        }
                        catch (Exception) { }
                    }
                    // var new_user = _db.Users.Find(id);
                    var result = new
                    {
                        status = 200,
                        message = "Update User Success"
                        // data = new_user
                    };
                    return Json(result);
                }
                
            }
        }

        [HttpPost]
        public IActionResult UpdateUserGroup(int id, int groupId)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            User user = _db.Users.Include(u => u.CanUseMachines).Where(u => u.UserId == id).FirstOrDefault();
            if (user == null)
            {
                var result = new
                {
                    status = 400,
                    message = (_lang == "en-US") ? "User information does not exist" : "指定されたユーザーは存在しません。"
                };
                return Json(result);
            }
            else
            {
                user.GroupId = groupId;
                _db.Users.Update(user);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = "Update User Success"
                    // data = new_user
                };
                return Json(result);
            }
        }

        /*
         * Get all user for choice user poup
         * **/
        public IActionResult GetAllUserToChoice(string userId)
        {
            var users = _db.Users.Where(u => u.Status > 0).ToList();
            int[] arrUserId = null;
            if (userId != null && userId.Length > 0)
            {
                arrUserId = userId.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }
            ViewBag.arrUserId = arrUserId;
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return PartialView("_GetAllUserToChoice", users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _db.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        

        private bool UsersExists(int id)
        {
            return _db.Users.Any(e => e.UserId == id);
        }

        private bool checkTelFormat(String tel)
        {
            Boolean iTotal = true;
            tel.ToList().ForEach(ch => {
                int iCode = ch;
                //number
                if ((iCode >= 48 && iCode <= 57) || iCode == 40 || iCode == 41 || iCode == 43 || iCode == 45 || iCode == 32)
                {
                    
                }
                else
                {
                    iTotal = false;
                }
            });
            return iTotal;
        }

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

        public void WriteLog(string strLog)
        {
            DateTime time = DateTime.Now;
            string logFilePath = _env.WebRootPath + "\\logs\\user_master_import_error_log_" + time.ToString("yyyy-MM-dd") + ".txt";
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
            {
                using (StreamWriter log = new StreamWriter(fileStream))
                {
                    log.WriteLine("================================" + time.ToString("yyyy-MM-dd HH:mm:ss") + "======================================");
                    log.WriteLine(strLog);
                }
            }
        }

        public IActionResult CsvImportProccess(CsvFormData data)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var attachedFile = Request.Form.Files["CsvFile"];
            if (attachedFile == null || attachedFile.Length <= 0) return Json(null);
            var csvReader = new StreamReader(attachedFile.OpenReadStream(), Encoding.GetEncoding(932), false);
            var uploadModelList = new List<UserCsvData>();
            string inputDataRead;
            var values = new List<string>();

            while ((inputDataRead = csvReader.ReadLine()) != null)
            {
                values.Add(inputDataRead.Trim());
            }
            Boolean err = false;

            //get header
            var head = values[0];
            var arrHead = head.Split(',');
            int total_col = arrHead.Length;
            int[] col_val = new int[total_col];
            if (total_col > 8)
            {
                for (int i = 8; i < total_col; i++)
                {
                    string machine_name = arrHead[i];
                    Machine m = _db.Machines.Where(m => m.MachineNameJp == machine_name).FirstOrDefault();
                    if(m == null)
                    {
                        err = true;
                        WriteLog("Error at header - Machine: [" + machine_name + "] not exist.");
                        break;
                    }
                    else
                    {
                        col_val[i] = m.MachineId;
                    }
                }
            }
            //remove header
            values.Remove(values[0]);
            /*values.Remove(values[values.Count - 1]);*/
            if (!err)
            {
                List<string> cate_name_list = new List<string>();
                int row = 1;
                foreach (var value in values)
                {
                    var uploadModelRecord = new UserCsvData();
                    var eachValue = value.Split(',');

                    if (eachValue[0] == "" || countJapaneseStringLength(eachValue[0]) > 100)
                    {
                        WriteLog("Error at rows: " + row + " - 所属名(GroupName): [" + eachValue[1] + "] too long.");
                        err = true;
                        break;
                    }
                    else
                    {
                        var group_name = eachValue[0];
                        var group = _db.Groups.Include(c => c.Users).Where(c => c.GroupNameJp == group_name && c.Status != 0).FirstOrDefault();
                        if (group == null)
                        {
                            WriteLog("Error at rows: " + row + " - 所属名 (GroupName): [" + group_name + "] not exist.");
                            err = true;
                            break;
                        }
                        else
                        {
                            uploadModelRecord.GroupName = group_name;
                            uploadModelRecord.GroupId = group.GroupId;

                            if (eachValue[1] == "" || countJapaneseStringLength(eachValue[1]) > 64)
                            {
                                WriteLog("Error at rows: " + row + " - ユーザーID (UserId): [" + eachValue[1] + "] too long.");
                                err = true;
                                break;
                            }
                            else
                            {
                                uploadModelRecord.LoginAccount = eachValue[1];
                            }

                            if (eachValue[2] == "" || countJapaneseStringLength(eachValue[2]) > 64)
                            {
                                WriteLog("Error at rows: " + row + " - パスワード　(LoginPassword): [" + eachValue[2] + "] too long.");
                                err = true;
                                break;
                            }
                            else
                            {
                                uploadModelRecord.LoginPassword = eachValue[2];
                            }

                            if (eachValue[3] == "" || countJapaneseStringLength(eachValue[3]) > 100)
                            {
                                WriteLog("Error at rows: " + row + " - ユーザー名(UserNameJp): [" + eachValue[3] + "] too long.");
                                err = true;
                                break;
                            }
                            else
                            {
                                uploadModelRecord.UserNameJp = eachValue[3];
                            }

                            if (eachValue[4] == "" || countJapaneseStringLength(eachValue[4]) > 100)
                            {
                                WriteLog("Error at rows: " + row + " - ユーザー名(UserNameEn): [" + eachValue[4] + "] too long.");
                                err = true;
                                break;
                            }
                            else
                            {
                                uploadModelRecord.UserNameEn = eachValue[4];
                            }

                            if (eachValue[5] == "" || countJapaneseStringLength(eachValue[5]) > 50)
                            {
                                WriteLog("Error at rows: " + row + " - ユーザー区分　(UserType): [" + eachValue[5] + "] too long.");
                                err = true;
                                break;
                            }
                            else
                            {
                                if (eachValue[5] == "管理者")
                                {
                                    uploadModelRecord.UserType = 1;
                                }
                                else
                                {
                                    uploadModelRecord.UserType = 0;
                                }

                            }

                            if (eachValue[6] == "" || countJapaneseStringLength(eachValue[6]) > 20 || countJapaneseStringLength(eachValue[6]) < 3)
                            {
                                WriteLog("Error at rows: " + row + " - Tel: [" + eachValue[6] + "] too long.");
                                err = true;
                                break;
                            }
                            else
                            {
                                if(checkTelFormat(eachValue[6])){
                                    uploadModelRecord.Tel = eachValue[6];
                                }
                                else
                                {
                                    WriteLog("Error at rows: " + row + " - Tel: [" + eachValue[6] + "] is not a Tel number.");
                                    err = true;
                                    break;
                                }
                            }

                            if (eachValue[7] == "" || countJapaneseStringLength(eachValue[7]) > 100)
                            {
                                WriteLog("Error at rows: " + row + " - Mail: [" + eachValue[7] + "] too long.");
                                err = true;
                                break;
                            }
                            else
                            {
                                uploadModelRecord.Mail = eachValue[7];
                            }

                            List<int> Lmachines = new List<int>();
                            if (total_col > 8)
                            { 
                                
                                for (int j = 8; j < total_col; j++)
                                {
                                    if(eachValue[j] == "1")
                                    {
                                        Lmachines.Add(col_val[j]);
                                    }

                                }
                            }
                            uploadModelRecord.MachineId = Lmachines;
                        }
                    }

                    uploadModelList.Add(uploadModelRecord);// newModel needs to be an object of type ContextTables.  
                    row++;
                }
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
                foreach(var item in uploadModelList)
                {
                    var user = _db.Users.Include(u => u.CanUseMachines).Where(u => u.LoginAccount == item.LoginAccount).FirstOrDefault();
                    if(user == null)
                    {
                        var us = new User();
                        us.LoginAccount = item.LoginAccount;
                        us.LoginPassword = item.LoginPassword;
                        us.GroupId = item.GroupId;
                        us.UserNameEn = item.UserNameEn;
                        us.UserNameJp = item.UserNameJp;
                        us.UserType = item.UserType;
                        us.Status = 1;
                        us.Tel = item.Tel;
                        us.Mail = item.Mail;
                        _db.Users.Add(us);
                        _db.SaveChanges();
                        var newUser = _db.Users.Include(u => u.CanUseMachines).Where(u => u.UserId == us.UserId).FirstOrDefault();
                        if (item.MachineId != null)
                        {
                            foreach (int i in item.MachineId)
                            {
                                var canUseMachine = new CanUseMachine();
                                canUseMachine.MachineId = i;
                                canUseMachine.UserId = us.UserId;
                                newUser.CanUseMachines.Add(canUseMachine);
                            }
                            _db.SaveChanges();
                        }
                    }
                    else
                    {
                        user.UserNameJp = item.UserNameJp;
                        user.UserNameEn = item.UserNameEn;
                        user.Tel = item.Tel;
                        user.Mail = item.Mail;
                        user.LoginAccount = item.LoginAccount;
                        user.LoginPassword = item.LoginPassword;
                        user.UserType = item.UserType;
                        user.Status = 1;
                        user.GroupId = item.GroupId;

                        _db.Users.Update(user);
                        user.CanUseMachines.Clear();
                        _db.SaveChanges();

                        if (item.MachineId != null)
                        {
                            foreach (int i in item.MachineId)
                            {
                                var canUseMachine = new CanUseMachine();
                                canUseMachine.MachineId = i;
                                canUseMachine.UserId = user.UserId;
                                user.CanUseMachines.Add(canUseMachine);
                            }
                            _db.SaveChanges();
                        }
                    }
                }

                var result = new
                {
                    status = 200,
                    message = "Import success",
                    data = uploadModelList
                };
                return Json(result);
            }
        }

        public IActionResult LoadUserGroupFilter(int status)
        {
            var groupId = HttpContext.Session.GetInt32("GroupId");
            if (groupId > 1)
            {
                var groups = new List<Group>();
                
                if (status == 1)
                {
                    var group = _db.Groups.Where(x => x.Status == 1 && x.GroupId == groupId).FirstOrDefault();
                    groups.Add(group);

                    var childgroups = _db.Groups.Where(x => x.Status == 1 && x.ParentGroupId == groupId).OrderBy(x => x.ParentGroupId).ToList();
                    foreach (var childgroup in childgroups)
                    {
                        groups.Add(childgroup);
                        var subchilds = _db.Groups.Where(x => x.Status == 1 && x.ParentGroupId == childgroup.GroupId).OrderBy(x => x.ParentGroupId).ToList();
                        foreach (var item in subchilds)
                        {
                            groups.Add(item);
                        }
                    }
                }
                else
                {
                    var group = _db.Groups.Where(x =>x.GroupId == groupId).FirstOrDefault();
                    groups.Add(group);

                    var childgroups = _db.Groups.Where(x =>x.ParentGroupId == groupId).OrderBy(x => x.ParentGroupId).ToList();
                    foreach (var childgroup in childgroups)
                    {
                        groups.Add(childgroup);
                        var subchilds = _db.Groups.Where(x =>x.ParentGroupId == childgroup.GroupId).OrderBy(x => x.ParentGroupId).ToList();
                        foreach (var item in subchilds)
                        {
                            groups.Add(item);
                        }
                    }
                }

                ViewBag.ListGroups = groups;
            }
            else
            {
                List<Group> groups = new List<Group>();

                if (status == 1)
                {
                    groups = _db.Groups.Where(x => x.Status == 1).OrderBy(x => x.ParentGroupId).ToList();
                }
                else
                {
                    groups = _db.Groups.OrderBy(x => x.ParentGroupId).ToList();
                }

                ViewBag.ListGroups = groups;
            }
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return PartialView("_LoadUserGroup");
        }
    }
}
