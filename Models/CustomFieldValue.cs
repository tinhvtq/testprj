using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class CustomFieldValue
    {
        [Key]
        public int ValueId { get; set; }
        public int FieldId { get; set; }
        public int ReserveId { get; set; }
        public string FieldValue { get; set; }

        public virtual Reservation Reservation { get; set; }
        public virtual CustomField CustomField { get; set; }
    }
}
