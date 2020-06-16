using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }
        public string GroupNameEn { get; set; }
        public string GroupNameJp { get; set; }
        public System.Nullable<int> ParentGroupId { get; set; }
        public bool BuiltinGroup { get; set; }
        public int Status { get; set; }

        public virtual IList<User> Users { get; set; }
        public virtual IList<Group> Groups { get; set; }
    }

    public class UserResult
    {
        public string UserNameEn { get; set; }
        public double Time { get; set; }
        public double Price { get; set; }
    }

    public class GroupResult
    {
        public int GroupId { get; set; }
        public string GroupNameEn { get; set; }
        public double SumTime { get; set; }
        public double SumPrice { get; set; }
        public ChildGroup ChildGroups { get; set; }
        public IList<UserResult> Users { get; set; }
        public int? ParentGroupId { get; set; }


    }

    public class ChildGroup
    {
        public double TotalTime { get; set; }
        public double TotalPrice { get; set; }
        public IList<GroupResult> Groups { get; set; }
    }


    public class UserUseTime
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public double TotalUseTime { get; set; }
        public int TotalPrice { get; set; }
        public int GroupId { get; set; }
    }
    public class GroupUseTime
    {
        public int GroupId { get; set; }
        public System.Nullable<int> ParentGroupId { get; set; }
        public string GroupName { get; set; }
        public List<GroupUseTime> ChildGroups { get; set; }
        public List<UserUseTime> UserUseTimes { get; set; }
        public double TotalUseTime { get; set; }
        public int TotalPrice { get; set; }
    }
}
