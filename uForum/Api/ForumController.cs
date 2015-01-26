using System;
using System.Collections.Generic;
using System.Linq;
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
        public void Comment(CommentViewModel model)
        {
            using (var cs = new CommentService())
            {
                var c = new Comment();
                c.Body = model.Body;
                c.MemberId = Members.GetCurrentMemberId();
                c.Created = DateTime.Now;
                c.ParentCommentId = model.Parent;
                c.TopicId = model.Topic;
                cs.Save(c);
            }
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
    }
}
