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
        
        public static string BuildExamineString(this string term, int boost, string field, bool andSearch)
        {
            term = Lucene.Net.QueryParsers.QueryParser.Escape(term);
            var terms = term.Trim().Split(' ');
            var qs = field + ":";
            qs += "\"" + term + "\"^" + (boost + 30000).ToString() + " ";
            qs += field + ":(+" + term.Replace(" ", " +") + ")^" + (boost + 5).ToString() + " ";
            if (!andSearch)
            {
                qs += field + ":(" + term + ")^" + boost.ToString() + " ";
            } return qs;
        }

    }
}