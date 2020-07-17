using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class EmployeePrincipal
    {
        [Key]
        public int ID {get;set;}
        public int employeeID {get; set;}   //員工ID
        public int principalID {get; set;}  //主管ID
        public int principalAgentID {get; set;} //主管職務代理人ID
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}