using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using RestSharp;
using RestSharp.Deserializers;
using umbraco;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;
using Member = umbraco.cms.businesslogic.member.Member;
using MemberGroup = umbraco.cms.businesslogic.member.MemberGroup;

namespace uForum.Library
{
    public class Utils
    {
        private const string SpamMemberGroupName = "potentialspam";
        private static readonly int PotentialSpammerThreshold = int.Parse(ConfigurationManager.AppSettings["PotentialSpammerThreshold"]);
        private static readonly int SpamBlockThreshold = int.Parse(ConfigurationManager.AppSettings["SpamBlockThreshold"]);
        private const int ReputationThreshold = 30;

        /// <summary>
        /// sanitize any potentially dangerous tags from the provided raw HTML input using 
        /// a whitelist based approach, leaving the "safe" HTML tags
        /// </summary>
        public static string Sanitize(string html)
        {


            html = Regex.Replace(html, "<script.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "<iframe>", "&lt;iframe&gt;", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            html = html.Replace("[code]", "<pre>");
            html = html.Replace("[/code]", "</pre>");

            var cleanHtml = CleanInvalidXmlChars(html);

            // Add links to URLs that aren't "properly" linked in a markdown way
            var regex = new Regex(@"(^|\s|>|;)(https?|ftp)(:\/\/[-A-Z0-9+&@#\/%?=~_|\[\]\(\)!:,\.;]*[-A-Z0-9+&@#\/%=~_|\[\]])($|\W)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var linkedHtml = regex.Replace(cleanHtml, "$1<a href=\"$2$3\">$2$3</a>$4").Replace("href=\"www", "href=\"http://www");

            var iframeRegex = new Regex("<iframe.*?</iframe>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var iframeMatches = iframeRegex.Matches(linkedHtml);
            for (var i = 0; i < iframeMatches.Count; i++)
            {
                linkedHtml = linkedHtml.Replace(iframeMatches[i].Value, string.Format("<pre>{0}</pre>", HttpContext.Current.Server.HtmlEncode(iframeMatches[i].Value)));
            }

            return linkedHtml;
        }

        public static string CleanInvalidXmlChars(string text)
        {
            // From xml spec valid chars:
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]    
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
            const string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }

        public static Member GetMember(int id)
        {
            try
            {
                return Member.GetMemberFromCache(id) ?? new Member(id);
            }
            catch (Exception exception)
            {
                Log.Add(LogTypes.Error, 0, string.Format("Could not get member {0} from the cache nor from the database - Exception: {1} {2} {3}", id, exception.Message, exception.StackTrace, exception.InnerException));
            }

            return null;
        }

        public static bool IsModerator()
        {
            var isModerator = false;

            var currentMemberId = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (currentMemberId != 0)
            {
                var moderatorRoles = new[] { "admin", "HQ", "Core", "MVP" };

                isModerator = moderatorRoles.Any(moderatorRole => IsMemberInGroup(moderatorRole, currentMemberId));
            }

            return isModerator;
        }

        public static bool IsMemberInGroup(string GroupName, int memberid)
        {
            Member m;
            try
            {
                m = Utils.GetMember(memberid);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, new User(0), -1, string.Format("Utills.GetMember({0}) failed - {1} {2} {3}", memberid, ex.Message, ex.StackTrace, ex.InnerException));
                return false;
            }

            foreach (MemberGroup mg in m.Groups.Values)
            {
                if (mg.Text == GroupName)
                    return true;
            }
            return false;
        }

        public static bool IsInGroup(string GroupName)
        {

            if (umbraco.library.IsLoggedOn())
                return IsMemberInGroup(GroupName, Member.CurrentMemberId());
            else
                return false;
        }

        public static void AddMemberToPotentialSpamGroup(Member member)
        {
            var memberGroup = MemberGroup.GetByName(SpamMemberGroupName);
            if (memberGroup == null)
                MemberGroup.MakeNew(SpamMemberGroupName, new User(0));

            memberGroup = MemberGroup.GetByName(SpamMemberGroupName);
            member.AddGroup(memberGroup.Id);
        }

        public static void RemoveMemberFromPotentialSpamGroup(Member member)
        {
            var memberGroup = MemberGroup.GetByName(SpamMemberGroupName);
            if (memberGroup == null)
                MemberGroup.MakeNew(SpamMemberGroupName, new User(0));

            memberGroup = MemberGroup.GetByName(SpamMemberGroupName);
            member.RemoveGroup(memberGroup.Id);
        }

        public static SpamResult CheckForSpam(Member member)
        {
            try
            {
                // Already blocked, nothing left to do here
                if (member.getProperty("blocked") != null && member.getProperty("blocked").Value != null && member.getProperty("blocked").Value.ToString() == "1")
                {
                    return new SpamResult
                           {
                               MemberId = member.Id,
                               Name = member.Text,
                               Blocked = true
                           };
                }

                // If reputation is > ReputationThreshold they've got enough karma, spammers never get that far

                var reputation = string.Empty;
                if (member.getProperty("reputationTotal") != null && member.getProperty("reputationTotal").Value != null)
                    reputation = member.getProperty("reputationTotal").Value.ToString();

                int reputationTotal;
                if (int.TryParse(reputation, out reputationTotal) && reputationTotal > ReputationThreshold)
                    return null;

                // If they're already marked as suspicious then no need to process again
                if (Roles.IsUserInRole(member.LoginName, SpamMemberGroupName))
                {
                    return new SpamResult
                           {
                               MemberId = member.Id,
                               Name = member.Text,
                               AlreadyInSpamRole = true
                           };
                }

                var spammer = CheckForSpam(member.Email, member.Text, false);

                if (spammer != null && spammer.TotalScore > PotentialSpammerThreshold)
                {
                    AddMemberToPotentialSpamGroup(member);

                    spammer.MemberId = member.Id;

                    SendPotentialSpamMemberMail(spammer);

                    if (spammer.Blocked)
                    {
                        member.getProperty("blocked").Value = true;
                        member.Save();

                        // If blocked, just redirect them to the home page where they'll get a message saying they're blocked
                        HttpContext.Current.Response.Redirect("/");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, new User(0), -1, string.Format("Error trying to CheckForSpam for a member {0} {1} {2}", ex.Message, ex.StackTrace, ex.InnerException));
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

        public static void SendMemberSignupMail(Member member)
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
                    Name = member.Text,
                    Company = member.GetProperty<string>("company"),
                    Bio = member.GetProperty<string>("profileText"),
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
                    Log.Add(LogTypes.Error, -1, string.Format("Error checking stopforumspam.org {0}", spamCheckResult.Error));
                }
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, -1, string.Format("Error checking stopforumspam.org {0}", ex.Message + ex.StackTrace));
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
