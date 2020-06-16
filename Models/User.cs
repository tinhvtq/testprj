using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string LoginAccount { get; set; }
        public string LoginPassword { get; set; }
        public string UserNameJp { get; set; }
        public string UserNameEn { get; set; }
        public int GroupId { get; set; }
        public int UserType { get; set; }
        public string Tel { get; set; }
        public string Mail { get; set; }
        public bool BuiltinUser { get; set; }
        public int Status { get; set; }
        public virtual ICollection<CanUseMachine> CanUseMachines { get; set; }
        public virtual ICollection<MachineAdmin> MachineAdmins { get; set; }
        public virtual Group Group { get; set; }
    }

    public class UserCsvData
    {
        public string GroupName { get; set; }
        public string LoginAccount { get; set; }
        public string LoginPassword { get; set; }
        public string UserNameJp { get; set; }
        public string UserNameEn { get; set; }
        public int UserType { get; set; }
        public string Tel { get; set; }
        public string Mail { get; set; }
        public List<int> MachineId { get; set; }
        public int GroupId { get; set; }
    }
}