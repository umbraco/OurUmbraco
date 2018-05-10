using System;
using System.Globalization;
using System.Xml.Linq;
using Skybrud.Essentials.Xml.Extensions;

namespace OurUmbraco.Community.BlogPosts {
    
    public class BlogRssUtils {

        public static DateTimeOffset ParseRfc822DateTime(string str) {

            // Trim and remove double whitespace, new lines etc.
            str = String.Join(" ", (str ?? "").Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)).Trim();

            // .NET doesn't know how to parse "UT", so we need to convert it to an offset instead
            if (str.EndsWith(" UT")) str = str.Substring(0, str.Length - 2) + "+0000";
            
            // Parse a valid RFC 822 formatted date
            return DateTimeOffset.ParseExact(str, "ddd, dd MMM yyyy HH:mm:ss K", CultureInfo.InvariantCulture).ToLocalTime();

        }
        public static string GetFileExtension(string blogLogoUrl)
        {
            var extension = ".png";
            var url = blogLogoUrl;
            if (url.Contains("?"))
                url = blogLogoUrl.Substring(0, blogLogoUrl.IndexOf("?", StringComparison.Ordinal));

            var lastUrlSlug = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal));
            if (lastUrlSlug.Contains("."))
                extension = lastUrlSlug.Substring(lastUrlSlug.LastIndexOf(".", StringComparison.Ordinal));
            return extension;
        }

        public static string RemoveLeadingCharacters(string inString)
        {
            if (inString == null)
                return null;

            var substringStart = 0;
            for (var i = substringStart; i < inString.Length; i++)
            {
                var character = inString[i];
                if (character != '<')
                    continue;

                // As soon as we find the XML opening tag, break and return the rest of the string, else XElement.Parse fails
                substringStart = i;
                break;
            }

            return inString.Substring(substringStart);
        }

        public static DateTimeOffset GetPublishDate(XElement item)
        {
            var publishDate = item.GetElementValue("pubDate");
            DateTimeOffset pubDate;
            try
            {
                pubDate = Skybrud.Essentials.Time.TimeUtils.Rfc822ToDateTimeOffset(publishDate);
            }
            catch (Exception e)
            {
                // special dateformat, try normal C# date parser
            }

            DateTimeOffset.TryParse(publishDate, out pubDate);
            return pubDate;
        }

    }

}