using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using reCAPTCHA.MVC;
using Umbraco.Core;
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
                if (memberToLogin.CreateDate < DateTime.Parse("2015-07-23") && memberToLogin.Properties.Contains(Constants.Conventions.Member.IsApproved) && memberToLogin.IsApproved == false)
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
            if (m.Properties.Contains(Constants.Conventions.Member.IsApproved) && m.IsApproved == false)
            {
                m.IsApproved = true;
                memberService.Save(m, false);
            }

            var resetToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var hashCode = SecureHasher.Hash(resetToken);
            var expiryDate = DateTime.Now.AddDays(1);

            m.SetValue("passwordResetToken", hashCode);
            m.SetValue("passwordResetTokenExpiryDate", expiryDate.ToString(CultureInfo.InvariantCulture));
            memberService.Save(m);

            var resetLink = "https://our.umbraco.org/member/reset-password/?token=" + resetToken + "&email=" + m.Email;

            var mail = "<p>Hi " + m.Name + "</p>";
            mail = mail + "<p>Someone requested a password reset for your account on https://our.umbraco.org</p>";
            mail = mail + "<p>If this wasn't you then you can ignore this email, otherwise, please click the following password reset link to continue:</p>";
            mail = mail + "<p>Please go to <a href=\"" + resetLink + "\">" + resetLink + "</a> to reset your password.</p>";
            mail = mail + "<br/><br/><p>All the best<br/> <em>The email robot</em></p>";

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = "Password reset requested for our.umbraco.org";
                mailMessage.Body = mail;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(m.Email));
                mailMessage.From = new MailAddress("robot@umbraco.org");

                using (var smtpClient = new SmtpClient())
                    smtpClient.Send(mailMessage);
            }

            return Redirect(CurrentPage.Url + "?success=true");
        }
    }

    internal class DuplicateMember
    {
        public int MemberId { get; set; }
        public int TotalKarma { get; set; }
    }
}
