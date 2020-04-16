using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class LeaveName
    {
        [Key]
        public int ID {get;set;}
        public string leaveName {get; set;}
        public int timeUnit {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}