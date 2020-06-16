using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class MaintenanceRecord
    {
        [Key]
        public int RecordId { get; set; }
        public int MachineId { get; set; }
        public int UserId { get; set; }
        public DateTime RecordDate { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public virtual User User { get; set; }
        public virtual Machine Machine { get; set; }
    }

    public class SearchResults
    {
        public Machine machine { get; set; }
    }


}
