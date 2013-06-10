using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uPowers.BusinessLogic;
using uForum.Businesslogic;


namespace our.custom_Handlers {
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

            if (a.Alias == "LikeComment" || a.Alias == "DisLikeComment") {
                Comment c = new Comment(e.ItemId);
                if (c != null) {
                    e.ReceiverId = c.MemberId;
                }
            } else if (a.Alias == "TopicSolved") {
                Topic t = Topic.GetTopic(new Comment(e.ItemId).TopicId);
                bool hasAnswer = (our.Data.SqlHelper.ExecuteScalar<int>("SELECT answer FROM forumTopics where id = @id", Data.SqlHelper.CreateParameter("@id", t.Id)) > 0);

                e.Cancel = hasAnswer;
            }
        }
      
    }
}
