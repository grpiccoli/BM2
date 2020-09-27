using System;

namespace BiblioMit.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetLastDayOfMonth(this DateTime dateTime) =>
            new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
    }
}
