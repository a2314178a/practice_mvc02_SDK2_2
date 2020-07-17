using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class GroupRule
    {
        [Key]
        public int ID {get;set;}
        public string groupName {get; set;}
        public int ruleParameter {get; set;}    //255=0xff=1111 1111
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}