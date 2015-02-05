using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uForum.Models;
using uForum.Services;
using Umbraco.Web.WebApi;

namespace uForum.Api
{
    [MemberAuthorize(AllowType = "member")]
    public class TopicController : UmbracoApiController
    {
        public void Post(TopicViewModel model)
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

        public void Put(int id, TopicViewModel model)
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

        //api/topic
        public void Delete(int id)
        {
            using (var cs = new TopicService())
            {
                var c = cs.GetById(id);

                if (c == null)
                    throw new Exception("Topic not found");

                if (!Library.Utills.IsModerator() && c.MemberId != Members.GetCurrentMemberId())
                    throw new Exception("You cannot delete this topic");

                cs.Delete(c);
            }
        }
    }
}
