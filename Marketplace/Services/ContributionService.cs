using Marketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Marketplace.Services
{
    public class ContributionService: IDisposable
    {
        private DatabaseContext DatabaseContext;

        public ContributionService()
        {
            init(ApplicationContext.Current.DatabaseContext);
        }

        public ContributionService(DatabaseContext dbContext)
        {
            init(dbContext);
        }

        private void init(DatabaseContext dbContext)
        {
            DatabaseContext = dbContext;
        }

        public IEnumerable<ProjectContributor> GetContributions(int projectId)
        {
            var sql = new Sql()
                .Select("*")
                .From<ProjectContributor>();

            sql.Where<ProjectContributor>(x => x.ProjectId == projectId);

            return DatabaseContext.Database.Query<ProjectContributor>(sql);
        }

        public void DeleteContributor(int projectId, int memberId)
        {
            DatabaseContext.Database.Delete<ProjectContributor>(
              "Where projectId=@0 and memberId=@1",
              projectId,memberId);
        }

        public void AddContributor(int projectId, int memberId)
        {
            var r = DatabaseContext.Database.SingleOrDefault<ProjectContributor>(
               "SELECT * FROM projectContributors WHERE projectId=@0 and memberId=@1",
               projectId,
               memberId);

            if (r == null)
            {
                var rec = new ProjectContributor();
                rec.MemberId = memberId;
                rec.ProjectId = projectId;

                DatabaseContext.Database.Insert(rec);
            }
        }


        public static ContributionService Instance
        {
            get
            {
                return Singleton<ContributionService>.UniqueInstance;
            }
        }

        public void Dispose()
        {
            DatabaseContext.DisposeIfDisposable();
        }
    }
}