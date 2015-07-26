using Umbraco.Core.Persistence;

namespace OurUmbraco.Project.Models
{
    [TableName("projectContributors")]
    public class ProjectContributor
    {
        [Column("projectId")]
        public int ProjectId { get; set; }

        [Column("memberId")]
        public int MemberId { get; set; }
    }
}