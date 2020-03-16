using System;
using System.Collections.Generic;
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

        public static double BusinessHoursSince(this DateTime fromDate)
        {
            var toDate = DateTime.Now;
            return fromDate.BusinessHoursUntil(toDate);
        }

        public static double BusinessDaysUntil(this DateTime fromDate, DateTime toDate)
        {
            var businessDays = new List<DateTime>();
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (date.IsBusinessDay() == false)
                    continue;

                businessDays.Add(date);
            }

            return businessDays.Count;
        }

        public static double BusinessHoursUntil(this DateTime fromDateTime, DateTime toDateTime)
        {
            var businessHours = new List<DateTime>();
            for (var date = fromDateTime; date <= toDateTime; date = date.AddHours(1))
            {
                if (date.IsBusinessDay() == false)
                    continue;

                businessHours.Add(date);
            }

            return businessHours.Count;
        }

        public static bool IsBusinessDay(this DateTime date)
        {
            return DateSystem.IsPublicHoliday(date, CountryCode.DK) == false && DateSystem.IsWeekend(date, CountryCode.DK) == false;
        }

        /// <summary>
        /// Gets Unix Timestamp from DateTime object
        /// </summary>
        /// <param name="dateTime">The DateTime object</param>
        public static double ToUnixTimestamp(this DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                    new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Gets DateTime object from Unix Timestamp
        /// </summary>
        /// <param name="unixTime">The Unix Timestamp</param>
        public static DateTime FromUnixTime(this double unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}
