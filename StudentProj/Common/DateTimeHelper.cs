using System;

namespace StudentProj.Common
{
    public static class DateTimeHelper
    {
        public static DateTime GetIndianStandardTime()
        {
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);
        }
    }
}
