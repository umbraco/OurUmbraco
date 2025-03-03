using Newtonsoft.Json;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace OurUmbraco.Forum.Services
{
    internal class DiscourseService
    {
        internal DiscourseTopic GetTopicByOldIdAsync(int id)
        {
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["DiscourseApiBaseUrl"]);
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
                    var discourseTopic = JsonConvert.DeserializeObject<DiscourseTopic>(resultContent);
                    return discourseTopic;
                }
            }
        }

        internal List<DiscourseTopic> GetLatestTopics(string categorySlug, int categoryId)
        {

            var cacheKey = "LatestDiscourseTopics" + categorySlug + categoryId;

            return (List<DiscourseTopic>)Umbraco.Core.ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                cacheKey,
                () =>
                {
                    var forumBaseUrl = System.Configuration.ConfigurationManager.AppSettings["DiscourseApiBaseUrl"];

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(forumBaseUrl);
                        client.DefaultRequestHeaders.Add("Api-Key", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiKey"]}");
                        client.DefaultRequestHeaders.Add("Api-Username", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiUsername"]}");

                        var endPoint = $"c/{categorySlug}/{categoryId}.json?order=created";
                        var result = client.GetAsync(endPoint).Result;
                        if (result.IsSuccessStatusCode == false)
                        {
                            var resultContent = result.Content.ReadAsStringAsync().Result;
                            var errorModel = JsonConvert.DeserializeObject<ErrorModel>(resultContent);
                            string errors = string.Join(", ", errorModel.Errors);

                            //Logger.Debug(typeof(DiscourseController), $"Listing lastest topic from {endPoint} didn't succeeed: {errors}");

                            return null;
                        }
                        else
                        {
                            var resultContent = result.Content.ReadAsStringAsync().Result;
                            var lastestTopics = JsonConvert.DeserializeObject<TopicListModel>(resultContent);
                            foreach (var topic in lastestTopics.TopicList.Topics)
                            {
                                var latestPostUser = topic.LastPosterUsername;
                                var user = lastestTopics.Users.FirstOrDefault(x => x.Username == latestPostUser);
                                if (user != null)
                                {
                                    topic.AuthorName = user.Name;
                                    topic.AuthorAvatar = $"{forumBaseUrl}{user.AvatarTemplate.Replace("{size}", "112")}";
                                }
                                topic.LastUpdatedFriendly = topic.LastPostedAt.ConvertToRelativeTime();
                                topic.ForumCategory = "Umbraco questions";
                            }

                            return lastestTopics.TopicList.Topics;
                        }
                    }
                }, TimeSpan.FromMinutes(1));
        }
    }
}
