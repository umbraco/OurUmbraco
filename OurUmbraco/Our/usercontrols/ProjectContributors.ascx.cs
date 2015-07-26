using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using our.Businesslogic;

namespace our.usercontrols
{
    public partial class ProjectContributors : System.Web.UI.UserControl
    {
        private int pageId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (umbraco.library.IsLoggedOn() && int.TryParse(Request.QueryString["id"], out pageId)) {

                Member mem = Member.GetCurrentMember();
                Document d = new Document(pageId);

                if ((d.getProperty("owner") != null && d.getProperty("owner").Value.ToString() == mem.Id.ToString()))
                {

                    holder.Visible = true;
                }
                else
                {
                    Response.Redirect("/");
                }

                }
        }

        protected void bt_submit_Click(object sender, EventArgs e)
        {
            confirm.Visible = false;
            error.Visible = false;

            Member m = Member.GetCurrentMember();

            int pId = 0;

            if (!string.IsNullOrEmpty(Request.QueryString["id"]) && int.TryParse(Request.QueryString["id"], out pId) && umbraco.library.IsLoggedOn())
            {

                Document d = new Document(pId);

                if ((int)d.getProperty("owner").Value == m.Id)
                {
                    Member c = Member.GetMemberFromLoginName(tb_email.Text);

                    if (c != null && c.Id != m.Id)
                    {
                        //member found

                        ProjectContributor pc = new ProjectContributor(d.Id, c.Id);
                        pc.Add();

                        confirm.Visible = true;
                        tb_email.Text = "";

                    }
                    else
                    {
                       
                        //member not found
                        error.Visible = true;
                    }
                }
            }

           
        }
    }
}