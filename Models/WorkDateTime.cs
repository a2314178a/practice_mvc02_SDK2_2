using System;
namespace practice_mvc02.Models
{
    public class WorkDateTime
    {
        public DateTime sWorkDt {get; set;}  //online work dateTime
        public DateTime eWorkDt {get; set;}   //offline work dateTime
        public DateTime sPunchDT {get; set;}  //可打卡時間
        public DateTime ePunchDT {get; set;}
        public DateTime sRestDt {get; set;} //開始休息時間
        public DateTime eRestDt {get; set;} //結束休息時間
        public bool workAllTime {get; set;}
    }
}