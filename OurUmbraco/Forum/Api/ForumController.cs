using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Newtonsoft.Json;
using OurUmbraco.Forum.AntiSpam;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Forum.Library;
using OurUmbraco.Forum.Models;
using OurUmbraco.Forum.Services;
using OurUmbraco.Our.Extensions;
using umbraco;
using umbraco.cms.helpers;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Forum.Api
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Client;

    using OurUmbraco.SignalRHubs;

    [MemberAuthorize(AllowType = "member")]
    public class ForumController : ForumControllerBase
    {
        /* COMMENTS */

        [HttpPost]
        public ExpandoObject Comment(CommentSaveModel model)
        {
            dynamic expandoObject = new ExpandoObject();
            var currentMember = Members.GetCurrentMember();

            var comment = new Comment
            {
                Body = model.Body,
                MemberId = currentMember.Id,
                Created = DateTime.Now,
                ParentCommentId = model.Parent,
                TopicId = model.Topic
            };

            comment.IsSpam = currentMember.GetPropertyValue<bool>("blocked") || comment.DetectSpam();
            CommentService.Save(comment);
            if (comment.IsSpam)
                SpamChecker.SendSlackSpamReport(comment.Body, comment.TopicId, "comment", comment.MemberId);

            expandoObject.id = comment.Id;
            expandoObject.body = comment.Body.Sanitize().ToString();
            expandoObject.topicId = comment.TopicId;
            expandoObject.authorId = comment.MemberId;
            expandoObject.created = comment.Created.ConvertToRelativeTime();
            expandoObject.authorKarma = currentMember.Karma();
            expandoObject.authorName = currentMember.Name;
            expandoObject.roles = currentMember.GetRoles().GetBadges();
            expandoObject.cssClass = model.Parent > 0 ? "level-2" : string.Empty;
            expandoObject.parent = model.Parent;
            expandoObject.isSpam = comment.IsSpam;

            SignalRcommentSaved(expandoObject);

            return expandoObject;
        }

        private void SignalRcommentSaved(dynamic expandoObject)
        {
            var root = Url.Content("~/");
            using (var hubConnection = new HubConnection(root + "/signalr"))
            {
                var conProxy = hubConnection.CreateHubProxy("forumPostHub");
                hubConnection.Start().Wait();
                conProxy.Invoke("SomeonePosted", expandoObject).Wait();
            }
        }

        private void SignalRCommentDeleted(int threadId, int commentId)
        {
            var root = Url.Content("~/");
            using (var hubConnection = new HubConnection(root + "/signalr"))
            {
                var conProxy = hubConnection.CreateHubProxy("forumPostHub");
                hubConnection.Start().Wait();
                conProxy.Invoke("CommentDeleted", threadId, commentId).Wait();
            }
        }


        private void SignalRcommentEdited(dynamic c)
        {
            var root = Url.Content("~/");
            using (var hubConnection = new HubConnection(root + "/signalr"))
            {
                var conProxy = hubConnection.CreateHubProxy("forumPostHub");
                hubConnection.Start().Wait();
                conProxy.Invoke("SomeoneEdited", c).Wait();
            }
        }

        [HttpPut]
        public void Comment(int id, CommentSaveModel model)
        {
            var c = CommentService.GetById(id);

            if (c == null)
                throw new Exception("Comment not found");

            if (c.MemberId != Members.GetCurrentMemberId() && Members.IsAdmin() == false)
                throw new Exception("You cannot edit this comment");

            c.Body = model.Body;
            // This is an edit, don't update topic post count
            SignalRcommentEdited(c);
            CommentService.Save(c, false);
        }

        [HttpDelete]
        public void Comment(int id)
        {
            var c = CommentService.GetById(id);

            if (c == null)
                throw new Exception("Comment not found");

            if (Members.IsAdmin() == false && c.MemberId != Members.GetCurrentMemberId())
                throw new Exception("You cannot delete this comment");

            CommentService.Delete(c);
            SignalRCommentDeleted(c.TopicId, id);
            if (Members.IsAdmin() && c.MemberId != Members.GetCurrentMemberId())
                SendSlackNotification(BuildDeleteNotifactionPost(Members.GetCurrentMember().Name, c.MemberId));
        }

        [HttpGet]
        public string CommentMarkdown(int id)
        {
            var c = CommentService.GetById(id);

            if (c == null)
                throw new Exception("Comment not found");

            return c.Body.SanitizeEdit();
        }

        [HttpPost]
        public void CommentAsSpam(int id)
        {
            var c = CommentService.GetById(id);

            if (Members.IsAdmin() == false)
                throw new Exception("You cannot mark this comment as spam");

            if (c == null)
                throw new Exception("Comment not found");

            c.IsSpam = true;

            CommentService.Save(c);
        }

        [HttpPost]
        public void CommentAsHam(int id)
        {
            var c = CommentService.GetById(id);

            if (Members.IsAdmin() == false)
                throw new Exception("You cannot mark this comment as ham");

            if (c == null)
                throw new Exception("Comment not found");

            c.IsSpam = false;

            CommentService.Save(c);
        }


        [HttpPost]
        public ExpandoObject Topic(TopicSaveModel model)
        {
            dynamic o = new ExpandoObject();

            var t = new Topic();
            t.Body = model.Body;
            t.Title = model.Title;
            t.MemberId = Members.GetCurrentMemberId();
            t.Created = DateTime.Now;
            t.ParentId = model.Forum;
            t.UrlName = url.FormatUrl(model.Title);
            t.Updated = DateTime.Now;
            t.Version = model.Version;
            t.Locked = false;
            t.LatestComment = 0;
            t.LatestReplyAuthor = 0;
            t.Replies = 0;
            t.Score = 0;
            t.Answer = 0;
            t.LatestComment = 0;
            t.IsSpam = Members.GetCurrentMember().GetPropertyValue<bool>("blocked") || t.DetectSpam();
            
            // If the chosen version is Umbraco Heartcore, overrule other categories
            if (model.Version == Constants.Forum.HeartcoreVersionNumber)
            {
                var heartCodeForumId = GetHeartCoreForumId();
                t.ParentId = heartCodeForumId;
            }
            
            TopicService.Save(t);

            if (t.IsSpam)
                SpamChecker.SendSlackSpamReport(t.Body, t.Id, "topic", t.MemberId);

            o.url = string.Format("{0}/{1}-{2}", library.NiceUrl(t.ParentId), t.Id, t.UrlName);

            return o;
        }


        [HttpPut]
        public ExpandoObject Topic(int id, TopicSaveModel model)
        {
            dynamic o = new ExpandoObject();

            var t = TopicService.GetById(id);

            if (t == null)
                throw new Exception("Topic not found");

            if (t.MemberId != Members.GetCurrentMemberId() && Members.IsAdmin() == false)
                throw new Exception("You cannot edit this topic");

            t.Updated = DateTime.Now;
            t.Body = model.Body;
            t.Version = model.Version;
            t.ParentId = model.Forum;
            t.Title = model.Title;
            
            // If the chosen version is Umbraco Heartcore, overrule other categories
            if (model.Version == Constants.Forum.HeartcoreVersionNumber)
            {
                var heartCodeForumId = GetHeartCoreForumId();
                t.ParentId = heartCodeForumId;
            }
            
            TopicService.Save(t);

            o.url = string.Format("{0}/{1}-{2}", library.NiceUrl(t.ParentId), t.Id, t.UrlName);

            return o;
        }

        private static int GetHeartCoreForumId()
        {
            var heartCoreForumId = 0;
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var rootNode = umbracoHelper.TypedContentAtRoot()
                .FirstOrDefault(x =>
                    string.Equals(x.DocumentTypeAlias, "Community", StringComparison.InvariantCultureIgnoreCase));
            
            if (rootNode != null)
            {
                var forumNode = rootNode.FirstChild(x => string.Equals(x.DocumentTypeAlias, "Forum"));
                if (forumNode != null)
                {
                    var heartCoreForumNode = forumNode.FirstChild(x => x.Name.Contains(Constants.Forum.UmbracoHeadlessName));
                    if (heartCoreForumNode != null)
                        heartCoreForumId = heartCoreForumNode.Id;
                }
            }

            return heartCoreForumId;
        }


        [HttpDelete]
        public void Topic(int id)
        {
            var c = CommentService.GetById(id);

            if (c == null)
                throw new Exception("Topic not found");

            if (Members.IsAdmin() == false && c.MemberId != Members.GetCurrentMemberId())
                throw new Exception("You cannot delete this topic");

            CommentService.Delete(c);

            if (Members.IsAdmin() && c.MemberId != Members.GetCurrentMemberId())
                SendSlackNotification(BuildDeleteNotifactionPost(Members.GetCurrentMember().Name, c.MemberId));
        }

        [HttpGet]
        public string TopicMarkdown(int id)
        {
            var t = TopicService.GetById(id);

            if (t == null)
                throw new Exception("Topic not found");

            return t.Body.SanitizeEdit();
        }

        [HttpPost]
        public void TopicAsHam(int id)
        {
            var t = TopicService.GetById(id);

            if (Members.IsAdmin() == false)
                throw new Exception("You cannot mark this topic as ham");

            if (t == null)
                throw new Exception("Topic not found");

            t.IsSpam = false;

            TopicService.Save(t);
        }

        [HttpPost]
        public void TopicAsSpam(int id)
        {
            var t = TopicService.GetById(id);

            if (Members.IsAdmin() == false)
                throw new Exception("You cannot mark this topic as spam");

            if (t == null)
                throw new Exception("Topic not found");

            t.IsSpam = true;

            TopicService.Save(t);
        }

        /* MEDIA */
        [HttpPost]
        public HttpResponseMessage EditorUpload()
        {
            dynamic result = new ExpandoObject();
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                string filename = string.Empty;

                Guid g = Guid.NewGuid();

                foreach (string file in httpRequest.Files)
                {

                    DirectoryInfo updir = new DirectoryInfo(HttpContext.Current.Server.MapPath("/media/upload/" + g));

                    if (!updir.Exists)
                        updir.Create();

                    var postedFile = httpRequest.Files[file];

                    var filePath = updir.FullName + "/" + postedFile.FileName;
                    postedFile.SaveAs(filePath);
                    filename = postedFile.FileName;

                }

                result.success = true;
                result.imagePath = "/media/upload/" + g + "/" + filename;
            }
            else
            {
                result.success = false;
                result.message = "No images found";
            }

            //jquery ajax file uploader expects html, it parses to json client side
            var response = new HttpResponseMessage();
            response.Content = new StringContent(JsonConvert.SerializeObject(result));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [HttpPost]
        public void BlockMember(int id)
        {
            if (Members.IsAdmin() == false)
                throw new Exception("You cannot block this member");

            var memberService = UmbracoContext.Application.Services.MemberService;
            var member = memberService.GetById(id);

            if (member == null)
                throw new Exception("Member not found");

            member.SetValue("blocked", true);
            memberService.Save(member);

            SendSlackNotification(BuildBlockedNotifactionPost(Members.GetCurrentMember().Name, member.Id, true));
        }

        [HttpPost]
        public void UnblockMember(int id)
        {
            if (Members.IsAdmin() == false)
                throw new Exception("You cannot unblock this member");

            var memberService = UmbracoContext.Application.Services.MemberService;
            var member = memberService.GetById(id);

            if (member == null)
                throw new Exception("Member not found");

            member.SetValue("blocked", false);
            memberService.Save(member);

            SendSlackNotification(BuildBlockedNotifactionPost(Members.GetCurrentMember().Name, member.Id, false));
        }

        [HttpDelete]
        public void DeleteMember(int id)
        {
            if (Members.IsHq() == false)
                throw new Exception("You cannot delete this member");

            var memberService = UmbracoContext.Application.Services.MemberService;
            var member = memberService.GetById(id);

            if (member == null)
                throw new Exception("Member not found");

            memberService.Delete(member);
        }

        [HttpDelete]
        public void DeleteMemberPlus(int id)
        {
            if (Members.IsHq() == false)
                throw new Exception("You cannot delete this member");

            var memberService = UmbracoContext.Application.Services.MemberService;
            var member = memberService.GetById(id);

            if (member == null)
                throw new Exception("Member not found");

            var topicService = new TopicService(ApplicationContext.Current.DatabaseContext);
            var commentService = new CommentService(ApplicationContext.Current.DatabaseContext, topicService);
            var comments = commentService.GetAllCommentsForMember(member.Id);
            foreach (var comment in comments)
            {
                commentService.Delete(comment);
            }

            var topics = topicService.GetLatestTopicsForMember(member.Id, false, 100);
            foreach (var topic in topics)
            {
                // Only delete if this member started the topic
                if (topic.MemberId == member.Id)
                    topicService.Delete(topic);
            }

            memberService.Delete(member);
        }

        [HttpPost]
        public int ApproveMember(int id)
        {
            if (Members.IsAdmin() == false)
                throw new Exception("You cannot approve this member");

            var memberService = UmbracoContext.Application.Services.MemberService;
            var member = memberService.GetById(id);

            if (member == null)
                throw new Exception("Member not found");

            var minimumKarma = 71;
            if (member.GetValue<int>("reputationCurrent") < minimumKarma)
            {
                member.SetValue("reputationCurrent", minimumKarma);
                member.SetValue("reputationTotal", minimumKarma);
                memberService.Save(member);
            }

            var rolesForUser = Roles.GetRolesForUser(member.Username);
            if (rolesForUser.Contains("potentialspam"))
                memberService.DissociateRole(member.Id, "potentialspam");
            if (rolesForUser.Contains("newaccount"))
                memberService.DissociateRole(member.Id, "newaccount");

            var topicService = new TopicService(ApplicationContext.Current.DatabaseContext);
            var commentService = new CommentService(ApplicationContext.Current.DatabaseContext, topicService);
            var comments = commentService.GetAllCommentsForMember(member.Id);
            foreach (var comment in comments)
            {
                if (comment.IsSpam)
                {
                    comment.IsSpam = false;
                    commentService.Save(comment);
                    var topic = topicService.GetById(comment.TopicId);
                    var topicUrl = topic.GetUrl();
                    var commentUrl = string.Format("{0}#comment-{1}", topicUrl, comment.Id);
                    var memberName = member.Name;
                    commentService.SendNotifications(comment, memberName, commentUrl);
                }
            }

            var topics = topicService.GetLatestTopicsForMember(member.Id, false, 100);
            foreach (var topic in topics)
            {
                if (topic.IsSpam)
                {
                    topic.IsSpam = false;
                    topicService.Save(topic);
                    topicService.SendNotifications(topic, member.Name, topic.GetUrl());
                }
            }

            var newForumTopicNotification = new NotificationsCore.Notifications.AccountApproved();
            newForumTopicNotification.SendNotification(member.Email);

            SendSlackNotification(BuildBlockedNotifactionPost(Members.GetCurrentMember().Name, member.Id, false));

            return minimumKarma;
        }

        [HttpPost]
        public void Flag(Flag flag)
        {
            var post = string.Format("A {0} has been flagged as spam for a moderator to check\n", flag.TypeOfPost);
            var member = Members.GetById(flag.MemberId);
            post = post + string.Format("Flagged by member {0} https://our.umbraco.com/member/{1}\n", member.Name, member.Id);

            var topicId = flag.Id;
            var posterId = 0;
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);
            if (flag.TypeOfPost == "comment")
            {
                var cs = new CommentService(ApplicationContext.Current.DatabaseContext, ts);
                var comment = cs.GetById(flag.Id);
                topicId = comment.TopicId;
                posterId = comment.MemberId;
            }

            var topic = ts.GetById(topicId);
            if (flag.TypeOfPost == "thread")
            {
                posterId = topic.MemberId;
            }

            post = post + string.Format("Topic title: *{0}*\nLink to author: https://our.umbraco.com/member/{1}\n Link to {2}: https://our.umbraco.com{3}{4}\n\n", topic.Title, posterId, flag.TypeOfPost, topic.GetUrl(), flag.TypeOfPost == "comment" ? "#comment-" + flag.Id : string.Empty);

            SendSlackNotification(post);
        }

        private static string BuildDeleteNotifactionPost(string adminName, int memberId)
        {
            var post = string.Format("Topic or comment deleted by admin {0}\n", adminName);
            post = post + string.Format("Go to affected member https://our.umbraco.com/member/{0}\n\n", memberId);
            return post;
        }

        private static string BuildBlockedNotifactionPost(string adminName, int memberId, bool blocked)
        {
            var post = string.Format("Member {0} by admin {1}\n", blocked ? "_blocked_" : "*unblocked/approved*", adminName);
            post = post + string.Format("Go to affected member https://our.umbraco.com/member/{0}\n\n", memberId);
            return post;
        }

        private static void SendSlackNotification(string post)
        {
            using (var client = new WebClient())
            {
                post = post.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

                var values = new NameValueCollection
                             {
                                 {"channel", ConfigurationManager.AppSettings["SlackChannel"]},
                                 {"token", ConfigurationManager.AppSettings["SlackToken"]},
                                 {"username", ConfigurationManager.AppSettings["SlackUsername"]},
                                 {"icon_url", ConfigurationManager.AppSettings["SlackIconUrl"]},
                                 {"text", post}
                             };

                try
                {
                    var data = client.UploadValues("https://slack.com/api/chat.postMessage", "POST", values);
                    var response = client.Encoding.GetString(data);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ForumController>("Posting update to Slack failed", ex);
                }
            }
        }
    }

    public class Flag
    {
        public int Id { get; set; }
        public string TypeOfPost { get; set; }
        public int MemberId { get; set; }
    }
}
