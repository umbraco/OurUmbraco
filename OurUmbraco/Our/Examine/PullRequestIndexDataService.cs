using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.SearchCriteria;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub.Models;

namespace OurUmbraco.Our.Examine
{
    /// <summary>
    /// Data service used for pull requests
    /// </summary>
    public class PullRequestIndexDataService : ISimpleDataService
    {
        public string IndexType { get; set; } = "pullrequest";

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var simpleDataSetList = new List<SimpleDataSet>();
            var pullRequestsJsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/GithubPullRequests.json");
            if (pullRequestsJsonPath == null || File.Exists(pullRequestsJsonPath) == false)
                return simpleDataSetList;

            var content = File.ReadAllText(pullRequestsJsonPath);
            var pulls = JsonConvert.DeserializeObject<List<GithubPullRequestModel>>(content);
            
            var contributors = GetContributors();

            foreach (var pullRequest in pulls)
            {
                var simpleDataSet = ToSimpleDataSet(pullRequest, contributors, indexType);
                simpleDataSetList.Add(simpleDataSet);
            }

            return simpleDataSetList;
        }

        public List<SearchResult> GetContributors()
        {
            var searcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
            var criteria = (LuceneSearchCriteria)searcher.CreateSearchCriteria();

            // TODO: Linq is inefficient. Find Lucene query to give all results which are not empty for the `github` field
            criteria = (LuceneSearchCriteria) criteria.RawQuery("*:*");
            var searchResults = searcher.Search(criteria);
            var contributors = searchResults
                .Where(x => x.Fields["github"] != null && string.IsNullOrWhiteSpace(x.Fields["github"]) == false).ToList();

            return contributors;
        }

        public void UpdateIndex(GithubPullRequestModel pullRequest, IEnumerable<SearchResult> contributors)
        {
            var simpleDataSet = ToSimpleDataSet(pullRequest, contributors, IndexType);
            var examineNode = simpleDataSet.RowData.ToExamineXml(pullRequest.Id, IndexType);
            ExamineManager.Instance.IndexProviderCollection["PullRequestIndexer"].ReIndexNode(examineNode, IndexType);
        }

        private static SimpleDataSet ToSimpleDataSet(GithubPullRequestModel pullRequest, IEnumerable<SearchResult> contributors, string indexType)
        {
            var simpleDataSet = new SimpleDataSet
            {
                RowData = new Dictionary<string, string>(),
                NodeDefinition = new IndexedNode { NodeId = pullRequest.Id, Type = indexType }
            };

            simpleDataSet.RowData.Add("repository", pullRequest.Repository ?? string.Empty);
            simpleDataSet.RowData.Add("state", pullRequest.State ?? string.Empty);
            simpleDataSet.RowData.Add("title", pullRequest.Title ?? string.Empty);
            simpleDataSet.RowData.Add("createdAt", pullRequest.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
            simpleDataSet.RowData.Add("updatedAt", pullRequest.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
            simpleDataSet.RowData.Add("closedAt", pullRequest.ClosedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
            simpleDataSet.RowData.Add("mergedAt", pullRequest.MergedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
            simpleDataSet.RowData.Add("userId", pullRequest.User?.Id.ToString() ?? string.Empty);
            simpleDataSet.RowData.Add("userLogin", pullRequest.User?.Login ?? string.Empty);

            if (pullRequest.User != null)
            {
                var contributor = contributors.FirstOrDefault(x => string.Equals(x.Fields["github"],
                    pullRequest.User.Login, StringComparison.InvariantCultureIgnoreCase));
                if (contributor != null)
                    simpleDataSet.RowData.Add("memberId", contributor.Id.ToString());
            }

            return simpleDataSet;
        }

        public void UpdateIndex(int memberId, string githubLogin)
        {
            var searcher = ExamineManager.Instance.SearchProviderCollection["PullRequestSearcher"];
            var criteria = (LuceneSearchCriteria)searcher.CreateSearchCriteria();
            criteria = (LuceneSearchCriteria)criteria.RawQuery($"userLogin:\"{githubLogin}\"");
            var searchResults = searcher.Search(criteria);
            foreach (var searchResult in searchResults)
            {
                // Don't change any of the fields
                var simpleDataSet = ToSimpleDataSet(searchResult);

                // Except put a new memberId in the memberId field
                simpleDataSet.RowData.Add("memberId", memberId.ToString());

                var examineNode = simpleDataSet.RowData.ToExamineXml(searchResult.Id, IndexType);
                ExamineManager.Instance.IndexProviderCollection["PullRequestIndexer"].ReIndexNode(examineNode, IndexType);
            }
        }

        private SimpleDataSet ToSimpleDataSet(SearchResult searchResult)
        {
            var simpleDataSet = new SimpleDataSet
            {
                RowData = new Dictionary<string, string>(),
                NodeDefinition = new IndexedNode { NodeId = searchResult.Id, Type = IndexType }
            };

            simpleDataSet.RowData.Add("repository", searchResult["repository"]);
            simpleDataSet.RowData.Add("state", searchResult["state"]);
            simpleDataSet.RowData.Add("title", searchResult["title"]);
            simpleDataSet.RowData.Add("createdAt", searchResult["createdAt"]);
            simpleDataSet.RowData.Add("updatedAt", searchResult["updatedAt"]);
            simpleDataSet.RowData.Add("closedAt", searchResult["closedAt"]);
            simpleDataSet.RowData.Add("mergedAt", searchResult["mergedAt"]);
            simpleDataSet.RowData.Add("userId", searchResult["userId"]);
            simpleDataSet.RowData.Add("userLogin", searchResult["userLogin"]);

            return simpleDataSet;
        }
    }
}
