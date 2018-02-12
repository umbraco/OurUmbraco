using System;
using System.Globalization;

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

    }

}