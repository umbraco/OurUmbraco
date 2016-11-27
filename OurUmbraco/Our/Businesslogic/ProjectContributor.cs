using System.Net.Mime;
using umbraco.BusinessLogic;

namespace OurUmbraco.Our.Businesslogic
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
            using (var sqlHelper = Application.SqlHelper)
            {
                var exists = sqlHelper.ExecuteScalar<int>(
                              "SELECT 1 FROM projectContributors WHERE projectId = @projectId and memberId = @memberId;",
                              sqlHelper.CreateParameter("@projectId", ProjectId),
                              sqlHelper.CreateParameter("@memberId", MemberId)) > 0;

                if (exists == false)
                {
                    sqlHelper.ExecuteNonQuery(
                        "INSERT INTO projectContributors(projectId,memberId) values(@projectId,@memberId);",
                        sqlHelper.CreateParameter("@projectId", ProjectId),
                        sqlHelper.CreateParameter("@memberId", MemberId));
                }
            }
        }
        public void Delete()
        {
            using (var sqlHelper = Application.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery(
                    "DELETE FROM projectContributors WHERE projectId = @projectId and memberId = @memberId;",
                    sqlHelper.CreateParameter("@projectId", ProjectId),
                    sqlHelper.CreateParameter("@memberId", MemberId));
            }
        }
    }
}
