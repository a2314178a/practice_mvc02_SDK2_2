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
        public int warnStatus {get; set;}   //0:未處理 1:已處理 2:忽略
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}