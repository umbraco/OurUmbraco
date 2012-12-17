using System;
using System.Collections.Generic;
using System.Text;
using uForum.Businesslogic;
using NotificationsCore;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.member;


namespace NotificationsWeb.EventHandlers
{
    public class Forum : umbraco.BusinessLogic.ApplicationBase 
    {
        public Forum()
        {
            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "NotificationsWeb.EventHandlers.Forum class events - starting");

            Comment.AfterCreate += new EventHandler<CreateEventArgs>(Comment_AfterCreate);

            Topic.AfterCreate += new EventHandler<CreateEventArgs>(Topic_AfterCreate);

            uForum.Businesslogic.Forum.AfterCreate += new EventHandler<CreateEventArgs>(Forum_AfterCreate);

            uForum.Businesslogic.Forum.BeforeDelete += new EventHandler<DeleteEventArgs>(Forum_BeforeDelete);

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "NotificationsWeb.EventHandlers.Forum class events - finishing");
        }

        void Forum_BeforeDelete(object sender, DeleteEventArgs e)
        {
            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Forum_BeforeDelete in NotificationsWeb.EventHandlers.Forum() class is starting");

            BusinessLogic.Forum.RemoveAllSubscriptions(((uForum.Businesslogic.Forum)sender).Id);

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Forum_BeforeDelete in NotificationsWeb.EventHandlers.Forum() class is finishing");
        }

        void Forum_AfterCreate(object sender, CreateEventArgs e)
        {
            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Forum_AfterCreate in NotificationsWeb.EventHandlers.Forum() class is starting");

            //subscribe project owner to created forum
            uForum.Businesslogic.Forum f = (uForum.Businesslogic.Forum)sender;

            Node n = new Node(f.ParentId);
            if (n.NodeTypeAlias == "Project")
            {
                NotificationsWeb.BusinessLogic.Forum.Subscribe(
                    f.Id, Convert.ToInt32(n.GetProperty("owner").Value));
            }

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Forum_AfterCreate in NotificationsWeb.EventHandlers.Forum() class is finishing");
        }

        void Comment_AfterCreate(object sender, uForum.Businesslogic.CreateEventArgs e)
        {
            Comment c = (Comment)sender;

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, c.Id, "Comment_AfterCreate in NotificationsWeb.EventHandlers.Forum() class is starting");

            NotificationsWeb.BusinessLogic.ForumTopic.Subscribe
                (c.TopicId, c.MemberId);

            //send notifications
            InstantNotification not = new InstantNotification();

            Member m = new Member(c.MemberId);

            not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "NewComment", c, NiceCommentUrl(c.TopicId,c,10),m);

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, c.Id, "Comment_AfterCreate in NotificationsWeb.EventHandlers.Forum() class is finishing");
        }

        private static string NiceCommentUrl(int topicId, Comment c, int itemsPerPage)
        {
            string url = uForum.Library.Xslt.NiceTopicUrl(topicId);
            if (!string.IsNullOrEmpty(url))
            {
              
                int position = c.Position - 1;

                int page = (int)(position / itemsPerPage);


                url += "?p=" + page.ToString() + "#comment" + c.Id.ToString();
            }

            return url;
        }

        void Topic_AfterCreate(object sender, CreateEventArgs e)
        {
            //subscribe topic created to notification
            Topic t = (Topic)sender;

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, t.Id, "Topic_AfterCreate in NotificationsWeb.EventHandlers.Forum() class is starting");
          
            NotificationsWeb.BusinessLogic.ForumTopic.Subscribe
                (t.Id, t.MemberId);

            Member m = new Member(t.MemberId);

            //send notification
            InstantNotification not = new InstantNotification();

            not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "NewTopic", t, NiceTopicUrl(t),m);

            //WB added to show these events are firing...
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, t.Id, "Topic_AfterCreate in NotificationsWeb.EventHandlers.Forum() class is finishing");
        }


        private static string NiceTopicUrl(Topic t)
        {
            
            if (t.Exists)
            {
                string _url = umbraco.library.NiceUrl(t.ParentId);

                if (umbraco.GlobalSettings.UseDirectoryUrls)
                {
                    return "/" + _url.Trim('/') + "/" + t.Id.ToString() + "-" + t.UrlName;
                }
                else
                {
                    return "/" + _url.Substring(0, _url.LastIndexOf('.')).Trim('/') + "/" + t.Id.ToString() + "-" + t.UrlName + ".aspx";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
