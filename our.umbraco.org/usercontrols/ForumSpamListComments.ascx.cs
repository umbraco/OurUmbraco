using System;
using System.Linq;
using System.Web.UI.WebControls;
using uForum;
using umbraco.cms.businesslogic.member;

namespace uForum.usercontrols
{
    public partial class ForumSpamListComments : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            FillSpamCommentGrid();
        }

        protected void NotSpamComment(Object sender, CommandEventArgs e)
        {
            var id = int.Parse(e.CommandArgument.ToString());
            var comment = Services.CommentService.Instance().GetById(id);
            comment.IsSpam = false;
            
            comment.Save(true);

            // Set reputation to at least 50 so their next posts won't be automatically marked as spam
            var member = new Member(comment.MemberId);
            int reputation;
            int.TryParse(member.getProperty("reputationTotal").Value.ToString(), out reputation);
            if (reputation < 50)
                member.getProperty("reputationTotal").Value = 50;

            int.TryParse(member.getProperty("reputationCurrent").Value.ToString(), out reputation);
            if (reputation < 50)
                member.getProperty("reputationCurrent").Value = 50;
            
            FillSpamCommentGrid();
        }

        private void FillSpamCommentGrid()
        {
            var comments = Comment.GetAllSpamComments();
            GridViewSpamComment.DataSource = comments;
            GridViewSpamComment.DataBind();
            GridViewSpamComment.PageIndexChanging += GridViewSpamComment_PageIndexChanging;
        }
        
        protected void GridViewSpamComment_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewSpamComment.PageIndex = e.NewPageIndex;
            var comments = Comment.GetAllSpamComments();
            GridViewSpamComment.DataSource = comments;
            GridViewSpamComment.DataBind();
        }
        
    }
}