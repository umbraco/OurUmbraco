using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Security;
using OurUmbraco.Forum.Extensions;
using RestSharp;
using RestSharp.Deserializers;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace OurUmbraco.Forum.Library
{
    public class Utils
    {
        private const string SpamMemberGroupName = "potentialspam";
        private static readonly int PotentialSpammerThreshold = int.Parse(ConfigurationManager.AppSettings["PotentialSpammerThreshold"]);
        private static readonly int SpamBlockThreshold = int.Parse(ConfigurationManager.AppSettings["SpamBlockThreshold"]);
        private const int ReputationThreshold = 30;
        
        public static IPublishedContent GetMember(int id)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);
            var currentMember = memberShipHelper.GetCurrentMember();
            return currentMember;
        }

        public static bool IsModerator()
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);
            var currentMember = memberShipHelper.GetCurrentMember();

            if (currentMember.Id == 0)
                return false;

            var moderatorRoles = new[] { "admin", "HQ", "Core", "MVP" };
            return moderatorRoles.Any(moderatorRole => IsMemberInGroup(moderatorRole, currentMember));
        }

        public static bool IsMemberInGroup(string groupName, IPublishedContent member)
        {
            return member.GetRoles().Any(memberGroup => memberGroup == groupName);
        }

        public static bool IsInGroup(string groupName)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(UmbracoContext.Current);
            var currentMember = memberShipHelper.GetCurrentMember();
            return currentMember != null && IsMemberInGroup(groupName, currentMember);
        }

        public static void AddMemberToPotentialSpamGroup(int memberId)
        {
            var memberService = ApplicationContext.Current.Services.MemberService;
            memberService.AssignRole(memberId, SpamMemberGroupName);
        }

        public static void RemoveMemberFromPotentialSpamGroup(int memberId)
        {
            var memberService = ApplicationContext.Current.Services.MemberService;
            memberService.DissociateRole(memberId, SpamMemberGroupName);
        }
        
        public static string GetIpAddress()
        {
            var context = HttpContext.Current;
            var ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddress))
                return context.Request.ServerVariables["REMOTE_ADDR"];

            var addresses = ipAddress.Split(',');
            return addresses.Length != 0 ? addresses[0] : context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static void SendPotentialSpamMemberMail(SpamResult spammer)
        {
            try
            {
                var notify = ConfigurationManager.AppSettings["uForumSpamNotify"];

                var body = GetSpamResultBody(spammer);

                var subject = string.Format("Umbraco community: member {0}", spammer.Blocked ? "blocked" : "flagged as potential spammer");

                var mailMessage = new MailMessage
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                foreach (var email in notify.Split(','))
                    mailMessage.To.Add(email);

                mailMessage.From = new MailAddress("our@umbraco.org");

                var smtpClient = new SmtpClient();
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, new User(0), -1, string.Format("Error sending potential spam member notification: {0} {1} {2}",
                    ex.Message, ex.StackTrace, ex.InnerException));
            }
        }

        public static void SendActivationMail(IMember member)
        {
            try
            {
                var subject = "Activate your account on our.umbraco.org";
                var body =
                    string.Format("Hi {0},<br /><br /> Thanks for signing up for the Umbraco community site. In order to be able to log in please click on the link below to activate your account: <br /><a href=\"https://our.umbraco.org/member/activate/?id={1}\">https://our.umbraco.org/member/activate/?id={1}</a><br /><br />Best regards,<br />The Umbraco Community robot.", member.Name, member.ProviderUserKey);

                var mailMessage = new MailMessage
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                 
                mailMessage.To.Add(member.Email);

                mailMessage.From = new MailAddress("robot@umbraco.org");

                var smtpClient = new SmtpClient();
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                var error = string.Format("*ERROR* sending activation mail for member {0} - {1} {2} {3}", member.Email, ex.Message, ex.StackTrace, ex.InnerException);
                SendSlackNotification(error);
                LogHelper.Error<Utils>(error, ex);
            }
        }

        private static void SendSlackNotification(string body)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    {"channel", ConfigurationManager.AppSettings["SlackChannel"]},
                    {"token", ConfigurationManager.AppSettings["SlackToken"]},
                    {"username", ConfigurationManager.AppSettings["SlackUsername"]},
                    {"icon_url", ConfigurationManager.AppSettings["SlackIconUrl"]},
                    {"text", body}
                };

                try
                {
                    var data = client.UploadValues("https://slack.com/api/chat.postMessage", "POST", values);
                    var response = client.Encoding.GetString(data);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<Utils>("Posting update to Slack failed", ex);
                }
            }
        }

        public static string GetForumName(IPublishedContent forum)
        {
            var forumName = forum.Name;
            var isProjectForum = string.Equals(forum.Parent.ContentType.Alias, "Project", StringComparison.InvariantCultureIgnoreCase);
            if (isProjectForum && forum.Name.ToLowerInvariant().Contains(forum.Parent.Name.ToLowerInvariant()) == false)
            {
                forumName = forum.Parent.Name + " - " + forumName;
            }

            return forumName;
        }

        private static string GetSpamResultBody(SpamResult spammer)
        {
            var body = string.Empty;

            if (spammer.MemberId != 0)
            {
                body = body + string.Format(
                           "<a href=\"http://our.umbraco.org/umbraco/members/editMember.aspx?id={0}\">Edit Member</a><br /><br />",
                           spammer.MemberId);

                body = body + string.Format("<a href=\"http://our.umbraco.org/member/{0}\">Go to member</a><br />", spammer.MemberId);
            }
            else if (spammer.Blocked)
            {
                body = body + string.Format("Member was never created.<br />");
            }

            body = body + string.Format(
                       "Blocked: {0}<br />Name: {1}<br />Company: {2}<br />Bio: {3}<br />Email: {4}<br />IP: {5}<br />" +
                       "Score IP: {6}<br />Frequency IP: {7}<br />Score e-mail: {8}<br />Frequency e-mail: {9}<br />Total score: {10}<br />Member Id: {11}",
                       spammer.Blocked,
                       spammer.Name ?? string.Empty,
                       spammer.Company ?? string.Empty,
                       spammer.Bio == null ? string.Empty : spammer.Bio.Replace("\n", "<br />"),
                       spammer.Email ?? string.Empty,
                       spammer.Ip ?? string.Empty,
                       spammer.ScoreIp ?? string.Empty,
                       spammer.FrequencyIp ?? string.Empty,
                       spammer.ScoreEmail ?? string.Empty,
                       spammer.FrequencyEmail ?? string.Empty,
                       spammer.TotalScore,
                       spammer.MemberId);
            
            return body;
        }
    }


    public class SpamCheckResult
    {
        public int Success { get; set; }
        public string Error { get; set; }
        public SpamCheckProperty Username { get; set; }
        public SpamCheckProperty Email { get; set; }
        public SpamCheckProperty Ip { get; set; }
    }

    public class SpamCheckProperty
    {
        public string LastSeen { get; set; }
        public int Frequency { get; set; }
        public int Appears { get; set; }
        public float Confidence { get; set; }
    }

    public class SpamResult
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Bio { get; set; }
        public string Ip { get; set; }
        public string Email { get; set; }
        public string FrequencyIp { get; set; }
        public string ScoreIp { get; set; }
        public string ScoreEmail { get; set; }
        public string FrequencyEmail { get; set; }
        public bool AlreadyInSpamRole { get; set; }
        public bool Blocked { get; set; }
        public float TotalScore { get; set; }
    }

    public struct ReplacePoint
    {
        public int open, close;

        public ReplacePoint(int open, int close)
        {
            this.open = open;
            this.close = close;
        }
    }
}
