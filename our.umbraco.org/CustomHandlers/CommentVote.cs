using System;
using uForum.Services;
using uPowers.BusinessLogic;

namespace our.CustomHandlers {
    /// <summary>
    /// This is a custom handler to catch all voting events on topics
    /// It uses some custom fields on the forum topics so this is why it is not included in the standard uForum
    /// </summary>
    public class CommentVoteHandler : umbraco.BusinessLogic.ApplicationBase {

        public CommentVoteHandler() {
            uPowers.BusinessLogic.Action.BeforePerform += new EventHandler<ActionEventArgs>(CommentVote);
            uPowers.BusinessLogic.Action.AfterPerform += new EventHandler<ActionEventArgs>(CommentScoring);
        }


        void CommentScoring(object sender, ActionEventArgs e) {
            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

            if (a.Alias == "LikeComment" || a.Alias == "DisLikeComment" || a.Alias == "TopicSolved") {

                int score = uPowers.Library.Xslt.Score(e.ItemId, a.DataBaseTable);

                //we then add the sum of the total score to the
                our.Data.SqlHelper.ExecuteNonQuery("UPDATE forumComments SET score = @score WHERE id = @id", Data.SqlHelper.CreateParameter("@id", e.ItemId), Data.SqlHelper.CreateParameter("@score", score));
            }
        }



        void CommentVote(object sender, ActionEventArgs e) {

            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;


            using (var ts = new TopicService())
            using(var cs = new CommentService())
            {
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
}
