using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class ManualFile
    {
        [Key]
        public int ManualFileId { get; set; }
        public int MachineId { get; set; }
        public string ManualFileName { get; set; }
        public string DisplayName { get; set; }
        public virtual Machine Machine { get; set; }
    }
}
