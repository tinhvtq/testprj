using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

namespace ReservationSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbConnectionClass _db;
        private readonly IStringLocalizer<HomeController> _localizer;
        public HomeController(DbConnectionClass db, IStringLocalizer<HomeController> localizer)
        {
            _db = db;
            _localizer = localizer;
        }
        /*
         * Home Index page
        * Load category, mainternance record information, language 
        */
        public IActionResult Index()
        {
            // check login
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }

            var login_id = HttpContext.Session.GetInt32("UserId");

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();

            if (login_id > 1)
            {
                var machines = _db.CanUseMachines.Include(u => u.Machine).Where(u => u.UserId == Convert.ToInt32(login_id)).ToList();
                var machineReference = _db.Machines.Where(m => m.Status == 1 && m.ReferenceAuth == 1).ToList();
                List<int> cateList = new List<int>();
                foreach (var m in machines)
                {
                    var cateId = m.Machine.CategoryId;
                    if (cateList.IndexOf(cateId) < 0)
                    {
                        cateList.Add(cateId);
                    }
                }

                foreach (var m in machineReference)
                {
                    var cateId = m.CategoryId;
                    if (cateList.IndexOf(cateId) < 0)
                    {
                        cateList.Add(cateId);
                    }
                }

                var categories = _db.Categories.Where(c => c.Status == 1 && cateList.Contains(c.CategoryId)).Include(c => c.Machines).ToList();
                var categoryIdReference = _db.Machines.Where(m => m.Status == 1 && m.ReferenceAuth == 1).Select(p => p.CategoryId).ToList();
                var machineIdReference = _db.Machines.Where(m => m.Status == 1 && m.ReferenceAuth == 1).Select(p => p.MachineId).ToList();
                ViewBag.categoryIdReference = categoryIdReference;
                ViewBag.machineIdReference = machineIdReference;
                ViewBag.Category = categories;
            }
            else
            {
                var categories = _db.Categories.Where(c => c.Status == 1).Include(c => c.Machines).ToList();
                ViewBag.Category = categories;
            }

            var informations = _db.MaintenanceRecords.Include(m => m.Machine).OrderByDescending(m => m.RecordDate).Take(10).ToList();
            return View(informations);
        }
        /**
         * Ajax return Day have Reservation in month
        */
        public IActionResult GetDayHaveReservationInMonth(string day)
        {
            DateTime start = DateTime.Parse(day);
            var startTime = new DateTime(start.Year, start.Month, 1);
            var endTime = startTime.AddMonths(1).AddDays(-1);
            var reservations = _db.Reservations.Where(r => r.StartTime >= startTime && r.EndTime <= endTime)
                                                .Select(r => r.StartTime.ToString("yyyy-MM-dd")).ToList();
            var data = reservations.Distinct();

            return Json(data);
        }

        /**
         * Ajax return Reservation in Day when select day in Homepage
        */
        public IActionResult GetMachineHaveReservationInDay(string day)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            DateTime start = DateTime.Parse(day);
            var startTime = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
            var endTime = startTime.AddDays(1);

            var reservations = _db.Reservations.Where(r => r.StartTime >= startTime && r.EndTime <= endTime)
                .Join(
                    _db.Users,
                    x => x.UserId,
                    user => user.UserId,
                    (res, user) => new
                    {
                        userId = user.UserId,
                        groupId = user.GroupId,
                        userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                        machineId = res.MachineId,
                        //userName = user.UserNameJp,
                        reserveId = res.ReserveId,
                        startTime = res.StartTime,
                        endTime = res.EndTime,
                        color = res.Color,
                        price = res.Price
                    }
                ).Join(
                    _db.Machines,
                    rs => rs.machineId,
                    machine => machine.MachineId,
                    (rs, machine) => new
                    {
                        machineName = (_lang == "en-US") ? machine.MachineNameEn :machine.MachineNameJp,
                        //machineName = machine.MachineNameJp,
                        userId = rs.userId,
                        groupId = rs.groupId,
                        userName = rs.userName,
                        machineId = rs.machineId,
                        reserveId = rs.reserveId,
                        startTime = rs.startTime,
                        endTime = rs.endTime,
                        color = rs.color,
                        price = rs.price
                    }
                )
                .Join(
                    _db.Groups,
                    rs => rs.groupId,
                    group => group.GroupId,
                    (rs, group) => new
                    {
                        machineName = rs.machineName,
                        groupName = group.GroupNameEn,
                        userId = rs.userId,
                        userName = rs.userName,
                        machineId = rs.machineId,
                        reserveId = rs.reserveId,
                        startTime = rs.startTime,
                        endTime = rs.endTime,
                        color = rs.color,
                        price = rs.price
                    }
                );
            var asc_data = reservations.OrderBy(o => o.machineId).ThenBy(o => o.startTime).ToList();
            List<MachineReservation> ListAsc = new List<MachineReservation>();
            List<int> listId = new List<int>();
            foreach(var item in asc_data)
            {
                if (!listId.Contains(item.machineId))
                {
                    listId.Add(item.machineId);
                    MachineReservation mr = new MachineReservation();
                    mr.MachineId = item.machineId;
                    mr.MachineName = item.machineName;
                    mr.Events = null;
                    ListAsc.Add(mr);
                } 
            }
            foreach (var mr in ListAsc)
            {
                List<Event> le = new List<Event>();
                foreach (var item in asc_data)
                {
                    if (mr.MachineId == item.machineId)
                    {
                        Event e = new Event();
                        e.MachineId = item.machineId;
                        e.MachineName = item.machineName;
                        e.ReserveId = item.reserveId;
                        e.UserId = item.userId;
                        e.UserName = item.userName;
                        e.GroupName = item.groupName;
                        e.StartTime = item.startTime;
                        e.EndTime = item.endTime;
                        e.Color = item.color;
                        e.Price = item.price;
                        le.Add(e);
                    }
                }
                mr.Events = le;
            }

            var desc_data = reservations.OrderByDescending(o => o.machineId).ThenBy(o => o.startTime).ToList();
            List<MachineReservation> ListDesc = new List<MachineReservation>();
            List<int> listIddesc = new List<int>();
            foreach (var item in desc_data)
            {
                if (!listIddesc.Contains(item.machineId))
                {
                    listIddesc.Add(item.machineId);
                    MachineReservation mr = new MachineReservation();
                    mr.MachineId = item.machineId;
                    mr.MachineName = item.machineName;
                    mr.Events = null;
                    ListDesc.Add(mr);
                }
            }

            foreach (var mr in ListDesc)
            {
                List<Event> le = new List<Event>();
                foreach (var item in asc_data)
                {
                    if (mr.MachineId == item.machineId)
                    {
                        Event e = new Event();
                        e.MachineId = item.machineId;
                        e.MachineName = item.machineName;
                        e.ReserveId = item.reserveId;
                        e.UserId = item.userId;
                        e.UserName = item.userName;
                        e.GroupName = item.groupName;
                        e.StartTime = item.startTime;
                        e.EndTime = item.endTime;
                        e.Color = item.color;
                        e.Price = item.price;
                        le.Add(e);
                    }
                }
                mr.Events = le;
            }

            ViewBag.AscData = ListAsc;
            ViewBag.DescData = ListDesc;
            return PartialView("_Home_Machine_Reservation");
        }
        /**
         * Get list revervation in a month to show on calendar
         * 
         **/
        public IActionResult GetListMachineReservationByMonth(string day, string machineId, bool isDelEvent)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            DateTime start = DateTime.Parse(day);
            var startTime = new DateTime(start.Year, start.Month, 1, 0, 0, 0);
            var endTime = new DateTime();
            if (isDelEvent)
            {
                endTime = startTime.AddMonths(2);
            }
            else
            {
                endTime = startTime.AddMonths(1);
            }
            
            var arrMachineId= machineId.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            var reservations = _db.Reservations.Where(r => r.StartTime >= startTime && r.EndTime < endTime && arrMachineId.Contains(r.MachineId))
                .Join(
                    _db.Users,
                    x => x.UserId,
                    user => user.UserId,
                    (res, user) => new
                    {
                        userId = user.UserId,
                        groupId = user.GroupId,
                        userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                        machineId = res.MachineId,
                        //userName = user.UserNameJp,
                        reserveId = res.ReserveId,
                        startTime = res.StartTime,
                        endTime = res.EndTime,
                        color = res.Color,
                        price = res.Price
                    }
                ).Join(
                    _db.Machines,
                    rs => rs.machineId,
                    machine => machine.MachineId,
                    (rs, machine) => new
                    {
                        machineName = (_lang == "en-US") ? machine.MachineNameEn : machine.MachineNameJp,
                        //machineName = machine.MachineNameJp,
                        userId = rs.userId,
                        groupId = rs.groupId,
                        userName = rs.userName,
                        machineId = rs.machineId,
                        reserveId = rs.reserveId,
                        startTime = rs.startTime,
                        endTime = rs.endTime,
                        color = rs.color,
                        price = rs.price
                    }
                )
                .Join(
                    _db.Groups,
                    rs => rs.groupId,
                    group => group.GroupId,
                    (rs, group) => new
                    {
                        machineName = rs.machineName,
                        groupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                        //groupName = group.GroupNameJp,
                        userId = rs.userId,
                        userName = rs.userName,
                        machineId = rs.machineId,
                        reserveId = rs.reserveId,
                        startTime = rs.startTime,
                        endTime = rs.endTime,
                        color = rs.color,
                        price = rs.price
                    }
                ).ToList();

            return Json(reservations);
        }
        /**
         * Function change language
         * **/
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) }
            );

            return LocalRedirect(returnUrl);
        }

    }
}
