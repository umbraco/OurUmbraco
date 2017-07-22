﻿using System;
using System.Configuration;
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
        public string AwardMasterBadge(string email)
        {
            var member = MemberService.GetByEmail(email);

            if (AwardBadge(member, "Master", "MasterBadgeKarmaPoints"))
            {
                return "Member " + email + " now has master status";
            }

            return "Failed to assign master status";
        }

        private bool AwardBadge(IMember member, string roleName, string karmaRewardAppSetting)
        {
            var karmaPoints = Convert.ToInt32(ConfigurationManager.AppSettings[karmaRewardAppSetting]);

            return AwardBadge(member, roleName, karmaPoints);
        }

        private bool AwardBadge(IMember member, string roleName, int karmaReward = 0)
        {
            if (member != null)
            {
                MemberService.AssignRole(member.Id, roleName);

                if (karmaReward > 0)
                {
                    member.IncreaseKarma(karmaReward);

                    return true;
                }
            }

            return false;
        }
    }
}