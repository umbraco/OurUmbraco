using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.member;

namespace our.usercontrols
{
    public partial class acceptTos : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void cool_Click(object sender, EventArgs e)
        {
            if (fair.Checked)
            {
                Member mem = Member.GetCurrentMember();
                if (mem != null)
                {
                    mem.getProperty("tos").Value =
                        DateTime.Now.ToString("s");
                    Response.Redirect("/");
                }
            }
            else {
                error.Visible = true;
            }
        }
    }
}