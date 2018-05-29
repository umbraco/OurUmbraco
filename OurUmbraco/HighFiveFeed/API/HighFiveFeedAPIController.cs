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
using OurUmbraco.HighFiveFeed.Services;

namespace OurUmbraco.HighFiveFeed.API
{
    public class HighFiveFeedAPIController : UmbracoApiController
    {

        public HighFiveFeedService _highFiveService;

        public HighFiveFeedAPIController()
        {
            _highFiveService = new HighFiveFeedService();
        }

        [Authorize]
        [HttpPost]
        public bool SubmitHighFive(int toUserId, int action, String url)
        {

            var memberService = ApplicationContext.Current.Services.MemberService;
            //you need to be logged in!
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
                return true;
            }

            return false;
        }

        [HttpGet]
        public string GetHighFiveFeed()
        {
            var response = _highFiveService.GetHighFiveFeed();
            var rawJson = JsonConvert.SerializeObject(response, Formatting.Indented);


            return rawJson;

        }

       
        public string GetCategories()
        {
            var Categories = _highFiveService.GetCategories();
            var rawJson = JsonConvert.SerializeObject(Categories, Formatting.Indented);
            return rawJson;
        }

        public List<Person> GetUmbracians(string name)
        {
            var peopleService = new PeopleService();

            var people = peopleService.GetPeopleByName(name);

            var currentMember = Members.GetCurrentMember();
            if (currentMember != null && currentMember.Id != 0)
            {
                return people.Where(p => p.MemberId != currentMember.Id).ToList();
            }

            return people;
        }

        public List<Person> GetRandomUmbracians()
        {
            var peopleService = new PeopleService();

            var people = peopleService.GetRandomPeople();

            var currentMember = Members.GetCurrentMember();
            if (currentMember != null && currentMember.Id != 0)
            {
                return people.Where(p => p.MemberId != currentMember.Id).ToList();
            }

            return people;
        }
    }
}