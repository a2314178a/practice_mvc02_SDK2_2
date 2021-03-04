using System;
using System.IO;

namespace practice_mvc02.Models
{
    static public class definePara
    {
        private const int UTC = 8;
        private const string specialName = "特休";
        private const string overtimeRestName = "補休";
        private const int DIMALV = 999;
        private const int workDay_to_workHour = 8;
        private const string noDepartment = "未指派";

        static public DateTime dtNow(){
            return DateTime.UtcNow.AddHours(UTC);
        }

        static public string[] getDefaultLeaveName(){
            var defaultLeaveName = new string[]{
                "公差", "特休", "事假", "病假", "公假", "調休", "喪假", "婚假", "產假", "陪產假", "其他", "排休", "補休"
            };
            return defaultLeaveName;
        }

        static public string annualName(){
            return specialName;
        }

        static public string otRestName(){
            return overtimeRestName;
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
        static public string noDepart(){
            return noDepartment;
        }

    }
}