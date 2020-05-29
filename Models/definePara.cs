using System;
using System.IO;

namespace practice_mvc02.Models
{
    static public class definePara
    {
        private const int UTC = 8;
        private const string specialName = "特休";
        private const int DIMALV = 999;
        private const int workDay_to_workHour = 8;

        static public DateTime dtNow(){
            return DateTime.UtcNow.AddHours(UTC);
        }
        static public string annualName(){
            return specialName;
        }

        static public string getPunchExcelSubFolder(){
            return Path.Combine("file", "excel", "punchLog");
        }

        static public int getDIMALV(){
            return DIMALV;
        }

        static public int dayToHour(){
            return workDay_to_workHour;
        }
    }
}