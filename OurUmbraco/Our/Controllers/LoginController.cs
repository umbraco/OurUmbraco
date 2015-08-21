using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class LoginController: SurfaceController
    {
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return Redirect("/");
        }
    }
}
