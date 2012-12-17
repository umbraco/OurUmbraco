using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using umbraco.cms.businesslogic.member;

namespace uForum.Library {
    public class Utills {
        private static Regex _tags = new Regex("<[^>]*(>|$)", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex _whitelist = new Regex(@"
            ^</?(a|b(lockquote)?|code|em|h(1|2|3)|i|li|ol|p(re)?|s(ub|up|trong|trike)?|ul)>$
            |^<(b|h)r\s?/?>$
            |^<a[^>]+>$
            |^<img[^>]+/?>$",
            RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace |
            RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>
        /// sanitize any potentially dangerous tags from the provided raw HTML input using 
        /// a whitelist based approach, leaving the "safe" HTML tags
        /// </summary>
        public static string Sanitize(string html) {

            /*
            var tagname = "";
            Match tag;
            var tags = _tags.Matches(html);

            List<ReplacePoint> replacePoints = new List<ReplacePoint>();           


            // iterate through all HTML tags in the input
            for (int i = tags.Count - 1; i > -1; i--) {
                tag = tags[i];
                tagname = tag.Value.ToLower();

                if (!_whitelist.IsMatch(tagname)) {


                    // not on our whitelist? Replace < and > with html entities
                    //html = html.Remove(tag.Index, tag.Length);

                    try
                    {
                        replacePoints.Add(new ReplacePoint(
                        html.IndexOf('<', tag.Index, tag.Length),
                        html.LastIndexOf('>', tag.Index, tag.Length)));
                    }
                    catch { }
                    
                    
                } else if (tagname.StartsWith("<img")) {
                    // detailed <img> tag checking
                    if (!IsMatch(tagname,
                        @"<img\s
              src=""https?://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+""
              (\swidth=""\d{1,3}"")?
              (\sheight=""\d{1,3}"")?
              (\salt=""[^""]*"")?
              (\stitle=""[^""]*"")?
              \s?/?>")) {

                        try
                        {
                         replacePoints.Add(new ReplacePoint(
                        html.IndexOf('<', tag.Index, tag.Length),
                        html.IndexOf('>', tag.Index, tag.Length)));
                        }
                        catch { }
                    }

                   
                }
                else if (tagname.StartsWith("<a") && tagname.Contains("{"))
                {
                        try
                        {
                            replacePoints.Add(new ReplacePoint(
                               html.IndexOf('<', tag.Index, tag.Length),
                               html.IndexOf('>', tag.Index, tag.Length)));
                        }
                        catch { }
                    
                }
            }


            char[] htmlchars = html.ToCharArray();

            foreach (ReplacePoint rp in replacePoints)
            {
                if (rp.open > -1)
                {
                    htmlchars[rp.open] = '°';
                }
              
                 if (rp.close > -1)
                {
                    htmlchars[rp.close] = '³';
                }              
            }


            html = string.Empty;
            foreach (char character in htmlchars)
            {
                html += character;
            }
                        
            html = html.Replace("°", "&lt;");
            html = html.Replace("³", "&gt;");
            */

            html = Regex.Replace(html, "<script.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            html = html.Replace("[code]", "<pre>");
            html = html.Replace("[/code]", "</pre>");

            return CleanInvalidXmlChars(html);
        }


        public static string CleanInvalidXmlChars(string text)
        {
            // From xml spec valid chars:
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]    
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }


        /// <summary>
        /// Utility function to match a regex pattern: case, whitespace, and line insensitive
        /// </summary>
        private static bool IsMatch(string s, string pattern) {
            return Regex.IsMatch(s, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase |
                RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
        }

        public static Member GetMember(int id) {
            Member m = Member.GetMemberFromCache(id);
            if (m == null)
                m = new Member(id);

            return m;
        }

        public static bool IsMember(int id) {
            return (uForum.Businesslogic.Data.SqlHelper.ExecuteScalar<int>("select count(nodeid) from cmsMember where nodeid = '" + id + "'") > 0);
        }
    }
    
    public struct ReplacePoint
    {
        public int open, close;

        public ReplacePoint(int open, int close)
        {
            this.open = open;
            this.close = close;
            
        }
    }
}
