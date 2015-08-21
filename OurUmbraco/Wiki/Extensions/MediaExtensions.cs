using System.Collections.Generic;
using OurUmbraco.Wiki.BusinessLogic;

namespace OurUmbraco.Wiki.Extensions
{
    public static class MediaExtensions
    {
        public static string ToVersionString(this List<UmbracoVersion> versions)
        {
            var verStr = string.Empty;
            foreach (var ver in versions)
            {
                verStr += ver.Version + ", ";
            }

            return verStr.Trim().TrimEnd(',');
        }

        public static string ToVersionNameString(this List<UmbracoVersion> versions)
        {
            var verStr = string.Empty;
            foreach (var ver in versions)
            {
                verStr += ver.Name + ", ";
            }

            return verStr.Trim().TrimEnd(',');
        }
    }
}
