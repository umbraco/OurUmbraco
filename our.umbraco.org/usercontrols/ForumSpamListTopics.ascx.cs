using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
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
            var topics = Topic.GetAllSpamTopics();
            GridViewSpamTopic.DataSource = topics;
            GridViewSpamTopic.DataBind();
            GridViewSpamTopic.PageIndexChanging += GridViewSpamTopic_PageIndexChanging;
        }


        protected void GridViewSpamTopic_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewSpamTopic.PageIndex = e.NewPageIndex;
            var topics = Topic.GetAllSpamTopics();
            GridViewSpamTopic.DataSource = topics;
            GridViewSpamTopic.DataBind();
        }
    }
}