using System.Linq;
using System.Web.Security;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace uForum
{
    public static class MemberForumExtensions
    {

        /* Core member */
        public static int Karma(this IPublishedContent member)
        {
            return member.GetPropertyValue<int>("reputationCurrent");
        }

        public static int ForumPosts(this IPublishedContent member)
        {
            return member.GetPropertyValue<int>("forumPosts");
        }

        public static string Avatar(this IPublishedContent member)
        {
            return member.GetPropertyValue<string>("avatar");
        }
        
        public static int Karma(this IMember member)
        {
            return member.GetValue<int>("reputationCurrent");
        }

        public static int ForumPosts(this IMember member)
        {
            return member.GetValue<int>("forumPosts");
        }

        public static void IncreaseForumPostCount(this IMember member, int increase = 1)
        {
            var posts = member.ForumPosts() + increase;
            member.SetValue("forumPosts", posts);
        }

        public static void IncreaseKarma(this IMember member, int increase = 1)
        {
            var posts = member.Karma() + increase;
            member.SetValue("reputationCurrent", posts);
        }

        public static string Avatar(this IMember member)
        {
            return string.Format("/media/avatar/{0}.jpg", member.Id);
        }

        private const string ModeratorRoles = "admin,HQ,Core,MVP";
        public static bool IsAdmin(this MembershipHelper helper)
        {
            return ModeratorRoles.Split(',').Any(Roles.IsUserInRole);
        }
    }
}
