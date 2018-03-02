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

        public static SpamResult CheckForSpam(IMember member)
        {
            try
            {
                // Already blocked, nothing left to do here
                if (member.GetValue<bool>("blocked"))
                {
                    return new SpamResult
                           {
                               MemberId = member.Id,
                               Name = member.Name,
                               Blocked = true
                           };
                }

                // If reputation is > ReputationThreshold they've got enough karma, spammers never get that far
                var reputation = member.GetValue<int>("reputationTotal");
                if (reputation > ReputationThreshold)
                    return null;

                // If they're already marked as suspicious then no need to process again
                if (Roles.IsUserInRole(member.Username, SpamMemberGroupName))
                {
                    return new SpamResult
                           {
                               MemberId = member.Id,
                               Name = member.Name,
                               AlreadyInSpamRole = true
                           };
                }

                var spammer = CheckForSpam(member.Email, member.Name, false);

                if (spammer != null && spammer.TotalScore > PotentialSpammerThreshold)
                {
                    var memberService = ApplicationContext.Current.Services.MemberService;
                    memberService.AssignRole(member.Id, SpamMemberGroupName);

                    spammer.MemberId = member.Id;

                    SendPotentialSpamMemberMail(spammer);

                    if (spammer.Blocked)
                    {
                        member.SetValue("blocked", true);
                        memberService.Save(member);

                        // If blocked, just redirect them to the home page where they'll get a message saying they're blocked
                        HttpContext.Current.Response.Redirect("/");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Utils>(string.Format("Error trying to CheckForSpam for member {0}", member.Email), ex);
            }

            return null;
        }

        public static SpamResult CheckForSpam(string email, string name, bool sendMail)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var client = new RestClient("http://api.stopforumspam.org");
                var request = new RestRequest(string.Format("api?ip={0}&email={1}&f=json", ipAddress, HttpUtility.UrlEncode(email)), Method.GET);
                var response = client.Execute(request);
                var jsonResult = new JsonDeserializer();
                var spamCheckResult = jsonResult.Deserialize<SpamCheckResult>(response);

                if (spamCheckResult.Success == 1)
                {
                    var score = spamCheckResult.Ip.Confidence + spamCheckResult.Email.Confidence;

                    var blocked = score > SpamBlockThreshold;
                    if (score > PotentialSpammerThreshold)
                    {
                        var spammer = new SpamResult
                        {
                            Name = name,
                            Email = email,
                            Ip = ipAddress,
                            ScoreEmail = spamCheckResult.Email.Confidence.ToString(CultureInfo.InvariantCulture),
                            FrequencyEmail = spamCheckResult.Email.Frequency.ToString(CultureInfo.InvariantCulture),
                            ScoreIp = spamCheckResult.Ip.Confidence.ToString(CultureInfo.InvariantCulture),
                            FrequencyIp = spamCheckResult.Ip.Frequency.ToString(CultureInfo.InvariantCulture),
                            Blocked = blocked,
                            TotalScore = score
                        };

                        if (sendMail)
                            SendPotentialSpamMemberMail(spammer);

                        return spammer;
                    }
                }
                else
                {
                    Log.Add(LogTypes.Error, -1, string.Format("Error checking stopforumspam.org {0}", spamCheckResult.Error));
                }
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, -1, string.Format("Error checking stopforumspam.org {0}", ex.Message + ex.StackTrace));
            }

            return null;
        }

        public static void SendMemberSignupMail(IMember member)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var client = new RestClient("http://api.stopforumspam.org");
                var request = new RestRequest(string.Format("api?ip={0}&email={1}&f=json", ipAddress, HttpUtility.UrlEncode(member.Email)), Method.GET);
                var response = client.Execute(request);
                var jsonResult = new JsonDeserializer();
                var spamCheckResult = jsonResult.Deserialize<SpamCheckResult>(response);

                var spammer = new SpamResult
                {
                    MemberId = member.Id,
                    Name = member.Name,
                    Company = member.GetValue<string>("company"),
                    Bio = member.GetValue<string>("profileText"),
                    Email = member.Email,
                    Ip = ipAddress
                };

                if (spamCheckResult.Success == 1)
                {
                    var score = spamCheckResult.Ip.Confidence + spamCheckResult.Email.Confidence;
                    var blocked = score > SpamBlockThreshold;

                    spammer.ScoreEmail = spamCheckResult.Email.Confidence.ToString(CultureInfo.InvariantCulture);
                    spammer.FrequencyEmail = spamCheckResult.Email.Frequency.ToString(CultureInfo.InvariantCulture);
                    spammer.ScoreIp = spamCheckResult.Ip.Confidence.ToString(CultureInfo.InvariantCulture);
                    spammer.FrequencyIp = spamCheckResult.Ip.Frequency.ToString(CultureInfo.InvariantCulture);
                    spammer.Blocked = blocked;
                    spammer.TotalScore = score;

                    SendNewMemberMail(spammer);
                }
                else
                {
                    LogHelper.Warn<Utils>(string.Format("Error checking stopforumspam.org {0}", spamCheckResult.Error));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Utils>("Error checking stopforumspam.org", ex);
            }
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

        public static void SendNewMemberMail(SpamResult spamResult)
        {
            try
            {
                var notify = ConfigurationManager.AppSettings["uForumSpamNotify"];
                var body = GetSpamResultBody(spamResult);

                var subject = string.Format("Umbraco community: new member signed up");

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
                Log.Add(LogTypes.Error, new User(0), -1, string.Format("Error sending new member notification: {0} {1} {2}",
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

            var querystring = string.Format("api?ip={0}&email={1}&f=json", spammer.Ip, HttpUtility.UrlEncode(spammer.Email));

            body = body + string.Format("<hr /><a href=\"http://api.stopforumspam.org/{0}\">Check full StopForumSpam response</a>", querystring);
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
