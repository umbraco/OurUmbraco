using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace OurUmbraco.Our.Models.GitHub.AutoReplies
{

    public class GitHubAutoReply
    {

        private GitHubAutoReplyPoco _poco;

        public int Id => _poco.Id;

        public string Repository => _poco.Repository;

        public int Number => _poco.Number;

        public GitHubAutoReplyType Type => (GitHubAutoReplyType) _poco.Type;

        public long CommentId => _poco.CommentId;

        public DateTime CreateDate => _poco.CreateDate;

        internal GitHubAutoReply(GitHubAutoReplyPoco poco)
        {
            _poco = poco;
        }


        public static GitHubAutoReply[] GetRepliesForIssue(string repo, int number)
        {

            if (string.IsNullOrWhiteSpace(repo)) throw new ArgumentNullException(nameof(repo), "A repository name must be specified.");

            var db = ApplicationContext.Current.DatabaseContext.Database;

            var syntax = ApplicationContext.Current.DatabaseContext.SqlSyntax;

            // Generate the SQL for the query
            var sql = new Sql()
                .Select("*")
                .From(GitHubAutoReplyPoco.TableName)
                .Where<GitHubAutoReplyPoco>(x => x.Repository == repo && x.Number == number, syntax);

            return db.Fetch<GitHubAutoReplyPoco>(sql).Select(x => new GitHubAutoReply(x)).ToArray();

        }

        public static void AddReply(string repo, int number, GitHubAutoReplyType type, long commentId)
        {

            var db = ApplicationContext.Current.DatabaseContext.Database;

            var poco = new GitHubAutoReplyPoco
            {
                Repository = repo,
                Number = number,
                Type = (int) type,
                CommentId = commentId,
                CreateDate = DateTime.UtcNow
            };

            db.Insert(poco);

        }

        public static bool HasReply(Issue issue, GitHubAutoReplyType type)
        {
            return HasReply(issue.RepoSlug, issue.Number, type);
        }

        public static bool HasReply(string repo, int number, GitHubAutoReplyType type)
        {
            return GetRepliesForIssue(repo, number).Any(x => x.Type == type);
        }

    }

}