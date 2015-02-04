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
using Umbraco.Web.WebApi;

namespace uForum.Api
{
    [MemberAuthorize( AllowType="member" )]
    public class ForumController : UmbracoApiController
    {
        /* COMMENTS */

        [HttpPost]
        public ExpandoObject Comment(CommentViewModel model)
        {
            dynamic o = new ExpandoObject();

            using (var cs = new CommentService())
            {
                var c = new Comment();
                c.Body = model.Body;
                c.MemberId = Members.GetCurrentMemberId();
                c.Created = DateTime.Now;
                c.ParentCommentId = model.Parent;
                c.TopicId = model.Topic;
                cs.Save(c);

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
            }

            return o;
        }

        [HttpPut]
        public void Comment(int id, CommentViewModel model)
        {
            using (var cs = new CommentService())
            {
                var c = cs.GetById(id);
                
                if (c == null)
                    throw new Exception("Comment not found");

                if(c.MemberId != Members.GetCurrentMemberId())
                    throw new Exception("You cannot edit this comment");
                
                c.Body = model.Body;
                cs.Save(c);
            }
        }

        [HttpDelete]
        public void Comment(int id)
        {
            using (var cs = new CommentService())
            {
                var c = cs.GetById(id);

                if (c == null)
                    throw new Exception("Comment not found");

                if (c.MemberId != Members.GetCurrentMemberId())
                    throw new Exception("You cannot delete this comment");

                cs.Delete(c);
            }
        }



        /* TOPICS */
        [HttpPost]
        public void Topic(TopicViewModel model)
        {
            using (var ts = new TopicService())
            {
                var t = new Topic();
                t.Body = model.Body;
                t.MemberId = Members.GetCurrentMemberId();
                t.Created = DateTime.Now;
                t.ParentId = model.Forum;
                ts.Save(t);
            }
        }


        [HttpPut]
        public void Topic(int id, TopicViewModel model)
        {
            using (var cs = new TopicService())
            {
                var c = cs.GetById(id);

                if (c == null)
                    throw new Exception("Topic not found");

                if (c.MemberId != Members.GetCurrentMemberId())
                    throw new Exception("You cannot edit this topic");

                c.Body = model.Body;
                cs.Save(c);
            }
        }


        [HttpDelete]
        public void Topic(int id)
        {
            using (var cs = new TopicService())
            {
                var c = cs.GetById(id);

                if (c == null)
                    throw new Exception("Topic not found");

                if (c.MemberId != Members.GetCurrentMemberId())
                    throw new Exception("You cannot delete this topic");

                cs.Delete(c);
            }
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
