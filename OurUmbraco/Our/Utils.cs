using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using umbraco.BusinessLogic;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Member = umbraco.cms.businesslogic.member.Member;
using MemberGroup = umbraco.cms.businesslogic.member.MemberGroup;

namespace OurUmbraco.Our
{
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

        public static bool HasMemberDownloadedPackage(int memberId, int projectId)
        {
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>("select COUNT(*) from projectDownload where projectId = @0 and memberId = @1", projectId, memberId);
            return result != 0;
        }


        public static int GetReleaseDownloadCount(int projectId)
        {
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int?>("SELECT COUNT(*) FROM [projectDownload] WHERE projectId = @0", projectId);
            return result ?? 0;
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

        public static IEnumerable<int> GetProjectsMemberHasVotedUp(int memberId)
        {
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                "SELECT DISTINCT id FROM [powersProject] where memberId = @memberId", new { memberId = memberId });
            return result;
        }

        public static IEnumerable<int> GetProjectsMemberHasDownloaded(int memberId)
        {
            var result = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                "SELECT DISTINCT projectId FROM [projectDownload] where memberId = @memberId", new { memberId = memberId });
            return result;
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
            var projects = new List<int>();
            using (var sqlHelper = Application.SqlHelper)
            using (var recordsReader = sqlHelper.ExecuteReader("SELECT * FROM projectContributors WHERE projectId = @projectId ", sqlHelper.CreateParameter("@projectId", projectId)))
            {
                while (recordsReader.Read())
                    projects.Add(recordsReader.GetInt("memberId"));

                return projects;
            }
        }

        public static bool IsProjectContributor(int memberId, int projectId)
        {
            using (var sqlHelper = Application.SqlHelper)
            {
                return sqlHelper.ExecuteScalar<int>(
                           "SELECT 1 FROM projectContributors WHERE projectId = @projectId and memberId = @memberId;",
                           sqlHelper.CreateParameter("@projectId", projectId),
                           sqlHelper.CreateParameter("@memberId", memberId)) > 0;
            }
        }

        public static string GetMemberAvatar(IPublishedContent member, int avatarSize, bool getRawUrl = false)
        {
            var memberAvatarPath = MemberAvatarPath(member);
            if (string.IsNullOrWhiteSpace(memberAvatarPath) == false)
            {
                return MemberAvatarPath(member) == string.Empty
                    ? GetGravatar(member.GetPropertyValue("Email").ToString(), avatarSize, member.Name, getRawUrl)
                    : GetLocalAvatar(member.GetPropertyValue("avatar").ToString(), avatarSize, member.Name, getRawUrl);
            }

            return GetGravatar(member.GetPropertyValue("Email").ToString(), avatarSize, member.Name, getRawUrl);
        }

        private static string MemberAvatarPath(IPublishedContent member)
        {
            try
            {
                var hasAvatar = member.HasValue("avatar");
                if (hasAvatar)
                {
                    var avatarPath = member.GetPropertyValue("avatar").ToString();
                    if (avatarPath.StartsWith("http://") || avatarPath.StartsWith("https://"))
                        return avatarPath;

                    var path = HostingEnvironment.MapPath(avatarPath);
                    if (System.IO.File.Exists(path))
                        return path;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Utils>("Could not get MemberAvatarPath", ex);
            }

            return string.Empty;
        }

        private static Random rnd = new Random();

        public static string GetScreenshotPath(string screenshot)
        {
            if (HttpContext.Current.IsDebuggingEnabled == false)
                return screenshot;

            //NOTE: I guess the below is all to do with testing - even the random images i guess

            try
            {
                if (string.IsNullOrWhiteSpace(screenshot) == false)
                {
                    var path = HostingEnvironment.MapPath(screenshot);
                    if (System.IO.File.Exists(path))
                    {
                        return screenshot;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Utils>("Could not get Screenshot", ex);
            }

            return "http://lorempixel.com/600/600/?" + rnd.Next();
        }

        public static Image GetMemberAvatarImage(IPublishedContent member)
        {
            var memberAvatarPath = MemberAvatarPath(member);
            if (string.IsNullOrWhiteSpace(memberAvatarPath) == false)
            {
                try
                {
                    return Image.FromFile(memberAvatarPath);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<Utils>(string.Format("Could not create Image object from {0}", memberAvatarPath), ex);
                }
            }

            return null;
        }

        public static string GetGravatar(string email, int size, string memberName, bool getRawUrl = false)
        {
            var emailId = email.ToLower();
            var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(emailId, "MD5").ToLower();

            var url = string.Format("https://www.gravatar.com/avatar/{0}?s={1}&d=mm&r=g&d=retro", hash, size);

            return !getRawUrl
                ? string.Format("<img src=\"{0}\" alt=\"{1}\" />", url, memberName)
                : url;
        }

        public static string GetLocalAvatar(string imgPath, int minSize, string memberName, bool getRawUrl = false)
        {
            var url = string.Format("{0}?width={1}&height={1}&mode=crop", imgPath.Replace(" ", "%20"), minSize);

            return !getRawUrl
                ? string.Format("<img src=\"{0}?width={1}&height={1}&mode=crop\" srcset=\"{0}?width={2}&height={2}&mode=crop 2x, {0}?width={3}&height={3}&mode=crop 3x\" alt=\"{4}\" />", imgPath.Replace(" ", "%20"), minSize, (minSize * 2), (minSize * 3), memberName)
                : url;
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
