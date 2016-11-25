using System;
using System.Linq;
using System.Web.Security;
using OurUmbraco.Forum.Services;
using OurUmbraco.Powers.BusinessLogic;
using OurUmbraco.Powers.Library;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Action = OurUmbraco.Powers.BusinessLogic.Action;

namespace OurUmbraco.Our.CustomHandlers
{
    /// <summary>
    /// This is a custom handler to catch all voting events on topics
    /// It uses some custom fields on the forum topics so this is why it is not included in the standard uForum
    /// </summary>
    public class TopicVoteHandler : ApplicationEventHandler
    {
        
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Action.BeforePerform += new EventHandler<ActionEventArgs>(TopicVote);
            Action.BeforePerform += new EventHandler<ActionEventArgs>(TopicSolved);
            Action.AfterPerform += new EventHandler<ActionEventArgs>(TopicScoring);
        }
        
        private const string ModeratorRoles = "admin,HQ,Core,MVP";

        void TopicSolved(object sender, ActionEventArgs e)
        {
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);
            var cs = new CommentService(ApplicationContext.Current.DatabaseContext, ts);

            Action a = (Action)sender;
            if (a.Alias == "TopicSolved")
            {
                var c = cs.GetById(e.ItemId);
                if (c != null)
                {
                    var t = ts.GetById(c.TopicId);

                    //if performer and author of the topic is the same... go ahead..
                    if ((e.PerformerId == t.MemberId || ModeratorRoles.Split(',').Any(x => Roles.IsUserInRole(x))) && t.Answer == 0)
                    {

                        //receiver of points is the comment author.
                        e.ReceiverId = c.MemberId;

                        //remove any previous votes by the author on this comment to ensure the solution is saved instead of just the vote
                        a.ClearVotes(e.PerformerId, e.ItemId);

                        //this uses a non-standard coloumn in the forum schema, so this is added manually..
                        t.Answer = c.Id;
                        ts.Save(t);
                    }

                }

            }
        }


        void TopicScoring(object sender, ActionEventArgs e)
        {
            if (e.Cancel)
                return;

            var action = (Action)sender;
            
            if (action.Alias != "LikeTopic" && action.Alias != "DisLikeTopic")
                return;

            var topicScore = Xslt.Score(e.ItemId, action.DataBaseTable);
            using (var sqlHelper = Application.SqlHelper)
            {
                //this uses a non-standard coloumn in the forum schema, so this is added manually..
                sqlHelper.ExecuteNonQuery("UPDATE forumTopics SET score = @score WHERE id = @id",
                    sqlHelper.CreateParameter("@id", e.ItemId),
                    sqlHelper.CreateParameter("@score", topicScore));
            }
        }

        void TopicVote(object sender, ActionEventArgs e)
        {
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);

            Action a = (Action)sender;

            if (a.Alias == "LikeTopic" || a.Alias == "DisLikeTopic")
            {
                var t = ts.GetById(e.ItemId);
                e.ReceiverId = t.MemberId;
            }
        }




    }
}
