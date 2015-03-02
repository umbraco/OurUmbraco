using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using uForum.Models;
using uForum.Services;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace uForum.Api
{
    [MemberAuthorize( AllowType="member" )]
    public class ForumController : ForumControllerBase
    {
        /* COMMENTS */

        [HttpPost]
        public ExpandoObject Comment(CommentViewModel model)
        {
            dynamic o = new ExpandoObject();
            var c = new Comment();
            c.Body = model.Body;
            c.MemberId = Members.GetCurrentMemberId();
            c.Created = DateTime.Now;
            c.ParentCommentId = model.Parent;
            c.TopicId = model.Topic;
            c.IsSpam = c.DetectSpam();
            CommentService.Save(c);
            if (c.IsSpam)
                AntiSpam.SpamChecker.SendSlackSpamReport(c.Body, c.TopicId, "comment", c.MemberId);

            o.id = c.Id;
            o.body = c.Body.Sanitize().ToString();
            o.topicId = c.TopicId;
            o.authorId = c.MemberId;
            o.created = c.Created.ConvertToRelativeTime();
            var author = c.Author();
            o.authorKarma = author.Karma();
            o.authorName = author.Name;
            o.roles = System.Web.Security.Roles.GetRolesForUser();
            o.cssClass = model.Parent > 0 ? "level-2" : string.Empty;
            o.parent = model.Parent;

            return o;
        }

        [HttpPut]
        public void Comment(int id, CommentViewModel model)
        {
            var c = CommentService.GetById(id);

            if (c == null)
                throw new Exception("Comment not found");

            if (c.MemberId != Members.GetCurrentMemberId())
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

            if (!Library.Utils.IsModerator() && c.MemberId != Members.GetCurrentMemberId())
                throw new Exception("You cannot delete this comment");

            CommentService.Delete(c);
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

            if (c == null)
                throw new Exception("Comment not found");

            c.IsSpam = true;

            CommentService.Save(c);
        }

        [HttpPost]
        public void CommentAsHam(int id)
        {
            var c = CommentService.GetById(id);

            if (c == null)
                throw new Exception("Comment not found");

            c.IsSpam = false;

            CommentService.Save(c);
        }
        

        [HttpPost]
        public ExpandoObject Topic(TopicViewModel model)
        {
            dynamic o = new ExpandoObject();

            var t = new Topic();
            t.Body = model.Body;
            t.Title = model.Title;
            t.MemberId = Members.GetCurrentMemberId();
            t.Created = DateTime.Now;
            t.ParentId = model.Forum;
            t.UrlName = umbraco.cms.helpers.url.FormatUrl(model.Title);
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
                AntiSpam.SpamChecker.SendSlackSpamReport(t.Body, t.Id, "topic", t.MemberId);

            o.url = string.Format("{0}/{1}-{2}", umbraco.library.NiceUrl(t.ParentId), t.Id, t.UrlName);

            return o;
        }


        [HttpPut]
        public ExpandoObject Topic(int id, TopicViewModel model)
        {
            dynamic o = new ExpandoObject();

            var t = TopicService.GetById(id);

            if (t == null)
                throw new Exception("Topic not found");

            if (t.MemberId != Members.GetCurrentMemberId())
                throw new Exception("You cannot edit this topic");

            t.Updated = DateTime.Now;
            t.Body = model.Body;
            t.Version = model.Version;
            t.ParentId = model.Forum;
            t.Title = model.Title;
            TopicService.Save(t);

            o.url = string.Format("{0}/{1}-{2}", umbraco.library.NiceUrl(t.ParentId), t.Id, t.UrlName);

            return o;
        }


        [HttpDelete]
        public void Topic(int id)
        {
            var c = CommentService.GetById(id);

            if (c == null)
                throw new Exception("Topic not found");

            if (c.MemberId != Members.GetCurrentMemberId())
                throw new Exception("You cannot delete this topic");

            CommentService.Delete(c);
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

            if (t == null)
                throw new Exception("Topic not found");

            t.IsSpam = false;

            TopicService.Save(t);
        }

        [HttpPost]
        public void TopicAsSpam(int id)
        {
            var t = TopicService.GetById(id);

            if (t == null)
                throw new Exception("Topic not found");

            t.IsSpam = true;

            TopicService.Save(t);
        }

        /* MEDIA */
        [HttpPost]
        public  HttpResponseMessage EditorUpload()
        {
            dynamic result = new ExpandoObject();
            var httpRequest = System.Web.HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                string filename = string.Empty;

                Guid g = Guid.NewGuid();

                foreach (string file in httpRequest.Files)
                {
                   
                    DirectoryInfo updir = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("/media/upload/" + g));
                  
                    if (!updir.Exists)
                        updir.Create();
                    
                    var postedFile = httpRequest.Files[file];
                    
                    var filePath = updir.FullName + "/" +  postedFile.FileName;
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
    }
}
