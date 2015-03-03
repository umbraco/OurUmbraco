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
    public class ProfileController: SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render()
        {
            var ms = Services.MemberService;
            var mem = ms.GetById(Members.GetCurrentMemberId());
            var m = new ProfileModel();
            m.Name = mem.Name;
            m.Email = mem.Email;
            m.Bio = mem.GetValue<string>("profileText");
            m.Location = mem.GetValue<string>("location");
            m.Company = mem.GetValue<string>("company");
            m.TwitterAlias = mem.GetValue<string>("twitter");
            m.Avatar = mem.GetValue<string>("avatar");

            return PartialView("~/Views/Partials/Members/Profile.cshtml", m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleSubmit(ProfileModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var ms = Services.MemberService;
            var mem = ms.GetById(Members.GetCurrentMemberId());

            if (mem.Email != model.Email && ms.GetByEmail(model.Email) != null)
            {
                ModelState.AddModelError("Email", "A Member with that email already exists");
                return CurrentUmbracoPage();

            
            }

            if(model.Password != model.RepeatPassword)
            {
                ModelState.AddModelError("Password", "Passwords need to match");
                ModelState.AddModelError("RepeatPassword", "Passwords need to match");
                return CurrentUmbracoPage();
            }

            mem.Name = model.Name ;
            mem.Email = model.Email;
            mem.Username = model.Email;
            mem.SetValue("profileText",model.Bio);
            mem.SetValue("location",model.Location);
            mem.SetValue("company",model.Company);
            mem.SetValue("twitter",model.TwitterAlias);
            mem.SetValue("avatar", model.Avatar);
            ms.Save(mem);

            if(!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.RepeatPassword) && model.Password == model.RepeatPassword)
                ms.SavePassword(mem, model.Password);       

            TempData["success"] = true;

            return RedirectToCurrentUmbracoPage();
        }
    }
}
