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
        public byte unit {get; set;}
        public int applyStatus {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}