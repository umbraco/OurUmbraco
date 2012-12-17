using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using umbraco.cms.businesslogic.member;
using System.Xml;

namespace uWiki.Library {
    public class Utills {
        private static Regex _tags = new Regex("<[^>]*(>|$)", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex _whitelist = new Regex(@"
            ^</?(a|b(lockquote)?|code|em|h(1|2|3)|i|li|ol|p(re)?|s(ub|up|trong|trike)?|table|tr|th|td|ul)>$
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

            var tagname = "";
            Match tag;
            var tags = _tags.Matches(html);

            // iterate through all HTML tags in the input
            for (int i = tags.Count - 1; i > -1; i--) {
                tag = tags[i];
                tagname = tag.Value.ToLower();

                if (!_whitelist.IsMatch(tagname)) {
                    // not on our whitelist? I SAY GOOD DAY TO YOU, SIR. GOOD DAY!
                    html = html.Remove(tag.Index, tag.Length);
                } else if (tagname.StartsWith("<a")) {
                    // detailed <a> tag checking
                    if (!IsMatch(tagname,
                        @"<a\s
                  href=""(\#\d+|(https?|ftp)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+)""
                  (\stitle=""[^""]+"")?\s?>")) {
                        html = html.Remove(tag.Index, tag.Length);
                    }
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
                        html = html.Remove(tag.Index, tag.Length);
                    }
                }

            }

            return html;
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

        public static bool IsInGroup(string GroupName)
        {

            if (umbraco.library.IsLoggedOn())
                return IsMemberInGroup(GroupName, Member.CurrentMemberId());
            else
                return false;
        }

        public static bool IsMemberInGroup(string GroupName, int memberid)
        {
            Member m = Utills.GetMember(memberid);

            foreach (MemberGroup mg in m.Groups.Values)
            {
                if (mg.Text == GroupName)
                    return true;
            }
            return false;
        }

        public static int GetWikiHelpFallBackPage(string section)
        {
            try
            {
                XmlDocument config = new XmlDocument();
                config.Load(
                    HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../config/UmbracoHelp.config"));

                int p = 0;

                if (config.SelectNodes(string.Format("//section [@alias = '{0}']", section)).Count > 0)
                    int.TryParse(config.SelectSingleNode(string.Format("//section [@alias = '{0}']", section)).Attributes["node"].Value, out p);
                else
                    int.TryParse(config.SelectSingleNode("//section [@alias = '*']").Attributes["node"].Value, out p);

                return p;
            }
            catch (Exception e)
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, 0, e.ToString());
                return 0;
            }

        }
    }


}
