using System;
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
    public class CommentVoteHandler : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Action.BeforePerform += new EventHandler<ActionEventArgs>(CommentVote);
            Action.AfterPerform += new EventHandler<ActionEventArgs>(CommentScoring);
        }


        void CommentScoring(object sender, ActionEventArgs e)
        {
            var action = (Action)sender;

            if (action.Alias != "LikeComment" && action.Alias != "DisLikeComment" && action.Alias != "TopicSolved")
                return;

            var score = Xslt.Score(e.ItemId, action.DataBaseTable);

            //we then add the sum of the total score to the
            using (var sqlHelper = Application.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery("UPDATE forumComments SET score = @score WHERE id = @id",
                    sqlHelper.CreateParameter("@id", e.ItemId), sqlHelper.CreateParameter("@score", score));
            }
        }

        void CommentVote(object sender, ActionEventArgs e)
        {

            Action a = (Action)sender;

            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);
            var cs = new CommentService(ApplicationContext.Current.DatabaseContext, ts);

            if (a.Alias == "LikeComment" || a.Alias == "DisLikeComment")
            {
                var c = cs.GetById(e.ItemId);
                if (c != null)
                {
                    e.ReceiverId = c.MemberId;
                }
            }
        }
    }
}
