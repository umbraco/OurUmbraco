using System;
using System.Collections.Generic;
using System.Linq;
using Nager.Date;

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

        public static double BusinessDaysSince(this DateTime fromDate)
        {
            var toDate = DateTime.Now;
            return BusinessDaysBetween(fromDate, toDate);
        }

        private static double BusinessDaysBetween(DateTime fromDate, DateTime toDate)
        {
            var businessDays = new List<DateTime>();
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (DateSystem.IsPublicHoliday(date, CountryCode.DK) || DateSystem.IsWeekend(date, CountryCode.DK))
                    continue;

                businessDays.Add(date);
            }

            return businessDays.Count;
        }
    }
}
