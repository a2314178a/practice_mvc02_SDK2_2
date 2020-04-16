using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class Department
    {
        [Key]
        public int ID {get;set;}
        public string department {get; set;}
        public string position {get; set;}
        public int principalID {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}