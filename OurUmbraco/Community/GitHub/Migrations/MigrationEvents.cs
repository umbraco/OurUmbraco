using Semver;
using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web;

namespace OurUmbraco.Community.GitHub.Migrations
{
    public class GitHubPullRequestMigrationEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            HandleStatisticsMigration();
        }

        private static void HandleStatisticsMigration()
        {
            const string productName = "GitHubPullRequest";
            var currentVersion = new SemVersion(0, 0, 0);

            // get all migrations for "Statistics" already executed
            var migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(productName);

            // get the latest migration for "Statistics" executed
            var latestMigration = migrations.OrderByDescending(x => x.Version).FirstOrDefault();

            if (latestMigration != null)
                currentVersion = latestMigration.Version;

            var targetVersion = new SemVersion(1, 0, 0);
            if (targetVersion == currentVersion)
                return;

            var migrationsRunner = new MigrationRunner(
                ApplicationContext.Current.Services.MigrationEntryService,
                ApplicationContext.Current.ProfilingLogger.Logger,
                currentVersion,
                targetVersion,
                productName);

            try
            {
                migrationsRunner.Execute(UmbracoContext.Current.Application.DatabaseContext.Database);
            }
            catch (Exception e)
            {
                LogHelper.Error<GitHubPullRequestMigrationEvents>("Error running GitHubPullRequest migration", e);
            }
        }
    }
}
