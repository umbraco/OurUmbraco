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
    public class TopicController : ForumControllerBase
    {
        public void Post(TopicSaveModel model)
        {
            var t = new Topic();
            t.Body = model.Body;
            t.MemberId = Members.GetCurrentMemberId();
            t.Created = DateTime.Now;
            t.ParentId = model.Forum;
            TopicService.Save(t);
        }

        public void Put(int id, TopicSaveModel model)
        {
            var c = TopicService.GetById(id);

            if (c == null)
                throw new Exception("Topic not found");

            if (c.MemberId != Members.GetCurrentMemberId())
                throw new Exception("You cannot edit this topic");

            c.Body = model.Body;
            TopicService.Save(c);
        }

        //api/topic
        public void Delete(int id)
        {
            var c = TopicService.GetById(id);

            if (c == null)
                throw new Exception("Topic not found");

            if (!Library.Utils.IsModerator() && c.MemberId != Members.GetCurrentMemberId())
                throw new Exception("You cannot delete this topic");

            TopicService.Delete(c);
        }
    }
}
