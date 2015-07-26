using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web.Http;
using OurUmbraco.Forum.Extensions;
using Umbraco.Core.Models;

namespace OurUmbraco.Forum.Api
{
    public class PublicForumController : ForumControllerBase
    {

        /* TOPICS */
        [HttpGet]
        public IEnumerable<ExpandoObject> LatestPaged(int page, int cat)
        {
            var l = new List<ExpandoObject>();
            foreach (var topic in TopicService.GetLatestTopics(50, page, true, cat))
            {
                dynamic o = new ExpandoObject();

                o.url = topic.GetUrl();
                o.title = topic.Title;
                o.replies = topic.Replies;
                o.hasAnswer = topic.Answer > 0;
                o.updated = topic.Updated.ConvertToRelativeTime();
                o.updatedLong = string.Format("{0:ddd, dd MMM yyyy} {0:HH:mm:ss} UTC+{1}", topic.Updated, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now));
                if (topic.LatestReplyAuthor > 0)
                {
                    o.memId = topic.LatestReplyAuthor;
                    o.memName = topic.LastReplyAuthorName;
                    //var mem = Members.GetById(topic.LatestReplyAuthor);
                    //if (mem != null)
                    //{
                    //    o.memId = mem.Id;
                    //    o.memName = mem.Name;
                    //}
                }
                else
                {
                    o.memId = topic.MemberId;
                    o.memName = topic.AuthorName;
                    //var author = Members.GetById(topic.MemberId);
                    //if (author != null)
                    //{
                    //    o.memId = author.Id;
                    //    o.memName = author.Name;
                    //}
                }

                IPublishedContent forum;
                if (topic.ParentId > 0 && ((forum = Umbraco.TypedContent(topic.ParentId)) != null))
                {
                    o.forumUrl = forum.Url;

                    o.forumName = Library.Utils.GetForumName(forum);
                }
                //var forum = Umbraco.TypedContent(topic.ParentId);
                //if (forum != null)
                //{
                //    o.forumUrl = forum.Url;
                //    o.forumName = forum.Name;
                //}

                l.Add(o);
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
