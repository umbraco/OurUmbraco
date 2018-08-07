using System.Web.Hosting;
using System.Web.Mvc;
using OurUmbraco.Community.People;
using OurUmbraco.Our.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class ProfileController: SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render()
        {
            var memberService = Services.MemberService;
            var member = memberService.GetById(Members.GetCurrentMemberId());
            var avatarService = new AvatarService();
            var avatarPath = avatarService.GetMemberAvatar(member);
            var avatarHtml = avatarService.GetImgWithSrcSet(avatarPath, member.Name, 100);

            var profileModel = new ProfileModel
            {
                Name = member.Name,
                Email = member.Email,
                Bio = member.GetValue<string>("profileText"),
                Location = member.GetValue<string>("location"),
                Company = member.GetValue<string>("company"),
                TwitterAlias = member.GetValue<string>("twitter"),
                Avatar = avatarPath,
                AvatarHtml = avatarHtml,
                GitHubUsername = member.GetValue<string>("github"),

                Latitude = member.GetValue<string>("latitude"), //TODO: Parse/cleanup bad data - auto remove it for user & resave the member?
                Longitude = member.GetValue<string>("longitude")
            };

            return PartialView("~/Views/Partials/Members/Profile.cshtml", profileModel);
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
            mem.SetValue("github", model.GitHubUsername);
            
            // Assume it's valid lat/lon data posted - as its a hidden field that a Google Map will update the lat & lon of hidden fields when marker moved
            mem.SetValue("latitude", model.Latitude); 
            mem.SetValue("longitude", model.Longitude);
            
            var avatarService = new AvatarService();
            var avatarImage = avatarService.GetMemberAvatarImage(HostingEnvironment.MapPath($"~{model.Avatar}"));
            if (avatarImage != null && (avatarImage.Width < 400 || avatarImage.Height < 400))
            {
                // Save the rest of the data, but not the new avatar yet as it's too small
                ms.Save(mem);
                ModelState.AddModelError("Avatar", "Please upload an avatar that is at least 400x400 pixels");
                return CurrentUmbracoPage();
            }

            mem.SetValue("avatar", model.Avatar);
            ms.Save(mem);

            if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.RepeatPassword) && model.Password == model.RepeatPassword)
                ms.SavePassword(mem, model.Password);

            ApplicationContext.ApplicationCache.RuntimeCache.ClearCacheItem("MemberData" + memberPreviousUserName);
            TempData["success"] = true;

            return RedirectToCurrentUmbracoPage();
        }
    }
}
