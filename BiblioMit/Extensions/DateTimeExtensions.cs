using System;

namespace BiblioMit.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetLastDayOfMonth(this DateTime dateTime) =>
            new(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
    }
}
