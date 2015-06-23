using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace our
{
    public class Hiccup : Page
    {
        protected Literal titleLiteral;
        protected Literal header1;
        protected Literal header3;
        protected Literal notAffect;

        protected void Page_Load(object sender, EventArgs e)
        {
            var host = Request.Url.Host.Replace("http://", "");
            titleLiteral.Text = host;
            header1.Text = host;
            header3.Text = host;
            notAffect.Text = host;
        }
    }
}
