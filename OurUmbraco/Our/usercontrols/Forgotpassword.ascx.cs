using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Umbraco.Core;
using Umbraco.Web.UI.Controls;

namespace our.usercontrols
{
    public partial class Forgotpassword : UmbracoUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void sendPass(object sender, EventArgs e)
        {
            message.Visible = false;
            error.Visible = false;
            retrieve_error.Visible = false;

            var email = tb_email.Text;
            
            var memberService = Services.MemberService;
            int totalMembers;
            var members = memberService.FindByEmail(email, 0, 100, out totalMembers);
            if (totalMembers > 1)
            {
                var duplicateMembers = new List<DuplicateMember>();
                foreach (var member in members)
                {
                    var totalKarma = member.GetValue<int>("reputationTotal");
                    var duplicateMember = new DuplicateMember {MemberId = member.Id, TotalKarma = totalKarma};
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

            var m = memberService.GetByEmail(email);
            if (m == null)
            {
                retrieve_error.Visible = true;
                return;
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
            
            var pass = RandomString(8, true);
            memberService.SavePassword(m, pass);

            var mail = "<p>Hi " + m.Name + "</p>";
            mail = mail + "<p>This is your new password for your account on http://our.umbraco.org:</p>";
            mail = mail + "<p><strong>" + pass + "</strong></p>";
            mail = mail + "<br/><br/><p>All the best<br/> <em>The email robot</em></p>";

            var mailMessage = new MailMessage
            {
                Subject = "Your password to our.umbraco.org",
                Body = mail,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(m.Email));

            mailMessage.From = new MailAddress("robot@umbraco.org");

            var smtpClient = new SmtpClient();
            smtpClient.Send(mailMessage);

            message.Visible = true;
            lt_email.Text = email;
        }

        private string RandomString(int size, bool lowerCase)
        {
            var builder = new StringBuilder();
            var random = new Random();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            
            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
    }
}