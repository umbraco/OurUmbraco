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

            var m = new ProfileModel
            {
                Name = mem.Name,
                Email = mem.Email,
                Bio = mem.GetValue<string>("profileText"),
                Location = mem.GetValue<string>("location"),
                Company = mem.GetValue<string>("company"),
                TwitterAlias = mem.GetValue<string>("twitter"),
                Avatar = mem.GetValue<string>("avatar"),
                GitHubUsername = mem.GetValue<string>("github"),

                Latitude = mem.GetValue<string>("latitude"), //TODO: Parse/cleanup bad data - auto remove it for user & resave the member?
                Longitude = mem.GetValue<string>("longitude")
            };

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

            //Assume it's valid lat/lon data posted - as its a hidden field that a Google Map will update the lat & lon of hidden fields when marker moved
            mem.SetValue("latitude", model.Latitude); 
            mem.SetValue("longitude", model.Longitude);

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
