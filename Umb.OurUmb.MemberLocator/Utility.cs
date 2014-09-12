using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;


namespace Umb.OurUmb.MemberLocator
{
    [Umbraco.Core.Macros.XsltExtension("MemberLocator")]
    public class Utility
    {
        /// <summary>
        /// Gets the google maps key.
        /// </summary>
        /// <returns></returns>
        public static string GetGoogleMapsKey()
        {
            try
            {
                XmlDocument config = new XmlDocument();
                config.Load(HttpContext.Current.Server.MapPath("/config/Umb.OurUmb.MemberLocator.config"));

                XmlNode key = config.SelectSingleNode(string.Format("/config/key [contains(@url,'{0}')]", HttpContext.Current.Request.Url.Host));

                return key.Attributes["value"].Value;
            }
            catch {

                throw new Exception("Google maps Key not found for domain " + HttpContext.Current.Request.Url.Host);
            }
        }

        /// <summary>
        /// Gets the number format info. (to avoid problems with the culture)
        /// </summary>
        /// <returns></returns>
        public static System.Globalization.NumberFormatInfo GetNumberFormatInfo()
        {
            System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();
            info.NumberDecimalSeparator = ".";
            info.NumberGroupSeparator = ",";
            return  info;
        }





    }
    
}
