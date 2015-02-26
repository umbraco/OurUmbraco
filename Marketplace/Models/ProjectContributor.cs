using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;

namespace Marketplace.Models
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