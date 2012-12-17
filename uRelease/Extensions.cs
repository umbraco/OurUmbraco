using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public static int AsFullVersion(this string ver)
        {
            var verArray = ver.Split('.');
            var fullVersion = verArray[0] + ((verArray[1].Length <= 1) ? "0" : "") + verArray[1] + ((verArray[2].Length <= 1) ? "0" : "") + verArray[2];
            return Int32.Parse(fullVersion);
        }
    }
}