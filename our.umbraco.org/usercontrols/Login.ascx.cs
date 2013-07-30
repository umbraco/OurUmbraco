using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace our.usercontrols {
    public partial class Login : System.Web.UI.UserControl {
        public int NextPage { get; set; }
        public string ErrorMessage { get; set; }

        protected void Page_Load(object sender, EventArgs e) {
            umbraco.library.RegisterJavaScriptFile("jquery.validation", "/scripts/jquery.validation.js");
        }

        protected void login(object sender, EventArgs e) {
            string email = tb_email.Text;
            string password = tb_password.Text;
            string redirectUrl = Request.QueryString["redirectUrl"];

            umbraco.cms.businesslogic.member.Member m = umbraco.cms.businesslogic.member.Member.GetMemberFromLoginNameAndPassword(email, password);

            if (m != null) {
                umbraco.cms.businesslogic.member.Member.AddMemberToCache(m, false, new TimeSpan(30, 0, 0, 0));

                uForum.Businesslogic.ForumEditor.SetEditorChoiceFromMemberProfile(m.Id);

                if (!string.IsNullOrEmpty(redirectUrl))
                    Response.Redirect(redirectUrl);

                if (NextPage > 0)
                    Response.Redirect(umbraco.library.NiceUrl(NextPage));

            } else {
                lt_err.Text = "<div class='error'><p>" + ErrorMessage + "</p></div>";
                lt_err.Visible = true;
            }
        }
    }
}