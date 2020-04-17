using System;

namespace practice_mvc02.Models
{
    static public class definePara
    {
        private const int UTC = 8;
        private const string specialName = "特休";

        static public DateTime dtNow(){
            return DateTime.UtcNow.AddHours(UTC);
        }
        static public string annualName(){
            return specialName;
        }
    }
}