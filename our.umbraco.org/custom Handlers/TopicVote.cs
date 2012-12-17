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
    public class TopicVoteHandler : umbraco.BusinessLogic.ApplicationBase {

        public TopicVoteHandler() {
            uPowers.BusinessLogic.Action.BeforePerform += new EventHandler<ActionEventArgs>(TopicVote);
            uPowers.BusinessLogic.Action.BeforePerform += new EventHandler<ActionEventArgs>(TopicSolved);
            uPowers.BusinessLogic.Action.AfterPerform +=new EventHandler<ActionEventArgs>(TopicScoring);
        }


        void TopicSolved(object sender, ActionEventArgs e) {
           
                uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

                if (a.Alias == "TopicSolved") {

                    Comment c = new Comment(e.ItemId);
                    
                    if (c != null) {
                        Topic t = new Topic(c.TopicId);

                        int answer = our.Data.SqlHelper.ExecuteScalar<int>("SELECT answer FROM forumTopics where id = @id", Data.SqlHelper.CreateParameter("@id", t.Id));

                        //if performer and author of the topic is the same... go ahead..

                        if (e.PerformerId == t.MemberId && answer == 0) {

                            //receiver of points is the comment author.
                            e.ReceiverId = c.MemberId;

                            //remove any previous votes by the author on this comment to ensure the solution is saved instead of just the vote
                            a.ClearVotes(e.PerformerId, e.ItemId);
                            
                            //this uses a non-standard coloumn in the forum schema, so this is added manually..
                            our.Data.SqlHelper.ExecuteNonQuery("UPDATE forumTopics SET answer = @answer WHERE id = @id", Data.SqlHelper.CreateParameter("@id", t.Id), Data.SqlHelper.CreateParameter("@answer", c.Id));
                        }

                    }
                }
        }


        void TopicScoring(object sender, ActionEventArgs e) {
            if (!e.Cancel) {
                uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

                if (a.Alias == "LikeTopic" || a.Alias == "DisLikeTopic") {
                    int topicScore = uPowers.Library.Xslt.Score(e.ItemId, a.DataBaseTable);

                    //this uses a non-standard coloumn in the forum schema, so this is added manually..
                    our.Data.SqlHelper.ExecuteNonQuery("UPDATE forumTopics SET score = @score WHERE id = @id", Data.SqlHelper.CreateParameter("@id", e.ItemId), Data.SqlHelper.CreateParameter("@score", topicScore));
                }
            }
        }

        void TopicVote(object sender, ActionEventArgs e) {
                uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

                if (a.Alias == "LikeTopic" || a.Alias == "DisLikeTopic") {
                    Topic t = new Topic(e.ItemId);
                    e.ReceiverId = t.MemberId;
                 }
        }


        

    } 
}
