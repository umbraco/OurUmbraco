using System;
using System.Linq;
using OurUmbraco.Release.Models;

namespace OurUmbraco.Release
{
    public static class Extensions
    {
        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is millisecods past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }
        
        /// <summary>
        /// Return the version number as a int to deal with minor versions with single digits
        /// </summary>
        /// <param name="ver"></param>
        /// <returns></returns>
        public static System.Version AsFullVersion(this string ver)
        {
            return new System.Version(ver);
        }
        
        public static string GetFieldValue(this YouTrackIssue issue, string fieldName)
        {
            var findField = issue.Field.FirstOrDefault(x => x.Name == fieldName);
            return findField != null ? findField.Value.ToString().Replace("[", string.Empty).Replace("]", string.Empty).Replace("\"", string.Empty).Trim() : string.Empty;
        }

        public static DateTime ConvertFromUnixDate(this long date)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(date);
        }

        public static string StateIcon(this IssueView issue)
        {
            switch (issue.state.ToLowerInvariant().Replace(" ", "").Replace("'", ""))
            {
                case "submitted":
                    return "icon-Checkbox-dotted";
                case "open":
                    return "icon-Checkbox-empty";
                case "review":
                    return "icon-Paper-plane-alt";
                case "inprogress":
                    return "icon-Paper-plane-alt";
                case "fixed":
                    return "icon-Check";
                case "duplicate":
                    return "icon-Multiple-windows";
                case "cantreproduce":
                    return "icon-Enter";
                case "obsolete":
                    return "icon-Scull";
                case "closed":
                    return "icon-Stop-alt";
                case "reopened":
                    return "icon-Undo";
                case "workaroundposted":
                    return "icon-Redo";
                case "resolved":
                    return "icon-Check";
                default:
                    return "";
            }
        }
    }
}