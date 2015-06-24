using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace uForum.EventHandlers
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
            var nonApprovedMembers = e.SavedEntities.Where(member => member.Properties[Constants.Conventions.Member.IsApproved] != null && member.IsApproved == false);
            var memberService = UmbracoContext.Current.Application.Services.MemberService;
            foreach (var member in nonApprovedMembers)
            {
                member.IsApproved = true;
                memberService.Save(member, false);
            }
        }
    }
}
