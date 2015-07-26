using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OurUmbraco.Our;

namespace our.Businesslogic
{
    public class ProjectContributor
    {
        public int ProjectId { get; set; }
        public int MemberId { get; set; }

        public ProjectContributor(int projectId, int memberId)
        {
            ProjectId = projectId;
            MemberId = memberId;
        }

        public void Add()
        {
            if (!(Data.SqlHelper.ExecuteScalar<int>("SELECT 1 FROM projectContributors WHERE projectId = @projectId and memberId = @memberId;",
                Data.SqlHelper.CreateParameter("@projectId", ProjectId),
                Data.SqlHelper.CreateParameter("@memberId", MemberId)) > 0))
            {
                Data.SqlHelper.ExecuteNonQuery(
                   "INSERT INTO projectContributors(projectId,memberId) values(@projectId,@memberId);",
                   Data.SqlHelper.CreateParameter("@projectId", ProjectId),
                   Data.SqlHelper.CreateParameter("@memberId", MemberId));

            }
        }
        public void Delete()
        {
            Data.SqlHelper.ExecuteNonQuery(
                "DELETE FROM projectContributors WHERE projectId = @projectId and memberId = @memberId;",
                Data.SqlHelper.CreateParameter("@projectId", ProjectId),
                Data.SqlHelper.CreateParameter("@memberId", MemberId));
        }
    }
}
