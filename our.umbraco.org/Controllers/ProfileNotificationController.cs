using our.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace our.Controllers
{
    public class ProfileNotificationController: SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render()
        {
            var ms = Services.MemberService;
            var mem = ms.GetById(Members.GetCurrentMemberId());
            var m = new ProfileNotificationModel();
            m.DontBug = mem.GetValue<bool>("bugMeNot");

            return PartialView("~/Views/Partials/Members/ProfileNotification.cshtml", m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleSubmit(ProfileNotificationModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var ms = Services.MemberService;
            var mem = ms.GetById(Members.GetCurrentMemberId());

            mem.SetValue("bugMeNot", model.DontBug);

            ms.Save(mem);

            TempData["success"] = true;

            return RedirectToCurrentUmbracoPage();

        }
    }
}
