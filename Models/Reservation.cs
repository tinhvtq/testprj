using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class Reservation
    {
        [Key]
        public int ReserveId { get; set; }
        public int MachineId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Color { get; set; }
        public int Price { get; set; }
        public string Note { get; set; }
        public virtual User User { get; set; }
        public virtual Machine Machine { get; set; }
        public virtual ICollection<CustomFieldValue> CustomFieldValues { get; set; }
    }

    public class Event
    {
        public int ReserveId { get; set; }
        public int MachineId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Color { get; set; }
        public int Price { get; set; }
        public string UserName { get; set; }
        public string GroupName { get; set; }
        public string MachineName { get; set; }
    }

    public class MachineReservation
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public virtual List<Event> Events { get; set; }
    }

}
