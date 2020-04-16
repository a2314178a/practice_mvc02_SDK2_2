using System;

namespace practice_mvc02.Models
{
    static public class definePara
    {
        private const int UTC = 8;
        static public DateTime dtNow(){
            return DateTime.UtcNow.AddHours(UTC);
        }
    }
}