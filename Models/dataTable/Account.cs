using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class Account
    {
        [Key]
        public int ID {get;set;}
        [Required]
        public string account {get; set;}
        [Required]
        public string password {get; set;}
        public string userName {get; set;}
        public int departmentID {get; set;}
        public int accLV {get; set;}
        public int groupID {get; set;}
        public int timeRuleID {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
        public string loginTime {get; set;}  
    }
}