using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace uForum.Models
{
    [TableName("forumForums")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    public class Forum
    {
        public string Title { get; set; }
        public string Description { get; set; }

        [Column("id")]
        public int Id { get; set; }

        [Column("parentId")]
        public int ParentId { get; set; }

        [Column("latestPostDate")]
        public DateTime LatestPostDate { get; set; }

        [Column("totalTopics")]
        public int TotalTopics { get; set; }
        [Column("totalComments")]
        public int TotalComments { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

    }
}
