using System;
using System.IO;

namespace practice_mvc02.Models
{
    static public class definePara
    {
        private const int UTC = 8;
        private const string specialName = "特休";
        public const int DIMALV = 999;

        static public DateTime dtNow(){
            return DateTime.UtcNow.AddHours(UTC);
        }
        static public string annualName(){
            return specialName;
        }

        static public string getPunchExcelSubFolder(){
            return Path.Combine("file", "excel", "punchLog");
        }
    }
}