using System;

namespace OurUmbraco.Our.Extensions
{
    public static class DateTimeExtensions
    {
        public static string AsDisplayedReleaseDate(this DateTime releaseDate)
        {
            return releaseDate == default(DateTime) ? "Not yet determined" : releaseDate.ToString("dddd, MMMM d yyyy");
        }

        public static int MonthsBetween(this DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
                return MonthsBetween(toDate, fromDate);

            var monthDiff = Math.Abs((toDate.Year * 12 + (toDate.Month - 1)) - (fromDate.Year * 12 + (fromDate.Month - 1)));

            return fromDate.AddMonths(monthDiff) > toDate || toDate.Day < fromDate.Day ? monthDiff - 1 : monthDiff;
        }

        public static DateTime GetDateLastDayOfMonth(this DateTime dateTime)
        {
            var lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            var returnDate = new DateTime(dateTime.Year, dateTime.Month, lastDay);
            return returnDate;
        }
    }
}
