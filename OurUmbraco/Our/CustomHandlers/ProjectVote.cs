using System;
using OurUmbraco.Powers.BusinessLogic;
using OurUmbraco.Powers.Library;
using Umbraco.Core;
using Umbraco.Web;
using Action = OurUmbraco.Powers.BusinessLogic.Action;

namespace OurUmbraco.Our.CustomHandlers
{
    public class ProjectVoteHandler : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Action.BeforePerform += new EventHandler<ActionEventArgs>(ProjectVote);
            Action.AfterPerform += new EventHandler<ActionEventArgs>(Action_AfterPerform);
        }

        void Action_AfterPerform(object sender, ActionEventArgs e)
        {
            Action a = (Action)sender;

            if (a.Alias == "ProjectUp")
            {
                var contentService = ApplicationContext.Current.Services.ContentService;
                var content = contentService.GetById(e.ItemId);
                if (content.GetValue<bool>("approved") == false &&
                    Xslt.Score(content.Id, "powersProject") >= 15)
                {
                    content.SetValue("approved", true);
                    contentService.SaveAndPublishWithStatus(content);
                }
            }
        }

        void ProjectVote(object sender, ActionEventArgs e)
        {
            Action a = (Action)sender;

            if (a.Alias == "ProjectUp" || a.Alias == "ProjectDown")
            {
                var contentService = ApplicationContext.Current.Services.ContentService;
                var content = contentService.GetById(e.ItemId);

                e.ReceiverId = content.GetValue<int>("owner");

                e.ExtraReceivers = Utils.GetProjectContributors(content.Id);
            }
        }
    }
}
