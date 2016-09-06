using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using OurUmbraco.Our.usercontrols;
using Umbraco.Core;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class LoginController: SurfaceController
    {

        [ChildActionOnly]
        public ActionResult Render()
        {
            var loginModel = new LoginModel();
            return PartialView("~/Views/Partials/Members/Login.cshtml", loginModel);
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


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return Redirect("/");
        }
    }
}
