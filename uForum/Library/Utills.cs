using System;
using System.Text.RegularExpressions;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;

namespace uForum.Library
{
    public class Utills
    {
        /// <summary>
        /// sanitize any potentially dangerous tags from the provided raw HTML input using 
        /// a whitelist based approach, leaving the "safe" HTML tags
        /// </summary>
        public static string Sanitize(string html)
        {

          
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
            const string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }
        
        public static Member GetMember(int id)
        {
            try
            {
                return Member.GetMemberFromCache(id) ?? new Member(id);
            }
            catch (Exception exception)
            {
                Log.Add(LogTypes.Error, 0, string.Format("Could not get member {0} from the cache nor from the database - Exception: {1}", id, exception.InnerException));
            }

            return null;
        }

        public static bool IsMember(int id)
        {
            return (Businesslogic.Data.SqlHelper.ExecuteScalar<int>("select count(nodeid) from cmsMember where nodeid = '" + id + "'") > 0);
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
