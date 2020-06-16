using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;
using Microsoft.VisualBasic;
using Z.EntityFramework.Plus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace ReservationSystem.Controllers
{
    public class UseTimeTotalizationController : Controller
    {
        private readonly DbConnectionClass _db;
        public UseTimeTotalizationController(DbConnectionClass db)
        {
            _db = db;
        }
        public IActionResult Test()
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var user_id = 2809;
            var group_id = 82;
            var arrUserId = GetUserInGroup(group_id).ToArray();
            var canUseMachine = _db.CanUseMachines.Where(u => arrUserId.Contains(u.UserId)).Select(u => u.MachineId).Distinct().ToList();

            /*get list user with total usetime & price */
            List<UserUseTime> list_UserUseTime = new List<UserUseTime>();
            if (user_id == 1)
            {
                var reservations = _db.Reservations
                                    .Where(r => r.MachineId == 2)
                                    .Join(
                                        _db.Users.Where(u => u.Status == 1),
                                        res => res.UserId,
                                        user => user.UserId,
                                        (res, user) => new
                                        {
                                            userId = user.UserId,
                                            groupId = user.GroupId,
                                            userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                            reserveId = res.ReserveId,
                                            startTime = res.StartTime,
                                            endTime = res.EndTime,
                                            price = res.Price
                                        }
                                    )
                                    .Join(
                                        _db.Groups.Where(g => g.Status == 1),
                                        rs => rs.groupId,
                                        group => group.GroupId,
                                        (rs, group) => new
                                        {
                                            GroupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                            ParentGroupId = group.ParentGroupId,
                                            GroupId = group.GroupId,
                                            userId = rs.userId,
                                            userName = rs.userName,
                                            reserveId = rs.reserveId,
                                            startTime = rs.startTime,
                                            endTime = rs.endTime,
                                            price = rs.price
                                        }
                                    ).Select(x => new
                                    {
                                        ParentGroupId = x.ParentGroupId,
                                        GroupName = x.GroupName,
                                        GroupId = x.GroupId,
                                        UserId = x.userId,
                                        UserName = x.userName,
                                        ReserveId = x.reserveId,
                                        UseTime = x.endTime.Subtract(x.startTime).TotalHours,
                                        Price = x.price
                                    }
                                    ).OrderBy(p => p.UserId).ToList();
                //first: get list userId

                var listUser = reservations.Select(x => new { x.UserId, x.UserName, x.GroupId }).Distinct().ToList();
                foreach (var u in listUser)
                {
                    var sumTime = reservations.Where(x => x.UserId == u.UserId).Select(x => x.UseTime).Sum();
                    var sumPrice = reservations.Where(x => x.UserId == u.UserId).Select(x => x.Price).Sum();
                    UserUseTime user = new UserUseTime();
                    user.UserId = u.UserId;
                    user.UserName = u.UserName;
                    user.TotalUseTime = sumTime;
                    user.TotalPrice = sumPrice;
                    user.GroupId = u.GroupId;
                    list_UserUseTime.Add(user);
                }
            }
            else
            {
                var reservations = _db.Reservations
                                    .Where(r => r.MachineId == 2 && arrUserId.Contains(r.UserId))
                                    .Join(
                                        _db.Users.Where(u => u.Status == 1),
                                        res => res.UserId,
                                        user => user.UserId,
                                        (res, user) => new
                                        {
                                            userId = user.UserId,
                                            groupId = user.GroupId,
                                            userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                            reserveId = res.ReserveId,
                                            startTime = res.StartTime,
                                            endTime = res.EndTime,
                                            price = res.Price
                                        }
                                    )
                                    .Join(
                                        _db.Groups.Where(g => g.Status == 1),
                                        rs => rs.groupId,
                                        group => group.GroupId,
                                        (rs, group) => new
                                        {
                                            GroupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                            ParentGroupId = group.ParentGroupId,
                                            GroupId = group.GroupId,
                                            userId = rs.userId,
                                            userName = rs.userName,
                                            reserveId = rs.reserveId,
                                            startTime = rs.startTime,
                                            endTime = rs.endTime,
                                            price = rs.price
                                        }
                                    ).Select(x => new
                                    {
                                        ParentGroupId = x.ParentGroupId,
                                        GroupName = x.GroupName,
                                        GroupId = x.GroupId,
                                        UserId = x.userId,
                                        UserName = x.userName,
                                        ReserveId = x.reserveId,
                                        UseTime = x.endTime.Subtract(x.startTime).TotalHours,
                                        Price = x.price
                                    }
                                    ).OrderBy(p => p.UserId).ToList();
                //first: get list userId

                var listUser = reservations.Select(x => new { x.UserId, x.UserName, x.GroupId }).Distinct().ToList();
                foreach (var u in listUser)
                {
                    var sumTime = reservations.Where(x => x.UserId == u.UserId).Select(x => x.UseTime).Sum();
                    var sumPrice = reservations.Where(x => x.UserId == u.UserId).Select(x => x.Price).Sum();
                    UserUseTime user = new UserUseTime();
                    user.UserId = u.UserId;
                    user.UserName = u.UserName;
                    user.TotalUseTime = sumTime;
                    user.TotalPrice = sumPrice;
                    user.GroupId = u.GroupId;
                    list_UserUseTime.Add(user);
                }
            }


            //second: add user to group
            List<GroupUseTime> list_GroupUseTime = new List<GroupUseTime>();
            List<int> GroupIds = new List<int>();
            foreach (var item in list_UserUseTime.OrderBy(x => x.GroupId))
            {
                var gid = item.GroupId;
                if (GroupIds.IndexOf(gid) < 0)
                {
                    GroupIds.Add(gid);

                    List<UserUseTime> list_time = new List<UserUseTime>();
                    list_time.Add(item);

                    var g = _db.Groups.Find(gid);
                    GroupUseTime gu = new GroupUseTime();
                    gu.GroupId = g.GroupId;
                    gu.ParentGroupId = g.ParentGroupId;
                    gu.GroupName = (_lang == "en-US") ? g.GroupNameEn : g.GroupNameJp;
                    gu.UserUseTimes = list_time;
                    gu.TotalUseTime = item.TotalUseTime;
                    gu.TotalPrice = item.TotalPrice;
                    list_GroupUseTime.Add(gu);
                }
                else
                {
                    var instackGroup = list_GroupUseTime.Where(g => g.GroupId == gid).FirstOrDefault();

                    var userUseTimes = instackGroup.UserUseTimes;
                    userUseTimes.Add(item);
                    instackGroup.UserUseTimes = userUseTimes;
                    instackGroup.TotalUseTime += item.TotalUseTime;
                    instackGroup.TotalPrice += item.TotalPrice;
                }
            }

            //third process group in list to add parent if group is child

            var new_list_GroupUseTime = formatGroupUseTime(list_GroupUseTime, _lang, user_id, group_id);
            var list = joinGroupUseTime(new_list_GroupUseTime, user_id);
            var final_list = checkChildGroupUseTime(list, _lang, user_id, group_id);

            /*var flag = false;
            foreach (var item in list)
            {
                if (item.ParentGroupId != null)
                {
                    flag = true;
                }
            }*/

            return Json(final_list);
        }
        //check if group is child of a parent group, then format group again
        public List<GroupUseTime> checkChildGroupUseTime(List<GroupUseTime> list_GroupUseTime, string _lang, int user_id, int group_id)
        {
            var flag = false;
            if (user_id == 1)
            {
                foreach (var item in list_GroupUseTime)
                {
                    if (item.ParentGroupId != null)
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                foreach (var item in list_GroupUseTime)
                {
                    if (item.GroupId != group_id)
                    {
                        flag = true;
                    }
                }
            }

            if (flag)
            {
                var newlist = formatGroupUseTime(list_GroupUseTime, _lang, user_id, group_id);
                var new_list = joinGroupUseTime(newlist, user_id);

                return checkChildGroupUseTime(new_list, _lang, user_id, group_id);
            }
            else
            {
                return list_GroupUseTime;
            }
        }
        //add usetime item to a user group
        public List<GroupUseTime> joinGroupUseTime(List<GroupUseTime> list_GroupUseTime, int user_id)
        {
            List<int> nGroupIds = new List<int>();
            List<GroupUseTime> new_list_GroupUseTime = new List<GroupUseTime>();

            foreach (var item in list_GroupUseTime)
            {
                if (nGroupIds.IndexOf(item.GroupId) < 0)
                {
                    nGroupIds.Add(item.GroupId);
                    new_list_GroupUseTime.Add(item);
                }
                else
                {
                    var current = new_list_GroupUseTime.Where(x => x.GroupId == item.GroupId).FirstOrDefault();
                    List<GroupUseTime> childGroup = new List<GroupUseTime>();
                    if (current.ChildGroups != null)
                    {
                        childGroup.AddRange(current.ChildGroups);
                    }
                    var curr_child = item.ChildGroups;
                    childGroup.AddRange(curr_child);

                    current.TotalUseTime += item.TotalUseTime;
                    current.TotalPrice += item.TotalPrice;
                    current.ChildGroups = childGroup;
                }
            }


            return new_list_GroupUseTime;
        }
        // join duplicate group
        public List<GroupUseTime> formatGroupUseTime(List<GroupUseTime> list_GroupUseTime, string _lang, int user_id, int group_id)
        {
            List<GroupUseTime> new_list_GroupUseTime = new List<GroupUseTime>();
            foreach (var grItem in list_GroupUseTime)
            {
                if (grItem.ParentGroupId == null)
                {
                    new_list_GroupUseTime.Add(grItem);
                }
                else
                {
                    if (user_id == 1 || group_id != grItem.GroupId)
                    {
                        var group = _db.Groups.Where(x => x.GroupId == grItem.ParentGroupId).FirstOrDefault();

                        List<GroupUseTime> childGroup = new List<GroupUseTime>();
                        childGroup.Add(grItem);

                        GroupUseTime current_item = new GroupUseTime();
                        current_item.GroupId = group.GroupId;
                        current_item.ParentGroupId = group.ParentGroupId;
                        current_item.GroupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp;
                        current_item.TotalUseTime = grItem.TotalUseTime;
                        current_item.TotalPrice = grItem.TotalPrice;
                        current_item.ChildGroups = childGroup;

                        new_list_GroupUseTime.Add(current_item);
                    }
                    else
                    {
                        new_list_GroupUseTime.Add(grItem);
                    }
                }
            }
            return new_list_GroupUseTime;
        }
        public IActionResult Index()
        {
            // check login
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }

            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            ViewBag.Lang = _lang;

            return View();
        }

        public List<int> GetUserInGroup(int group_id)
        {
            List<int> GroupIdList = new List<int>();
            var users = _db.Users.Where(u => u.GroupId == group_id && u.Status == 1).ToList();
            foreach (var u in users)
            {
                if (GroupIdList.IndexOf(u.UserId) < 0)
                {
                    GroupIdList.Add(u.UserId);
                }
            }
            var child_groups = _db.Groups.Where(g => g.ParentGroupId == group_id && g.Status == 1).ToList();
            if (child_groups.Count > 0)
            {
                foreach (var g in child_groups)
                {
                    var user_in_group = GetUserInGroup(g.GroupId);
                    GroupIdList.AddRange(user_in_group);
                }
            }
            return GroupIdList;
        }

        /*
        * Returns list data machine with total useTime
        * Params string firstDay, string lastDay
        * author Hieu
        *
        */
        public IActionResult LoadAccordingToMachine(string firstDay, string lastDay)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            var user_id = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

            var start_time = Convert.ToDateTime("1/1/1970");
            var end_time = DateTime.Now;
            if (!string.IsNullOrEmpty(firstDay))
            {
                start_time = Convert.ToDateTime(firstDay);
            }
            if (!string.IsNullOrEmpty(lastDay))
            {
                end_time = Convert.ToDateTime(lastDay);
            }

            var loginGroup = Convert.ToInt32(HttpContext.Session.GetInt32("GroupId"));
            var arrUserId = GetUserInGroup(loginGroup).ToArray();
            var canUseMachine = _db.CanUseMachines.Where(u => arrUserId.Contains(u.UserId)).Distinct().Select(u => u.MachineId).ToList();

            List<MachineUseTime> listItems = new List<MachineUseTime>();
            var machines = _db.Machines.Where(m => m.Status == 1).ToList();
            foreach (var m in machines)
            {
                if (canUseMachine.IndexOf(m.MachineId) > -1 || user_id == 1)
                {
                    //var lTime = _db.Reservations.Where(r => r.MachineId == m.MachineId).Select(r => r.EndTime.Subtract(r.StartTime).TotalHours).ToList();
                    var sumTime = GetMachineTotalUseTime(start_time, end_time, m.MachineId, user_id, arrUserId);
                    var sumPrice = GetMachineTotalPrice(start_time, end_time, m.MachineId, user_id, arrUserId);
                    if (sumTime > 0)
                    {
                        var d = new MachineUseTime();
                        d.MachineId = m.MachineId;
                        d.MachineName = (_lang == "en-US") ? m.MachineNameEn : m.MachineNameJp;
                        d.UseTime = sumTime;
                        d.UsePrice = sumPrice;
                        listItems.Add(d);
                    }
                }
                else if (m.ReferenceAuth == 1)
                {
                    var sumTime = GetMachineTotalUseTime(start_time, end_time, m.MachineId, user_id, arrUserId);
                    var sumPrice = GetMachineTotalPrice(start_time, end_time, m.MachineId, user_id, arrUserId);
                    if (sumTime > 0)
                    {
                        var d = new MachineUseTime();
                        d.MachineId = m.MachineId;
                        d.MachineName = (_lang == "en-US") ? m.MachineNameEn : m.MachineNameJp;
                        d.UseTime = sumTime;
                        d.UsePrice = sumPrice;
                        listItems.Add(d);
                    }
                }
            }
            ViewBag.ListMachine_ASC = listItems.OrderBy(p => p.MachineName);
            // Sort data to desc
            ViewBag.ListMachine_DESC = listItems.OrderByDescending(p => p.MachineName);
            ViewBag.CanUseMachine = canUseMachine;
            return PartialView("_LoadAccordingToMachine");

        }
        /*
        * Returns total useTime of a machine
        * Params string firstDay, string lastDay, int machineId
        * author Hieu
        */
        public double GetMachineTotalUseTime(DateTime start_time, DateTime end_time, int machineId, int user_id, int[] arrUserId)
        {
            double total_useTime = 0;
            if (user_id == 1)
            {
                var totalTime = _db.Reservations.Where(r => r.MachineId == machineId && r.StartTime.Date >= start_time && r.EndTime.Date <= end_time)
                .Join(
                    _db.Users.Where(u => u.Status == 1),
                    res => res.UserId,
                    user => user.UserId,
                    (res, user) => new
                    {
                        GroupId = user.GroupId,
                        StartTime = res.StartTime,
                        EndTime = res.EndTime
                    }
                )
                .Join(
                    _db.Groups.Where(g => g.Status == 1),
                    res => res.GroupId,
                    gr => gr.GroupId,
                    (res, gr) => new
                    {
                        StartTime = res.StartTime,
                        EndTime = res.EndTime
                    }
                )
                .Select(r => r.EndTime.Subtract(r.StartTime).TotalHours).ToList();
                total_useTime = totalTime.Sum();
            }
            else
            {
                var totalTime = _db.Reservations.Where(r => r.MachineId == machineId && arrUserId.Contains(r.UserId) && r.StartTime.Date >= start_time && r.EndTime.Date <= end_time)
                .Join(
                    _db.Users.Where(u => u.Status == 1),
                    res => res.UserId,
                    user => user.UserId,
                    (res, user) => new
                    {
                        GroupId = user.GroupId,
                        StartTime = res.StartTime,
                        EndTime = res.EndTime
                    }
                )
                .Join(
                    _db.Groups.Where(g => g.Status == 1),
                    res => res.GroupId,
                    gr => gr.GroupId,
                    (res, gr) => new
                    {
                        StartTime = res.StartTime,
                        EndTime = res.EndTime
                    }
                )
                .Select(r => r.EndTime.Subtract(r.StartTime).TotalHours).ToList();
                total_useTime = totalTime.Sum();
            }

            return total_useTime;
        }
        /*
        * Returns total Price of a machine
        * Params string firstDay, string lastDay, int machineId
        * author Hieu
        */
        public int GetMachineTotalPrice(DateTime start_time, DateTime end_time, int machineId, int user_id, int[] arrUserId)
        {
            int total_price = 0;
            if (user_id == 1)
            {
                total_price = _db.Reservations.Where(r => r.MachineId == machineId && r.StartTime.Date >= start_time && r.EndTime.Date <= end_time)
                        .Join(
                            _db.Users.Where(u => u.Status == 1),
                            res => res.UserId,
                            user => user.UserId,
                            (res, user) => new
                            {
                                GroupId = user.GroupId,
                                Price = res.Price
                            }
                        )
                        .Join(
                            _db.Groups.Where(g => g.Status == 1),
                            res => res.GroupId,
                            gr => gr.GroupId,
                            (res, gr) => new
                            {
                                Price = res.Price
                            }
                        )
                        .Select(r => r.Price).Sum();
            }
            else
            {
                total_price = _db.Reservations.Where(r => r.MachineId == machineId && arrUserId.Contains(r.UserId) && r.StartTime.Date >= start_time && r.EndTime.Date <= end_time)
                        .Join(
                            _db.Users.Where(u => u.Status == 1),
                            res => res.UserId,
                            user => user.UserId,
                            (res, user) => new
                            {
                                GroupId = user.GroupId,
                                Price = res.Price
                            }
                        )
                        .Join(
                            _db.Groups.Where(g => g.Status == 1),
                            res => res.GroupId,
                            gr => gr.GroupId,
                            (res, gr) => new
                            {
                                Price = res.Price
                            }
                        )
                        .Select(r => r.Price).Sum();
            }
            return total_price;
        }
        /*
        * Returns group User have reservation this machine
        * Params string firstDay, string lastDay, int machineId
        * author Hieu
        */
        public IActionResult GetGroupOfMachine(int idMachine, string firstDay, string lastDay)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var user_id = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            var group_id = Convert.ToInt32(HttpContext.Session.GetInt32("GroupId"));

            var arrUserId = GetUserInGroup(group_id).ToArray();

            var start_time = Convert.ToDateTime("1/1/1970");
            var end_time = DateTime.Now;
            if (!string.IsNullOrEmpty(firstDay))
            {
                start_time = Convert.ToDateTime(firstDay);
            }
            if (!string.IsNullOrEmpty(lastDay))
            {
                end_time = Convert.ToDateTime(lastDay);
            }

            /*get list user with total usetime & price */
            List<UserUseTime> list_UserUseTime = new List<UserUseTime>();
            if (user_id == 1)
            {
                var reservations = _db.Reservations
                                    .Where(r => r.MachineId == idMachine && r.StartTime.Date >= start_time && r.EndTime.Date <= end_time)
                                    .Join(
                                        _db.Users.Where(u => u.Status == 1),
                                        res => res.UserId,
                                        user => user.UserId,
                                        (res, user) => new
                                        {
                                            userId = user.UserId,
                                            groupId = user.GroupId,
                                            userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                            reserveId = res.ReserveId,
                                            startTime = res.StartTime,
                                            endTime = res.EndTime,
                                            price = res.Price
                                        }
                                    )
                                    .Join(
                                        _db.Groups.Where(g => g.Status == 1),
                                        rs => rs.groupId,
                                        group => group.GroupId,
                                        (rs, group) => new
                                        {
                                            GroupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                            ParentGroupId = group.ParentGroupId,
                                            GroupId = group.GroupId,
                                            userId = rs.userId,
                                            userName = rs.userName,
                                            reserveId = rs.reserveId,
                                            startTime = rs.startTime,
                                            endTime = rs.endTime,
                                            price = rs.price
                                        }
                                    ).Select(x => new
                                    {
                                        ParentGroupId = x.ParentGroupId,
                                        GroupName = x.GroupName,
                                        GroupId = x.GroupId,
                                        UserId = x.userId,
                                        UserName = x.userName,
                                        ReserveId = x.reserveId,
                                        UseTime = x.endTime.Subtract(x.startTime).TotalHours,
                                        Price = x.price
                                    }
                                    ).OrderBy(p => p.UserId).ToList();
                //first: get list userId

                var listUser = reservations.Select(x => new { x.UserId, x.UserName, x.GroupId }).Distinct().ToList();
                foreach (var u in listUser)
                {
                    var sumTime = reservations.Where(x => x.UserId == u.UserId).Select(x => x.UseTime).Sum();
                    var sumPrice = reservations.Where(x => x.UserId == u.UserId).Select(x => x.Price).Sum();
                    UserUseTime user = new UserUseTime();
                    user.UserId = u.UserId;
                    user.UserName = u.UserName;
                    user.TotalUseTime = sumTime;
                    user.TotalPrice = sumPrice;
                    user.GroupId = u.GroupId;
                    list_UserUseTime.Add(user);
                }
            }
            else
            {
                var reservations = _db.Reservations
                                    .Where(r => r.MachineId == idMachine && arrUserId.Contains(r.UserId) && r.StartTime.Date >= start_time && r.EndTime.Date <= end_time)
                                    .Join(
                                        _db.Users.Where(u => u.Status == 1),
                                        res => res.UserId,
                                        user => user.UserId,
                                        (res, user) => new
                                        {
                                            userId = user.UserId,
                                            groupId = user.GroupId,
                                            userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                            reserveId = res.ReserveId,
                                            startTime = res.StartTime,
                                            endTime = res.EndTime,
                                            price = res.Price
                                        }
                                    )
                                    .Join(
                                        _db.Groups.Where(g => g.Status == 1),
                                        rs => rs.groupId,
                                        group => group.GroupId,
                                        (rs, group) => new
                                        {
                                            GroupName = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                            ParentGroupId = group.ParentGroupId,
                                            GroupId = group.GroupId,
                                            userId = rs.userId,
                                            userName = rs.userName,
                                            reserveId = rs.reserveId,
                                            startTime = rs.startTime,
                                            endTime = rs.endTime,
                                            price = rs.price
                                        }
                                    ).Select(x => new
                                    {
                                        ParentGroupId = x.ParentGroupId,
                                        GroupName = x.GroupName,
                                        GroupId = x.GroupId,
                                        UserId = x.userId,
                                        UserName = x.userName,
                                        ReserveId = x.reserveId,
                                        UseTime = x.endTime.Subtract(x.startTime).TotalHours,
                                        Price = x.price
                                    }
                                    ).OrderBy(p => p.UserId).ToList();
                //first: get list userId

                var listUser = reservations.Select(x => new { x.UserId, x.UserName, x.GroupId }).Distinct().ToList();
                foreach (var u in listUser)
                {
                    var sumTime = reservations.Where(x => x.UserId == u.UserId).Select(x => x.UseTime).Sum();
                    var sumPrice = reservations.Where(x => x.UserId == u.UserId).Select(x => x.Price).Sum();
                    UserUseTime user = new UserUseTime();
                    user.UserId = u.UserId;
                    user.UserName = u.UserName;
                    user.TotalUseTime = sumTime;
                    user.TotalPrice = sumPrice;
                    user.GroupId = u.GroupId;
                    list_UserUseTime.Add(user);
                }
            }


            //second: add user to group
            List<GroupUseTime> list_GroupUseTime = new List<GroupUseTime>();
            List<int> GroupIds = new List<int>();
            foreach (var item in list_UserUseTime.OrderBy(x => x.GroupId))
            {
                var gid = item.GroupId;
                if (GroupIds.IndexOf(gid) < 0)
                {
                    GroupIds.Add(item.GroupId);

                    List<UserUseTime> list_time = new List<UserUseTime>();
                    list_time.Add(item);

                    var g = _db.Groups.Find(gid);
                    GroupUseTime gu = new GroupUseTime();
                    gu.GroupId = g.GroupId;
                    gu.ParentGroupId = g.ParentGroupId;
                    gu.GroupName = (_lang == "en-US") ? g.GroupNameEn : g.GroupNameJp;
                    gu.UserUseTimes = list_time;
                    gu.TotalUseTime = item.TotalUseTime;
                    gu.TotalPrice = item.TotalPrice;
                    list_GroupUseTime.Add(gu);
                }
                else
                {
                    var instackGroup = list_GroupUseTime.Where(g => g.GroupId == gid).FirstOrDefault();

                    var userUseTimes = instackGroup.UserUseTimes;
                    userUseTimes.Add(item);
                    instackGroup.UserUseTimes = userUseTimes;
                    instackGroup.TotalUseTime += item.TotalUseTime;
                    instackGroup.TotalPrice += item.TotalPrice;
                }
            }

            //third process group in list to add parent if group is child

            var new_list_GroupUseTime = formatGroupUseTime(list_GroupUseTime, _lang, user_id, group_id);
            var list = joinGroupUseTime(new_list_GroupUseTime, user_id);

            var final_list = checkChildGroupUseTime(list, _lang, user_id, group_id);

            ViewBag.GroupOfMachine = final_list;

            return PartialView("_GetGroupOfMachine");
        }

        /// <summary>
        /// Returns list data machine
        /// </summary>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        /// <returns></returns>
        /// 
        public IActionResult LoadAccordingToMachine2(string firstDay, string lastDay)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            var user_id = HttpContext.Session.GetInt32("UserId");

            var _machine_list = GetListMachineQuery(firstDay, lastDay);

            List<MachineList> machineList = new List<MachineList>();
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();
            ViewBag.CanUseMachine = machines;
            var info_userLogin = _db.Users.FirstOrDefault(u => u.Status == 1 && u.UserId == user_id);

            foreach (var item in _machine_list)
            {

                if (CheckGroupExist(item.MachineId, firstDay, lastDay) > 0)
                {
                    var list_userID = ListIdUser(item.MachineId);
                    double temp = 0;
                    for (int i = 0; i < list_userID.Count(); i++)
                    {
                        temp += item.Reservations.Where(rv => rv.UserId == list_userID[i]).Sum(rv => (rv.EndTime.Subtract(rv.StartTime)).TotalHours);
                    }

                    machineList.Add(new MachineList
                    {
                        MachineId = item.MachineId,
                        MachineNameEn = (_lang == "en-US") ? item.MachineNameEn : item.MachineNameJp,
                        //Total_Hours = item.Reservations.Sum(rv => (rv.EndTime.Subtract(rv.StartTime)).TotalHours),
                        Total_Hours = temp,
                        Total_Price = item.Reservations.Sum(rv => rv.Price)
                    });
                }
            }

            // Sort data to asc
            ViewBag.ListMachine_ASC = machineList.OrderBy(p => p.MachineNameEn);
            // Sort data to desc
            ViewBag.ListMachine_DESC = machineList.OrderByDescending(p => p.MachineNameEn);

            return PartialView("_AccordingToMachine", machineList);
        }

        /// <summary>
        /// Returns list data group of machine
        /// </summary>
        /// <param name="idMachine"></param>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        /// <returns></returns>
        public IActionResult GetGroupOfMachine2(int idMachine, string firstDay, string lastDay)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var query = GetReservationsQuery(idMachine, firstDay, lastDay);
            var user_id = HttpContext.Session.GetInt32("UserId");
            var info_userLogin = _db.Users.FirstOrDefault(u => u.Status == 1 && u.UserId == user_id);
            var reservations = query
                                    .Join(
                                        _db.Users.Where(u => u.Status == 1),
                                        res => res.UserId,
                                        user => user.UserId,
                                        (res, user) => new
                                        {
                                            userId = user.UserId,
                                            groupId = user.GroupId,
                                            macchineId = res.MachineId,
                                            userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                            reserveId = res.ReserveId,
                                            startTime = res.StartTime,
                                            endTime = res.EndTime,
                                            price = res.Price
                                        }
                                    )
                                    .Join(
                                        _db.Groups.Where(g => g.Status == 1),
                                        rs => rs.groupId,
                                        group => group.GroupId,
                                        (rs, group) => new
                                        {
                                            GroupNameEn = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                            ParentGroupId = group.ParentGroupId,
                                            GroupId = group.GroupId,
                                            MachineId = rs.macchineId,
                                            userId = rs.userId,
                                            userName = rs.userName,
                                            reserveId = rs.reserveId,
                                            startTime = rs.startTime,
                                            endTime = rs.endTime,
                                            price = rs.price
                                        }
                                    )
                                    .OrderBy(p => p.ParentGroupId)
                                    .ToList();

            List<GroupLevel> Levels = new List<GroupLevel>();

            foreach (var item in reservations)
            {
                if (item.ParentGroupId == null)
                {

                    List<GroupLevel> Levels_1 = new List<GroupLevel>();

                    foreach (var itemx in reservations)
                    {
                        if (item.GroupId == itemx.ParentGroupId)
                        {
                            List<GroupLevel> Levels_2 = new List<GroupLevel>();

                            foreach (var itemy in reservations)
                            {
                                if (itemx.GroupId == itemy.ParentGroupId)
                                {
                                    GroupLevel gr_2 = new GroupLevel
                                    {
                                        GroupId = itemy.GroupId,
                                        GroupNameEn = itemy.GroupNameEn,
                                    };

                                    if (Levels_2.Count() > 0)
                                    {
                                        if (Levels_2.FirstOrDefault(p => p.GroupId == gr_2.GroupId) == null)
                                        {
                                            var total_time = reservations.Where(p => (p.GroupId == itemx.GroupId) || (p.ParentGroupId == itemx.GroupId)).ToList().Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours);
                                            var total_price = reservations.Where(p => (p.GroupId == itemx.GroupId) || (p.ParentGroupId == itemx.GroupId)).ToList().Sum(p => p.price);
                                            gr_2.Total_Hours_Group = total_time;
                                            gr_2.Total_Price_Group = total_price;
                                            Levels_2.Add(gr_2);
                                        }
                                    }
                                    else
                                    {
                                        var total_time = reservations.Where(p => (p.GroupId == itemx.GroupId) || (p.ParentGroupId == itemx.GroupId)).ToList().Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours);
                                        var total_price = reservations.Where(p => (p.GroupId == itemx.GroupId) || (p.ParentGroupId == itemx.GroupId)).ToList().Sum(p => p.price);
                                        gr_2.Total_Hours_Group = total_time;
                                        gr_2.Total_Price_Group = total_price;
                                        Levels_2.Add(gr_2);
                                    }
                                }
                            }

                            GroupLevel gr_1 = new GroupLevel
                            {
                                GroupId = itemx.GroupId,
                                GroupNameEn = itemx.GroupNameEn,
                                GroupLevels = Levels_2,
                                Total_Hours_Group = Levels_2.Sum(p => p.Total_Hours_Group),
                                Total_Price_Group = Levels_2.Sum(p => p.Total_Price_Group)

                            };

                            if (Levels_1.Count() > 0)
                            {
                                if (Levels_1.FirstOrDefault(p => p.GroupId == gr_1.GroupId) == null)
                                {
                                    Levels_1.Add(gr_1);
                                }
                            }
                            else
                            {
                                Levels_1.Add(gr_1);
                            }
                        }
                    }

                    GroupLevel gr = new GroupLevel
                    {
                        GroupId = item.GroupId,
                        GroupNameEn = item.GroupNameEn,
                        GroupLevels = Levels_1,
                        Total_Hours_Group = Levels_1.Sum(p => p.Total_Hours_Group),
                        Total_Price_Group = Levels_1.Sum(p => p.Total_Price_Group)
                    };

                    if (Levels.Count() > 0)
                    {
                        if (Levels.FirstOrDefault(p => p.GroupId == gr.GroupId) == null)
                        {
                            //in the absence of subgroups
                            List<UserResult> list_user = new List<UserResult>();

                            if (gr.GroupLevels.Count == 0)
                            {
                                var temp_idUser = 0;

                                foreach (var rv in reservations.Where(p => p.GroupId == gr.GroupId))
                                {
                                    if (temp_idUser != rv.userId)
                                    {
                                        temp_idUser = rv.userId;

                                        var reservations_list = reservations.Where(p => p.userId == rv.userId).ToList();

                                        list_user.Add(new UserResult
                                        {
                                            UserNameEn = rv.userName,
                                            Time = reservations_list.Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours),
                                            Price = reservations_list.Sum(p => p.price)

                                        });
                                    }
                                }
                            }

                            gr.Group_User = list_user;
                            gr.Total_Hours_Group = list_user.Sum(p => p.Time);
                            gr.Total_Price_Group = list_user.Sum(p => p.Price);

                            Levels.Add(gr);
                        }
                    }
                    else
                    {
                        //in the absence of subgroups
                        List<UserResult> list_user = new List<UserResult>();

                        if (gr.GroupLevels.Count == 0)
                        {
                            var temp_idUser = 0;

                            foreach (var rv in reservations.Where(p => p.GroupId == gr.GroupId))
                            {
                                if (temp_idUser != rv.userId)
                                {
                                    temp_idUser = rv.userId;

                                    var reservations_list = reservations.Where(p => p.userId == rv.userId).ToList();

                                    list_user.Add(new UserResult
                                    {
                                        UserNameEn = rv.userName,
                                        Time = reservations_list.Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours),
                                        Price = reservations_list.Sum(p => p.price)

                                    });
                                }
                            }
                        }

                        gr.Group_User = list_user;
                        gr.Total_Hours_Group = list_user.Sum(p => p.Time);
                        gr.Total_Price_Group = list_user.Sum(p => p.Price);

                        Levels.Add(gr);
                    }
                }
                else
                {
                    // Find parent group
                    var find_group = _db.Groups.Where(g => g.Status == 1).FirstOrDefault(p => p.GroupId == item.ParentGroupId);

                    if (find_group != null) // have data
                    {
                        if (find_group.ParentGroupId == null)
                        {
                            List<GroupLevel> groupx = new List<GroupLevel>
                            {
                                new GroupLevel
                                {
                                    GroupId = item.GroupId,
                                    GroupNameEn = item.GroupNameEn,
                                    Total_Hours_Group = reservations.Where(p => p.ParentGroupId == item.ParentGroupId).ToList().Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours),
                                    Total_Price_Group = reservations.Where(p => p.ParentGroupId == item.ParentGroupId).ToList().Sum(p => p.price)
                                 }
                            };

                            GroupLevel gr = new GroupLevel
                            {
                                GroupId = find_group.GroupId,
                                GroupNameEn = find_group.GroupNameEn,
                                GroupLevels = groupx
                            };

                            if (Levels.Count() > 0)
                            {
                                if (Levels.FirstOrDefault(p => p.GroupId == gr.GroupId) == null)
                                {
                                    gr.Total_Hours_Group = gr.GroupLevels.Sum(p => p.Total_Hours_Group);
                                    gr.Total_Price_Group = gr.GroupLevels.Sum(p => p.Total_Price_Group);

                                    Levels.Add(gr);
                                }
                            }
                            else
                            {
                                gr.Total_Hours_Group = gr.GroupLevels.Sum(p => p.Total_Hours_Group);
                                gr.Total_Price_Group = gr.GroupLevels.Sum(p => p.Total_Price_Group);
                                Levels.Add(gr);
                            }
                        }
                        else
                        {
                            var find_group_parent = _db.Groups.Where(g => g.Status == 1).FirstOrDefault(p => p.GroupId == find_group.ParentGroupId);

                            if (find_group_parent.ParentGroupId == null)
                            {

                                List<GroupLevel> Level3 = new List<GroupLevel>
                                {
                                    new GroupLevel
                                    {
                                        GroupId = item.GroupId,
                                        GroupNameEn = item.GroupNameEn,
                                        Total_Hours_Group = reservations.Where(p => p.ParentGroupId == find_group.ParentGroupId).ToList().Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours),
                                        Total_Price_Group = reservations.Where(p => p.ParentGroupId == find_group.ParentGroupId).ToList().Sum(p => p.price)
                                    }
                                };

                                List<GroupLevel> Level2 = new List<GroupLevel>
                                        {
                                            new GroupLevel
                                            {
                                                GroupId = find_group.GroupId,
                                                GroupNameEn = find_group.GroupNameEn,
                                                GroupLevels = Level3,
                                                Total_Hours_Group = Level3.Sum(p=>p.Total_Hours_Group),
                                                Total_Price_Group = Level3.Sum(p=>p.Total_Price_Group)
                                            }
                                        };

                                GroupLevel gr = new GroupLevel
                                {
                                    GroupId = find_group_parent.GroupId,
                                    GroupNameEn = find_group_parent.GroupNameEn,
                                    GroupLevels = Level2,
                                };

                                if (Levels.Count() > 0)
                                {
                                    if (Levels.FirstOrDefault(p => p.GroupId == gr.GroupId) == null)
                                    {
                                        gr.Total_Hours_Group = gr.GroupLevels.Sum(p => p.Total_Hours_Group);
                                        gr.Total_Price_Group = gr.GroupLevels.Sum(p => p.Total_Price_Group);

                                        Levels.Add(gr);
                                    }
                                }
                                else
                                {
                                    Levels.Add(gr);
                                }
                            }
                        }
                    }
                }
            }

            ViewBag.GroupOfMachine = Levels;

            ViewBag.idGroups = getUserGroup(info_userLogin.GroupId);

            return PartialView("_LoadGroup", ViewBag.GroupOfMachine);
        }

        /// <summary>
        /// Returns list data group
        /// </summary>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        /// <returns></returns>
        public IActionResult LoadAccordingToGroup(string firstDay, string lastDay)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            List<GroupResult> groupResults = new List<GroupResult>();
            List<Group> list_group = new List<Group>();

            var user_id = HttpContext.Session.GetInt32("UserId");
            var user_type = HttpContext.Session.GetInt32("UserType");
            var builtin_user = HttpContext.Session.GetInt32("BuiltinUser");

            var use = _db.Users.Find(user_id);

            if (user_id != 1)
            {

                // find group parent
                Group gr = new Group();

                var findParentGroup = _db.Groups.Where(g => g.Status == 1 && g.GroupId == use.GroupId)
                                                .Include(gr => gr.Groups)
                                                .IncludeFilter(p => p.Users.Where(u => u.Status == 1))
                                                .FirstOrDefault();
                gr = findParentGroup;

                var result_group = GroupResult(gr, firstDay, lastDay, _lang);
                if (result_group.ChildGroups == null && result_group.GroupId == 0 && result_group.GroupNameEn == null && result_group.SumPrice == 0 && result_group.SumTime == 0 && result_group.Users == null)
                {
                    groupResults = null;
                }
                else
                {
                    if (result_group.SumTime > 0)
                    {
                        groupResults.Add(result_group);
                    }
                    else
                    {
                        groupResults = null;
                    }
                }
            }
            else
            {
                list_group = _db.Groups.Include(g => g.Groups).Where(g => g.Status == 1 && g.ParentGroupId == null).IncludeFilter(p => p.Users.Where(u => u.Status == 1)).ToList();

                foreach (Group item in list_group)
                {

                    groupResults.Add(GroupResult(item, firstDay, lastDay, _lang));
                }
            }

            ViewBag.ListAccGroup = groupResults != null ? groupResults.OrderBy(p => p.ParentGroupId) : null;
            ViewBag.ListAccGroup_ASC = groupResults != null ? groupResults.OrderBy(p => p.GroupNameEn) : null;
            ViewBag.ListAccGroup_DESC = groupResults != null ? groupResults.OrderByDescending(p => p.GroupNameEn) : null;

            return PartialView("_AccordingToGroup");
        }

        /// <summary>
        /// Returns all child group of that group
        /// </summary>
        /// <param name="groups">IList<Group> groups</param>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        /// <returns></returns>
        public ChildGroup GetChildGroup(IList<Group> groups, string firstDay, string lastDay)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            if (groups is null)
            {
                throw new ArgumentNullException(nameof(groups));
            }

            ChildGroup childGroup = new ChildGroup();
            IList<GroupResult> _childGroup = new List<GroupResult>();

            double sum_time = 0, sum_price = 0, total_time, total_price;

            foreach (var gr in groups)
            {
                GroupResult group = new GroupResult();
                total_time = 0;
                total_price = 0;
                if (gr.Users != null)
                {
                    foreach (var gr_us in gr.Users)
                    {
                        //var reservations = _db.Reservations.Where(p => p.UserId == gr_us.UserId);
                        var reservations = GetReservationsGroupQuery(gr_us.UserId, firstDay, lastDay).ToList();
                        double us_time = reservations.Sum(p => (p.EndTime.Subtract(p.StartTime)).TotalHours);
                        double us_price = reservations.Sum(p => p.Price);

                        total_time += us_time;
                        total_price += us_price;
                    }
                }
                else
                {
                    var findUserByGroupId = _db.Users.Where(u => u.GroupId == gr.GroupId && u.Status == 1).ToList();

                    foreach (var gr_us in findUserByGroupId)
                    {
                        var reservations = GetReservationsGroupQuery(gr_us.UserId, firstDay, lastDay).ToList();
                        double us_time = reservations.Sum(p => (p.EndTime.Subtract(p.StartTime)).TotalHours);
                        double us_price = reservations.Sum(p => p.Price);

                        total_time += us_time;
                        total_price += us_price;
                    }
                }

                group.GroupId = gr.GroupId;
                group.GroupNameEn = (_lang == "en-US") ? gr.GroupNameEn : gr.GroupNameJp;
                group.SumPrice = total_price;
                group.SumTime = total_time;
                _childGroup.Add(group);

                sum_price += total_price;
                sum_time += total_time;
            }

            childGroup.TotalTime = sum_time;
            childGroup.TotalPrice = sum_price;
            childGroup.Groups = _childGroup;

            return childGroup;
        }

        /// <summary>
        /// Returns all users of that group
        /// </summary>
        /// <param name="idGroup"></param>
        /// <returns></returns>
        public IActionResult GetUserInGroupChild(int idGroup)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            ViewBag.Lang = _lang;

            var user_child_group = _db.Groups.Where(p => p.GroupId == idGroup && p.Status == 1).IncludeFilter(p => p.Users.Where(u => u.Status == 1)).FirstOrDefault();
            List<UserResult> list_user = new List<UserResult>();
            if (user_child_group != null)
            {
                if (user_child_group.Users.Count != 0)
                {
                    foreach (var gr_us in user_child_group.Users)
                    {
                        var reservations = _db.Reservations.Where(p => p.UserId == gr_us.UserId);
                        double us_time = 0;
                        double us_price = 0;
                        foreach (var rv in reservations)
                        {
                            us_time += ((rv.EndTime - rv.StartTime).TotalHours);
                            us_price += rv.Price;
                        }

                        list_user.Add(new UserResult
                        {
                            UserNameEn = (_lang == "en-US") ? gr_us.UserNameEn : gr_us.UserNameJp,
                            Time = us_time,
                            Price = us_price

                        });
                    }
                }
            }

            ViewBag.ListUserInGroupChild = list_user;

            return PartialView("_LoadUserInChildGroup", ViewBag.ListUserInGroupChild);
        }

        /// <summary>
        /// Returns all users of that group and machine
        /// </summary>
        /// <param name="idMachine"></param>
        /// <param name="idGroup"></param>
        /// <returns></returns>
        public IActionResult GetUserInGroupMachine(int idMachine, int idGroup)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            List<UserResult> list_user = new List<UserResult>();

            var reservations = _db.Reservations.Where(r => r.MachineId == idMachine)
                             .Join(
                              _db.Users.Where(u => u.Status == 1),
                              res => res.UserId,
                              user => user.UserId,
                              (res, user) => new
                              {
                                  userId = user.UserId,
                                  groupId = user.GroupId,
                                  macchineId = res.MachineId,
                                  userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                  reserveId = res.ReserveId,
                                  startTime = res.StartTime,
                                  endTime = res.EndTime,
                                  price = res.Price
                              }
                             )
                             .Join(
                              _db.Groups.Where(p => p.GroupId == idGroup && p.Status == 1),
                              rs => rs.groupId,
                              group => group.GroupId,
                              (rs, group) => new
                              {
                                  GroupNameEn = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                  ParentGroupId = group.ParentGroupId,
                                  GroupId = group.GroupId,
                                  MachineId = rs.macchineId,
                                  userId = rs.userId,
                                  userName = rs.userName,
                                  reserveId = rs.reserveId,
                                  startTime = rs.startTime,
                                  endTime = rs.endTime,
                                  price = rs.price
                              }
                             )
                             .OrderBy(p => p.userId)
                             .ToList();

            var temp_idUser = 0;

            foreach (var rv in reservations)
            {
                if (temp_idUser != rv.userId)
                {
                    temp_idUser = rv.userId;

                    var reservations_list = reservations.Where(p => p.userId == rv.userId).ToList();

                    list_user.Add(new UserResult
                    {
                        UserNameEn = rv.userName,
                        Time = reservations_list.Sum(p => (p.endTime - p.startTime).TotalHours),
                        Price = reservations_list.Sum(p => p.price)

                    });
                }
            }

            ViewBag.ListUserInGroupChild = list_user;

            return PartialView("_LoadUserInChildGroup", ViewBag.ListUserInGroupChild);
        }

        /// <summary>
        /// Returns list machine
        /// </summary>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        /// <returns></returns>
        public List<Machine> GetListMachineQuery(string firstDay, string lastDay)
        {
            var query = _db.Machines.Where(p => p.Status == 1);
            /**
             * Where there exists a start date and not exists end date
             */
            if (!string.IsNullOrEmpty(firstDay) && string.IsNullOrEmpty(lastDay))
            {
                return query.IncludeFilter(p => p.Reservations.Where(p => p.StartTime.Date >= Convert.ToDateTime(firstDay))).ToList();
            }

            /**
             * Where there exists a start date and end date
             */
            if (!string.IsNullOrEmpty(firstDay) && !string.IsNullOrEmpty(lastDay))
            {
                return query.IncludeFilter(p => p.Reservations.Where(p => p.StartTime.Date >= Convert.ToDateTime(firstDay) && p.EndTime.Date <= Convert.ToDateTime(lastDay))).ToList();
            }

            /**
             * Returns if no condition
             */
            return query.Include(p => p.Reservations).ToList();
        }

        /// <summary>
        /// Returns IQueryable Reservations according Machine to conditions
        /// </summary>
        /// <param name="idMachine"></param>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        /// <returns></returns>
        public IQueryable<Reservation> GetReservationsQuery(int idMachine, string firstDay, string lastDay)
        {
            DbSet<Reservation> query = _db.Reservations;

            /**
             * Where there exists a start date and not exists end date
             */
            if (!string.IsNullOrEmpty(firstDay) && string.IsNullOrEmpty(lastDay))
            {
                return query.Where(p => p.MachineId == idMachine && p.StartTime.Date >= Convert.ToDateTime(firstDay));
            }

            /**
             * Where there exists a start date and end date
             */

            if (!string.IsNullOrEmpty(firstDay) && !string.IsNullOrEmpty(lastDay))
            {
                return query.Where(p => p.MachineId == idMachine && p.StartTime.Date >= Convert.ToDateTime(firstDay) && p.StartTime.Date <= Convert.ToDateTime(lastDay));
            }

            /**
             * Returns if no condition
             */
            return query.Where(r => r.MachineId == idMachine);
        }

        /// <summary>
        /// Returns IQueryable Reservations according Group to conditions
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        /// <returns></returns>
        public IQueryable<Reservation> GetReservationsGroupQuery(int idUser, string firstDay, string lastDay)
        {
            DbSet<Reservation> query = _db.Reservations;

            /**
             * Where there exists a start date and not exists end date
             */
            if (!string.IsNullOrEmpty(firstDay) && string.IsNullOrEmpty(lastDay))
            {
                return query.Where(p => p.UserId == idUser && p.StartTime.Date >= Convert.ToDateTime(firstDay));
            }

            /**
             * Where there exists a start date and end date
             */

            if (!string.IsNullOrEmpty(firstDay) && !string.IsNullOrEmpty(lastDay))
            {
                return query.Where(p => p.UserId == idUser && p.StartTime.Date >= Convert.ToDateTime(firstDay) && p.EndTime.Date <= Convert.ToDateTime(lastDay));
            }

            /**
             * Returns if no condition
             */
            return query.Where(p => p.UserId == idUser);
        }

        /// <summary>
        /// Returns file .csv
        /// </summary>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ExportUseTimeCSV(string firstDay, string lastDay, string tabActive)
        {
            // Set cocolumn Headers
            var columnHeaders = new string[] { };

            if (tabActive.Contains("acc_machine"))
            {
                columnHeaders = new string[]
                {
                       "機器名(日)",
                       "機器名(英)",
                       "グループ名（日）",
                       "グループ名（英）",
                       "ユーザー名（日）",
                       "ユーザー名（英）",
                       "使用時間合計",
                       "使用金額合計"
                };
            }
            else
            {
                columnHeaders = new string[]
                {
                       "グループ名（日）",
                       "グループ名（英）",
                       "ユーザー名（日）",
                       "ユーザー名（英）",
                       "使用時間合計",
                       "使用金額合計",
                };
            }

            // Set file name
            var filenameCSV = $"UseTime.csv";

            // Query retrieves data from database
            List<object[]> query = new List<object[]>();
            List<ParamaterExport> queryDataExport = new List<ParamaterExport>();

            if (tabActive.Contains("acc_machine"))
            {
                if (HttpContext.Session.GetInt32("UserId") != 1)
                {
                    query = List_Data_Obj(firstDay, lastDay);
                }
                else
                {
                    if (!string.IsNullOrEmpty(firstDay) && string.IsNullOrEmpty(lastDay))
                    {
                        queryDataExport = (from reser in _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(firstDay)).OrderBy(p => p.MachineId)
                                           join machine in _db.Machines on reser.MachineId equals machine.MachineId
                                           join user in _db.Users on reser.UserId equals user.UserId
                                           join gr in _db.Groups on user.GroupId equals gr.GroupId
                                           select new ParamaterExport
                                           {
                                               MachineNameJp = machine.MachineNameJp,
                                               MachineNameEn = machine.MachineNameEn,
                                               GroupNameJp = gr.GroupNameJp,
                                               GroupNameEn = gr.GroupNameEn,
                                               UserNameJp = user.UserNameJp,
                                               UserNameEn = user.UserNameEn,
                                               UserId = user.UserId,
                                               EndTime = reser.EndTime,
                                               StartTime = reser.StartTime,
                                               Price = reser.Price
                                           }).ToList();

                    }
                    else if (!string.IsNullOrEmpty(firstDay) && !string.IsNullOrEmpty(lastDay))
                    {
                        queryDataExport = (from reser in _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(firstDay) && a.EndTime.Date <= Convert.ToDateTime(lastDay)).OrderBy(a => a.MachineId)
                                           join machine in _db.Machines on reser.MachineId equals machine.MachineId
                                           join user in _db.Users on reser.UserId equals user.UserId
                                           join gr in _db.Groups on user.GroupId equals gr.GroupId
                                           select new ParamaterExport
                                           {
                                               MachineNameJp = machine.MachineNameJp,
                                               MachineNameEn = machine.MachineNameEn,
                                               GroupNameJp = gr.GroupNameJp,
                                               GroupNameEn = gr.GroupNameEn,
                                               UserNameJp = user.UserNameJp,
                                               UserNameEn = user.UserNameEn,
                                               UserId = user.UserId,
                                               EndTime = reser.EndTime,
                                               StartTime = reser.StartTime,
                                               Price = reser.Price
                                           }).ToList();
                    }
                    else
                    {
                        queryDataExport = (from reser in _db.Reservations.OrderBy(a => a.MachineId)
                                           join machine in _db.Machines on reser.MachineId equals machine.MachineId
                                           join user in _db.Users on reser.UserId equals user.UserId
                                           join gr in _db.Groups on user.GroupId equals gr.GroupId
                                           select new ParamaterExport
                                           {
                                               MachineNameJp = machine.MachineNameJp,
                                               MachineNameEn = machine.MachineNameEn,
                                               GroupNameJp = gr.GroupNameJp,
                                               GroupNameEn = gr.GroupNameEn,
                                               UserNameJp = user.UserNameJp,
                                               UserNameEn = user.UserNameEn,
                                               UserId = user.UserId,
                                               EndTime = reser.EndTime,
                                               StartTime = reser.StartTime,
                                               Price = reser.Price
                                           }).ToList();
                    }

                    // get array id user
                    var arrayUserId = queryDataExport.Select(p => p.UserId).Distinct().ToArray();
                    foreach (var item in arrayUserId)
                    {
                        var listGroup = queryDataExport.Where(p => p.UserId == item).ToArray();
                        var sumtime = queryDataExport.Where(p => p.UserId == item).Select(r => r.EndTime.Subtract(r.StartTime).TotalHours).ToList().Sum();
                        var sumprice = queryDataExport.Where(p => p.UserId == item).Select(r => r.Price).Sum();

                        query.Add(new object[]
                        {
                             !string.IsNullOrEmpty(listGroup[0].MachineNameJp) ? listGroup[0].MachineNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].MachineNameJp}\"" : $"{listGroup[0].MachineNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].MachineNameEn) ? listGroup[0].MachineNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].MachineNameEn}\"" : $"{listGroup[0].MachineNameEn}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].GroupNameJp) ? listGroup[0].GroupNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].GroupNameJp}\"" : $"{listGroup[0].GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].GroupNameEn) ? listGroup[0].GroupNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].GroupNameEn}\"" : $"{listGroup[0].GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].UserNameJp) ? listGroup[0].UserNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].UserNameJp}\"" : $"{listGroup[0].UserNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].UserNameEn) ? listGroup[0].UserNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].UserNameEn}\"" : $"{listGroup[0].UserNameEn}" : $"",
                             $"{sumtime}",
                             $"{sumprice}"
                        });

                    }

                }
            }
            else
            {
                if (HttpContext.Session.GetInt32("UserId") != 1)
                {
                    query = List_Data_Obj_Refer(firstDay, lastDay);
                }
                else
                {
                    if (!string.IsNullOrEmpty(firstDay) && string.IsNullOrEmpty(lastDay))
                    {
                        queryDataExport = (from reser in _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(firstDay)).OrderBy(p => p.MachineId)
                                           join user in _db.Users on reser.UserId equals user.UserId
                                           join gr in _db.Groups on user.GroupId equals gr.GroupId
                                           where gr.Status == 1 && user.Status == 1
                                           select new ParamaterExport
                                           {
                                               GroupNameJp = gr.GroupNameJp,
                                               GroupNameEn = gr.GroupNameEn,
                                               UserNameJp = user.UserNameJp,
                                               UserNameEn = user.UserNameEn,
                                               UserId = user.UserId,
                                               EndTime = reser.EndTime,
                                               StartTime = reser.StartTime,
                                               Price = reser.Price
                                           }).ToList();

                    }
                    else if (!string.IsNullOrEmpty(firstDay) && !string.IsNullOrEmpty(lastDay))
                    {
                        queryDataExport = (from reser in _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(firstDay) && a.EndTime.Date <= Convert.ToDateTime(lastDay)).OrderBy(a => a.MachineId)
                                           join user in _db.Users on reser.UserId equals user.UserId
                                           join gr in _db.Groups on user.GroupId equals gr.GroupId
                                           where gr.Status == 1 && user.Status == 1
                                           select new ParamaterExport
                                           {
                                               GroupNameJp = gr.GroupNameJp,
                                               GroupNameEn = gr.GroupNameEn,
                                               UserNameJp = user.UserNameJp,
                                               UserNameEn = user.UserNameEn,
                                               UserId = user.UserId,
                                               EndTime = reser.EndTime,
                                               StartTime = reser.StartTime,
                                               Price = reser.Price
                                           }).ToList();
                    }
                    else
                    {
                        queryDataExport = (from reser in _db.Reservations.OrderBy(a => a.MachineId)
                                           join user in _db.Users on reser.UserId equals user.UserId
                                           join gr in _db.Groups on user.GroupId equals gr.GroupId
                                           where gr.Status == 1 && user.Status == 1
                                           select new ParamaterExport
                                           {
                                               GroupNameJp = gr.GroupNameJp,
                                               GroupNameEn = gr.GroupNameEn,
                                               UserNameJp = user.UserNameJp,
                                               UserNameEn = user.UserNameEn,
                                               UserId = user.UserId,
                                               EndTime = reser.EndTime,
                                               StartTime = reser.StartTime,
                                               Price = reser.Price
                                           }).ToList();
                    }
                }

                // get array id user
                var arrUserId = queryDataExport.Select(p => p.UserId).Distinct().ToArray();
                foreach (var item in arrUserId)
                {
                    var listGroup = queryDataExport.Where(p => p.UserId == item).ToArray();
                    var sumtime = queryDataExport.Where(p => p.UserId == item).Select(r => r.EndTime.Subtract(r.StartTime).TotalHours).ToList().Sum();
                    var sumprice = queryDataExport.Where(p => p.UserId == item).Select(r => r.Price).Sum();

                    query.Add(new object[]
                    {
                             !string.IsNullOrEmpty(listGroup[0].GroupNameJp) ? listGroup[0].GroupNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].GroupNameJp}\"" : $"{listGroup[0].GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].GroupNameEn) ? listGroup[0].GroupNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].GroupNameEn}\"" : $"{listGroup[0].GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].UserNameJp) ? listGroup[0].UserNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].UserNameJp}\"" : $"{listGroup[0].UserNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].UserNameEn) ? listGroup[0].UserNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].UserNameEn}\"" : $"{listGroup[0].UserNameEn}" : $"",
                             $"{sumtime}",
                             $"{sumprice}"
                    });
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
        public List<object[]> List_Data_Obj_2(string startDate, string endDate)
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

            for (int i = 0; i < machines.Count; i++)
            {
                var query = sqlQuery.Join(
                                    _db.Machines.Where(m => m.MachineId == machines[i] && m.Status == 1),
                                    res => res.MachineId,
                                    mac => mac.MachineId,
                                    (res, mac) => new
                                    {
                                        MachineNameEn = mac.MachineNameEn,
                                        MachineNameJp = mac.MachineNameJp,
                                        EndTime = res.EndTime,
                                        StartTime = res.StartTime,
                                        Price = res.Price,
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
                                        EndTime = rs.EndTime,
                                        StartTime = rs.StartTime,
                                        Price = rs.Price,
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
                                        EndTime = r.EndTime,
                                        StartTime = r.StartTime,
                                        Price = r.Price,
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
                             !string.IsNullOrEmpty(item.GroupNameJp) ? item.GroupNameJp.IndexOf(",") > -1 ? $"\"{item.GroupNameJp}\"" : $"{item.GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(item.GroupNameEn) ? item.GroupNameEn.IndexOf(",") > -1 ? $"\"{item.GroupNameEn}\"" : $"{item.GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(item.UserNameJp) ? item.UserNameJp.IndexOf(",") > -1 ? $"\"{item.UserNameJp}\"" : $"{item.UserNameJp}" : $"",
                             !string.IsNullOrEmpty(item.UserNameEn) ? item.UserNameEn.IndexOf(",") > -1 ? $"\"{item.UserNameEn}\"" : $"{item.UserNameEn}" : $"",
                             $"{(item.EndTime-item.StartTime).TotalHours}",
                             $"{item.Price}"
                    });
                }
            }

            return obj;
        }

        /// <summary>
        /// Check if the machine exists group or not
        /// </summary>
        /// <param name="idMachine"></param>
        /// <param name="firstDay"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int CheckGroupExist(int idMachine, string firstDay, string lastDay)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var query = GetReservationsQuery(idMachine, firstDay, lastDay);

            var reservations = query
                                    .Join(
                                        _db.Users.Where(u => u.Status == 1),
                                        res => res.UserId,
                                        user => user.UserId,
                                        (res, user) => new
                                        {
                                            userId = user.UserId,
                                            groupId = user.GroupId,
                                            macchineId = res.MachineId,
                                            userName = (_lang == "en-US") ? user.UserNameEn : user.UserNameJp,
                                            reserveId = res.ReserveId,
                                            startTime = res.StartTime,
                                            endTime = res.EndTime,
                                            price = res.Price
                                        }
                                    )
                                    .Join(
                                        _db.Groups.Where(g => g.Status == 1),
                                        rs => rs.groupId,
                                        group => group.GroupId,
                                        (rs, group) => new
                                        {
                                            GroupNameEn = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp,
                                            ParentGroupId = group.ParentGroupId,
                                            GroupId = group.GroupId,
                                            MachineId = rs.macchineId,
                                            userId = rs.userId,
                                            userName = rs.userName,
                                            reserveId = rs.reserveId,
                                            startTime = rs.startTime,
                                            endTime = rs.endTime,
                                            price = rs.price
                                        }
                                    )
                                    .OrderBy(p => p.ParentGroupId)
                                    .ToList();

            List<GroupLevel> Levels = new List<GroupLevel>();

            foreach (var item in reservations)
            {
                if (item.ParentGroupId == null)
                {

                    List<GroupLevel> Levels_1 = new List<GroupLevel>();

                    foreach (var itemx in reservations)
                    {
                        if (item.GroupId == itemx.ParentGroupId)
                        {
                            List<GroupLevel> Levels_2 = new List<GroupLevel>();

                            foreach (var itemy in reservations)
                            {
                                if (itemx.GroupId == itemy.ParentGroupId)
                                {
                                    GroupLevel gr_2 = new GroupLevel
                                    {
                                        GroupId = itemy.GroupId,
                                        GroupNameEn = itemy.GroupNameEn,
                                    };

                                    if (Levels_2.Count() > 0)
                                    {
                                        if (Levels_2.FirstOrDefault(p => p.GroupId == gr_2.GroupId) == null)
                                        {
                                            var total_time = reservations.Where(p => (p.GroupId == itemx.GroupId) || (p.ParentGroupId == itemx.GroupId)).ToList().Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours);
                                            var total_price = reservations.Where(p => (p.GroupId == itemx.GroupId) || (p.ParentGroupId == itemx.GroupId)).ToList().Sum(p => p.price);
                                            gr_2.Total_Hours_Group = total_time;
                                            gr_2.Total_Price_Group = total_price;
                                            Levels_2.Add(gr_2);
                                        }
                                    }
                                    else
                                    {
                                        var total_time = reservations.Where(p => (p.GroupId == itemx.GroupId) || (p.ParentGroupId == itemx.GroupId)).ToList().Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours);
                                        var total_price = reservations.Where(p => (p.GroupId == itemx.GroupId) || (p.ParentGroupId == itemx.GroupId)).ToList().Sum(p => p.price);
                                        gr_2.Total_Hours_Group = total_time;
                                        gr_2.Total_Price_Group = total_price;
                                        Levels_2.Add(gr_2);
                                    }
                                }
                            }

                            GroupLevel gr_1 = new GroupLevel
                            {
                                GroupId = itemx.GroupId,
                                GroupNameEn = itemx.GroupNameEn,
                                GroupLevels = Levels_2,
                                Total_Hours_Group = Levels_2.Sum(p => p.Total_Hours_Group),
                                Total_Price_Group = Levels_2.Sum(p => p.Total_Price_Group)

                            };

                            if (Levels_1.Count() > 0)
                            {
                                if (Levels_1.FirstOrDefault(p => p.GroupId == gr_1.GroupId) == null)
                                {
                                    Levels_1.Add(gr_1);
                                }
                            }
                            else
                            {
                                Levels_1.Add(gr_1);
                            }
                        }
                    }

                    GroupLevel gr = new GroupLevel
                    {
                        GroupId = item.GroupId,
                        GroupNameEn = item.GroupNameEn,
                        GroupLevels = Levels_1,
                        Total_Hours_Group = Levels_1.Sum(p => p.Total_Hours_Group),
                        Total_Price_Group = Levels_1.Sum(p => p.Total_Price_Group)
                    };

                    if (Levels.Count() > 0)
                    {
                        if (Levels.FirstOrDefault(p => p.GroupId == gr.GroupId) == null)
                        {
                            //in the absence of subgroups
                            List<UserResult> list_user = new List<UserResult>();

                            if (gr.GroupLevels.Count == 0)
                            {
                                var temp_idUser = 0;

                                foreach (var rv in reservations.Where(p => p.GroupId == gr.GroupId))
                                {
                                    if (temp_idUser != rv.userId)
                                    {
                                        temp_idUser = rv.userId;

                                        var reservations_list = reservations.Where(p => p.userId == rv.userId).ToList();

                                        list_user.Add(new UserResult
                                        {
                                            UserNameEn = rv.userName,
                                            Time = reservations_list.Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours),
                                            Price = reservations_list.Sum(p => p.price)

                                        });
                                    }
                                }
                            }

                            gr.Group_User = list_user;
                            gr.Total_Hours_Group = list_user.Sum(p => p.Time);
                            gr.Total_Price_Group = list_user.Sum(p => p.Price);

                            Levels.Add(gr);
                        }
                    }
                    else
                    {
                        //in the absence of subgroups
                        List<UserResult> list_user = new List<UserResult>();

                        if (gr.GroupLevels.Count == 0)
                        {
                            var temp_idUser = 0;

                            foreach (var rv in reservations.Where(p => p.GroupId == gr.GroupId))
                            {
                                if (temp_idUser != rv.userId)
                                {
                                    temp_idUser = rv.userId;

                                    var reservations_list = reservations.Where(p => p.userId == rv.userId).ToList();

                                    list_user.Add(new UserResult
                                    {
                                        UserNameEn = rv.userName,
                                        Time = reservations_list.Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours),
                                        Price = reservations_list.Sum(p => p.price)

                                    });
                                }
                            }
                        }

                        gr.Group_User = list_user;
                        gr.Total_Hours_Group = list_user.Sum(p => p.Time);
                        gr.Total_Price_Group = list_user.Sum(p => p.Price);

                        Levels.Add(gr);
                    }
                }
                else
                {
                    // Find parent group
                    var find_group = _db.Groups.Where(g => g.Status == 1).FirstOrDefault(p => p.GroupId == item.ParentGroupId);

                    if (find_group != null) // have data
                    {
                        if (find_group.ParentGroupId == null)
                        {
                            List<GroupLevel> groupx = new List<GroupLevel>
                            {
                                new GroupLevel
                                {
                                    GroupId = item.GroupId,
                                    GroupNameEn = item.GroupNameEn,
                                    Total_Hours_Group = reservations.Where(p => p.ParentGroupId == item.ParentGroupId).ToList().Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours),
                                    Total_Price_Group = reservations.Where(p => p.ParentGroupId == item.ParentGroupId).ToList().Sum(p => p.price)
                                 }
                            };

                            GroupLevel gr = new GroupLevel
                            {
                                GroupId = find_group.GroupId,
                                GroupNameEn = find_group.GroupNameEn,
                                GroupLevels = groupx
                            };

                            if (Levels.Count() > 0)
                            {
                                if (Levels.FirstOrDefault(p => p.GroupId == gr.GroupId) == null)
                                {
                                    gr.Total_Hours_Group = gr.GroupLevels.Sum(p => p.Total_Hours_Group);
                                    gr.Total_Price_Group = gr.GroupLevels.Sum(p => p.Total_Price_Group);

                                    Levels.Add(gr);
                                }
                            }
                            else
                            {
                                gr.Total_Hours_Group = gr.GroupLevels.Sum(p => p.Total_Hours_Group);
                                gr.Total_Price_Group = gr.GroupLevels.Sum(p => p.Total_Price_Group);
                                Levels.Add(gr);
                            }
                        }
                        else
                        {
                            var find_group_parent = _db.Groups.Where(g => g.Status == 1).FirstOrDefault(p => p.GroupId == find_group.ParentGroupId);

                            if (find_group_parent.ParentGroupId == null)
                            {

                                List<GroupLevel> Level3 = new List<GroupLevel>
                                {
                                    new GroupLevel
                                    {
                                        GroupId = item.GroupId,
                                        GroupNameEn = item.GroupNameEn,
                                        Total_Hours_Group = reservations.Where(p => p.ParentGroupId == find_group.ParentGroupId).ToList().Sum(p => (p.endTime.Subtract(p.startTime)).TotalHours),
                                        Total_Price_Group = reservations.Where(p => p.ParentGroupId == find_group.ParentGroupId).ToList().Sum(p => p.price)
                                    }
                                };

                                List<GroupLevel> Level2 = new List<GroupLevel>
                                        {
                                            new GroupLevel
                                            {
                                                GroupId = find_group.GroupId,
                                                GroupNameEn = find_group.GroupNameEn,
                                                GroupLevels = Level3,
                                                Total_Hours_Group = Level3.Sum(p=>p.Total_Hours_Group),
                                                Total_Price_Group = Level3.Sum(p=>p.Total_Price_Group)
                                            }
                                        };

                                GroupLevel gr = new GroupLevel
                                {
                                    GroupId = find_group_parent.GroupId,
                                    GroupNameEn = find_group_parent.GroupNameEn,
                                    GroupLevels = Level2,
                                };

                                if (Levels.Count() > 0)
                                {
                                    if (Levels.FirstOrDefault(p => p.GroupId == gr.GroupId) == null)
                                    {
                                        gr.Total_Hours_Group = gr.GroupLevels.Sum(p => p.Total_Hours_Group);
                                        gr.Total_Price_Group = gr.GroupLevels.Sum(p => p.Total_Price_Group);

                                        Levels.Add(gr);
                                    }
                                }
                                else
                                {
                                    Levels.Add(gr);
                                }
                            }
                        }
                    }
                }
            }

            return Levels.Count();
        }

        /// <summary>
        /// Retrieve the list of userID in table Reservations
        /// </summary>
        /// <param name="machineId"></param>
        /// <returns></returns>
        public List<int> ListIdUser(int machineId)
        {
            List<int> _list_userId = new List<int>();

            var userID_tblReser = _db.Reservations.Where(p => p.MachineId == machineId).Select(p => p.UserId).Distinct().ToList();

            if (userID_tblReser.Count() > 0)
            {
                for (int i = 0; i < userID_tblReser.Count(); i++)
                {
                    var user = _db.Users.FirstOrDefault(us => us.UserId == userID_tblReser[i] && us.Status == 1);

                    if (user != null)
                    {
                        if (user.Status != 0 && !_list_userId.Contains(userID_tblReser[i]))
                        {
                            _list_userId.Add(userID_tblReser[i]);
                        }
                    }
                }
            }

            return _list_userId;
        }


        public GroupResult GroupResult(Group group, string firstDay, string lastDay, string _lang)
        {
            GroupResult groupParent = new GroupResult();

            ChildGroup groupChild = new ChildGroup();

            List<UserResult> list_user = new List<UserResult>();

            if (group.Users == null)
            {
                return groupParent;
            }

            if (group.Users.Count() > 0)
            {
                double total_time = 0;
                double total_price = 0;

                foreach (var gr_us in group.Users)
                {
                    var reservations = GetReservationsGroupQuery(gr_us.UserId, firstDay, lastDay).ToList();
                    double us_time = reservations.Sum(p => (p.EndTime.Subtract(p.StartTime)).TotalHours);
                    double us_price = reservations.Sum(p => p.Price);

                    total_time += us_time;
                    total_price += us_price;

                    list_user.Add(new UserResult
                    {
                        UserNameEn = (_lang == "en-US") ? gr_us.UserNameEn : gr_us.UserNameJp,
                        Time = us_time,
                        Price = us_price
                    });
                }

                groupParent.SumTime = total_time;
                groupParent.SumPrice = total_price;

                if (group.Groups.Count() > 0)
                {
                    groupChild = GetChildGroup(group.Groups, firstDay, lastDay);
                    groupParent.SumTime += groupChild.TotalTime;
                    groupParent.SumPrice += groupChild.TotalPrice;
                }
            }
            else
            {
                var list_child_group = _db.Groups.Where(p => p.ParentGroupId == group.GroupId && p.Status == 1).Include(p => p.Users).ToList();
                groupChild = GetChildGroup(list_child_group, firstDay, lastDay);
                groupParent.SumTime = groupChild.TotalTime;
                groupParent.SumPrice = groupChild.TotalPrice;

            }

            groupParent.GroupId = group.GroupId;
            groupParent.GroupNameEn = (_lang == "en-US") ? group.GroupNameEn : group.GroupNameJp;
            groupParent.Users = list_user;
            groupParent.ChildGroups = groupChild;
            groupParent.ParentGroupId = group.ParentGroupId;


            return groupParent;

        }

        public List<int> getUserGroup(int groupId)
        {
            List<int> idGroup = new List<int>();

            // find group parent
            Group gr = new Group();

            var findParentGroup = _db.Groups.Where(g => g.Status == 1 && g.GroupId == groupId)
                                            .Include(gr => gr.Groups)
                                            .FirstOrDefault();

            if (findParentGroup.ParentGroupId == null)
            {
                gr = findParentGroup;
            }
            else
            {
                var findParentGroup2 = _db.Groups.Include(g => g.Groups).Where(g => g.Status == 1 && g.GroupId == findParentGroup.ParentGroupId).IncludeFilter(p => p.Users.Where(u => u.Status == 1)).FirstOrDefault();

                if (findParentGroup2.ParentGroupId == null)
                {
                    gr = findParentGroup2;
                }
                else
                {

                }
            }

            idGroup.Add(gr.GroupId);
            if (gr.Groups.Count() > 0)
            {
                foreach (var item in gr.Groups)
                {
                    idGroup.Add(item.GroupId);
                }
            }

            return idGroup;
        }

        /// <summary>
        /// Get data export if user not admin
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<object[]> List_Data_Obj_Refer(string startDate, string endDate)
        {
            List<object[]> obj = new List<object[]>();
            var user_id = HttpContext.Session.GetInt32("UserId");
            var machines = _db.CanUseMachines.Where(u => u.UserId == Convert.ToInt32(user_id)).Select(u => u.MachineId).ToList();
            var use = _db.Users.Find(user_id);
            var sqlQuery = _db.Reservations.OrderBy(p => p.MachineId);


            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate)).OrderBy(p => p.MachineId);
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate) && a.EndTime.Date <= Convert.ToDateTime(endDate)).OrderBy(p => p.MachineId);
            }

            var query = sqlQuery.Join(
                                _db.Users.Where(u => u.Status == 1),
                                rs => rs.UserId,
                                us => us.UserId,
                                (rs, us) => new
                                {
                                    EndTime = rs.EndTime,
                                    StartTime = rs.StartTime,
                                    Price = rs.Price,
                                    UserId = rs.UserId,
                                    UserNameEn = us.UserNameEn,
                                    UserNameJp = us.UserNameJp,
                                    GroupId = us.GroupId
                                }
                            )
                          .Join(
                                _db.Groups.Where(g => g.Status == 1 && g.GroupId == use.GroupId || g.Status == 1 && g.ParentGroupId == use.GroupId),
                                r => r.GroupId,
                                gr => gr.GroupId,
                                (r, gr) => new
                                {
                                    EndTime = r.EndTime,
                                    StartTime = r.StartTime,
                                    Price = r.Price,
                                    UserId = r.UserId,
                                    UserNameEn = r.UserNameEn,
                                    UserNameJp = r.UserNameJp,
                                    GroupId = r.GroupId,
                                    GroupNameEn = gr.GroupNameEn,
                                    GroupNameJp = gr.GroupNameJp
                                }
                            ).ToList();

            // get array id user
            var arrUserId = query.Select(p => p.UserId).Distinct().ToArray();
            foreach (var item in arrUserId)
            {
                var listGroup = query.Where(p => p.UserId == item).ToArray();
                var sumtime = query.Where(p => p.UserId == item).Select(r => r.EndTime.Subtract(r.StartTime).TotalHours).ToList().Sum();
                var sumprice = query.Where(p => p.UserId == item).Select(r => r.Price).Sum();

                obj.Add(new object[]
                {
                             !string.IsNullOrEmpty(listGroup[0].GroupNameJp) ? listGroup[0].GroupNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].GroupNameJp}\"" : $"{listGroup[0].GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].GroupNameEn) ? listGroup[0].GroupNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].GroupNameEn}\"" : $"{listGroup[0].GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].UserNameJp) ? listGroup[0].UserNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].UserNameJp}\"" : $"{listGroup[0].UserNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].UserNameEn) ? listGroup[0].UserNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].UserNameEn}\"" : $"{listGroup[0].UserNameEn}" : $"",
                             $"{sumtime}",
                             $"{sumprice}"
                });

            }

            return obj;
        }

        public List<object[]> List_Data_Obj(string startDate, string endDate)
        {
            List<object[]> obj = new List<object[]>();
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            var loginGroup = Convert.ToInt32(HttpContext.Session.GetInt32("GroupId"));
            var user_id = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            var start_time = Convert.ToDateTime("1/1/1970");
            var end_time = DateTime.Now;
            if (!string.IsNullOrEmpty(startDate))
            {
                start_time = Convert.ToDateTime(startDate);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                end_time = Convert.ToDateTime(endDate);
            }
            var arrUserId = GetUserInGroup(loginGroup).ToArray();
            var canUseMachine = _db.CanUseMachines.Where(u => arrUserId.Contains(u.UserId)).Distinct().Select(u => u.MachineId).ToList();

            List<MachineUseTime> listItems = new List<MachineUseTime>();
            var machines = _db.Machines.Where(m => m.Status == 1).ToList();
            foreach (var m in machines)
            {
                if (canUseMachine.IndexOf(m.MachineId) > -1 || user_id == 1)
                {
                    var sumTime = GetMachineTotalUseTime(start_time, end_time, m.MachineId, user_id, arrUserId);
                    var sumPrice = GetMachineTotalPrice(start_time, end_time, m.MachineId, user_id, arrUserId);
                    if (sumTime > 0)
                    {
                        var d = new MachineUseTime();
                        d.MachineId = m.MachineId;
                        d.MachineName = (_lang == "en-US") ? m.MachineNameEn : m.MachineNameJp;
                        d.UseTime = sumTime;
                        d.UsePrice = sumPrice;
                        listItems.Add(d);
                    }
                }
                else if (m.ReferenceAuth == 1)
                {
                    var sumTime = GetMachineTotalUseTime(start_time, end_time, m.MachineId, user_id, arrUserId);
                    var sumPrice = GetMachineTotalPrice(start_time, end_time, m.MachineId, user_id, arrUserId);
                    if (sumTime > 0)
                    {
                        var d = new MachineUseTime();
                        d.MachineId = m.MachineId;
                        d.MachineName = (_lang == "en-US") ? m.MachineNameEn : m.MachineNameJp;
                        d.UseTime = sumTime;
                        d.UsePrice = sumPrice;
                        listItems.Add(d);
                    }
                }
            }

            listItems.OrderBy(p => p.MachineName);

            var sqlQuery = _db.Reservations.OrderBy(p => p.MachineId);

            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate)).OrderBy(p => p.MachineId);
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlQuery = _db.Reservations.Where(a => a.StartTime.Date >= Convert.ToDateTime(startDate) && a.EndTime.Date <= Convert.ToDateTime(endDate)).OrderBy(p => p.MachineId);
            }

            foreach (var itemm in listItems)
            {
                var query = sqlQuery.Join(
                                    _db.Machines.Where(m => m.MachineId == itemm.MachineId && m.Status == 1),
                                    res => res.MachineId,
                                    mac => mac.MachineId,
                                    (res, mac) => new
                                    {
                                        MachineNameEn = mac.MachineNameEn,
                                        MachineNameJp = mac.MachineNameJp,
                                        EndTime = res.EndTime,
                                        StartTime = res.StartTime,
                                        Price = res.Price,
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
                                        EndTime = rs.EndTime,
                                        StartTime = rs.StartTime,
                                        Price = rs.Price,
                                        UserId = rs.UserId,
                                        UserNameEn = us.UserNameEn,
                                        UserNameJp = us.UserNameJp,
                                        GroupId = us.GroupId
                                    }
                                )
                              .Join(
                                    _db.Groups.Where(g => g.Status == 1 && g.GroupId == loginGroup || g.Status == 1 && g.ParentGroupId == loginGroup),
                                    r => r.GroupId,
                                    gr => gr.GroupId,
                                    (r, gr) => new
                                    {
                                        MachineNameEn = r.MachineNameEn,
                                        MachineNameJp = r.MachineNameJp,
                                        EndTime = r.EndTime,
                                        StartTime = r.StartTime,
                                        Price = r.Price,
                                        UserId = r.UserId,
                                        UserNameEn = r.UserNameEn,
                                        UserNameJp = r.UserNameJp,
                                        GroupId = r.GroupId,
                                        GroupNameEn = gr.GroupNameEn,
                                        GroupNameJp = gr.GroupNameJp
                                    }
                                ).ToList();


                // get array id user
                var arrayUserId = query.Select(p => p.UserId).Distinct().ToArray();
                foreach (var item in arrayUserId)
                {
                    var listGroup = query.Where(p => p.UserId == item).ToArray();
                    var sumtime = query.Where(p => p.UserId == item).Select(r => r.EndTime.Subtract(r.StartTime).TotalHours).ToList().Sum();
                    var sumprice = query.Where(p => p.UserId == item).Select(r => r.Price).Sum();

                    obj.Add(new object[]
                    {
                             !string.IsNullOrEmpty(listGroup[0].MachineNameJp) ? listGroup[0].MachineNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].MachineNameJp}\"" : $"{listGroup[0].MachineNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].MachineNameEn) ? listGroup[0].MachineNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].MachineNameEn}\"" : $"{listGroup[0].MachineNameEn}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].GroupNameJp) ? listGroup[0].GroupNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].GroupNameJp}\"" : $"{listGroup[0].GroupNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].GroupNameEn) ? listGroup[0].GroupNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].GroupNameEn}\"" : $"{listGroup[0].GroupNameEn}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].UserNameJp) ? listGroup[0].UserNameJp.IndexOf(",") > -1 ? $"\"{listGroup[0].UserNameJp}\"" : $"{listGroup[0].UserNameJp}" : $"",
                             !string.IsNullOrEmpty(listGroup[0].UserNameEn) ? listGroup[0].UserNameEn.IndexOf(",") > -1 ? $"\"{listGroup[0].UserNameEn}\"" : $"{listGroup[0].UserNameEn}" : $"",
                             $"{sumtime}",
                             $"{sumprice}"
                    });

                }
            }
            return obj;
        }
    }
}