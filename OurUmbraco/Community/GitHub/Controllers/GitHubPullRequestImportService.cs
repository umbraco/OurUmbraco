using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub.Models;
using RestSharp;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace OurUmbraco.Community.GitHub.Controllers
{
    public class GitHubPullRequestImportService
    {
        private readonly string _token = ConfigurationManager.AppSettings["GitHubAccessToken"];

        public async Task UpdatePageOfStoredPullRequests(string repository, string cursor = "")
        {
            var database = ApplicationContext.Current.DatabaseContext.Database;
            var query = new Sql()
                .Select("id")
                .From<GitHubPullRequestDataModel>(new SqlServerSyntaxProvider());
            var ids = new HashSet<string>(database.Fetch<string>(query));

            var client = new RestClient("https://api.github.com/graphql");
            var prs = await RequestPage(client, cursor, repository);

            var uowProvider = new PetaPocoUnitOfWorkProvider();
            using (var uow = uowProvider.GetUnitOfWork())
            {
                foreach (var pr in prs.PullRequests.Where(p => !ids.Contains(p.Id)))
                {
                    uow.Database.Insert(pr);
                }
                uow.Commit();
            }

            if (prs.PullRequests.Any() && prs.PullRequests.All(p => !ids.Contains(p.Id)))
            {
                Hangfire.BackgroundJob.Schedule(() => UpdatePageOfStoredPullRequests(repository, prs.LastCursor), TimeSpan.FromSeconds(5));
            }
        }

        private async Task<PrPage> RequestPage(IRestClient client, string lastCursor, string repository)
        {
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"bearer {_token}");
            request.AddJsonBody(new { query = new MergedPullRequestsQuery(repository, lastCursor).ToString() });

            var response = await client.ExecuteTaskAsync(request);
            if (response.IsSuccessful)
            {
                var st = JsonConvert.DeserializeObject<Rootobject>(response.Content);
                if (st.PullRequests.Any())
                {
                    var pullRequests = st.PullRequests
                        .Where(e => e.Author != null)
                        .Select(pr => new GitHubPullRequestDataModel
                        {
                            Id = pr.Id,
                            Number = pr.Number,
                            Created = pr.CreatedAt,
                            LastModified = pr.UpdatedAt,
                            Additions = pr.Additions,
                            Deletions = pr.Deletions,
                            AuthorAvatarUrl = pr.Author.AvatarUrl,
                            AuthorUrl = pr.Author.Url,
                            AuthorLogin = pr.Author.Login,
                            Url = pr.Url
                        })
                        .ToList();

                    return new PrPage { PullRequests = pullRequests, LastCursor = st.LastCursor };
                }
                else
                {
                    return new PrPage { PullRequests = Enumerable.Empty<GitHubPullRequestDataModel>(), LastCursor = "" };
                }
            }
            return null; // TODO : Not null
        }
        
        internal class MergedPullRequestsQuery
        {
            private const string _queryFormat = "{{repository(owner: \"Umbraco\", name: \"{0}\") {{ pullRequests(last: 50, states: MERGED {1} ) {{ edges {{ node {{ id, number, url, additions, deletions, updatedAt, createdAt, author {{ login, url, avatarUrl }} }} cursor }} }} }} }}";

            private readonly string _repository;
            private readonly string _lastCursor;

            public MergedPullRequestsQuery(string repository, string lastCursor = null)
            {
                _repository = repository;
                _lastCursor = lastCursor;
            }

            public override string ToString()
            {
                return string.Format(_queryFormat, _repository, string.IsNullOrEmpty(_lastCursor) ? "" : $"before: \"{_lastCursor}\"");
            }
        }

        internal class PrPage
        {
            public IEnumerable<GitHubPullRequestDataModel> PullRequests { get; set; }
            public string LastCursor { get; set; }
        }


        internal class Rootobject
        {
            public Data Data { get; set; }
            public IEnumerable<Node> PullRequests => Data.Repository.PullRequests.Edges.Select(e => e.Node);
            public string LastCursor => Data.Repository.PullRequests.Edges.First().Cursor;
        }

        internal class Data
        {
            public Repository Repository { get; set; }
        }

        internal class Repository
        {
            public Pullrequests PullRequests { get; set; }
        }

        internal class Pullrequests
        {
            public Edge[] Edges { get; set; }
        }

        internal class Edge
        {
            public Node Node { get; set; }
            public string Cursor { get; set; }
        }

        internal class Node
        {
            public string Id { get; set; }
            public int Number { get; set; }
            public string Url { get; set; }
            public Author Author { get; set; }
            public int? Additions { get; set; }
            public int? Deletions { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        internal class Author
        {
            public string Login { get; set; }
            public string Url { get; set; }
            public string AvatarUrl { get; set; }
        }

    }
}
