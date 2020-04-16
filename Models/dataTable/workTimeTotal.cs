using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class workTimeTotal
    {
        [Key]
        public int ID {get;set;}
        public int accountID {get; set;}
        public DateTime dateMonth {get; set;}
        public double totalTime {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}