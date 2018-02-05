using System.Web.Mvc;
using OurUmbraco.Our.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
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
            m.GitHubUsername = mem.GetValue<string>("github");

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

            var memberPreviousUserName = mem.Username;

            if (model.Password != model.RepeatPassword)
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
            mem.SetValue("github", model.GitHubUsername);
            ms.Save(mem);

            var avatarImage = Utils.GetMemberAvatarImage(Members.GetById(mem.Id));
            if (avatarImage != null && (avatarImage.Width < 400 || avatarImage.Height < 400))
            {
                ModelState.AddModelError("Avatar", "Please upload an avatar that is at least 400x400 pixels");
                return CurrentUmbracoPage();
            }

            if(!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.RepeatPassword) && model.Password == model.RepeatPassword)
                ms.SavePassword(mem, model.Password);
            ApplicationContext.ApplicationCache.RuntimeCache.ClearCacheItem("MemberData" + memberPreviousUserName);
                
            TempData["success"] = true;


            return RedirectToCurrentUmbracoPage();
        }
    }
}
