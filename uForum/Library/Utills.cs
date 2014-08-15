using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;
using RestSharp;
using RestSharp.Deserializers;
using uForum.Businesslogic;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;

namespace uForum.Library
{
    public class Utills
    {
        /// <summary>
        /// sanitize any potentially dangerous tags from the provided raw HTML input using 
        /// a whitelist based approach, leaving the "safe" HTML tags
        /// </summary>

        private const string SpamMemberGroupName = "potentialspam";

        public static string Sanitize(string html)
        {


            html = Regex.Replace(html, "<script.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            html = html.Replace("[code]", "<pre>");
            html = html.Replace("[/code]", "</pre>");

            var cleanHtml = CleanInvalidXmlChars(html);

            // Add links to URLs that aren't "properly" linked in a markdown way
            var regex = new Regex(@"(^|\s|>|;)(https?|ftp)(:\/\/[-A-Z0-9+&@#\/%?=~_|\[\]\(\)!:,\.;]*[-A-Z0-9+&@#\/%=~_|\[\]])($|\W)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var linkedHtml = regex.Replace(cleanHtml, "$1<a href=\"$2$3\" rel=\"nofollow\">$2$3</a>$4").Replace("href=\"www", "href=\"http://www");

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
                Log.Add(LogTypes.Error, 0, string.Format("Could not get member {0} from the cache nor from the database - Exception: {1}", id, exception.InnerException));
            }

            return null;
        }

        public static bool IsMember(int id)
        {
            return (Businesslogic.Data.SqlHelper.ExecuteScalar<int>("select count(nodeid) from cmsMember where nodeid = '" + id + "'") > 0);
        }

        public static bool IsModerator()
        {
            var isModerator = false;

            var currentMemberId = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (currentMemberId != 0)
            {
                var moderatorRoles = new[] { "admin", "HQ", "Core", "MVP" };

                isModerator = moderatorRoles.Any(moderatorRole => Xslt.IsMemberInGroup(moderatorRole, currentMemberId));
            }

            return isModerator;
        }

        public static bool CanSeeTopic(int topicId)
        {
            var topic = Topic.GetTopic(topicId);
            if (topic != null && topic.IsSpam == false)
                return true;

            var currentMember = Member.GetCurrentMember();

            if (topic != null && currentMember != null && topic.IsSpam)
                if (IsModerator() || topic.MemberId == currentMember.Id)
                    return true;

            return false;
        }

        public static bool CanSeeComment(int commentId)
        {
            var comment = new Comment(commentId);
            if (comment.IsSpam == false)
                return true;

            var currentMember = Member.GetCurrentMember();

            if (currentMember != null && comment.IsSpam)
                if (IsModerator() || comment.MemberId == currentMember.Id)
                    return true;

            return false;
        }

        public static void AddMemberToPotentialSpamGroup(Member member)
        {
            var memberGroup = MemberGroup.GetByName(SpamMemberGroupName);
            if (memberGroup == null)
                MemberGroup.MakeNew(SpamMemberGroupName, new User(0));

            memberGroup = MemberGroup.GetByName(SpamMemberGroupName);

            if (Roles.IsUserInRole(member.LoginName, SpamMemberGroupName) == false)
                member.AddGroup(memberGroup.Id);
        }

        public static void RemoveMemberFromPotentialSpamGroup(Member member)
        {
            var memberGroup = MemberGroup.GetByName(SpamMemberGroupName);
            if (memberGroup == null)
                MemberGroup.MakeNew(SpamMemberGroupName, new User(0));

            memberGroup = MemberGroup.GetByName(SpamMemberGroupName);

            if (Roles.IsUserInRole(member.LoginName, SpamMemberGroupName))
                member.RemoveGroup(memberGroup.Id);
        }

