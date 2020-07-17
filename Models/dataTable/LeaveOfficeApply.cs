using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class LeaveOfficeApply
    {
        [Key]
        public int ID {get;set;}
        public int accountID {get; set;}
        public int principalID {get; set;}
        public int leaveID {get; set;}
        public int optionType {get; set;}
        public string note {get; set;}
        public DateTime startTime {get; set;}
        public DateTime endTime {get; set;}
        public float unitVal {get; set;}
        public byte unit {get; set;}    //1:全天 2:半天 3:小時
        public int applyStatus {get; set;}  //0:申請中 1:通過 2:沒通過
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}