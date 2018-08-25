using System;

namespace OurUmbraco.Our.Extensions
{
    public static class DateTimeExtensions
    {
        public static string AsDisplayedReleaseDate(this DateTime releaseDate)
        {
            return releaseDate == default(DateTime) ? "Not yet determined" : releaseDate.ToString("dddd, MMMM d yyyy");
        }
    }
}
