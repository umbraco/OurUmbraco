using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core;

namespace OurUmbraco.Community.GitHub;

public class UsersService
{
    public Task<HashSet<string>> GetIgnoredGitHubUsers()
    {
        return (Task<HashSet<string>>) ApplicationContext.Current.ApplicationCache.RuntimeCache.
            GetCacheItem("IgnoredGitHubUsers", GetCacheItem, TimeSpan.FromHours(1));
    }

    private async Task<HashSet<string>> GetCacheItem()
    {
        var bearerToken = ConfigurationManager.AppSettings["CollabBearerToken"];
        const string url = "https://collaboratorsv2.euwest01.umbraco.io/umbraco/api/users/GetIgnoredUsers";

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
        var httpContent = new StringContent("[]", System.Text.Encoding.UTF8, "application/json");
        
        var response = await httpClient.PostAsync(url, httpContent);
        if (response.IsSuccessStatusCode == false)
        {
            return new HashSet<string>();
        }
        
        
        var result = response.Content.ReadAsStringAsync().Result;
        var users = JsonConvert.DeserializeObject<HashSet<string>>(result);
        return users;

    }
}