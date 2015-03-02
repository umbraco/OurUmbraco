using System;
using uForum.Services;
using uPowers.BusinessLogic;
using Umbraco.Core;

namespace our.CustomHandlers
{
    /// <summary>
    /// This is a custom handler to catch all voting events on topics
    /// It uses some custom fields on the forum topics so this is why it is not included in the standard uForum
    /// </summary>
    public class CommentVoteHandler : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            uPowers.BusinessLogic.Action.BeforePerform += new EventHandler<ActionEventArgs>(CommentVote);
            uPowers.BusinessLogic.Action.AfterPerform += new EventHandler<ActionEventArgs>(CommentScoring);
        }


        void CommentScoring(object sender, ActionEventArgs e)
        {
            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

            if (a.Alias == "LikeComment" || a.Alias == "DisLikeComment" || a.Alias == "TopicSolved")
            {

                int score = uPowers.Library.Xslt.Score(e.ItemId, a.DataBaseTable);

                //we then add the sum of the total score to the
                our.Data.SqlHelper.ExecuteNonQuery("UPDATE forumComments SET score = @score WHERE id = @id", Data.SqlHelper.CreateParameter("@id", e.ItemId), Data.SqlHelper.CreateParameter("@score", score));
            }
        }



        void CommentVote(object sender, ActionEventArgs e)
        {

            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

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
            else if (a.Alias == "TopicSolved")
            {
                var c = cs.GetById(e.ItemId);
                var t = ts.GetById(c.TopicId);
                e.Cancel = t.Answer > 0;
            }

        }

    }
}
