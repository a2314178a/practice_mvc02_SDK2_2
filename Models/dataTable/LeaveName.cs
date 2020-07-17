using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class LeaveName
    {
        [Key]
        public int ID {get;set;}
        public string leaveName {get; set;}
        public int timeUnit {get; set;}     //1:全天 2:半天 3:小時
        public bool halfVal {get; set;} //能否輸入小數點
        public int lastOperaAccID {get; set;}
        public bool enable {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}