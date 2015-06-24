using System;
using System.Collections.Generic;
using System.Linq;
using umbraco;
using Umbraco.Core;
using Umbraco.Web.UI.Controls;

namespace our.usercontrols
{
    public partial class Login : UmbracoUserControl
    {
        public int NextPage { get; set; }
        public string ErrorMessage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            library.RegisterJavaScriptFile("jquery.validation", "/scripts/jquery.validation.js");
        }

        protected void login(object sender, EventArgs e)
        {
            var email = tb_email.Text;
            var password = tb_password.Text;
            var redirectUrl = Request.QueryString["redirectUrl"];

            var memberService = Services.MemberService;
            int totalMembers;
            var members = memberService.FindByEmail(email, 0, 100, out totalMembers).ToList();
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

            var memberToLogin = memberService.FindByEmail(email, 0, 1, out totalMembers).SingleOrDefault();
            if (memberToLogin != null)
            {
                // Automatically approve all members, as we don't have an approval process now
                // This is needed as we added new membership after upgrading so IsApproved is 
                // currently empty. First time a member gets saved now (login also saves the member)
                // IsApproved would turn false (default value of bool) so we want to prevent that
                if (memberToLogin.Properties.Contains(Constants.Conventions.Member.IsApproved) && memberToLogin.IsApproved == false)
                {
                    memberToLogin.IsApproved = true;
                    memberService.Save(memberToLogin, false);
                }
            }

            if (Members.Login(email, password))
            {
                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    var nextUrl = new Uri(redirectUrl);
                    if (Request.Url.Host == nextUrl.Host)
                        Response.Redirect(redirectUrl);
                }
                else
                {
                    Response.Redirect("/");
                }
            }
            else
            {
                lt_err.Text = ErrorMessage;
                errors.Visible = true;
            }
        }
    }

    internal class DuplicateMember
    {
        public int MemberId { get; set; }
        public int TotalKarma { get; set; }
    }
}