        public static Spammer CheckForSpam(Member member)
        {
            // Already blocked, nothing left to do here
            if (member.getProperty("blocked").Value.ToString() == "1")
            {
                return new Spammer
                       {
                           MemberId = member.Id,
                           Name = member.Text,
                           Blocked = true
                       };
            }

            // If reputation is > 30 they've gotten at least 10 karma, spammers never get that far
            var reputation = member.getProperty("reputationTotal").Value.ToString();
            int reputationTotal;
            if (int.TryParse(reputation, out reputationTotal) && reputationTotal > 30)
                return null;

            // If they're already marked as suspicious then no need to process again
            if (Roles.IsUserInRole(member.LoginName, SpamMemberGroupName))
            {
                return new Spammer
                       {
                           MemberId = member.Id,
                           Name = member.Text,
                           AlreadyInSpamRole = true
                       };
            }

            try
            {
                var ipAddress = GetIpAddress();
                var client = new RestClient("http://api.stopforumspam.org");
                var request = new RestRequest(string.Format("api?ip={0}&email={1}&f=json", ipAddress, HttpUtility.UrlEncode(member.Email)), Method.GET);
                var response = client.Execute(request);
                var jsonResult = new JsonDeserializer();
                var spamCheckResult = jsonResult.Deserialize<SpamCheckResult>(response);

                if (spamCheckResult.Success == 1)
                {
                    var score = spamCheckResult.Ip.Confidence + spamCheckResult.Email.Confidence;

                    if (score > 100)
                    {
                        AddMemberToPotentialSpamGroup(member);

                        // That's 90+ on both e-mail and IP score, must be a spammer - instant block
                        if (score > 190)
                        {
                            member.getProperty("blocked").Value = true;
                            member.Save();
                        }

                        var spammer = new Spammer
                                      {
                                          MemberId = member.Id,
                                          Name = member.Text,
                                          Email = member.Email,
                                          Ip = GetIpAddress(),
                                          ScoreEmail = spamCheckResult.Email.Confidence.ToString(CultureInfo.InvariantCulture),
                                          ScoreIp = spamCheckResult.Ip.Confidence.ToString(CultureInfo.InvariantCulture),
                                          Blocked = score > 180
                                      };

                        SendPotentialSpamMemberMail(spammer);
                        
                        // If blocked, just redirect them to the home page where they'll get a message saying they're blocked
                        if(spammer.Blocked)
                            HttpContext.Current.Response.Redirect("/");

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

        public static string GetIpAddress()
        {
            var context = HttpContext.Current;
            var ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddress))
                return context.Request.ServerVariables["REMOTE_ADDR"];

            var addresses = ipAddress.Split(',');
            return addresses.Length != 0 ? addresses[0] : context.Request.ServerVariables["REMOTE_ADDR"];
        }

        internal static void SendPotentialSpamMemberMail(Spammer spammer)
        {
            try
            {
                var notify = ConfigurationManager.AppSettings["uForumSpamNotify"];
                var body = string.Format("Edit member <a href=\"http://our.umbraco.org/umbraco/members/editMember.aspx?id={0}\">http://our.umbraco.org/umbraco/members/editMember.aspx?id={0}</a><br /><br />", spammer.MemberId);
                body = body + string.Format("Go to member <a href=\"http://our.umbraco.org/member/{0}\">http://our.umbraco.org/member/{0}</a><br />", spammer.MemberId);
                body = body + string.Format("Blocked: {0}<br /> Email: {1}<br /> IP: {2}<br /> - Score IP: {3}<br /> Score e-mail: {4}<br />Already marked before: {5}<br/>Name: {6}",
                                            spammer.Blocked, spammer.Email, spammer.Ip, spammer.ScoreIp, spammer.ScoreEmail, spammer.AlreadyInSpamRole, spammer.Name);

                var mailMessage = new MailMessage
                {
                    Subject = string.Format("Umbraco community: member flagged as potential spammer"),
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
                Log.Add(LogTypes.Error, new User(0), -1, "Error sending potential spam member notification: " + ex.Message + " " + ex.StackTrace);
            }
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

    public class Spammer
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Email { get; set; }
        public string ScoreIp { get; set; }
        public string ScoreEmail { get; set; }
        public bool AlreadyInSpamRole { get; set; }
        public bool Blocked { get; set; }
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
