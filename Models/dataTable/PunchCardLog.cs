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
        public int punchStatus {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}