using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using OurUmbraco.Community.People;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Gitter
{
    
    public class GitterApiController : UmbracoApiController
    {
        /// <summary>
        /// Called from JS - in attempt to update the username mentions in chat messages
        /// </summary>
        /// <param name="usernameToFind"></param>
        /// <returns></returns>
        [HttpGet]
        public int? GetMemberId(string usernameToFind)
        {
            var peopleService = new PeopleService();
            return peopleService.GetMemberIdFromGithubName(usernameToFind);
        }
    }
}
