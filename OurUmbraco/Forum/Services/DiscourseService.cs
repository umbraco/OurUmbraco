using Newtonsoft.Json;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Forum.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace OurUmbraco.Forum.Services
{
    internal class DiscourseService
    {
        internal async Task<DiscourseTopic> GetTopicByOldIdAsync(int id)
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

                var result = await client.GetAsync($"t/external_id/{id}.json");

                if (result.IsSuccessStatusCode == false)
                {
                    return null;
                }
                else
                {
                    var resultContent = await result.Content.ReadAsStringAsync();
                    var discourseTopic = JsonConvert.DeserializeObject<DiscourseTopic>(resultContent);
                    return discourseTopic;
                }
            }
        }

        internal async Task<List<DiscourseTopic>> GetLatestTopicsAsync(string categorySlug, int categoryId)
        {
            var cacheKey = "LatestDiscourseTopics" + categorySlug + categoryId;

            return await AsyncMemoryCache.GetOrAddAsync("myCacheKey", async () =>
            {
                var forumBaseUrl = System.Configuration.ConfigurationManager.AppSettings["DiscourseApiBaseUrl"];

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(forumBaseUrl);
                    client.DefaultRequestHeaders.Add("Api-Key", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiKey"]}");
                    client.DefaultRequestHeaders.Add("Api-Username", $"{System.Configuration.ConfigurationManager.AppSettings["DiscourseApiUsername"]}");

                    var endPoint = $"c/{categorySlug}/{categoryId}.json?order=created";
                    var result = await client.GetAsync(endPoint);
                    if (!result.IsSuccessStatusCode)
                    {
                        var resultContent = await result.Content.ReadAsStringAsync();
                        var errorModel = JsonConvert.DeserializeObject<ErrorModel>(resultContent);
                        string errors = string.Join(", ", errorModel.Errors);

                        // Logger.Debug(typeof(DiscourseController), $"Listing latest topics from {endPoint} didn't succeed: {errors}");

                        return null;
                    }
                    else
                    {
                        var resultContent = await result.Content.ReadAsStringAsync();
                        var latestTopics = JsonConvert.DeserializeObject<TopicListModel>(resultContent);
                        foreach (var topic in latestTopics.TopicList.Topics)
                        {
                            var latestPostUser = topic.LastPosterUsername;
                            var user = latestTopics.Users.FirstOrDefault(x => x.Username == latestPostUser);
                            if (user != null)
                            {
                                topic.AuthorName = user.Name;
                                topic.AuthorAvatar = $"{forumBaseUrl}{user.AvatarTemplate.Replace("{size}", "112")}";
                            }
                            topic.LastUpdatedFriendly = topic.LastPostedAt.ConvertToRelativeTime();
                            topic.ForumCategory = "Umbraco questions";
                        }

                        return latestTopics.TopicList.Topics.OrderByDescending(x => x.LastPostedAt).ToList();
                    }
                }
            }, TimeSpan.FromMinutes(5));
        }
    }
}


public static class AsyncMemoryCache
{
    private static readonly MemoryCache _cache = MemoryCache.Default;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();

    public static async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan absoluteExpiration)
    {
        if (_cache.Contains(key))
        {
            return (T)_cache.Get(key);
        }

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        try
        {
            await semaphore.WaitAsync();

            // Double-check if the value was added while waiting for the lock
            if (_cache.Contains(key))
            {
                return (T)_cache.Get(key);
            }

            T result;
            try
            {
                result = await valueFactory();
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                throw; // Re-throw or handle as needed
            }

            if (result != null)
            {
                _cache.Add(key, result, DateTimeOffset.Now.Add(absoluteExpiration));
            }

            return result;
        }
        finally
        {
            semaphore.Release();
            _locks.TryRemove(key, out _); // Clean up the lock
        }
    }
}