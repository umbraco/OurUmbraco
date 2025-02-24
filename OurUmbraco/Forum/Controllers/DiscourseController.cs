using Newtonsoft.Json;
using OurUmbraco.Forum.Models;
using OurUmbraco.Forum.Services;
using System.Net.Http;
using System;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurUmbraco.Forum.Controllers
{
    public class DiscourseController : UmbracoApiController
    {
        [HttpPost]
        public IHttpActionResult Post(TopicModel topicModel)
        {
            var topicService = new TopicService(ApplicationContext.DatabaseContext);
            var topic = topicService.GetById(topicModel.TopicId);
            var forumUrl = System.Configuration.ConfigurationManager.AppSettings["OurUmbracoUrl"];

            var body = topic.Body.Replace("/media/upload/", $"{forumUrl}/media/upload/");
            var topicUrl = $"{forumUrl}/forum/{topic.Id}-{topic.Title.ToUrlSegment()}";
            body = body + $"\n<hr>\n<small>This is a companion discussion topic for the original entry at <a href=\"{topicUrl}\">{topicUrl}</a></small>";
            var discourseCreateTopic = new DiscourseCreateTopic
            {
                Title = topic.Title,
                Raw = body,
                Category = 5,
                CreatedAt = topic.Created,
                EmbedUrl = topicUrl,
                ExternalId = topic.Id
            };
            var redirectUrl = CreateTopicOnDiscourse(discourseCreateTopic);
            return Ok(redirectUrl);
        }

        [HttpPost]
        public IHttpActionResult TopicCreated(DiscourseTopicModel topicModel)
        {
            var secret = HttpContext.Current.Request.QueryString["secret"];
            if (secret == System.Configuration.ConfigurationManager.AppSettings["DiscourseWebhookSecret"])
            {
                // this is called as a webhook from Discourse to Our - it contains any newly created topic.
                // If it has an external id that matches a forum post on Our, we notify the people involved in the thread.
                var newUrl = "https://forum.umbraco.com/t/" + topicModel.Topic.Slug + "/" + topicModel.Topic.Id;

                if (int.TryParse(topicModel.Topic.ExternalId, out int externalId))
                {
                    var newForumCommentNotification = new NotificationsCore.Notifications.NewForumComment();
                    newForumCommentNotification.SendNewForumTopicCreatedNotification(externalId, newUrl);
                }
            }

            return Ok();
        }

        private string CreateTopicOnDiscourse(DiscourseCreateTopic createTopic)
        {
            var forumBaseUrl = System.Configuration.ConfigurationManager.AppSettings["DiscourseApiBaseUrl"];
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(forumBaseUrl);
                client.DefaultRequestHeaders.Add("Api-Key", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiKey"]}");
                client.DefaultRequestHeaders.Add("Api-Username", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiUsername"]}");

                var result = client.PostAsJsonAsync("posts.json", createTopic).Result;
                if (result.IsSuccessStatusCode == false)
                {
                    var resultContent = result.Content.ReadAsStringAsync().Result;
                    var errorModel = JsonConvert.DeserializeObject<ErrorModel>(resultContent);
                    var firstError = errorModel.Errors.FirstOrDefault();
                    if (firstError != null && firstError == "External has already been taken")
                    {
                        // the topic exists, see if we can find the URL for it
                        var topicUrl = GetTopicByExternalId(createTopic.ExternalId);
                        return topicUrl;
                    }
                    return null;
                }
                else
                {
                    var resultContent = result.Content.ReadAsStringAsync().Result;
                    var discourseTopic = JsonConvert.DeserializeObject<DiscourseTopic>(resultContent);
                    return forumBaseUrl + discourseTopic.PostUrl;
                }
            }
        }

        private string GetTopicByExternalId(int id)
        {
            var forumBaseUrl = System.Configuration.ConfigurationManager.AppSettings["DiscourseApiBaseUrl"];
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(forumBaseUrl);
                client.DefaultRequestHeaders.Add("Api-Key", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiKey"]}");
                client.DefaultRequestHeaders.Add("Api-Username", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiUsername"]}");

                var result = client.GetAsync($"t/external_id/{id}.json").Result;
                if (result.IsSuccessStatusCode == false)
                {
                    return null;
                }
                else
                {
                    var resultContent = result.Content.ReadAsStringAsync().Result;
                    var discourseTopic = JsonConvert.DeserializeObject<DiscoursePostStream>(resultContent);
                    var firstPost = discourseTopic.PostStream.Posts.FirstOrDefault();
                    if(firstPost == null)
                    {
                        return null;
                    }
                    return forumBaseUrl + firstPost.PostUrl;
                }
            }
        }

        public class TopicModel
        {
            public int TopicId { get; set; }
        }

        internal class DiscourseTopic
        {
            [JsonProperty("post_url")]
            public string PostUrl { get; set; }
        }

        internal class ErrorModel
        {
            [JsonProperty("action")]
            public string Action { get; set; }

            [JsonProperty("errors")]
            public List<string> Errors { get; set; }
        }

        internal class DiscoursePostStream
        {
            [JsonProperty("post_stream")]
            public PostStream PostStream { get; set; }
        }

        internal class PostStream
        {
            [JsonProperty("posts")]
            public List<DiscoursePost> Posts { get; set; }
        }
        internal class DiscoursePost
        {
            [JsonProperty("post_url")]
            public string PostUrl { get; set; }
        }

        // the model that the webhook sends us
        public class DiscourseTopicModel
        {
            [JsonProperty("topic")]
            public Topic Topic { get; set; }
        }

        public class Topic
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("slug")]
            public string Slug { get; set; }

            [JsonProperty("external_id")]
            public string ExternalId { get; set; }
        }

    }
}
