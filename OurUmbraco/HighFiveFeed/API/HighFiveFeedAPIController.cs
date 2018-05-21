using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.WebApi;
using Umbraco.Core;
using OurUmbraco.HighFiveFeed.Models;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.Persistence;
using OurUmbraco.Community.People.Models;
using OurUmbraco.Community.People;
using OurUmbraco.Forum.Services;
using Umbraco.Web.Mvc;

namespace OurUmbraco.HighFiveFeed.API
{
    public class HighFiveFeedAPIController : UmbracoApiController
    {
        public List<HighFiveCategory> Categories;
        public HighFiveFeedAPIController()
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

        //this will be for authorized members only
        [HttpPost]
        public void SubmitHighFive(int toUserId, int action, String url)
        {

            var memberService = ApplicationContext.Current.Services.MemberService;

            var currentMember = Members.GetCurrentMember();
            if (currentMember != null && currentMember.Id != 0)
            {
                var fromUserId = currentMember.Id;

                var dbContext = ApplicationContext.Current.DatabaseContext;
                var highFive = new OurUmbraco.HighFiveFeed.Models.HighFiveFeed();
                highFive.FromMemberId = fromUserId;
                highFive.ToMemberId = toUserId;
                highFive.ActionId = action;
                highFive.Link = url;
                highFive.CreatedDate = DateTime.Now;

                dbContext.Database.Insert(highFive);
            }
        }
        [HttpGet]
        public string GetHighFiveFeed()
        {
            var dbContext = ApplicationContext.Current.DatabaseContext;
            var sql = new Sql().Select("*").From("highFivePosts");
            var result = dbContext.Database.Fetch<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>(sql).OrderByDescending(x=>x.CreatedDate);
            var avatarService = new AvatarService();
            var response = new HighFiveFeedResponse();
            foreach (var dbEntry in result.Take(10))
            {
                var toMember = Members.GetById(dbEntry.ToMemberId);
                var fromMember = Members.GetById(dbEntry.FromMemberId);
                var toAvatar = avatarService.GetMemberAvatar(toMember);
                var fromAvatar = avatarService.GetMemberAvatar(toMember);
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
            var rawJson = JsonConvert.SerializeObject(response, Formatting.Indented);


            return rawJson;

        }

        private string GetActionType(int actionId)
        {
            return "";
        }

        public string GetCategories()
        {
            
            var rawJson = JsonConvert.SerializeObject(Categories, Formatting.Indented);
            return rawJson;
        }

        public List<Person> GetUmbracians(string name)
        {
            var peopleService = new PeopleService();

            var people = peopleService.GetPeopleByName(name);

            return people;
        }

        public List<Person> GetRandomUmbracians()
        {
            var peopleService = new PeopleService();

            var people = peopleService.GetRandomPeople();

            return people;
        }
    }
}