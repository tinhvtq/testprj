using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

namespace ReservationSystem.Controllers
{
    public class ReservationController : Controller
    {
        private readonly DbConnectionClass _db;

        public ReservationController (DbConnectionClass db)
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
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();
            ViewBag.CanUseMachine = machines;

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return View(categories);
        }
        public IActionResult TimeSelect(int id)
        {
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }
            Machine machine = _db.Machines.Include(m=> m.CustomFields).Where(m => m.MachineId == id).FirstOrDefault();

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return View(machine);
        }

        public IActionResult GetReservationByMachineID(int id, string start_time)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            DateTime start = DateTime.Parse(start_time);
            var startTime = new DateTime(start.Year, start.Month, 1);
            var endTime = startTime.AddMonths(1);

            var reservations = _db.Reservations.Where(r => r.MachineId == id && r.StartTime >= startTime && r.EndTime < endTime)
                .Join(
                    _db.Users,
                    res => res.UserId,
                    user => user.UserId,
                    (res, user) => new
                    {
                        userId = user.UserId,
                        groupId = user.GroupId,
                        userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                        machineId = res.MachineId,
                        reserveId = res.ReserveId,
                        startTime = res.StartTime,
                        endTime = res.EndTime,
                        color = res.Color
                    }
                ).Join(
                    _db.Machines,
                    rs => rs.machineId,
                    machine => machine.MachineId,
                    (rs, machine) => new
                    {
                        machineName = (_lang == "en-US") ? machine.MachineNameEn : machine.MachineNameJp,
                        userId = rs.userId,
                        groupId = rs.groupId,
                        userName = rs.userName,
                        machineId = rs.machineId,
                        reserveId = rs.reserveId,
                        startTime = rs.startTime,
                        endTime = rs.endTime,
                        color = rs.color
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
                        userId = rs.userId,
                        userName = rs.userName,
                        machineId = rs.machineId,
                        reserveId = rs.reserveId,
                        startTime = rs.startTime,
                        endTime = rs.endTime,
                        color = rs.color
                    }
                )
                .ToList();
            return Json(reservations);

            
        }

        public IActionResult GetReservationDetail(int id)
        {
            Reservation reservation = _db.Reservations.Include(r => r.CustomFieldValues).Where(r => r.ReserveId == id).FirstOrDefault();
            Machine machine = _db.Machines.Include(m => m.CustomFields).Where(m => m.MachineId == reservation.MachineId).FirstOrDefault();
            User user = _db.Users.Include(m => m.Group).Where(m => m.UserId == reservation.UserId).FirstOrDefault();

            ViewBag.Reservation = reservation;
            ViewBag.User = user;

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();
            return PartialView("_ReservationUpdateFrom", machine);
        }

        public IActionResult LoadViewReservationDetail(int id)
        {
            Reservation reservation = _db.Reservations.Include(r => r.CustomFieldValues).Where(r => r.ReserveId == id).FirstOrDefault();
            Machine machine = _db.Machines.Include(m => m.CustomFields).Where(m => m.MachineId == reservation.MachineId).FirstOrDefault();
            User user = _db.Users.Include(m => m.Group).Where(m => m.UserId == reservation.UserId).FirstOrDefault();

            ViewBag.Reservation = reservation;
            ViewBag.User = user;

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            ViewBag.Lang = culture.ToString();

            return PartialView("_ReservationDetail", machine);
        }

        public IActionResult DeletedReservation(int id)
        {
            Reservation data = _db.Reservations.Include(r => r.CustomFieldValues).Where(r => r.ReserveId == id).FirstOrDefault();
            if (data == null)
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
                data.CustomFieldValues.Clear();
                //remove group
                _db.Reservations.Remove(data);
                try
                {
                    _db.SaveChanges();
                }
                catch (Exception)
                {

                    throw;
                }
                var result = new
                {
                    status = 200,
                    message = "Delete Reservation Success"
                };
                return Json(result);
            }
        }
        public IActionResult UpdateReservationTime(int id, string start, string end)
        {
            Reservation res = _db.Reservations.Find(id);
            if (res == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Update Fail. Reservation not found."
                };
                return Json(result);
            }
            else
            {
                res.StartTime = DateTime.Parse(start);
                res.EndTime = DateTime.Parse(end);
                _db.Reservations.Update(res);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = "Update Time success"
                };
                return Json(result);
            }
        }

        public IActionResult CreateReservation(int machinedId, string note, int color, string str_item_id, string str_item_value, int price, string reservation_date, string start_time, string end_time)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            var user_id = HttpContext.Session.GetInt32("UserId");
            string[] arr_item_val = null;
            //if (str_item_value != "0")
            if (!string.IsNullOrEmpty(str_item_value) && str_item_value != "0")
            {
                arr_item_val = str_item_value.Split(',').ToArray();
            }
            int[] arr_item_id = null;
            //if (str_item_id != "0")
            if (!string.IsNullOrEmpty(str_item_id) && str_item_id != "0")
            {
                arr_item_id = str_item_id.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }
            var str_start = reservation_date + ' ' + start_time + ":00";
            DateTime event_start = DateTime.Parse(str_start);
            var str_end = reservation_date + ' ' + end_time + ":00";
            //DateTime event_end = DateTime.Parse(str_end);
            DateTime event_end = new DateTime();
            //add code 24h
            //if (end_time == "00:0")
            //{
            //    event_end = DateTime.Parse(str_end).AddDays(1);
            //}
            //else
            //{
            //    event_end = DateTime.Parse(str_end);
            //}
            event_end = end_time switch
            {
                "00:0" => DateTime.Parse(str_end).AddDays(1),
                "00:15" => DateTime.Parse(reservation_date + ' ' + "00:00" + ":00").AddDays(1),
                "00:30" => DateTime.Parse(reservation_date + ' ' + "00:00" + ":00").AddDays(1),
                "00:45" => DateTime.Parse(reservation_date + ' ' + "00:00" + ":00").AddDays(1),
                _ => DateTime.Parse(str_end),
            };
            var count = _db.Reservations.Where(r => r.MachineId == machinedId && ((r.StartTime >= event_start && r.StartTime <= event_end) || (r.EndTime >= event_start && r.EndTime <= event_end))).ToList().Count;
            if (count == 0)
            {

                Reservation res = new Reservation();
                res.MachineId = machinedId;
                res.UserId = Convert.ToInt32(user_id);
                res.Color = color;
                res.Price = price;
                res.Note = note;
                res.StartTime = event_start;
                res.EndTime = event_end;
                _db.Add(res);
                _db.SaveChanges();

                Reservation reservation = _db.Reservations.Include(r => r.CustomFieldValues).Include(r => r.Machine).Where(r => r.ReserveId == res.ReserveId).FirstOrDefault();
                if (arr_item_id != null)
                {
                    for (int i = 0; i < arr_item_id.Length; i++)
                    {
                        if (arr_item_val != null)
                        {
                            if (arr_item_val[i] != "")
                            {
                                var item = new CustomFieldValue();
                                item.FieldId = arr_item_id[i];
                                item.ReserveId = res.ReserveId;
                                item.FieldValue = arr_item_val[i];
                                reservation.CustomFieldValues.Add(item);
                            }
                        }
                    }
                    _db.SaveChanges();
                }
                User user = _db.Users.Include(u => u.Group).Where(u => u.UserId == Convert.ToInt32(user_id)).FirstOrDefault();
                Event ev = new Event();
                ev.ReserveId = reservation.ReserveId;
                ev.UserId = reservation.UserId;
                ev.UserName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp;
                ev.GroupName = (_lang == "en-US") ? user.Group.GroupNameEn : user.Group.GroupNameJp;
                ev.MachineId = reservation.MachineId;
                ev.MachineName = (_lang == "en-US") ? reservation.Machine.MachineNameEn : reservation.Machine.MachineNameJp;
                ev.Color = reservation.Color;
                ev.Price = reservation.Price;
                ev.StartTime = reservation.StartTime;
                ev.EndTime = reservation.EndTime;
                var result = new
                {
                    status = 200,
                    message = "Create success.",
                    data = ev
                };
                return Json(result);
            }
            else
            {
                var result = new
                {
                    status = 400,
                    message = (_lang =="en-US") ? "The Reservation Overlaps." : "予約が重複しています。"
                };
                return Json(result);
            }

        }

        public IActionResult UpdateReservation(int reserveId, int machinedId, string note, int color, string str_item_id, string str_item_value, int price, string reservation_date, string start_time, string end_time)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var user_id = HttpContext.Session.GetInt32("UserId");
            string[] arr_item_val = null;
            //if (str_item_value != "0")
            if (!string.IsNullOrEmpty(str_item_value) && str_item_value != "0")
            {
                arr_item_val = str_item_value.Split(',').ToArray();
            }
            int[] arr_item_id = null;
            //if (str_item_id != "0")
            if (!string.IsNullOrEmpty(str_item_id) && str_item_id != "0")
            {
                arr_item_id = str_item_id.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }
            
            var str_start = reservation_date + ' ' + start_time + ":00";
            DateTime event_start = DateTime.Parse(str_start);
            var str_end = reservation_date + ' ' + end_time + ":00";
            //DateTime event_end = DateTime.Parse(str_end);
            DateTime event_end = new DateTime();
            //add code 24h
            event_end = end_time switch
            {
                "00:0" => DateTime.Parse(str_end).AddDays(1),
                "00:15" => DateTime.Parse(reservation_date + ' ' + "00:00" + ":00").AddDays(1),
                "00:30" => DateTime.Parse(reservation_date + ' ' + "00:00" + ":00").AddDays(1),
                "00:45" => DateTime.Parse(reservation_date + ' ' + "00:00" + ":00").AddDays(1),
                _ => DateTime.Parse(str_end),
            };
            Reservation reservation = _db.Reservations.Include(r => r.CustomFieldValues).Include(r => r.Machine).Where(r => r.ReserveId == reserveId).FirstOrDefault();
            if (reservation == null)
            {
                var result = new
                {
                    status = 400,
                    message = "Update Fail. Reservation not found."
                };
                return Json(result);
            }
            else
            {
                var count = _db.Reservations.Where(r => r.ReserveId != reserveId &&  r.MachineId == machinedId
                                                    && (r.StartTime <= event_start && r.EndTime >= event_start
                                                        || r.StartTime <= event_end && r.EndTime >= event_end)).ToList().Count;
                if (count > 0)
                {
                    var result = new
                    {
                        status = 400,
                        message = (_lang == "en-US") ? "The Reservation Overlaps." : "予約が重複しています。"
                    };
                    return Json(result);
                }
                else
                {
                    reservation.MachineId = machinedId;
                    reservation.UserId = Convert.ToInt32(user_id);
                    reservation.Color = color;
                    reservation.Price = price;
                    reservation.Note = note;
                    reservation.StartTime = event_start;
                    reservation.EndTime = event_end;
                    _db.Reservations.Update(reservation);
                    _db.SaveChanges();
                  
                    reservation.CustomFieldValues.Clear();
                    if (arr_item_id != null)
                    {
                        for (int i = 0; i < arr_item_id.Length; i++)
                        {
                            if (arr_item_id != null)
                            {
                                if (arr_item_val[i] != "")
                                {
                                    var item = new CustomFieldValue();
                                    item.FieldId = arr_item_id[i];
                                    item.ReserveId = reservation.ReserveId;
                                    item.FieldValue = arr_item_val[i];
                                    reservation.CustomFieldValues.Add(item);
                                }
                            }
                        }
                        _db.SaveChanges();
                    }

                    User user = _db.Users.Include(u => u.Group).Where(u => u.UserId == Convert.ToInt32(user_id)).FirstOrDefault();
                    Event ev = new Event();
                    ev.ReserveId = reservation.ReserveId;
                    ev.UserId = reservation.UserId;
                    ev.UserName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp;
                    ev.GroupName = (_lang == "en-US") ? user.Group.GroupNameEn : user.Group.GroupNameJp;
                    ev.MachineId = reservation.MachineId;
                    ev.MachineName = (_lang == "en-US") ? reservation.Machine.MachineNameEn : reservation.Machine.MachineNameJp;
                    ev.Color = reservation.Color;
                    ev.Price = reservation.Price;
                    ev.StartTime = reservation.StartTime;
                    ev.EndTime = reservation.EndTime;

                    //return Json(ev);
                    var result = new
                    {
                        status = 200,
                        message = "Update success.",
                        data = ev
                    };
                    return Json(result);
                }

            }
        }
        
    }

}