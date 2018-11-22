using System.Linq;
using System.Web.Security;
using OurUmbraco.Community.People;
using OurUmbraco.MarketPlace.Extensions;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;

namespace OurUmbraco.Forum.Extensions
{
    public static class MemberForumExtensions
    {

        /* Core member */
        public static int Karma(this IPublishedContent member)
        {
            if (member == null || member.HasProperty("reputationCurrent") == false)
                return 0;

            return member.GetPropertyValue<int>("reputationCurrent");
        }

        public static int ForumPosts(this IPublishedContent member)
        {
            return member.GetPropertyValue<int>("forumPosts");
        }

        public static string Avatar(this IPublishedContent member)
        {
            var avatarService = new AvatarService();
            return avatarService.GetMemberAvatar(member);
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

        public static bool IsHq(this MembershipHelper helper)
        {
            return Roles.IsUserInRole("HQ");
        }

        public static bool IsTeamUmbraco(this MembershipHelper helper)
        {
            return Roles.IsUserInRole("HQ") || Roles.IsUserInRole("TeamUmbraco");
        }

        public static bool IsAdmin(this IPublishedContent helper)
        {
            return ModeratorRoles.Split(',').Any(Roles.IsUserInRole);
        }

        public static bool IsHq(this IPublishedContent helper)
        {
            return Roles.IsUserInRole("HQ");
        }

        public static bool IsSpam(this IPublishedContent member)
        {
            if (member.GetPropertyValue<bool>("blocked"))
                return true;

            // Members with over 71 karma are trusted automatically
            if (member.Karma() >= 71)
                return false;

            var typedMember = (MemberPublishedContent) member;
            if (typedMember == null)
                // Member is not logged in, don't bother
                return false;

            var roles = Roles.GetRolesForUser(typedMember.UserName);
            var isSpam = roles.Contains("potentialspam") || roles.Contains("newaccount");
            return isSpam;
        }
    }
}
