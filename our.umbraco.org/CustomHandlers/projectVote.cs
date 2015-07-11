using System;
using uPowers.BusinessLogic;
using Umbraco.Core;
using Umbraco.Web;

namespace our.CustomHandlers
{
    public class ProjectVoteHandler : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            uPowers.BusinessLogic.Action.BeforePerform += new EventHandler<ActionEventArgs>(ProjectVote);
            uPowers.BusinessLogic.Action.AfterPerform += new EventHandler<ActionEventArgs>(Action_AfterPerform);
        }

        void Action_AfterPerform(object sender, ActionEventArgs e)
        {
            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

            if (a.Alias == "ProjectUp")
            {
                var contentService = UmbracoContext.Current.Application.Services.ContentService;
                var content = contentService.GetById(e.ItemId);
                if (content.GetValue<bool>("approved") == false &&
                    uPowers.Library.Xslt.Score(content.Id, "powersProject") >= 15)
                {
                    content.SetValue("approved", true);
                    contentService.SaveAndPublishWithStatus(content);
                }
            }
        }

        void ProjectVote(object sender, ActionEventArgs e)
        {
            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

            if (a.Alias == "ProjectUp" || a.Alias == "ProjectDown")
            {
                var contentService = UmbracoContext.Current.Application.Services.ContentService;
                var content = contentService.GetById(e.ItemId);

                e.ReceiverId = content.GetValue<int>("owner");

                e.ExtraReceivers = Utils.GetProjectContributors(content.Id);
            }
        }
    }
}
