using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class AnnualDaysOffset
    {
        [Key]
        public int ID {get;set;}
        public int emAnnualID {get; set;}
        public string reason {get; set;}
        public int value {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}