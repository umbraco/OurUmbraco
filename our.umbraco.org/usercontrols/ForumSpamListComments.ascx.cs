using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using PetaPoco;
using uForum.Businesslogic;

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
            var comment = new Comment(id) { IsSpam = false };
            comment.Save(true);
            FillSpamCommentGrid();
        }

        private void FillSpamCommentGrid()
        {
            var comments = GetAllComments();
            GridViewSpamComment.DataSource = comments;
            GridViewSpamComment.DataBind();
            GridViewSpamComment.PageIndexChanging += GridViewSpamComment_PageIndexChanging;
        }
        
        protected void GridViewSpamComment_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewSpamComment.PageIndex = e.NewPageIndex;
            var comments = GetAllComments();
            GridViewSpamComment.DataSource = comments;
            GridViewSpamComment.DataBind();
        }
        
        private static List<Comment> GetAllComments()
        {
            using (var db = new Database("umbracoDbDSN"))
            {
                return db.Fetch<Comment>("SELECT * FROM forumComments WHERE isSpam = 1 ORDER BY id DESC");
            }
        }
    }
}