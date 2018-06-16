using OurUmbraco.Community.People;
using OurUmbraco.HighFiveFeed.Models;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace OurUmbraco.HighFiveFeed.Services
{
    public class HighFiveFeedService
    {
        public List<HighFiveCategory> Categories;
        public HighFiveFeedService()
        {
            Categories = new List<HighFiveCategory>();
            Categories.Add(new HighFiveCategory(1, "A Package"));
            Categories.Add(new HighFiveCategory(2, "A Talk"));
            Categories.Add(new HighFiveCategory(3, "A Blog Post"));
            Categories.Add(new HighFiveCategory(4, "A Meetup"));
            Categories.Add(new HighFiveCategory(5, "A Skrift Article"));
            Categories.Add(new HighFiveCategory(6, "A Tutorial"));
            Categories.Add(new HighFiveCategory(7, "Advice"));
            Categories.Add(new HighFiveCategory(8, "A Video"));
            Categories.Add(new HighFiveCategory(9, "A PR"));
        }

        public HighFiveFeedResponse GetHighFiveFeedForMember(int memberId)
        {
            var Members = ApplicationContext.Current.Services.MemberService;
            var dbContext = ApplicationContext.Current.DatabaseContext;
            var sql = new Sql().Select("*").From("highFivePosts").Where<Models.HighFiveFeed>(x=>x.ToMemberId== memberId);
            sql.OrderByDescending<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>(x => x.CreatedDate);
            var result = dbContext.Database.Page<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>(1, 5, sql);
            var avatarService = new AvatarService();
            var response = new HighFiveFeedResponse();
            foreach (var dbEntry in result.Items)
            {
                var toAvatar = "";
                var fromAvatar = "";
                var toMember = Members.GetById(dbEntry.ToMemberId);
                var fromMember = Members.GetById(dbEntry.FromMemberId);
                if (toMember != null)
                {
                    toAvatar = avatarService.GetMemberAvatar(toMember);
                }
                if (fromMember != null)
                {
                    fromAvatar = avatarService.GetMemberAvatar(fromMember);
                }
                var type = GetActionType(dbEntry.ActionId);
                var highFive = new HighFiveResponse()
                {
                    Category = type,
                    From = fromMember.Name,
                    FromAvatarUrl = fromAvatar,
                    To = toMember.Name,
                    ToAvatarUrl = toAvatar,
                    Id = dbEntry.Id,
                    LinkUrl = dbEntry.Link,
                    LinkTitle = string.IsNullOrWhiteSpace(dbEntry.LinkTitle) ? dbEntry.Link : dbEntry.LinkTitle,
                    CreatedDate = dbEntry.CreatedDate


                };
                response.HighFives.Add(highFive);
            }
            return response;
        }

        public HighFiveFeedResponse GetHighFiveFeed(int page = 1, int numberOfItems = 10)
        {
          

            var Members = ApplicationContext.Current.Services.MemberService;
            var dbContext = ApplicationContext.Current.DatabaseContext;
            var sql = new Sql().Select("*").From<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>();
            sql.OrderByDescending<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>(x => x.CreatedDate);

            var result = dbContext.Database.Page<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>(page, numberOfItems, sql);
            var avatarService = new AvatarService();
            var response = new HighFiveFeedResponse();

            foreach (var dbEntry in result.Items)
            {
                var toAvatar = "";
                var fromAvatar = "";
                var toMember = Members.GetById(dbEntry.ToMemberId);
                var fromMember = Members.GetById(dbEntry.FromMemberId);
                if (toMember != null)
                {
                    toAvatar = avatarService.GetMemberAvatar(toMember);
                }
                if (fromMember != null)
                {
                    fromAvatar = avatarService.GetMemberAvatar(fromMember);
                }
                var type = GetActionType(dbEntry.ActionId);
                var highFive = new HighFiveResponse()
                {
                    Category = type,
                    From = fromMember.Name,
                    FromAvatarUrl = fromAvatar,
                    To = toMember.Name,
                    ToAvatarUrl = toAvatar,
                    Id = dbEntry.Id,
                    LinkUrl = dbEntry.Link,
                    LinkTitle = string.IsNullOrWhiteSpace(dbEntry.LinkTitle) ? dbEntry.Link : dbEntry.LinkTitle,
                    CreatedDate = dbEntry.CreatedDate


                };
                response.HighFives.Add(highFive);
            }
            return response;
        }

        private string GetActionType(int actionId)
        {
            return Categories.Where(x => x.Id == actionId).FirstOrDefault().CategoryText;
        }

        public List<HighFiveCategory> GetCategories()
        {
            return Categories;

        }
    }
}
