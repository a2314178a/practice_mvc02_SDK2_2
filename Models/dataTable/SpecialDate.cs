using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class SpecialDate
    {
        [Key]
        public int ID {get;set;}
        public DateTime date {get; set;}
        public string departClass {get; set;}
        public int status {get; set;}
        public string note {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}