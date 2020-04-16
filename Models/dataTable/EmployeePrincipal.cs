using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class EmployeePrincipal
    {
        [Key]
        public int ID {get;set;}
        public int employeeID {get; set;}
        public int principalID {get; set;}
        public int principalAgentID {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}