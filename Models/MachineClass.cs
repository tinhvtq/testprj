using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReservationSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Models
{
    public class MachineClass
    {
        [Key]
        public int MachineId { get; set; }
        public string MachineNameJp { get; set; }
        public string MachineNameEn { get; set; }
        public int CategoryId { get; set; }
        public int UnitPrice { get; set; }
        public string MakerNameJp { get; set; }
        public string MakerNameEn { get; set; }
        public string ModelJp { get; set; }
        public string ModelEn { get; set; }
        public string SpecJp { get; set; }
        public string SpecEn { get; set; }
        public string PlaceJp { get; set; }
        public string PlaceEn { get; set; }
        public string ImageFile { get; set; }
        public string ExplanationJp { get; set; }
        public string ExplanationEn { get; set; }
        public string ManualFile { get; set; }
        public string PurchaseDate { get; set; }
        public string PurchaseCampany { get; set; }
        public string EquipmentNumber { get; set; }
        public string BusinessPerson { get; set; }
        public string BusinessAddress { get; set; }
        public string BusinessTel { get; set; }
        public string BusinessFax { get; set; }
        public string BusinessMail { get; set; }
        public string TechnicalPersion { get; set; }
        public string TechnicalAddress { get; set; }
        public string TechnicalTel { get; set; }
        public string TechnicalFax { get; set; }
        public string TechnicalMail { get; set; }
        public string ExpendableSupplies { get; set; }
        public int Status { get; set; }
        public string URL { get; set; }
        public string URL2 { get; set; }
        public string URL3 { get; set; }
        public int ReferenceAuth { get; set; }
        public int ShowOrder { get; set; }
        //public virtual ICollection<CanUseMachine> CanUseMachines { get; set; }
        public virtual ICollection<MachineAdmin> MachineAdmins { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
