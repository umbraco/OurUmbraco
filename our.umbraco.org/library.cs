using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using umbraco.cms.businesslogic.member;
using umbraco.BusinessLogic;
using System.Xml.XPath;

namespace our {
    [Umbraco.Core.Macros.XsltExtension("our.library")]
    public class Utils {
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
        /// 


        public static string Sanitize(string html) {
           string re = Regex.Replace(html, "<script.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
           re = CleanInvalidXmlChars(re);

           return re;
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

        public static bool IsAdmin(int id)
        {
            Member m = new Member(id);
            return m.Groups.ContainsKey(MemberGroup.GetByName("admin").Id);
        }

        public static bool IsHq(int id)
        {
            Member m = new Member(id);
            return m.Groups.ContainsKey(MemberGroup.GetByName("HQ").Id);
        }

        public static int GetProjectTotalDownloadCount(int projectId)
        {
            try
            {
                return Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>("select count(*) from projectDownload where projectId = @0", projectId);
            }
            catch
            {
                return 0;
            }
        }

        public static int GetProjectTotalKarma(int projectId)
        {
            try
            {
                return Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>("SELECT sum([points]) FROM [powersProject] where id = @0", projectId);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns a dictionary of project id => total karma
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetProjectTotalKarma()
        {
            return Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.Fetch<dynamic>("SELECT id, sum([points]) as points FROM [powersProject] GROUP BY id")
                .ToDictionary(x => (int)x.id, x => (int)x.points);
        }

        /// <summary>
        /// Returns a dictionary of project id => total downloads
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetProjectTotalDownload()
        {
            return Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.Fetch<dynamic>("select projectId, count(*) as total from projectDownload GROUP BY projectId")
                .ToDictionary(x => (int)x.projectId, x => (int)x.total);
        }

        public static int GetProjectFileDownloadCount(int fileId)
        {
            try
            {
                return Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>("Select downloads from wikiFiles where id = @0", fileId);
            }
            catch
            {
                return 0;
            }
        }

        public static string GetAllTags(string group)
        {

            string output = "[";

            foreach(umbraco.interfaces.ITag tag in umbraco.editorControls.tags.library.GetTagsFromGroupAsITags(group))
            {
                output += "\"" + tag.TagCaption +"\",";
            }

            output = output.Substring(0, output.Length - 1);
            output += "]";

            return output;
        }

        public static string StripHTML(string inputString)
        {
            return Regex.Replace
              (inputString, "<.*?>", string.Empty);
        }

        public static List<int> GetProjectContributors(int projectId)
        {
            

            List<int> projects = new List<int>();

            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader("SELECT * FROM projectContributors WHERE projectId = " + projectId);

            while (dr.Read())
            {
                projects.Add(dr.GetInt("memberId"));
            }
            return projects;
        }

        public static bool IsProjectContributor(int memberId, int projectId)
        {
            return ((Data.SqlHelper.ExecuteScalar<int>("SELECT 1 FROM projectContributors WHERE projectId = @projectId and memberId = @memberId;",
                Data.SqlHelper.CreateParameter("@projectId", projectId),
                Data.SqlHelper.CreateParameter("@memberId", memberId)) > 0));


        }

        /*
        public static XPathNodeIterator ProjectsContributing(int memberId)
        {
            return uForum.Businesslogic.Data.GetDataSet
               ("SELECT * FROM projectContributors WHERE memberId = " + memberId, "projects");
        }

        public static XPathNodeIterator ProjectContributors(int projectId)
        {
            return uForum.Businesslogic.Data.GetDataSet
                ("SELECT * FROM projectContributors WHERE projectId = " + projectId, "contributors");
        }*/
    }

    public struct ReplacePoint {
        public int open, close;

        public ReplacePoint(int open, int close) {
            this.open = open;
            this.close = close;

        }
    }
}
