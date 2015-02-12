using our.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace our.Api
{
    public class ProfileController: SurfaceController
    {
        public ActionResult Edit(ProfileModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            return RedirectToCurrentUmbracoPage();
        }
    }
}
