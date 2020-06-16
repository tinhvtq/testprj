using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class Infomation
    {
        [Key]
        public int InfomationId { get; set; }
        public int UserId { get; set; }
        public DateTime InfomationDate { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public string InfomationFile { get; set; }
        public virtual User User { get; set; }
    }

    public class ParamaterInfo
    {
        public int InfomationId { get; set; }
        public int UserId { get; set; }
        public DateTime InfomationDate { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public string InfomationFile { get; set; }
        public string InfomationFile_Old { get; set; }
        public int StatusRemoveFile { get; set; } // 1 remove
    }
}
