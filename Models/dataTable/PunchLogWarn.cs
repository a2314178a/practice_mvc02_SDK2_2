using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class PunchLogWarn
    {
        [Key]
        public int ID {get;set;}
        public int accountID {get; set;}
        public int principalID {get; set;}
        public int punchLogID {get; set;}
        public int warnStatus {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}