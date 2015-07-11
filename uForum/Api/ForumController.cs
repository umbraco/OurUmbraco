using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Newtonsoft.Json;
using uForum.AntiSpam;
using uForum.Extensions;
using uForum.Library;
using uForum.Models;
using uForum.Services;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.helpers;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace uForum.Api
{
    [MemberAuthorize(AllowType = "member")]
    public class ForumController : ForumControllerBase
    {
        /* COMMENTS */

        [HttpPost]
        public ExpandoObject Comment(CommentSaveModel model)
        {
            dynamic o = new ExpandoObject();
            var currentMemberId = Members.GetCurrentMemberId();

            var c = new Comment();
            c.Body = model.Body;
            c.MemberId = currentMemberId;
            c.Created = DateTime.Now;
            c.ParentCommentId = model.Parent;
            c.TopicId = model.Topic;
            c.IsSpam = c.DetectSpam();
            CommentService.Save(c);
            if (c.IsSpam)
                SpamChecker.SendSlackSpamReport(c.Body, c.TopicId, "comment", c.MemberId);

            o.id = c.Id;
            o.body = c.Body.Sanitize().ToString();
            o.topicId = c.TopicId;
            o.authorId = c.MemberId;
            o.created = c.Created.ConvertToRelativeTime();
            var author = Members.GetById(currentMemberId);
            o.authorKarma = author.Karma();
            o.authorName = author.Name;
            o.roles = Roles.GetRolesForUser();
            o.cssClass = model.Parent > 0 ? "level-2" : string.Empty;
            o.parent = model.Parent;

            return o;
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
            CommentService.Save(c);
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

            if (Members.IsAdmin() && c.MemberId != Members.GetCurrentMemberId())
                SendSlackNotification(BuildDeleteNotifactionPost(Members.GetCurrentMember().Name, c.MemberId));
        }

        [HttpGet]
        public string CommentMarkdown(int id)
        {
            var c = CommentService.GetById(id);

            if (c == null)
                throw new Exception("Comment not found");

            return c.Body;
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
            t.IsSpam = t.DetectSpam();
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
            TopicService.Save(t);

            o.url = string.Format("{0}/{1}-{2}", library.NiceUrl(t.ParentId), t.Id, t.UrlName);

            return o;
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

            return t.Body;
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

        [HttpPost]
        public int ApproveMember(int id)
        {
            if (Members.IsHq() == false)
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
            }

            memberService.Save(member);

            return minimumKarma;
        }

        [HttpPost]
        public void Flag(Flag flag)
        {
            var post = string.Format("A {0} has been flagged as spam for a moderator to check\n", flag.TypeOfPost);
            var member = Members.GetById(flag.MemberId);
            post = post + string.Format("Flagged by member {0} https://our.umbraco.org/member/{1}\n", member.Name, member.Id);

            var topicId = flag.Id;
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);
            if (flag.TypeOfPost == "comment")
            {
                var cs = new CommentService(ApplicationContext.Current.DatabaseContext, ts);
                var comment = cs.GetById(flag.Id);
                topicId = comment.TopicId;
            }
            var topic = ts.GetById(topicId);

            post = post+ string.Format("Topic title: *{0}*\n\n Link to {1}: http://our.umbraco.org{2}{3}\n\n", topic.Title, flag.TypeOfPost, topic.GetUrl(),    flag.TypeOfPost == "comment" ? "#comment-" + flag.Id : string.Empty);

            SendSlackNotification(post);
        }

        private static string BuildDeleteNotifactionPost(string adminName, int memberId)
        {
            var post = "Topic or comment deleted by admin " + adminName;
            post = post + string.Format("Go to affected member https://our.umbraco.org/member/{0}\n\n", memberId);

            if (memberId != 0)
            {
                var member = global::Umbraco.Web.UmbracoContext.Current.Application.Services.MemberService.GetById(memberId);

                if (member != null)
                {
                    var querystring = string.Format("api?ip={0}&email={1}&f=json", Utils.GetIpAddress(), HttpUtility.UrlEncode(member.Email));
                    post = post + string.Format("Check the StopForumSpam rating: http://api.stopforumspam.org/{0}", querystring);
                }
            }

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
