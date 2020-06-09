using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class OperateLog
    {
        [Key]
        public int ID {get;set;}
        public int operateID {get; set;}
        public int employeeID {get; set;}
        public string active {get; set;}
        public string category {get; set;}
        public string content {get; set;}
        public string remark {get; set;}
        public DateTime createTime {get; set;}
    }

    public class ViewOpLog
    {
        public string opName { get; set; }
        public string emName { get; set; }
        public string active { get; set; }
        public string category { get; set; }
        public string content { get; set; }
        public string opTime { get; set; }
    }
    
    public class OpLogFilter{
        public DateTime sDate { get; set; }
        public DateTime eDate { get; set; }
        public string active { get; set; }
        public string category { get; set; }
        public int opID { get; set; }
        public int emID { get; set; }
    }
}