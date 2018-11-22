using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Skybrud.Essentials.Strings.Extensions;
using Skybrud.Essentials.Time;

namespace OurUmbraco.Our.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static HtmlString RenderTimestamp<T>(this HtmlHelper<T> helper, EssentialsDateTime timestamp, bool multiline = false, bool showDiff = true)
        {
            if (timestamp == null || timestamp.IsZero)
                return new HtmlString("<em>N/A</em>");

            var sb = new StringBuilder();
            sb.Append(timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));

            if (!showDiff)
                return new HtmlString(sb + "");

            sb.AppendLine(multiline ? "<br />" : "");
            sb.AppendLine("<small class=\"comment\">(" + GetTimeDiff(timestamp) + ")</small>");

            return new HtmlString(sb + "");
        }

        public static int GetTimeDiffInHours(this EssentialsDateTime timestamp)
        {
            if (timestamp == null || timestamp.IsZero)
                return 0;

            var seconds = EssentialsDateTime.CurrentUnixTimestamp - timestamp.UnixTimestamp;
            var hours = (long)Math.Floor(seconds / 60.0 / 60.0);
            return (int) hours;
        }

        public static string GetTimeDiff(EssentialsDateTime dt)
        {
            return dt == null ? "Never" : GetTimeDiff(dt.UnixTimestamp);
        }

        public static string GetTimeDiff(long timestamp)
        {
            if (timestamp == 0)
                return "Never";

            var temp = new List<string>();

            var seconds = EssentialsDateTime.CurrentUnixTimestamp - timestamp;
            if (seconds < 60)
                return "Now";

            if (seconds < 86400)
            {
                var hours = (long)Math.Floor(seconds / 60.0 / 60.0);
                seconds = seconds - (hours * 60 * 60);

                var minutes = (long)Math.Round(seconds / 60.0);
                
                if (hours == 1)
                {
                    temp.Add("one hour");
                }
                else if (hours > 1)
                {
                    temp.Add(hours + " hours");
                }

                if (minutes == 1)
                {
                    temp.Add("one minute");
                }
                else if (minutes > 1)
                {
                    temp.Add(minutes + " minutes");
                }

                return "~" + string.Join(" and ", temp).FirstCharToUpper() + " ago";
            }

            var days = (long)Math.Round(seconds / 60.0 / 60.0 / 24.0);

            var years = (long)Math.Floor(days / 365.00);
            days = (long)Math.Floor(days - (years * 365.00));

            if (years == 1)
            {
                temp.Add("one year");
            }
            else if (years > 1)
            {
                temp.Add(years + " years");
            }

            if (days == 1)
            {
                temp.Add("one day");
            }
            else if (days > 1)
            {
                temp.Add(days + " days");
            }

            return "~" + string.Join(" and ", temp) + " ago";
        }
    }
}
