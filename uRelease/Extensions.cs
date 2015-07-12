using System;
using System.Linq;
using uRelease.Models;
using Version = System.Version;

namespace uRelease
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
        public static Version AsFullVersion(this string ver)
        {
            return new Version(ver);
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
    }
}