using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string CategoryNameJp { get; set; }
        public string CategoryNameEn { get; set; }
        public int Status { get; set; }
        public virtual ICollection<Machine> Machines { get; set; }
    }
}
