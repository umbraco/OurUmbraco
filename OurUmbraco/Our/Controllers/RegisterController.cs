using System.Web.Mvc;
using OurUmbraco.Our.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class RegisterController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render()
        {

            var m = new RegisterModel();

            return PartialView("~/Views/Partials/Members/Register.cshtml", m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleSubmit(ProfileModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var ms = Services.MemberService;
           

            if (ms.GetByEmail(model.Email) != null)
            {
                ModelState.AddModelError("Email", "A Member with that email already exists");
                return CurrentUmbracoPage();


            }

            if (model.Password != model.RepeatPassword)
            {
                ModelState.AddModelError("Password", "Passwords need to match");
                ModelState.AddModelError("RepeatPassword", "Passwords need to match");
                return CurrentUmbracoPage();
            }

            var mem = ms.CreateMemberWithIdentity(model.Email, model.Email, model.Name, "member");

            mem.SetValue("profileText", model.Bio);
            mem.SetValue("location", model.Location);
            mem.SetValue("company", model.Company);
            mem.SetValue("twitter", model.TwitterAlias);
            mem.SetValue("avatar", model.Avatar);
           

            ms.AssignRole(mem.Username, "standard");
           
            ms.Save(mem);
            
            ms.SavePassword(mem, model.Password);         

            Members.Login(model.Email, model.Password);

            return Redirect("/");

        }
    }
}
