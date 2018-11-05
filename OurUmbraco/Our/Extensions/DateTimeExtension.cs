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
            return fromDate.BusinessDaysUntil(toDate);
        }

        public static double BusinessDaysUntil(this DateTime fromDate, DateTime toDate)
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

        public static bool IsBusinessDay(this DateTime date)
        {
            return DateSystem.IsPublicHoliday(date, CountryCode.DK) == false && DateSystem.IsWeekend(date, CountryCode.DK) == false;
        }

        public static double BusinessHoursUntil(this DateTime fromDateTime, DateTime toDateTime)
        {
            double hoursOpen = 0;
            if (fromDateTime.IsBusinessDay())
            {
                var dayEndTime = new DateTime(fromDateTime.Year, fromDateTime.Month, fromDateTime.Day, 23, 59, 59);
                hoursOpen = hoursOpen + (dayEndTime - fromDateTime).TotalHours;
            }

            if (toDateTime.IsBusinessDay())
            {
                var dayStartTime = new DateTime(toDateTime.Year, toDateTime.Month, toDateTime.Day, 0, 0, 0);
                hoursOpen = hoursOpen + (toDateTime - dayStartTime).TotalHours;
            }

            // We've already added the hours of the opening day and the close day, so calculate the days excluding those days
            var startFullDay = DateTime.Parse(fromDateTime.AddDays(1).ToString("yyyy MM dd 00:00:00"));
            var endFullDay = DateTime.Parse(toDateTime.AddDays(-1).ToString("yyyy MM dd 23:59:59"));
            var businessDaysOpen = startFullDay.BusinessDaysUntil(endFullDay);
            hoursOpen = Math.Round(hoursOpen + (businessDaysOpen * 24), 0);
            return hoursOpen;
        }
    }
}
