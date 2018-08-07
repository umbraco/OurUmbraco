using System;
using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Community.People;
using OurUmbraco.Our.Models;
using reCAPTCHA.MVC;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class RegisterController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render()
        {
            var registerModel = new RegisterModel();
            return PartialView("~/Views/Partials/Members/Register.cshtml", registerModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CaptchaValidator]
        public ActionResult HandleSubmit(RegisterModel model)
        {
            var recaptcha = ModelState["ReCaptcha"];
            if (recaptcha != null && HttpContext.Request.IsLocal)
                recaptcha.Errors.Clear();

            if (!ModelState.IsValid || model.AgreeTerms == false)
            {
                if (model.AgreeTerms == false)
                    ModelState.AddModelError("AgreeTerms", "You can only continue if you agree to our terms and conditions.");

                return CurrentUmbracoPage();
            }

            var memberService = Services.MemberService;

            if (memberService.GetByEmail(model.Email) != null)
            {
                ModelState.AddModelError("Email", "A member with that email address already exists");
                return CurrentUmbracoPage();
            }

            if (string.IsNullOrWhiteSpace(model.Flickr) == false || string.IsNullOrWhiteSpace(model.Bio) == false)
            {
                //These fields are hidden, only a bot will know to fill them in
                //This honeypot catches them
                return Redirect("/");
            }

            // these values are enforced in MemberDto which is internal ;-(
            // we should really have ways to query for Core meta-data!
            const int maxEmailLength = 400;
            const int maxLoginNameLength = 200;
            const int maxPasswordLength = 400;
            const int maxPropertyLength = 400;

            if (model.Email != null && model.Email.Length > maxEmailLength
                || model.Name != null && model.Name.Length > maxLoginNameLength
                || model.Password != null && model.Password.Length > maxPasswordLength
                || model.Location != null && model.Location.Length > maxPropertyLength
                || model.Longitude != null && model.Longitude.Length > maxPropertyLength
                || model.Latitude != null && model.Latitude.Length > maxPropertyLength
                || model.TwitterAlias != null && model.TwitterAlias.Length > maxPropertyLength
                || model.GitHubUsername != null && model.GitHubUsername.Length > maxPropertyLength
                )
            {
                // has to be a rogue registration
                // go away!
                return Redirect("/");
            }

            var member = memberService.CreateMember(model.Email, model.Email, model.Name, "member");
            member.SetValue("location", model.Location);
            member.SetValue("longitude", model.Longitude);
            member.SetValue("latitude", model.Latitude);
            member.SetValue("company", model.Company);
            member.SetValue("twitter", model.TwitterAlias);
            member.SetValue("github", model.GitHubUsername);

            member.SetValue("treshold", "-10");
            member.SetValue("bugMeNot", false);

            member.SetValue("reputationTotal", 20);
            member.SetValue("reputationCurrent", 20);
            member.SetValue("forumPosts", 0);

            member.SetValue("tos", DateTime.Now);

            member.IsApproved = false;
            memberService.Save(member);

            // Now that we have a memberId we can use it
            var avatarPath = GetAvatarPath(member);
            member.SetValue("avatar", avatarPath);
            memberService.Save(member);

            memberService.AssignRole(member.Username, "standard");

            memberService.SavePassword(member, model.Password);

            Members.Login(model.Email, model.Password);

            var emailService = new Services.EmailService();
            emailService.SendActivationMail(member);

            memberService.AssignRole(member.Id, "notactivated");
            memberService.AssignRole(member.Id, "newaccount");

            var redirectPage = "/";
            var contentService = ApplicationContext.Current.Services.ContentService;
            var rootNode = contentService.GetRootContent().OrderBy(x => x.SortOrder).First(x => x.ContentType.Alias == "Community");

            var memberNode = rootNode.Children().FirstOrDefault(x => x.Name == "Member");
            if (memberNode != null)
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var pendingActivationPage = memberNode.Children().FirstOrDefault(x => x.Name == "Pending activation");
                if (pendingActivationPage != null)
                {
                    var pendingActivationContentItem = umbracoHelper.TypedContent(pendingActivationPage.Id);
                    if (pendingActivationContentItem != null)
                        redirectPage = pendingActivationContentItem.Url;
                }
            }

            return Redirect(redirectPage);
        }

        private static string GetAvatarPath(IMember member)
        {
            var avatarService = new AvatarService();
            var avatarPath = avatarService.GetMemberAvatar(member);
            return avatarPath.Contains("?") ? avatarPath.Substring(0, avatarPath.IndexOf("?", StringComparison.Ordinal)) : avatarPath;
        }
    }
}
