using OurUmbraco.Community.People;
using OurUmbraco.Community.People.Models;
using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Community.Badges.Controllers.Http
{
    public class BadgeController : UmbracoApiController
    {
        private readonly PeopleService _peopleService;

        public BadgeController()
        {
            _peopleService = new PeopleService();
        }

        public IHttpActionResult GetBadgeGroups()
        {
            var badgeGroups = new List<BadgeGroup>();

            var contributors = _peopleService.GetMembersInRole("CoreContrib");
            var contributorsGroup = new BadgeGroup
            {
                Name = "C-Trib",
                Description = "Contributor to one of our open source projects: Umbraco CMS, Umbraco Documentation, Our Umbraco, etc.",
                Members = contributors
            };

            badgeGroups.Add(contributorsGroup);


            var admins = _peopleService.GetMembersInRole("admin");
            var adminsGroup = new BadgeGroup
            {
                Name = "Admin",
                Description = "Administrator on the forum, admins have access to moderation tools on the forum",
                Members = admins
            };

            badgeGroups.Add(adminsGroup);

            var hqMembers = _peopleService.GetMembersInRole("HQ");
            var hqMembersGroup = new BadgeGroup
            {
                Name = "HQ",
                Description = "Members who work at Umbraco headquarters, the company behind Umbraco",
                Members = hqMembers
            };

            badgeGroups.Add(hqMembersGroup);

            return Ok(badgeGroups);
        }
    }
}
