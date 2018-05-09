using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using System.Web.Security;
using OurUmbraco.Our.Models;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class ResetPasswordController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render()
        {
            bool.TryParse(Request.QueryString["success"], out var success);

            var model = new PasswordResetModel
            {
                Email = Request.QueryString["email"],
                Token = Request.QueryString["token"],
                Success = success
            };

            if (success == false)
            {
                var member = VerifyResetData(model);
                if (member == null)
                    model.Error = "Can't reset your password with the provided information";
            }

            return PartialView("~/Views/Partials/Members/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(PasswordResetModel model)
        {
            if (ModelState.IsValid == false)
                return CurrentUmbracoPage();

            var member = VerifyResetData(model);
            if (member == null)
            {
                ModelState.AddModelError("", "Can't reset your password with the provided information");
                return CurrentUmbracoPage();
            }

            var queryString = new NameValueCollection();

            try
            {
                var memberShip = Membership.GetUser(model.Email);
                if (memberShip != null)
                {
                    var memberService = ApplicationContext.Current.Services.MemberService;

                    // We need to unlock first else we are not allowed to do ResetPassword
                    member.IsLockedOut = false;
                    member.FailedPasswordAttempts = 0;
                    memberService.Save(member);

                    // Need to do this silly little dance, generate a new password first and
                    // then use it to reset the users password with the one they gave us.
                    // This is because allowManuallyChangingPassword="false"
                    var newPassword = memberShip.ResetPassword();
                    memberShip.ChangePassword(newPassword, model.NewPassword);

                    // Reset the token and expiry fields so that the password reset
                    // can not be repeated by using the same reset link
                    member.SetValue("passwordResetToken", string.Empty);
                    member.SetValue("passwordResetTokenExpiryDate", string.Empty);

                    memberService.Save(member);
                    queryString.Add("success", "true");
                }
                else
                {
                    ModelState.AddModelError("", "Can't reset your password with the provided information");
                    return CurrentUmbracoPage();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<ResetPasswordController>("Couldn't reset password", ex);
            }
            
            return RedirectToCurrentUmbracoPage(queryString);
        }

        private static IMember VerifyResetData(PasswordResetModel model)
        {
            try
            {
                var memberService = ApplicationContext.Current.Services.MemberService;
                var member = memberService.GetByEmail(model.Email);
                if (member == null)
                    return null;

                if (member.GetValue<DateTime>("passwordResetTokenExpiryDate") < DateTime.Now)
                    return null;

                var hashedToken = member.GetValue<string>("passwordResetToken");
                var verifyToken = SecureHasher.Verify(model.Token, hashedToken);

                return verifyToken ? member : null;
            }
            catch (Exception ex)
            {
                LogHelper.Error<ResetPasswordController>("Couldn't verify reset data", ex);
            }

            return null;
        }
    }
}
