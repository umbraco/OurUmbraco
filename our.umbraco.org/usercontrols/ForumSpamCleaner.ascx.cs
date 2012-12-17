using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using uForum.Businesslogic;
using umbraco.cms.businesslogic.member;

namespace uForum.usercontrols
{
    public partial class ForumSpamCleaner : System.Web.UI.UserControl
    {
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
    }
}