using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class PunchCardLog
    {
        [Key]
        public int ID {get;set;}
        [Required]
        public int accountID {get; set;}
        public int departmentID {get; set;}
        public DateTime logDate {get; set;}
        public DateTime onlineTime {get; set;}
        public DateTime offlineTime {get; set;}
        public int punchStatus {get; set;}  //0x01:正常 0x02:遲到 0x04:早退 0x08:加班 0x10:缺卡 0x20:曠職 0x40:請假
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}