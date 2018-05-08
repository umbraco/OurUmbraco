using System;
using System.Web.Mvc;
using System.Web.Security;
using OurUmbraco.Our.Models;
using Umbraco.Core;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class ResetPasswordController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render()
        {
            var model = new PasswordResetModel
            {
                Email = Request.QueryString["email"],
                Token = Request.QueryString["token"]
            };

            return PartialView("~/Views/Partials/Members/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(PasswordResetModel model)
        {
            if (ModelState.IsValid == false)
                return CurrentUmbracoPage();

            var memberService = ApplicationContext.Current.Services.MemberService;
            var member = memberService.GetByEmail(model.Email);
            if (member == null)
            {
                ModelState.AddModelError("", "Can't reset your password with the provided information");
                return CurrentUmbracoPage();
            }

            if (member.GetValue<DateTime>("passwordResetTokenExpiryDate") < DateTime.Now)
            {
                ModelState.AddModelError("", "The reset token has expired, you can't use it any more");
                return CurrentUmbracoPage();
            }

            var hashedToken = member.GetValue<string>("passwordResetToken");
            var verifyToken = SecureHasher.Verify(model.Token, hashedToken);
            if (verifyToken == false)
            {
                ModelState.AddModelError("", "Can't reset your password with the provided information");
                return CurrentUmbracoPage();
            }

            var memberShip = Membership.GetUser(model.Email);
            if (memberShip != null)
            {
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
            }
            else
            {
                ModelState.AddModelError("", "Can't reset your password with the provided information");
                return CurrentUmbracoPage();
            }

            TempData["success"] = true;
            return RedirectToCurrentUmbracoPage();
        }
    }
}
