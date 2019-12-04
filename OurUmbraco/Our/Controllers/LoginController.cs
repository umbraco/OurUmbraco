using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using OurUmbraco.Community.GitHub;
using reCAPTCHA.MVC;
using Skybrud.Social.GitHub.Models.Users;
using Skybrud.Social.GitHub.OAuth;
using Skybrud.Social.GitHub.Responses.Authentication;
using Skybrud.Social.GitHub.Responses.Users;
using Skybrud.Social.GitHub.Scopes;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class LoginController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult RenderLogin()
        {
            var loginModel = new LoginModel();
            return PartialView("~/Views/Partials/Members/Login.cshtml", loginModel);
        }

        [ChildActionOnly]
        public ActionResult RenderForgotPassword()
        {
            var loginModel = new LoginModel();
            return PartialView("~/Views/Partials/Members/ForgotPassword.cshtml", loginModel);
        }
        
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid == false)
                return CurrentUmbracoPage();

            var memberService = Services.MemberService;
            int totalMembers;
            var members = memberService.FindByEmail(model.Username, 0, 100, out totalMembers).ToList();
            if (totalMembers > 1)
            {
                var duplicateMembers = new List<DuplicateMember>();
                foreach (var member in members)
                {
                    var totalKarma = member.GetValue<int>("reputationTotal");
                    var duplicateMember = new DuplicateMember { MemberId = member.Id, TotalKarma = totalKarma };
                    duplicateMembers.Add(duplicateMember);
                }

                // rename username/email for each duplicate member
                // EXCEPT for the one with the highest karma (Skip(1))
                foreach (var duplicateMember in duplicateMembers.OrderByDescending(x => x.TotalKarma).ThenByDescending(x => x.MemberId).Skip(1))
                {
                    var member = memberService.GetById(duplicateMember.MemberId);
                    var newUserName = member.Username.Replace("@", "@__" + member.Id);
                    member.Username = newUserName;
                    member.Email = newUserName;
                    memberService.Save(member, false);
                }
            }

            var memberToLogin = memberService.FindByEmail(model.Username, 0, 1, out totalMembers).SingleOrDefault();
            if (memberToLogin != null)
            {
                // Note: After July 23rd 2015 we need people to activate their accounts! Don't approve automatically
                // otherwise:
                // Automatically approve all members, as we don't have an approval process now
                // This is needed as we added new membership after upgrading so IsApproved is 
                // currently empty. First time a member gets saved now (login also saves the member)
                // IsApproved would turn false (default value of bool) so we want to prevent that
                if (memberToLogin.CreateDate < DateTime.Parse("2015-07-23") && memberToLogin.Properties.Contains(global::Umbraco.Core.Constants.Conventions.Member.IsApproved) && memberToLogin.IsApproved == false)
                {
                    memberToLogin.IsApproved = true;
                    memberService.Save(memberToLogin, false);
                }
            }

            if (Members.Login(model.Username, model.Password))
            {
                return Redirect("/");
            }

            ModelState.AddModelError("", "That username and password combination isn't valid here");
            return CurrentUmbracoPage();
        }

        [CaptchaValidator]
        public ActionResult ForgotPassword(LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
                return CurrentUmbracoPage();

            var memberService = Services.MemberService;
            int totalMembers;
            var members = memberService.FindByEmail(model.Username, 0, 100, out totalMembers);
            if (totalMembers > 1)
            {
                var duplicateMembers = new List<DuplicateMember>();
                foreach (var member in members)
                {
                    var totalKarma = member.GetValue<int>("reputationTotal");
                    var duplicateMember = new DuplicateMember { MemberId = member.Id, TotalKarma = totalKarma };
                    duplicateMembers.Add(duplicateMember);
                }

                // rename username/email for each duplicate member
                // EXCEPT for the one with the highest karma (Skip(1))
                foreach (
                    var duplicateMember in
                        duplicateMembers.OrderByDescending(x => x.TotalKarma).ThenByDescending(x => x.MemberId).Skip(1))
                {
                    var member = memberService.GetById(duplicateMember.MemberId);
                    var newUserName = member.Username.Replace("@", "@__" + member.Id);
                    member.Username = newUserName;
                    member.Email = newUserName;
                    memberService.Save(member);
                }
            }

            var m = memberService.GetByEmail(model.Username);
            if (m == null)
            {
                // Don't add an error and reveal that someone with this email address exists on this site
                return Redirect(CurrentPage.Url + "?success=true");
            }

            // Automatically approve all members, as we don't have an approval process now
            // This is needed as we added new membership after upgrading so IsApproved is 
            // currently empty. First time a member gets saved now (login also saves the member)
            // IsApproved would turn false (default value of bool) so we want to prevent that
            if (m.Properties.Contains(global::Umbraco.Core.Constants.Conventions.Member.IsApproved) && m.IsApproved == false)
            {
                m.IsApproved = true;
                memberService.Save(m, false);
            }

            var resetToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var hashCode = SecureHasher.Hash(resetToken);
            var expiryDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");

            m.SetValue("passwordResetToken", hashCode);
            m.SetValue("passwordResetTokenExpiryDate", expiryDate.ToString(CultureInfo.InvariantCulture));
            memberService.Save(m);

            var resetLink = "https://our.umbraco.com/member/reset-password/?token=" + resetToken + "&email=" + m.Email;

            var mail = "<p>Hi " + m.Name + "</p>";
            mail = mail + "<p>Someone requested a password reset for your account on https://our.umbraco.com</p>";
            mail = mail + "<p>If this wasn't you then you can ignore this email, otherwise, please click the following password reset link to continue:</p>";
            mail = mail + "<p>Please go to <a href=\"" + resetLink + "\">" + resetLink + "</a> to reset your password.</p>";
            mail = mail + "<br/><br/><p>All the best<br/> <em>The email robot</em></p>";

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = "Password reset requested for our.umbraco.com";
                mailMessage.Body = mail;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(m.Email));
                mailMessage.From = new MailAddress("robot@umbraco.org");

                using (var smtpClient = new SmtpClient())
                    smtpClient.Send(mailMessage);
            }

            return Redirect(CurrentPage.Url + "?success=true");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GitHub()
        {

            string rootUrl = Request.Url.GetLeftPart(UriPartial.Authority);

            GitHubOAuthClient client = new GitHubOAuthClient();
            client.ClientId = WebConfigurationManager.AppSettings["GitHubClientId"];
            client.ClientSecret = WebConfigurationManager.AppSettings["GitHubClientSecret"];
            client.RedirectUri = rootUrl + "/umbraco/surface/Login/GitHub";

            // Set the state (a unique/random value)
            string state = Guid.NewGuid().ToString();
            Session["GitHub_" + state] = "Unicorn rainbows";

            // Construct the authorization URL
            string authorizatioUrl = client.GetAuthorizationUrl(state, GitHubScopes.UserEmail);

            // Redirect the user to the OAuth dialog
            return Redirect(authorizatioUrl);

        }

        [HttpGet]
        public ActionResult GitHub(string state, string code)
        {

            // Get the member of the current ID
            int memberId = Members.GetCurrentMemberId();
            if (memberId > 0) return GetErrorResult("It seems that you're already logged into Our Umbraco. Log out out if you wish to log in with another account.");

            GitHubService github = new GitHubService();

            try
            {

                IPublishedContent profilePage = Umbraco.TypedContent(1057);
                if (profilePage == null)
                {
                    LogHelper.Info<LoginController>("Profile page not found. Has this been unpublished in Umbraco?");
                    return GetErrorResult("Oh noes! This really shouldn't happen. This is us, not you.");
                }

                // Initialize the OAuth client
                GitHubOAuthClient client = new GitHubOAuthClient
                {
                    ClientId = WebConfigurationManager.AppSettings["GitHubClientId"],
                    ClientSecret = WebConfigurationManager.AppSettings["GitHubClientSecret"]
                };

                // Validate state - Step 1
                if (string.IsNullOrWhiteSpace(state))
                {
                    LogHelper.Info<LoginController>("No OAuth state specified in the query string.");
                    return GetErrorResult("No state specified in the query string. Please go back to the login page and click the \"Login with GitHub\" to try again ;)");
                }

                // Validate state - Step 2
                string session = Session["GitHub_" + state] as string;
                if (string.IsNullOrWhiteSpace(session))
                {
                    LogHelper.Info<LoginController>("Failed finding OAuth session item. Most likely the session expired.");
                    return GetErrorResult("Session expired? Please go back to the login page and click the \"Login with GitHub\" to try again ;)");
                }

                // Remove the state from the session
                Session.Remove("GitHub_" + state);

                // Exchange the auth code for an access token
                GitHubTokenResponse accessTokenResponse;
                try
                {
                    accessTokenResponse = client.GetAccessTokenFromAuthorizationCode(code);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<LoginController>("Unable to retrieve access token from GitHub API", ex);
                    return GetErrorResult("Oh noes! An error happened in the communication with the GitHub API. It may help going back to the login page and click the \"Login with GitHub\" to try again ;)");
                }
                
                // Initialize a new service instance from the retrieved access token
                var service = Skybrud.Social.GitHub.GitHubService.CreateFromAccessToken(accessTokenResponse.Body.AccessToken);

                // Get some information about the authenticated GitHub user
                GitHubUser user;
                try
                {
                    user = service.User.GetUser().Body;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<LoginController>("Unable to get user information from the GitHub API", ex);
                    return GetErrorResult("Oh noes! An error happened in the communication with the GitHub API. It may help going back to the login page and click the \"Login with GitHub\" to try again ;)");
                }

                // Get the total amount of members with the GitHub user ID
                var memberIds = github.GetMemberIdsFromGitHubUserId(user.Id);

                // No matching members means that the GitHub user has not yet been linked with any Our members, so we
                // create a new Our member instead
                if (memberIds.Length == 0)
                {
                    return RegisterFromGitHub(service, user);
                }

                // More than one matching member indicates an error as there should not be more than one Our member
                // linked with the same GitHub user
                if (memberIds.Length > 1)
                {
                    LogHelper.Info<LoginController>("Multiple Our members are linked with the same GitHub account: " + user.Login + " (ID: " + user.Id + "). Matching member IDs are: " + string.Join(", ", memberIds));
                    return GetErrorResult("Oh noes! This really shouldn't happen. This is us, not you.");
                }

                // Get a reference to the member
                var member = Services.MemberService.GetById(memberIds[0]);

                // Check whether the member was found
                if (member == null)
                {
                    LogHelper.Info<LoginController>("No Our member found with the ID " + memberIds[0]);
                    return GetErrorResult("Oh noes! This really shouldn't happen. This is us, not you.");
                }

                FormsAuthentication.SetAuthCookie(member.Username, false);
                return Redirect("/");

            }
            catch (Exception ex)
            {
                LogHelper.Error<LoginController>("Failed logging in member from GitHub", ex);
                return GetErrorResult("Oh noes! This really shouldn't happen. This is us, not you." + ex);
            }

        }

        private ActionResult RegisterFromGitHub(Skybrud.Social.GitHub.GitHubService service, GitHubUser user)
        {
            
            var memberService = Services.MemberService;

            string email = user.Email;

            if (string.IsNullOrWhiteSpace(email))
            {

                var response = service.User.GetEmails();

                var primary = response.Body.FirstOrDefault(x => x.IsPrimary);

                if (primary == null)
                {
                    return GetErrorResult("Your primary email address on GitHub is missing.");
                }

                if (primary.IsVerified == false)
                {
                    return GetErrorResult("Your primary email address on GitHub is not verified. Make sure to verify your email address, and then try again.");
                }

                email = primary.Email;

            }

            if (memberService.GetByEmail(email) != null)
            {
                return GetErrorResult("A member with that email address already exists.");
            }

            // Since names are not mandatory on Github we use the username as fallback
            var name = !string.IsNullOrWhiteSpace(user.Name) ? user.Name : user.Login;

            var member = memberService.CreateMember(email, email, name, "member");
            member.SetValue("github", user.Login);
            member.SetValue("githubId", user.Id);
            member.SetValue("githubData", user.JObject.ToString());

            member.SetValue("treshold", "-10");
            member.SetValue("bugMeNot", false);

            member.SetValue("reputationTotal", 20);
            member.SetValue("reputationCurrent", 20);
            member.SetValue("forumPosts", 0);

            //member.SetValue("tos", DateTime.Now);

            member.IsApproved = true;
            memberService.Save(member);

            memberService.AssignRole(member.Username, "standard");

            FormsAuthentication.SetAuthCookie(email, false);
            return Redirect("/");

        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("/");
        }

        private ActionResult GetErrorResult(string message)
        {
            return View("~/Views/Minimal/GitHub/Error.cshtml", new GitHubLoginError
            {
                Title = "Authentication with GitHub failed",
                Message = message,
                PrimaryActionText = "Return to the login page",
                PrimaryActionUrl = "/member/login/"
            });
        }
    }

    public class GitHubLoginError
    {

        public string Title { get; set; }

        public string Message { get; set; }

        public string PrimaryActionText { get; set; }

        public string PrimaryActionUrl { get; set; }

        public bool HasPrimaryAction => string.IsNullOrWhiteSpace(PrimaryActionText) == false && string.IsNullOrWhiteSpace(PrimaryActionUrl) == false;

    }

    internal class DuplicateMember
    {
        public int MemberId { get; set; }
        public int TotalKarma { get; set; }
    }
}
