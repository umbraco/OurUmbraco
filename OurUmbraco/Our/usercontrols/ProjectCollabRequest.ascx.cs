using System;
using System.Net.Mail;
using umbraco.cms.businesslogic.member;

namespace OurUmbraco.Our.usercontrols
{
    public partial class ProjectCollabRequest : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int pId = 0;

                if (!string.IsNullOrEmpty(Request.QueryString["id"]) && int.TryParse(Request.QueryString["id"], out pId) && umbraco.library.IsLoggedOn())
                {
                    umbraco.presentation.nodeFactory.Node p = new umbraco.presentation.nodeFactory.Node(pId);

                    lt_title.Text = p.Name;

                }
                else
                    projectCollabForm.Visible = false;
            }
        }

        protected void bt_submit_Click(object sender, EventArgs e)
        {
            umbraco.presentation.nodeFactory.Node p = new umbraco.presentation.nodeFactory.Node(int.Parse(Request.QueryString["id"]));

            Member owner = new Member(int.Parse(p.GetProperty("owner").Value));
            Member m = Member.GetCurrentMember();


            MailMessage mm = new MailMessage();
            mm.Subject = "Umbraco community: Request to contribute to project";

            mm.Body =
                string.Format("The Umbraco Community member '{0}'  would like to contribute to your project '{1}'.  You can add the member to the project from your profile on our.umbraco.com.",
                m.Text, p.Name);

            mm.Body = mm.Body + string.Format("\n\r\n\rMessage from {0}: \n\r\n\r", m.Text) + tb_message.Text;

            mm.To.Add(owner.Email);
            mm.From = new MailAddress(m.Email);

            SmtpClient c = new SmtpClient();
            c.Send(mm);


            umbraco.presentation.nodeFactory.Node current = umbraco.presentation.nodeFactory.Node.GetCurrent();

            Response.Redirect(umbraco.library.NiceUrl(current.Children[0].Id));
        }
    }
}