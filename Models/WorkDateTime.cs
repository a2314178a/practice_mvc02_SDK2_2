using System;
namespace practice_mvc02.Models
{
    public class WorkDateTime
    {
        public byte type {get; set;}
        public DateTime sWorkDt {get; set;}  //online work dateTime
        public DateTime eWorkDt {get; set;}   //offline work dateTime
        public int elasticityMin {get; set;}    //彈性時間
        public DateTime sPunchDT {get; set;}  //可打卡時間
        public DateTime ePunchDT {get; set;}
        public DateTime sRestDt {get; set;} //開始休息時間
        public DateTime eRestDt {get; set;} //結束休息時間
        public bool workAllTime {get; set;}
        public int lessStHour {get; set;}   //可打卡時間 從上班時間開始扣
        public int addEtHour {get; set;}    //可打卡時間 從下班時間開始加

        public WorkDateTime()
        {
            this.lessStHour = -2*60;    //單位改為分鐘
            this.addEtHour = 13*60;     //單位改為分鐘
        }
    }
}