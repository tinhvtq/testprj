using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReservationSystem.Models
{
    public class UseRecordList
    {
    }

    public class TimeLogResult
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string UseTime { get; set; }
        public string UserNameEn { get; set; }
        public string GroupName { get; set; }
        public string Run { get; set; }
        public string Purpose_Remark { get; set; }
        public string fieldNameEn0 { get; set; }
        public string fieldNameEn1 { get; set; }
        public string fieldNameEn2 { get; set; }
        public string fieldNameEn3 { get; set; }
        public string fieldNameEn4 { get; set; }
        public string fieldNameEn5 { get; set; }
        public string fieldNameEn6 { get; set; }
        public string fieldNameEn7 { get; set; }
        public string fieldNameEn8 { get; set; }
        public string fieldNameEn9 { get; set; }
        public string fieldNameEn10 { get; set; }
    }

    public class DetailedMachineInfo
    {
        public string image { get; set; }
        public string machine_name { get; set; }
        public int unit_price { get; set; }
        public string marker_name { get; set; }
        public string model { get; set; }
        public string performance { get; set; }
        public string categorry { get; set; }
        public string set_up_area { get; set; }
        public string explanation { get; set; }
        public string url { get; set; }
        public string url2 { get; set; }
        public string url3 { get; set; }
        public List<ModalFileRecord> manual_file { get; set; }
        public List<ModalUseUser> users { get; set; }
        public string purchaseDate { get; set; }
        public string storePurchased { get; set; }
        public string vesselNumber { get; set; }
        public string businessPerson { get; set; }
        public string businessAddress { get; set; }
        public string businessTel { get; set; }
        public string businessFax { get; set; }
        public string businessMail { get; set; }
        public string technicalPersion { get; set; }
        public string technicalAddress { get; set; }
        public string technicalTel { get; set; }
        public string technicalFax { get; set; }
        public string technicalMail { get; set; }
        public string expendableSupplies { get; set; }
    }

    public class ModalFileRecord
    {
        public string manualFileName { get; set; }
        public string displayName { get; set; }
    }

    public class ModalUseUser
    {
        public int userId { get; set; }
        public string userNameEn { get; set; }
    }

    public class ModalUserDetail
    {
        public string username { get; set; }
        public string groupname { get; set; }
        public string email { get; set; }
    }

    public class ResultSearch
    {
        public int MachineId { set; get; }
        public string MachineNameEn { set; get; }
        public List<TimeLogResult> TimeLog { set; get; }
    }

    public class ParamaterSearch
    {
        public int id { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }

    }

    public class ParamaterExport
    {
        public int UserId { get; set; }
        public string UserNameJp { get; set; }
        public string UserNameEn { get; set; }
        public string GroupNameJp { get; set; }
        public string GroupNameEn { get; set; }
        public string MachineNameJp { get; set; }
        public string MachineNameEn { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public int Price { get; set; }
    }
}
