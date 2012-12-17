using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.member;

namespace our.custom_Handlers {
    public class ForumPostsCounter : umbraco.BusinessLogic.ApplicationBase {

        public ForumPostsCounter() {

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "ForumPostsCounter class events - starting");

            uForum.Businesslogic.Topic.AfterCreate += new EventHandler<uForum.Businesslogic.CreateEventArgs>(Topic_AfterCreate);
            uForum.Businesslogic.Comment.AfterCreate += new EventHandler<uForum.Businesslogic.CreateEventArgs>(Comment_AfterCreate);

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "ForumPostsCounter class events - finishing");
        }

        void Comment_AfterCreate(object sender, uForum.Businesslogic.CreateEventArgs e) {
            uForum.Businesslogic.Comment c = (uForum.Businesslogic.Comment)sender;

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, c.Id, "Comment_AfterCreate in ForumPostsCounter() class is starting");

            Member mem = new Member(c.MemberId);
            int posts = 0;
            int.TryParse(mem.getProperty("forumPosts").Value.ToString(), out posts);

            mem.getProperty("forumPosts").Value = (posts + 1);
            mem.Save();

            mem.XmlGenerate(new System.Xml.XmlDocument());

            //Performs the action NewTopic in case we want to reward people for creating new posts.
            uPowers.BusinessLogic.Action a = new uPowers.BusinessLogic.Action("NewComment");
            a.Perform(mem.Id, c.Id, "New comment created");

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, c.Id, "Comment_AfterCreate in ForumPostsCounter() class is finishing");
        }


        void Topic_AfterCreate(object sender, uForum.Businesslogic.CreateEventArgs e) {
            uForum.Businesslogic.Topic t = (uForum.Businesslogic.Topic)sender;

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, t.Id, "Topic_AfterCreate in ForumPostsCounter() class is starting");

            Member mem = new Member(t.MemberId);
            int posts = 0;
            int.TryParse(mem.getProperty("forumPosts").Value.ToString(), out posts);

            mem.getProperty("forumPosts").Value = (posts + 1);
            mem.Save();

            mem.XmlGenerate(new System.Xml.XmlDocument());
            
            //Performs the action NewTopic in case we want to reward people for creating new posts.
            uPowers.BusinessLogic.Action a = new uPowers.BusinessLogic.Action("NewTopic");
            a.Perform(mem.Id, t.Id, "New topic created");

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, t.Id, "Topic_AfterCreate in ForumPostsCounter() class is finishing");
        }

    }
}
