using System;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using OurUmbraco.Forum.Extensions;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class BadgesController : UmbracoApiController
    {
        private IMemberService MemberService;

        public BadgesController()
        {
            MemberService = ApplicationContext.Services.MemberService;
        }

        [HttpPost]
        [VerifyTokenHeader("BadgesApiToken")]
        public string AwardMasterBadge(string email)
        {
            var member = MemberService.GetByEmail(email);

            if (AwardBadge(member, "Master", "MasterBadgeKarmaPoints"))
            {
                return "Member " + email + " now has master status";
            }

            return "Failed to assign master status";
        }

        [HttpPost]
        [VerifyTokenHeader("BadgesApiToken")]
        public string AwardCoreContribBadge(string email)
        {
            var member = MemberService.GetByEmail(email);

            if (AwardBadge(member, "CoreContrib", "CoreContribBadgeKarmaPoints"))
            {
                return "Member " + email + " now has c-trib status";
            }

            return "Failed to assign c-trib status";
        }

        /// <summary>
        /// Assigns a badge to a given member and awards karma points
        /// </summary>
        /// <param name="member">The member</param>
        /// <param name="badgeName">The name of the member group (badge) to assign to the member</param>
        /// <param name="karmaRewardKey">The appSettings key for the amount of karma points to reward the member</param>
        /// <returns></returns>
        private bool AwardBadge(IMember member, string badgeName, string karmaRewardKey)
        {
            var karmaPoints = Convert.ToInt32(ConfigurationManager.AppSettings[karmaRewardKey]);

            return AwardBadge(member, badgeName, karmaPoints);
        }

        /// <summary>
        /// Assigns a badge to a given member and awards karma points
        /// </summary>
        /// <param name="member">The member</param>
        /// <param name="badgeName">The name of the member group (badge) to assign to the member</param>
        /// <param name="karmaReward">The amount of karma points to reward the member</param>
        /// <returns></returns>
        private bool AwardBadge(IMember member, string badgeName, int karmaReward = 0)
        {
            if (member == null)
            {
                return false;
            }

            // get the members current badges
            var currentRoles = MemberService.GetAllRoles(member.Id);

            // check the member does not have the badge yet
            if (!currentRoles.Contains(badgeName))
            {
                // assign the badge to the member
                MemberService.AssignRole(member.Id, badgeName);

                if (karmaReward > 0)
                {
                    // award karma points to the member
                    member.IncreaseKarma(karmaReward);

                    return true;
                }
            }

            return false;
        }
    }
}