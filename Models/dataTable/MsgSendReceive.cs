using System;
using System.ComponentModel.DataAnnotations;

namespace practice_mvc02.Models.dataTable
{
    public class MsgSendReceive
    {
        [Key]
        public int ID {get;set;}
        public int messageID {get; set;}
        public int sendID {get; set;}
        public int receiveID {get; set;}
        public byte read {get; set;}    //0:未讀 1:已讀
        public bool sDelete {get; set;}
        public bool rDelete {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}