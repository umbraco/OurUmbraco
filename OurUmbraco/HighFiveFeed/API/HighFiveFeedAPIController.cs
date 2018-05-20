using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Web.WebApi;
using Umbraco.Core;
using OurUmbraco.HighFiveFeed.Models;

namespace OurUmbraco.HighFiveFeed.API
{
    public class HighFiveFeedAPIController : UmbracoApiController
    {
        private readonly DatabaseContext _dbContext;


        [HttpGet]
        public void SubmitHighFive(int fromUserId, int toUserId, int action, String url)
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

        public List<HighFiveCategory> GetCategories()
        {
            //refactor to a service
            var categories = new List<HighFiveCategory>();
            categories.Add(new HighFiveCategory(1, "A Package"));
            categories.Add(new HighFiveCategory(2, "A Talk"));
            return categories;
        }
    }
}

