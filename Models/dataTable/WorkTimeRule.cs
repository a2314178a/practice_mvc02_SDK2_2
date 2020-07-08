using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace practice_mvc02.Models.dataTable
{
    public class WorkTimeRule
    {
        [Key]
        public int ID {get;set;}
        public string name {get; set;}
        public byte type {get; set;}

        [Column(TypeName="time")]
        public TimeSpan startTime {get; set;}
        [Column(TypeName="time")]
        public TimeSpan endTime {get; set;}
        [Column(TypeName="time")]
        public TimeSpan sRestTime {get; set;}
        [Column(TypeName="time")]
        public TimeSpan eRestTime {get; set;}

        public int elasticityMin {get; set;}
        public int lastOperaAccID {get; set;}
        public DateTime createTime {get; set;}
        public DateTime updateTime {get; set;}
    }
}