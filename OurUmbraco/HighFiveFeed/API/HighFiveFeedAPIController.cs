using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.WebApi;
using Umbraco.Core;
using OurUmbraco.HighFiveFeed.Models;
<<<<<<< HEAD
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.Persistence;

=======
>>>>>>> 03f85a8b89320d3fa07d44a57a5130336884ca7c
namespace OurUmbraco.HighFiveFeed.API
{
    public class HighFiveFeedAPIController : UmbracoApiController
    {
        private readonly DatabaseContext _dbContext;

        public HighFiveFeedAPIController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public HighFiveFeedAPIController() { }

       [HttpGet]
        public void SubmitHighFive(int fromUserId, int toUserId, string action, String url)
        {
            var memberService = ApplicationContext.Current.Services.MemberService;
            var member = memberService.GetById(fromUserId);


            var highFive = new OurUmbraco.HighFiveFeed.Models.HighFiveFeed();
            highFive.FromMemberId = fromUserId;
            highFive.ToMemberId = toUserId;
            highFive.ActionId = action;
            highFive.Link = url;

            _dbContext.Database.Insert(highFive);
        }
        [HttpGet]
        public List<OurUmbraco.HighFiveFeed.Models.HighFiveFeed> GetHighFiveFeed()
        {
            var sql = new Sql().Select("*").From("highFivePosts");
            var result = _dbContext.Database.Fetch<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>(sql);
            return result;

        }


        public string GetCategories()
        {
            var categories = new List<HighFiveCategory>();
            categories.Add(new HighFiveCategory(1, "A Package"));
            categories.Add(new HighFiveCategory(2, "A Talk"));
            categories.Add(new HighFiveCategory(3, "A Blog Post"));
            categories.Add(new HighFiveCategory(4, "A Meetup"));
            categories.Add(new HighFiveCategory(5, "A Skrift Article"));
            categories.Add(new HighFiveCategory(6, "A Tutorial"));
            categories.Add(new HighFiveCategory(7, "Advice"));
            categories.Add(new HighFiveCategory(8, "A Video"));
            categories.Add(new HighFiveCategory(9, "A PR"));
            var rawJson = JsonConvert.SerializeObject(categories, Formatting.Indented);


            return rawJson;

           
        }
    }
}

