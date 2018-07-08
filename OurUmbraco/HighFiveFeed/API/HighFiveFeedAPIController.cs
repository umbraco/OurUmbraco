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
using HtmlAgilityPack;
using System.Text.RegularExpressions;

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
        public bool SubmitHighFive(int toUserId, int action, String url, String linkTitle="")
        {
           
            var memberService = ApplicationContext.Current.Services.MemberService;
            //you need to be logged in!
            var currentMember = Members.GetCurrentMember();
            if (currentMember != null && currentMember.Id != 0)
            {
                var fromUserId = currentMember.Id;
                var dbContext = ApplicationContext.Current.DatabaseContext;
                var highFive = new Models.HighFiveFeed();
                highFive.FromMemberId = fromUserId;
                highFive.ToMemberId = toUserId;
                highFive.ActionId = action;
                highFive.Link = url;
                highFive.CreatedDate = DateTime.Now;
                highFive.LinkTitle = linkTitle;
                dbContext.Database.Insert(highFive);
                return true;
            }

            return false;
        }

        /*
         * Return High Five feed.
         */
        [HttpGet]
        public string GetHighFiveFeed()
        {
            var response = _highFiveService.GetHighFiveFeed();
            var rawJson = JsonConvert.SerializeObject(response, Formatting.Indented);
            return rawJson;

        }

        [Authorize]
        [HttpGet]
        public Person GetCurrentMember()
        {
            var currentMember = Members.GetCurrentMember();
            var memberService = ApplicationContext.Current.Services.MemberService;
            
            if (currentMember != null && currentMember.Id != 0)
            {
                var member = memberService.GetById(currentMember.Id);
                //  var rawJson = JsonConvert.SerializeObject(currentMember, Formatting.Indented);
                var returnItem = new Person(member);

                return returnItem;
             }
            return null;
        }


        public string GetCategories()
        {
            var Categories = _highFiveService.GetCategories();
            var rawJson = JsonConvert.SerializeObject(Categories, Formatting.Indented);
            return rawJson;
        }

        /*
         * Return avatar for a member id.
         */
        public string GetMemberAvatar(int memberId)
        {
            var member = Members.GetById(memberId);

            if (member != null)
            {
                var avatarService = new AvatarService();
                var avatar = avatarService.GetMemberAvatar(member);
                return avatar;
            }
            return null;
        }
        /*
         * Return a list of matching members for a given string.
         */
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
        /*
         * Returns the title tag of a provided URL.
         */
        public string GetTitleTag(string url)
        {
            //return the title of the page or just return the url if there isn't one.
            var uri = new UriBuilder(url).Uri.AbsoluteUri;
            var webGet = new HtmlWeb();
            try
            {
                var document = webGet.Load(uri.ToString());
                var titleNode = document.DocumentNode.SelectSingleNode("html/head/title");
                if (titleNode != null && !String.IsNullOrEmpty(titleNode.InnerHtml))
                {
                    return titleNode.InnerText;
                }
                return url;
            }
            catch (Exception e)
            {
                return url;
            }
            
        }
    }
}