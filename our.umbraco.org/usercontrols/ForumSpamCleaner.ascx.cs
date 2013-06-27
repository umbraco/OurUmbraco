using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PetaPoco;
using uForum.Businesslogic;
using umbraco.cms.businesslogic.member;

namespace uForum.usercontrols
{
    public partial class ForumSpamCleaner : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            FillSpamCommentGrid();
            FillSpamTopicGrid();
        }

        protected void CleanSpamClick(object s, EventArgs e)
        {
            //figure out who you are going to delete comments for.

            var mId = 0;
            if (Int32.TryParse(memberId.Text, out mId))
            {
                var member = new Member(mId);
                AreYouSurePanel.Visible = true;
                MemberName.Text = member.Text;

                FillGrid();
            }
            else
            {
                AreYouSurePanel.Visible = false;
            }
        }

        protected void NotSpamComment(Object sender, CommandEventArgs e)
        {
            var id = int.Parse(e.CommandArgument.ToString());
            var comment = new Comment(id) { IsSpam = false };
            comment.Save(true);
            FillSpamCommentGrid();
        }


        protected void NotSpamTopic(Object sender, CommandEventArgs e)
        {
            var id = int.Parse(e.CommandArgument.ToString());
            var topic = Topic.GetTopic(id);
            topic.Save(false, true);
            FillSpamTopicGrid();
        }

        protected void grdContact_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(commentsGrid.DataKeys[e.RowIndex].Values[0].ToString());
            var comment = new Comment(id);
            comment.Delete();
            FillGrid();
        }

        private void FillGrid()
        {
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader("SELECT * FROM forumComments WHERE memberId = " + memberId.Text);
            commentsGrid.DataSource = dr;
            commentsGrid.DataBind();
            dr.Close();
            dr.Dispose();
        }

        private void FillSpamCommentGrid()
        {
            var comments = GetAllComments();
            GridViewSpamComment.DataSource = comments;
            GridViewSpamComment.DataBind();
            GridViewSpamComment.PageIndexChanging += GridViewSpamComment_PageIndexChanging;
        }
        private void FillSpamTopicGrid()
        {
            var topics = GetAllTopics();
            GridViewSpamTopic.DataSource = topics;
            GridViewSpamTopic.DataBind();
            GridViewSpamTopic.PageIndexChanging += GridViewSpamTopic_PageIndexChanging;
        }

        protected void GridViewSpamComment_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewSpamComment.PageIndex = e.NewPageIndex;
            var comments = GetAllComments();
            GridViewSpamComment.DataSource = comments;
            GridViewSpamComment.DataBind();
        }

        protected void GridViewSpamTopic_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewSpamComment.PageIndex = e.NewPageIndex;
            var topics = GetAllTopics();
            GridViewSpamComment.DataSource = topics;
            GridViewSpamComment.DataBind();
        }

        private static List<Comment> GetAllComments()
        {
            using (var db = new Database("umbracoDbDSN"))
            {
                return db.Fetch<Comment>("select * from forumComments where isSpam = 1");
            }
        }
        private static List<Topic> GetAllTopics()
        {
            using (var db = new Database("umbracoDbDSN"))
            {
                return db.Fetch<Topic>("select * from forumTopics where isSpam = 1");
            }
        }
    }
}