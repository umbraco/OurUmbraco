using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using PetaPoco;
using uForum.Businesslogic;

namespace uForum.usercontrols
{
    public partial class ForumSpamListTopics : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            FillSpamTopicGrid();
        }
        
        protected void NotSpamTopic(Object sender, CommandEventArgs e)
        {
            var id = int.Parse(e.CommandArgument.ToString());
            var topic = Topic.GetTopic(id);
            topic.Save(false, true);
            FillSpamTopicGrid();
        }

        private void FillSpamTopicGrid()
        {
            var topics = GetAllTopics();
            GridViewSpamTopic.DataSource = topics;
            GridViewSpamTopic.DataBind();
            GridViewSpamTopic.PageIndexChanging += GridViewSpamTopic_PageIndexChanging;
        }


        protected void GridViewSpamTopic_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewSpamTopic.PageIndex = e.NewPageIndex;
            var topics = GetAllTopics();
            GridViewSpamTopic.DataSource = topics;
            GridViewSpamTopic.DataBind();
        }

        private static List<Topic> GetAllTopics()
        {
            using (var db = new Database("umbracoDbDSN"))
            {
                return db.Fetch<Topic>("SELECT * FROM forumTopics WHERE isSpam = 1 ORDER BY id DESC");
            }
        }
    }
}