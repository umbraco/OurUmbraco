using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using uForum.Businesslogic;
using umbraco.cms.businesslogic.member;

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

            // Set reputation to at least 50 so their next posts won't be automatically marked as spam
            var member = new Member(topic.MemberId);
            int reputation;
            int.TryParse(member.getProperty("reputationTotal").Value.ToString(), out reputation);
            if (reputation < 50)
                member.getProperty("reputationTotal").Value = 50;

            int.TryParse(member.getProperty("reputationCurrent").Value.ToString(), out reputation);
            if (reputation < 50)
                member.getProperty("reputationCurrent").Value = 50;

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