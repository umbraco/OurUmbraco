using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using umbraco.BusinessLogic;
using System.Xml.XPath;
using Umbraco.Core.Models;
using Umbraco.Web;
using Member = umbraco.cms.businesslogic.member.Member;
using MemberGroup = umbraco.cms.businesslogic.member.MemberGroup;

namespace our
{
    [Umbraco.Core.Macros.XsltExtension("our.library")]
    public class Utils
    {
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


        public static string Sanitize(string html)
        {
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

        public static Member GetMember(int id)
        {
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
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int?>("select SUM(downloads) from [wikiFiles] where nodeId = @0", projectId);
            return result ?? 0;
        }


        public static int GetReleaseDownloadCount(int projectId)
        {
            var releaseDownload = UmbracoContext.Current.ContentCache.GetById(projectId);
            var releaseDownloadCorrection = releaseDownload.GetPropertyValue<int>("numberOfDownloads");
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int?>("SELECT COUNT(*) FROM [projectDownload] WHERE projectId = @0", projectId);
            if (result == null)
                result = releaseDownloadCorrection;
            else
                result = result + releaseDownloadCorrection;

            return result.Value;
        }

        public static int GetProjectTotalVotes(int projectId)
        {
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int?>("SELECT sum([points]) FROM [powersProject] where id = @0", projectId);
            return result ?? 0;
        }

        public static int GetProjectMemberVotes(int projectId, params int[] memberIds)
        {
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int?>(
                "SELECT sum([points]) FROM [powersProject] where id = @projectId AND memberId IN (@memberIds)", new { projectId = projectId, memberIds = memberIds });
            return result ?? 0;
        }

        public static IEnumerable<string> GetProjectCompatibleVersions(int projectId)
        {
            return Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.Fetch<string>(
                "SELECT DISTINCT [version] FROM DeliVersionCompatibility WHERE isCompatible = 1 AND projectId = @projectId", new { projectId = projectId });
        }

        /// <summary>
        /// Returns a dictionary of project id => total karma
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetProjectTotalVotes()
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
            return Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.Fetch<dynamic>("select nodeId as projectId, SUM(downloads) as total from wikiFiles GROUP BY nodeId")
                .ToDictionary(x => (int)x.projectId, x => (int)x.total);
        }

        /// <summary>
        /// Returns a dictionary of project id => any version that has been flagged as being compatible
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, IEnumerable<string>> GetProjectCompatibleVersions()
        {
            var compatVersions = new Dictionary<int, IEnumerable<string>>();
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.Fetch<dynamic>("SELECT DISTINCT projectId, [version] FROM DeliVersionCompatibility WHERE isCompatible = 1");
            foreach (var project in result.GroupBy(x => x.projectId))
            {
                compatVersions.Add(project.Key, project.Select(x => (string)x.version).ToArray());
            }
            return compatVersions;
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

            foreach (umbraco.interfaces.ITag tag in umbraco.editorControls.tags.library.GetTagsFromGroupAsITags(group))
            {
                output += "\"" + tag.TagCaption + "\",";
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

        public static string GetMemberAvatar(IPublishedContent member, int avatarSize)
        {
            var hasAvatar = member.HasValue("avatar");
            if (hasAvatar)
            {
                var avatarPath = member.GetPropertyValue("avatar").ToString();
                var path = HostingEnvironment.MapPath(avatarPath);
                if (System.IO.File.Exists(path))
                    return GetLocalAvatar(member.GetPropertyValue("avatar").ToString(), avatarSize, member.Name);
            } 
            
            return GetGravatar(member.GetPropertyValue("Email").ToString(), avatarSize, member.Name);
        }

        public static string GetGravatar(string email, int size, string memberName)
        {
            var emailId = email.ToLower();
            var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(emailId, "MD5").ToLower();

            return string.Format("<img src=\"//www.gravatar.com/avatar/{0}?s={1}&d=mm&r=g&d=retro\" alt=\"{2}\" />", hash, size, memberName);
        }

        public static string GetLocalAvatar(string imgPath, int size, string memberName)
        {
            return string.Format("<img src=\"{0}?width={1}&height={1}&mode=crop\" alt=\"{2}\" />", imgPath, size, memberName);
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
