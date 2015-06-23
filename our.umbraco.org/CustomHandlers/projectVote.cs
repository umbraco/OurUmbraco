using System;
using umbraco.cms.businesslogic.web;
using uPowers.BusinessLogic;
using Umbraco.Core;

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
                Document d = new Document(e.ItemId);

                if (d.getProperty("approved").Value != null &&
                     d.getProperty("approved").Value.ToString() != "1" &&
                     uPowers.Library.Xslt.Score(d.Id, "powersProject") >= 15)
                {
                    //set approved flag
                    d.getProperty("approved").Value = true;

                    d.Save();
                    d.Publish(new umbraco.BusinessLogic.User(0));

                    umbraco.library.UpdateDocumentCache(d.Id);
                    umbraco.library.RefreshContent();
                }
            }
        }

        void ProjectVote(object sender, ActionEventArgs e)
        {
            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

            if (a.Alias == "ProjectUp" || a.Alias == "ProjectDown")
            {

                Document d = new Document(e.ItemId);

                e.ReceiverId = (int)d.getProperty("owner").Value;

                e.ExtraReceivers = Utils.GetProjectContributors(d.Id);
            }
        }
    }
}
