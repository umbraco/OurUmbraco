using uProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace uProject.Services
{
    public class ContributionService
    {
        private readonly DatabaseContext _dbContext;

        public ContributionService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<ProjectContributor> GetContributors(int projectId)
        {
            var sql = new Sql()
                .Select("*")
                .From<ProjectContributor>();

            sql.Where<ProjectContributor>(x => x.ProjectId == projectId);

            return _dbContext.Database.Query<ProjectContributor>(sql);
        }

        public void DeleteContributor(int projectId, int memberId)
        {
            _dbContext.Database.Delete<ProjectContributor>(
              "Where projectId=@0 and memberId=@1",
              projectId,memberId);
        }

        public void AddContributor(int projectId, int memberId)
        {
            var r = _dbContext.Database.SingleOrDefault<ProjectContributor>(
               "SELECT * FROM projectContributors WHERE projectId=@0 and memberId=@1",
               projectId,
               memberId);

            if (r == null)
            {
                var rec = new ProjectContributor();
                rec.MemberId = memberId;
                rec.ProjectId = projectId;

                _dbContext.Database.Insert(rec);
            }
        }
    }
}