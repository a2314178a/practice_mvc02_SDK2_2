using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class AnnualLeaveRule
    {
        [Key]
        public int ID {get;set;}
        public float seniority {get; set;}
        public int specialDays {get; set;}
        public int buffDays {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}