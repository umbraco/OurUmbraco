﻿using System;
using System.Linq;
using System.Web.Security;
using uForum.Services;
using uPowers.BusinessLogic;
using Umbraco.Core;

namespace our.CustomHandlers
{
    /// <summary>
    /// This is a custom handler to catch all voting events on topics
    /// It uses some custom fields on the forum topics so this is why it is not included in the standard uForum
    /// </summary>
    public class TopicVoteHandler : ApplicationEventHandler
    {
        
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            uPowers.BusinessLogic.Action.BeforePerform += new EventHandler<ActionEventArgs>(TopicVote);
            uPowers.BusinessLogic.Action.BeforePerform += new EventHandler<ActionEventArgs>(TopicSolved);
            uPowers.BusinessLogic.Action.AfterPerform += new EventHandler<ActionEventArgs>(TopicScoring);
        }
        
        private const string ModeratorRoles = "admin,HQ,Core,MVP";

        void TopicSolved(object sender, ActionEventArgs e)
        {
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);
            var cs = new CommentService(ApplicationContext.Current.DatabaseContext, ts);

            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;
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
            if (!e.Cancel)
            {
                uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

                if (a.Alias == "LikeTopic" || a.Alias == "DisLikeTopic")
                {
                    int topicScore = uPowers.Library.Xslt.Score(e.ItemId, a.DataBaseTable);

                    //this uses a non-standard coloumn in the forum schema, so this is added manually..
                    our.Data.SqlHelper.ExecuteNonQuery("UPDATE forumTopics SET score = @score WHERE id = @id", Data.SqlHelper.CreateParameter("@id", e.ItemId), Data.SqlHelper.CreateParameter("@score", topicScore));
                }
            }
        }

        void TopicVote(object sender, ActionEventArgs e)
        {
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);

            uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

            if (a.Alias == "LikeTopic" || a.Alias == "DisLikeTopic")
            {
                var t = ts.GetById(e.ItemId);
                e.ReceiverId = t.MemberId;
            }
        }




    }
}
