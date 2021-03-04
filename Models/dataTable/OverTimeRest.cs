using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class OverTimeRest
    {
        [Key]
        public int ID {get;set;}
        public int accountID {get; set;}
        public int canRestTime {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}