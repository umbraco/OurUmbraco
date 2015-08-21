using System;
using Umbraco.Web.UI.Controls;

namespace OurUmbraco.Project.usercontrols.Deli.Package.Steps
{
    public partial class Details : UmbracoUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("/member/profile/projects/details?projectId=" + Request.QueryString["id"]);
        }
    }
}