using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace uForum
{
    public static class MemberForumExtensions
    {

        /* Core member */
        public static int Karma(this IPublishedContent member)
        {
            return (int)member.GetProperty("reputationCurrent").Value;
        }

        public static int ForumPosts(this IPublishedContent member)
        {
            return (int)member.GetProperty("forumPosts").Value;
        }

        public static string Avatar(this IPublishedContent member)
        {
            return "/media/avatar/" + member.Id.ToString() + ".jpg";
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
            var posts = member.ForumPosts()+1;
            member.SetValue("forumPosts", posts);
        }

        public static void IncreaseKarma(this IMember member, int increase = 1)
        {
            var posts = member.Karma() + 1;
            member.SetValue("reputationCurrent", posts);
        }

        public static string Avatar(this IMember member)
        {
            return "/media/avatar/" + member.Id.ToString() + ".jpg";
        }

        private const string ModeratorRoles = "admin,HQ,Core,MVP";
        public static bool IsAdmin(this Umbraco.Web.Security.MembershipHelper helper)
        {
            return ModeratorRoles.Split(',').Any(x => Roles.IsUserInRole(x));
        }

        /** GET ACCESS TO AUTHOR */
        public static IPublishedContent Author(this uForum.Models.Comment comment)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(Umbraco.Web.UmbracoContext.Current);
            return memberShipHelper.GetById(comment.MemberId);
        }

        public static IPublishedContent Author(this uForum.Models.Topic topic)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(Umbraco.Web.UmbracoContext.Current);
            return memberShipHelper.GetById(topic.MemberId);
        }

        public static IPublishedContent LastActiveMember(this uForum.Models.Topic topic)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(Umbraco.Web.UmbracoContext.Current);
            if (topic.LatestReplyAuthor > 0)
                return memberShipHelper.GetById(topic.LatestReplyAuthor);
            else
                return topic.Author();
        }


        /* ACCCESS TO AUTHOR AS IMEMBER */

        public static IMember AuthorAsMember(this uForum.Models.Topic topic)
        {
            return Umbraco.Web.UmbracoContext.Current.Application.Services.MemberService.GetById(topic.MemberId);
        }

        public static IMember AuthorAsMember(this uForum.Models.Comment comment)
        {
            return Umbraco.Web.UmbracoContext.Current.Application.Services.MemberService.GetById(comment.MemberId);
        }

    }
}
