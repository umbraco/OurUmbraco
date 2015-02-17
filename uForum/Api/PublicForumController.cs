using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using uForum.Services;
using Umbraco.Web.WebApi;

namespace uForum.Api
{
    public class PublicForumController: UmbracoApiController
    {

        /* TOPICS */
        [HttpGet]
        public IEnumerable<ExpandoObject> LatestPaged(int page, int cat)
        {
            var l = new List<ExpandoObject>();
            using (var ts = new TopicService())
            {
                foreach (var topic in ts.GetLatestTopics(50, page, true, cat).Items)
                {
                    dynamic o = new ExpandoObject();

                    o.url = topic.Url;
                    o.title = topic.Title;
                    o.replies = topic.Replies;
                    o.hasAnswer = topic.Answer > 0;
                    o.updated = topic.Updated.ConvertToRelativeTime();
                    if (topic.LatestReplyAuthor > 0)
                    {
                        var ms = Services.MemberService;
                        var mem = ms.GetById(topic.LatestReplyAuthor);
                        o.memId = mem.Id;
                        o.memName = mem.Name;
                    }
                    else
                    {
                        o.memId = topic.Author().Id;
                        o.memName = topic.Author().Name;
                    }


                    var forum = Umbraco.TypedContent(topic.ParentId);
                    o.forumUrl = forum.Url;
                    o.forumName = forum.Name;


                    l.Add(o);
                }
            }
            return l;
        }

        [HttpGet]
        public string CategoryUrl(int id)
        {
            return Umbraco.TypedContent(id).Url;
        }
    }
}
