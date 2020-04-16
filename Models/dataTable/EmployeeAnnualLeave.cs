using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class EmployeeAnnualLeave
    {
        [Key]
        public int ID {get;set;}
        public int employeeID {get; set;}
        public int ruleID {get; set;}
        public int specialDays {get; set;}
        public float remainHours {get; set;}
        public DateTime deadLine {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}