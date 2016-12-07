using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;
using OurUmbraco.Our.Models;
using OurUmbraco.Our.usercontrols;
using reCAPTCHA.MVC;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
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
            var locationInvalid = string.IsNullOrEmpty(model.Latitude) || string.IsNullOrEmpty(model.Longitude);
            if (!ModelState.IsValid || locationInvalid || model.AgreeTerms == false)
            {
                if(locationInvalid)
                    ModelState.AddModelError("Location", "Please tell us a little bit about where you live.");

                if(model.AgreeTerms == false)
                    ModelState.AddModelError("AgreeTerms", "You can only continue if you agree to our terms and conditions.");

                return CurrentUmbracoPage();
            }

            var memberService = Services.MemberService;

            if (memberService.GetByEmail(model.Email) != null)
            {
                ModelState.AddModelError("Email", "A member with that email address already exists");
                return CurrentUmbracoPage();
            }

            // If spammer then this will stop account creation
            var spamResult = Forum.Library.Utils.CheckForSpam(model.Email, model.Name, true);
            if (spamResult != null && spamResult.Blocked)
                return Redirect("/");

            if (string.IsNullOrWhiteSpace(model.Flickr) == false || string.IsNullOrWhiteSpace(model.Bio) == false)
            {
                //These fields are hidden, only a bot will know to fill them in
                //This honeypot catches them
                return Redirect("/");
            }

            // these values are enforced in MemberDto which is internal ;-(
            // we should really have ways to query for Core meta-data!
            const int maxEmailLength = 900; // 1000*.9 for safety
            const int maxLoginNameLength = 900; // 1000*.9 for safety
            const int maxPasswordLength = 900; // 1000*.9 for safety
            const int maxPropertyLength = 900; // keep it safe too

            if (model.Email.Length > maxEmailLength
                || model.Name.Length > maxLoginNameLength
                || model.Password.Length > maxPasswordLength
                || model.Location.Length > maxPropertyLength
                || model.Longitude.Length > maxPropertyLength
                || model.Latitude.Length > maxPropertyLength
                || model.Company.Length > maxPropertyLength
                || model.TwitterAlias.Length > maxPropertyLength
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

            if (spamResult != null && spamResult.TotalScore >= int.Parse(ConfigurationManager.AppSettings["PotentialSpammerThreshold"]))
            {
                spamResult.MemberId = member.Id;

                memberService.AssignRole(member.Id, "potentialspam");
                Forum.Library.Utils.SendPotentialSpamMemberMail(spamResult);
            }
            else
            {
                Forum.Library.Utils.SendActivationMail(member);
                Forum.Library.Utils.SendMemberSignupMail(member);
            }
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
                    if(pendingActivationContentItem != null)
                        redirectPage = pendingActivationContentItem.Url;
                }
            }

            return Redirect(redirectPage);
        }

        private static string GetAvatarPath(IMembershipUser member)
        {
            var url = "http://www.gravatar.com/avatar/" + member.Email.ToMd5() + "?s=400&d=retro";

            try
            {
                var avatarFileName = "/media/avatar/" + member.Id + ".jpg";
                var path = HostingEnvironment.MapPath(avatarFileName);

                if (path != null)
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);

                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile(url, path);
                    }

                    return avatarFileName;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Signup>("Could not save gravatar locally", ex);
            }

            return url;
        }
    }
}
