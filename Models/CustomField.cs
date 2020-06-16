using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class CustomField
    {
        [Key]
        public int FieldId { get; set; }
        public int MachineId { get; set; }
        public string FieldNameJp { get; set; }
        public string FieldNameEn { get; set; }
        public bool Required { get; set; }
        public virtual Machine Machine { get; set; }
        public virtual ICollection<CustomFieldValue> CustomFieldValues { get; set; }
    }
}
