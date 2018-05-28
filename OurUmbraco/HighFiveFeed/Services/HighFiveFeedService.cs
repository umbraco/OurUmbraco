using OurUmbraco.Community.People;
using OurUmbraco.HighFiveFeed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using OurUmbraco.Community.People.Models;
using OurUmbraco.Forum.Services;
using Umbraco.Web.Mvc;
using OurUmbraco.HighFiveFeed.Services;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

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

        public HighFiveFeedResponse GetHighFiveFeed()
        {
            var Members = ApplicationContext.Current.Services.MemberService;
            var dbContext = ApplicationContext.Current.DatabaseContext;
            var sql = new Sql().Select("*").From("highFivePosts");
            var result = dbContext.Database.Fetch<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>(sql).OrderByDescending(x => x.CreatedDate);
            var avatarService = new AvatarService();
            var response = new HighFiveFeedResponse();

            foreach (var dbEntry in result.Take(10))
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
                    Url = dbEntry.Link,
                    Type = type,
                    From = fromMember.Name,
                    FromAvatarUrl = fromAvatar,
                    To = toMember.Name,
                    ToAvatarUrl = toAvatar,
                    Id = dbEntry.Id,
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
