using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class CanUseMachine
    {
        [Key]
        public int UserId { get; set; }
        public User User { get; set; }
        public int MachineId { get; set; }
        public Machine Machine { get; set; }

    }
}
