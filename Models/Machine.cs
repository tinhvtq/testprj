using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class Machine
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
        public virtual ICollection<CanUseMachine> CanUseMachines { get; set; }
        public virtual ICollection<MachineAdmin> MachineAdmins { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<CustomField> CustomFields { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<ManualFile> ManualFiles { get; set; }
    }

    public class MachineFormData
    {
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
        public string URL { get; set; }
        public string URL2 { get; set; }
        public string URL3 { get; set; }
        public int ReferenceAuth { get; set; }
        public string charge_user { get; set; }
        public string use_user { get; set; }
        public string item_name_jp { get; set; }
        public string item_name_en { get; set; }
        public string arrCheck { get; set; }
        public int StatusRemoveImage { get; set; }
        public string ManualFileLink { get; set; }
        public string ItemId { get; set; }
    }

    public class GroupLevel
    {
        public int GroupId { get; set; }
        public string GroupNameEn { get; set; }
        public List<GroupLevel> GroupLevels { get; set; }
        public List<UserResult> Group_User { get; set; }
        public double Total_Hours_Group { get; set; }
        public double Total_Price_Group { get; set; }
    }

    public class MachineList
    {
        public int MachineId { get; set; }
        public string MachineNameEn { get; set; }
        public double Total_Hours { get; set; }
        public double Total_Price { get; set; }
	}
    public class CsvFormData
    {
        string CsvFile { get; set; }
    }

    public class CsvData
    {
        public string CategoryName { get; set; }
        public string MachineNameJp { get; set; }
        public string MachineNameEn { get; set; }
        public string MakerNameJp { get; set; }
        public string ModelJp { get; set; }
        public string ModelEn { get; set; }
        public string PlaceJp { get; set; }
        public string PlaceEn { get; set; }

    }

    public class MachineUseTime
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public double UseTime { get; set; }
        public int UsePrice { get; set; }
    }
}
