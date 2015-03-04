using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace uForum.Models
{
    [TableName("forumForums")]
    [PrimaryKey("id", autoIncrement = false)]
    [ExplicitColumns]
    public class Forum
    {
        [Ignore]
        public string Title { get; set; }
        [Ignore]
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

        [Column("latestComment")]
        public int LatestComment { get; set; }

        [Column("latestTopic")]
        public int LatestTopic { get; set; }

        [Column("latestAuthor")]
        public int LatestAuthor { get; set; }

    }
}
