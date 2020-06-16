using System;
using System.Collections.Generic;
using System.IO;
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
    public class MaintenanceRecordsController : Controller
    {
        private readonly DbConnectionClass _db;
        public MaintenanceRecordsController(DbConnectionClass db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            // check login
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }
            var categories = _db.Categories.Where(c => c.Status == 1).Include(c => c.Machines).ToList();
            ViewBag.ListCategories = categories;
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();
            ViewBag.CanUseMachine = machines;

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();

            return View();
        }

        public IActionResult GetAllRecords()
        {
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();
            ViewBag.CanUseMachine = machines;
            var merchines = _db.Machines.Include(m => m.Category).Where(p=>p.Status == 1).ToList();

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();

            return PartialView("_RecordsList", merchines);
        }

        public IActionResult GetMaintenanceByMachineId(int id, string startDate, string endDate)
        {
            List<MaintenanceRecord> maintenance = new List<MaintenanceRecord>();
            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                maintenance = _db.MaintenanceRecords.OrderBy(a => a.RecordDate).Where(p => p.MachineId == id && p.RecordDate.Date >= Convert.ToDateTime(startDate)).Include(p => p.Machine).ToList();
                return PartialView("_MaintenanceRecords", maintenance);
            }

            if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                maintenance = _db.MaintenanceRecords.OrderBy(a => a.RecordDate).Where(p => p.MachineId == id && p.RecordDate.Date <= Convert.ToDateTime(endDate)).Include(p => p.Machine).ToList();
                return PartialView("_MaintenanceRecords", maintenance);
            }

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {

                maintenance = _db.MaintenanceRecords.OrderBy(a => a.RecordDate).Where(p => p.MachineId == id && p.RecordDate.Date >= Convert.ToDateTime(startDate) && p.RecordDate.Date <= Convert.ToDateTime(endDate)).Include(p => p.Machine).ToList();
                return PartialView("_MaintenanceRecords", maintenance);
            }

            maintenance = _db.MaintenanceRecords.OrderBy(a => a.RecordDate).Where(p => p.MachineId == id).Include(p => p.Machine).ToList();

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();

            return PartialView("_MaintenanceRecords", maintenance);


        }

        public IActionResult CreateUpdateMaintenanceRecord(MaintenanceRecord record)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            if (record.RecordId != 0) // update
            {

                var getRecordById = _db.MaintenanceRecords.Find(record.RecordId);

                if (getRecordById == null)
                {
                    var result = new
                    {
                        status = 400,
                        message = (_lang == "en-US") ? "Specified Maintenance Record infromation does not exist" : "指定されたメンテナンス記録は存在しません。",
                        
                    };
                    return Json(result);
                }
                else
                {
                    getRecordById.RecordDate = record.RecordDate;
                    getRecordById.Title = record.Title;
                    getRecordById.Note = record.Note;

                    _db.MaintenanceRecords.Update(getRecordById);
                    _db.SaveChanges();

                    var result = new
                    {
                        status = 200,
                        message = (_lang == "en-US") ? "Category information was updated." : "メンテナンス記録情報を更新しました。",
                    };
                    return Json(result);
                }
            }
            else // insert
            {

                var maintenance = new MaintenanceRecord()
                {
                    MachineId = record.MachineId,
                    UserId = record.UserId,
                    RecordDate = record.RecordDate,
                    Title = record.Title,
                    Note = record.Note
                };

                _db.MaintenanceRecords.Add(maintenance);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = (_lang == "en-US") ? "Category information was registed." : "メンテナンス記録情報を登録しました。",
                };
                return Json(result);
            }
        }

        public IActionResult DeleteRecord(int id)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var record = _db.MaintenanceRecords.Find(id);
            if (record == null)
            {
                var result = new
                {
                    status = 400,
                    message = (_lang == "en-US") ? "Specified Maintenance Record infromation does not exist" : "指定されたメンテナンス記録は存在しません。",
                };
                return Json(result);
            }
            else
            {
                _db.MaintenanceRecords.Remove(record);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = (_lang == "en-US") ? "Category information was deleted." : "メンテナンス記録情報を削除しました。"
                };
                return Json(result);
            }
        }

        public IActionResult DetailRecord(int id, string startDate, string endDate)
        {
            ParamaterSearch paramater = new ParamaterSearch
            {
                id = id,
                startDate = startDate,
                endDate = endDate
            };

            ViewBag.Parameter = paramater;
            return PartialView("_DetailRecord", ViewBag.Parameter);
        }

        public IActionResult GetDataDetailRecord(int id, string startDate, string endDate)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            var dateFormat = (_lang == "en-US") ? "MM/dd/yyyy" : "yyyy/MM/dd";

            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                return Json((from maint in _db.MaintenanceRecords.Where(p => p.MachineId == id && p.RecordDate.Date >= Convert.ToDateTime(startDate))
                             select new
                             {
                                 recordId = maint.RecordId,
                                 recordDate = (maint.RecordDate).ToString(dateFormat),
                                 title = maint.Title,
                                 note = maint.Note
                             }).ToList().OrderBy(p => p.recordDate));
            }

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                return Json((from maint in _db.MaintenanceRecords.Where(p => p.MachineId == id && p.RecordDate.Date >= Convert.ToDateTime(startDate) && p.RecordDate.Date <= Convert.ToDateTime(endDate))
                             select new
                             {
                                 recordId = maint.RecordId,
                                 recordDate = (maint.RecordDate).ToString(dateFormat),
                                 title = maint.Title,
                                 note = maint.Note
                             }).ToList().OrderBy(p => p.recordDate));
            }

            var query = (from maint in _db.MaintenanceRecords.Where(p => p.MachineId == id)
                         select new
                         {
                             recordId = maint.RecordId,
                             recordDate = (maint.RecordDate).ToString(dateFormat),
                             title = maint.Title,
                             note = maint.Note
                         }).ToList().OrderBy(p => p.recordDate);

            return Json(query);
        }

        public IActionResult SearchRecordsMantain(string startDate, string endDate)
        {
            List<MaintenanceRecord> query = new List<MaintenanceRecord>();
            List<SearchResults> resultSearches = new List<SearchResults>();
            List<MaintenanceRecord> _listMaint = new List<MaintenanceRecord>();

            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                query = _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate)).OrderBy(p => p.MachineId).ToList();
            }

            if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                query = _db.MaintenanceRecords.Where(a => a.RecordDate.Date <= Convert.ToDateTime(endDate)).OrderBy(p => p.MachineId).ToList();
            }

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                query = _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate) && a.RecordDate.Date <= Convert.ToDateTime(endDate)).OrderBy(p => p.MachineId).ToList();
            }

            if (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                return GetAllRecords();
            }

            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();

            if (user_id != 1)
            {
                for (int i = query.Count() - 1; i >= 0; i--)
                {
                    if (machines.IndexOf(query[i].MachineId) > -1)
                    {

                    }
                    else
                    {
                        query.RemoveAt(i);
                    }
                }
            }

            if (query.Count() > 0)
            {
                var id_temp = 0;

                foreach (var item in query)
                {
                    if (id_temp != item.MachineId)
                    {
                        id_temp = item.MachineId;

                        var machine = _db.Machines.Include(m => m.Category).FirstOrDefault(p => p.MachineId == id_temp);

                        resultSearches.Add(new SearchResults
                        {
                            machine = machine,
                        });

                    }
                }
            }

            ViewBag.Result = resultSearches;
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();

            return PartialView("_ResultSearch", ViewBag.Result);
        }

        [HttpGet]
        public IActionResult MaintenExportCSV(string startDate, string endDate, int machineId, int flag_export)
        {
            // Set cocolumn Headers
            var columnHeaders = new string[]
            {
                "機器名(日)",
                "機器名(英)",
                "登録日",
                "タイトル",
                "内容"
            };

            // Set file name
            var filenameCSV = $"MRecord.csv";

            // Query retrieves data from database

            List<object[]> query = new List<object[]>();

            if (HttpContext.Session.GetInt32("UserId") != 1)
            {
                if (flag_export != 0)
                {
                    query = List_Data_Obj_DetailMainten(startDate, endDate, machineId);
                }
                else
                {
                    query = List_Data_Obj(startDate, endDate);
                }
                
            }
            else
            {
                if (flag_export != 0)
                {
                    query = List_Data_Obj_Admin_DetailMainten(startDate, endDate, machineId);
                }
                else
                {
                    query = List_Data_Obj_Admin(startDate, endDate);
                }
            }

            if (query.Count() != 0)
            {
                // Build the file content
                var maintencsv = new StringBuilder();
                query.ForEach(line =>
                {
                    maintencsv.AppendLine(string.Join(",", line));
                });

                byte[] buffer = Encoding.GetEncoding("Shift-JIS").GetBytes($"{string.Join(",", columnHeaders)}\r\n{maintencsv}");

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

            var sqlQuery = _db.MaintenanceRecords.OrderBy(a => a.MachineId);


            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate)).OrderBy(a => a.MachineId);
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate) && a.RecordDate.Date <= Convert.ToDateTime(endDate)).OrderBy(a => a.MachineId);
            }

            for (int i = 0; i < machines.Count; i++)
            {
                var query = sqlQuery.Join(
                                    _db.Machines.Where(m => m.MachineId == machines[i]),
                                    res => res.MachineId,
                                    mac => mac.MachineId,
                                    (res, mac) => new
                                    {
                                        MachineNameEn = mac.MachineNameEn,
                                        MachineNameJp = mac.MachineNameJp,
                                        RecordDate = res.RecordDate,
                                        Title = res.Title,
                                        Note = res.Note
                                    }
                               )
                             .ToList();



                foreach (var item in query)
                {
                    obj.Add(new object[]
                    {
                            !string.IsNullOrEmpty(item.MachineNameJp) ? item.MachineNameJp.IndexOf(",") > -1 ? $"\"{item.MachineNameJp}\"" : $"{item.MachineNameJp}" : $"",
                            !string.IsNullOrEmpty(item.MachineNameEn) ? item.MachineNameEn.IndexOf(",") > -1 ? $"\"{item.MachineNameEn}\"" : $"{item.MachineNameEn}" : $"",
                            item.RecordDate.ToString("yyyy/MM/dd H:mm:ss"),
                            !string.IsNullOrEmpty(item.Title) ? item.Title.IndexOf(",") > -1 ? $"\"{item.Title}\"" : $"{item.Title}" : $"",
                            !string.IsNullOrEmpty(item.Note) ? item.Note.IndexOf(",") > -1 ? $"\"{item.Note}\"" : $"{item.Note}" : $""
                    });
                }
            }

            return obj;
        }

        /// <summary>
        /// Get data export if user not admin
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<object[]> List_Data_Obj_DetailMainten(string startDate, string endDate, int machineId)
        {
            List<object[]> obj = new List<object[]>();
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();

            var sqlQuery = _db.MaintenanceRecords.Where(a=>a.MachineId == machineId).OrderBy(a => a.MachineId);


            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate) && a.MachineId == machineId).OrderBy(a => a.MachineId);
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate) && a.RecordDate.Date <= Convert.ToDateTime(endDate) && a.MachineId == machineId).OrderBy(a => a.MachineId);
            }

            for (int i = 0; i < machines.Count; i++)
            {
                var query = sqlQuery.Join(
                                    _db.Machines.Where(m => m.MachineId == machines[i]),
                                    res => res.MachineId,
                                    mac => mac.MachineId,
                                    (res, mac) => new
                                    {
                                        MachineNameEn = mac.MachineNameEn,
                                        MachineNameJp = mac.MachineNameJp,
                                        RecordDate = res.RecordDate,
                                        Title = res.Title,
                                        Note = res.Note
                                    }
                               )
                             .ToList();



                foreach (var item in query)
                {
                    obj.Add(new object[]
                    {
                            !string.IsNullOrEmpty(item.MachineNameJp) ? item.MachineNameJp.IndexOf(",") > -1 ? $"\"{item.MachineNameJp}\"" : $"{item.MachineNameJp}" : $"",
                            !string.IsNullOrEmpty(item.MachineNameEn) ? item.MachineNameEn.IndexOf(",") > -1 ? $"\"{item.MachineNameEn}\"" : $"{item.MachineNameEn}" : $"",
                            item.RecordDate.ToString("yyyy/MM/dd H:mm:ss"),
                            !string.IsNullOrEmpty(item.Title) ? item.Title.IndexOf(",") > -1 ? $"\"{item.Title}\"" : $"{item.Title}" : $"",
                            !string.IsNullOrEmpty(item.Note) ? item.Note.IndexOf(",") > -1 ? $"\"{item.Note}\"" : $"{item.Note}" : $""
                    });
                }
            }

            return obj;
        }



        /// <summary>
        /// Get data export if user admin
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<object[]> List_Data_Obj_Admin(string startDate, string endDate)
        {
            List<object[]> obj = new List<object[]>();
            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                obj = (from mainten in _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate)).OrderBy(a => a.MachineId)
                         join machine in _db.Machines on mainten.MachineId equals machine.MachineId
                         select new object[]
                         {
                             // In the case of [,], the data is surrounded by ["]
                            !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                            !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                            mainten.RecordDate.ToString("yyyy/MM/dd H:mm:ss"),
                            !string.IsNullOrEmpty(mainten.Title) ? mainten.Title.IndexOf(",") > -1 ? $"\"{mainten.Title}\"" : $"{mainten.Title}" : $"",
                            !string.IsNullOrEmpty(mainten.Note) ? mainten.Note.IndexOf(",") > -1 ? $"\"{mainten.Note}\"" : $"{mainten.Note}" : $""
                         }).ToList();
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                obj = (from mainten in _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate) && a.RecordDate.Date <= Convert.ToDateTime(endDate)).OrderBy(p => p.MachineId)
                         join machine in _db.Machines on mainten.MachineId equals machine.MachineId
                         select new object[]
                         {
                            // In the case of [,], the data is surrounded by ["]
                            !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                            !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                            mainten.RecordDate.ToString("yyyy/MM/dd H:mm:ss"),
                            !string.IsNullOrEmpty(mainten.Title) ? mainten.Title.IndexOf(",") > -1 ? $"\"{mainten.Title}\"" : $"{mainten.Title}" : $"",
                            !string.IsNullOrEmpty(mainten.Note) ? mainten.Note.IndexOf(",") > -1 ? $"\"{mainten.Note}\"" : $"{mainten.Note}" : $""
                         }).ToList();
            }
            else
            {
                obj = (from mainten in _db.MaintenanceRecords.OrderBy(a => a.MachineId)
                         join machine in _db.Machines on mainten.MachineId equals machine.MachineId
                         select new object[]
                         {
                           // In the case of [,], the data is surrounded by ["]
                            !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                            !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                            mainten.RecordDate.ToString("yyyy/MM/dd H:mm:ss"),
                            !string.IsNullOrEmpty(mainten.Title) ? mainten.Title.IndexOf(",") > -1 ? $"\"{mainten.Title}\"" : $"{mainten.Title}" : $"",
                            !string.IsNullOrEmpty(mainten.Note) ? mainten.Note.IndexOf(",") > -1 ? $"\"{mainten.Note}\"" : $"{mainten.Note}" : $""
                         }).ToList();
            }

            return obj;
        }

        /// <summary>
        /// Get data export if user admin
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<object[]> List_Data_Obj_Admin_DetailMainten(string startDate, string endDate, int machineId)
        {
            List<object[]> obj = new List<object[]>();
            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                obj = (from mainten in _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate) && a.MachineId == machineId).OrderBy(a => a.MachineId)
                       join machine in _db.Machines on mainten.MachineId equals machine.MachineId
                       select new object[]
                       {
                             // In the case of [,], the data is surrounded by ["]
                            !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                            !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                            mainten.RecordDate.ToString("yyyy/MM/dd H:mm:ss"),
                            !string.IsNullOrEmpty(mainten.Title) ? mainten.Title.IndexOf(",") > -1 ? $"\"{mainten.Title}\"" : $"{mainten.Title}" : $"",
                            !string.IsNullOrEmpty(mainten.Note) ? mainten.Note.IndexOf(",") > -1 ? $"\"{mainten.Note}\"" : $"{mainten.Note}" : $""
                       }).ToList();
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                obj = (from mainten in _db.MaintenanceRecords.Where(a => a.RecordDate.Date >= Convert.ToDateTime(startDate) && a.RecordDate.Date <= Convert.ToDateTime(endDate) && a.MachineId == machineId).OrderBy(p => p.MachineId)
                       join machine in _db.Machines on mainten.MachineId equals machine.MachineId
                       select new object[]
                       {
                            // In the case of [,], the data is surrounded by ["]
                            !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                            !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                            mainten.RecordDate.ToString("yyyy/MM/dd H:mm:ss"),
                            !string.IsNullOrEmpty(mainten.Title) ? mainten.Title.IndexOf(",") > -1 ? $"\"{mainten.Title}\"" : $"{mainten.Title}" : $"",
                            !string.IsNullOrEmpty(mainten.Note) ? mainten.Note.IndexOf(",") > -1 ? $"\"{mainten.Note}\"" : $"{mainten.Note}" : $""
                       }).ToList();
            }
            else
            {
                obj = (from mainten in _db.MaintenanceRecords.Where(a=>a.MachineId == machineId).OrderBy(a => a.MachineId)
                       join machine in _db.Machines on mainten.MachineId equals machine.MachineId
                       select new object[]
                       {
                           // In the case of [,], the data is surrounded by ["]
                            !string.IsNullOrEmpty(machine.MachineNameJp) ? machine.MachineNameJp.IndexOf(",") > -1 ? $"\"{machine.MachineNameJp}\"" : $"{machine.MachineNameJp}" : $"",
                            !string.IsNullOrEmpty(machine.MachineNameEn) ? machine.MachineNameEn.IndexOf(",") > -1 ? $"\"{machine.MachineNameEn}\"" : $"{machine.MachineNameEn}" : $"",
                            mainten.RecordDate.ToString("yyyy/MM/dd H:mm:ss"),
                            !string.IsNullOrEmpty(mainten.Title) ? mainten.Title.IndexOf(",") > -1 ? $"\"{mainten.Title}\"" : $"{mainten.Title}" : $"",
                            !string.IsNullOrEmpty(mainten.Note) ? mainten.Note.IndexOf(",") > -1 ? $"\"{mainten.Note}\"" : $"{mainten.Note}" : $""
                       }).ToList();
            }

            return obj;
        }
    }
}