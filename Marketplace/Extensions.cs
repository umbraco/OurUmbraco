using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace uProject
{
    public static class Extensions
    {
        public static string StripHtmlAndLimit(this String str, int chars)
        {
            str = umbraco.library.StripHtml(str);

            if (str.Length > chars)
                str = str.Substring(0, chars);

            return str;

        }
    }
}