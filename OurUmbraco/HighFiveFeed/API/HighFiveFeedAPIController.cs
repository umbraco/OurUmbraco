﻿using System;
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

namespace OurUmbraco.HighFiveFeed.API
{
    public class HighFiveFeedAPIController : UmbracoApiController
    {
        public HighFiveFeedAPIController() { }

        //this will be for authorized members only
        [HttpPost]
        public void SubmitHighFive(int fromUserId, int toUserId, string action, String url)
        {
            var memberService = ApplicationContext.Current.Services.MemberService;
            var member = memberService.GetById(fromUserId);

            var dbContext = ApplicationContext.Current.DatabaseContext;
            var highFive = new OurUmbraco.HighFiveFeed.Models.HighFiveFeed();
            highFive.FromMemberId = fromUserId;
            highFive.ToMemberId = toUserId;
            highFive.ActionId = action;
            highFive.Link = url;

            dbContext.Database.Insert(highFive);
        }
        [HttpGet]
        public List<OurUmbraco.HighFiveFeed.Models.HighFiveFeed> GetHighFiveFeed()
        {
            var dbContext = ApplicationContext.Current.DatabaseContext;
            var sql = new Sql().Select("*").From("highFivePosts");
            var result = dbContext.Database.Fetch<OurUmbraco.HighFiveFeed.Models.HighFiveFeed>(sql);
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

        public List<Person> GetUmbracians(string name)
        {
            var peopleService = new PeopleService();

            var people = peopleService.GetPeopleByName(name);

            return people;
        }
    }
}
