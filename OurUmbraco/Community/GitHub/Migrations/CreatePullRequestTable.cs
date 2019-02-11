using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurUmbraco.Community.GitHub.Models;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace OurUmbraco.Community.GitHub.Migrations
{
    [Migration("1.0.0", 1, "GitHubPullRequest")]
    public class CreateGitHubPullRequestTable : MigrationBase
    {
    
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        public CreateGitHubPullRequestTable(ISqlSyntaxProvider sqlSyntax, Umbraco.Core.Logging.ILogger logger)
            : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        public override void Up()
        {
            _schemaHelper.CreateTable<GitHubPullRequestDataModel>(false);
        }

        public override void Down()
        {
            _schemaHelper.DropTable<GitHubPullRequestDataModel>();
        }
    }
}
