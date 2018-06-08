using System;
using System.Web.Hosting;
using OurUmbraco.Community.People;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Forum.Services;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Models
{
    public class OurUmbracoTemplatePage : UmbracoTemplatePage
    {
        public MemberData MemberData;

        public OurUmbracoTemplatePage()
        {
            MemberData = GetMemberData();
        }

        public override void Execute()
        {
        }
        
        private static MemberData GetMemberData()
        {
            var membershipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);

            if (membershipHelper.GetCurrentLoginStatus().IsLoggedIn == false)
                return null;

            var userName = membershipHelper.CurrentUserName;

            var memberData = ApplicationContext.Current.ApplicationCache.RuntimeCache
                .GetCacheItem<MemberData>("MemberData" + userName, () =>
                {
                    var member = membershipHelper.GetCurrentMember();

                    var avatarService = new AvatarService();
                    var memberAvatarPath = avatarService.GetMemberAvatar(member);
                    memberAvatarPath = HostingEnvironment.MapPath($"~{memberAvatarPath}");
                    var avatarImage = avatarService.GetMemberAvatarImage(memberAvatarPath);

                    var roles = member.GetRoles();

                    var topicService = new TopicService(ApplicationContext.Current.DatabaseContext);
                    var latestTopics = topicService.GetLatestTopicsForMember(member.Id, maxCount: 100);

                    var newTosAccepted = true;
                    var tosAccepted = member.GetPropertyValue<DateTime>("tos");
                    var newTosDate = new DateTime(2018, 03, 26);
                    if ((newTosDate - tosAccepted).TotalDays > 1)
                        newTosAccepted = false;

                    var avatarPath = avatarService.GetMemberAvatar(member);
                    var avatarHtml = avatarService.GetImgWithSrcSet(avatarPath, member.Name, 100);

                    var data = new MemberData
                    {
                        Member = member,
                        AvatarImage = avatarImage,
                        AvatarImageTooSmall = avatarImage != null && (avatarImage.Width < 400 || avatarImage.Height < 400),
                        Roles = roles,
                        LatestTopics = latestTopics,
                        AvatarHtml = avatarHtml,
                        AvatarPath = avatarPath,
                        NumberOfForumPosts = member.ForumPosts(),
                        Karma = member.Karma(),
                        TwitterHandle = member.GetPropertyValue<string>("twitter").Replace("@", string.Empty),
                        GitHubUsername = member.GetPropertyValue<string>("github"),
                        IsAdmin = member.IsAdmin(),
                        Email = member.GetPropertyValue<string>("Email"),
                        IsBlocked = member.GetPropertyValue<bool>("blocked"),
                        NewTosAccepted = newTosAccepted
                    };

                    return data;
                }, TimeSpan.FromMinutes(5));

            return memberData;
        }
    }
}
