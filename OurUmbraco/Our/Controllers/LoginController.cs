using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;

namespace our.Controllers
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
