using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace OurUmbraco.Forum.EventHandlers
{
    public class MemberApprovedEventhandler : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            MemberService.Saving += SetIsApproved;
        }

        static void SetIsApproved(IMemberService sender, SaveEventArgs<IMember> e)
        {
            // Automatically approve all members, as we don't have an approval process now
            // This is needed as we added new membership after upgrading so IsApproved is 
            // currently empty. First time a member gets saved now (login also saves the member)
            // IsApproved would turn false (default value of bool) so we want to prevent that

            // Note: Since July 23rd 2015 we need people to activate their accounts! Don't 
            // approve automatically. 
            var nonApprovedMembers = e.SavedEntities.Where(
                member => member.CreateDate < DateTime.Parse("2015-07-23") 
                && member.Properties.Contains(Umbraco.Core.Constants.Conventions.Member.IsApproved) 
                && member.IsApproved == false);

            var memberService = ApplicationContext.Current.Services.MemberService;
            foreach (var member in nonApprovedMembers)
            {
                // Adds test for member having an Id, if they don't then it's a NEW member, 
                // new members don't automatically get approved any more, only existing 
                // member before July 23rd 2015
                if (member.HasIdentity)
                {
                    member.IsApproved = true;
                    memberService.Save(member, false);
                }
            }
        }
    }
}
