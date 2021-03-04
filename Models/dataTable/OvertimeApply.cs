using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class OvertimeApply
    {
        [Key]
        public int ID {get;set;}
        public int accountID {get; set;}
        public string note {get; set;}
        public DateTime workDate {get; set;}
        public int timeLength {get; set;}
        public int applyStatus {get; set;}  //0:申請中 1:通過 2:沒通過
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}