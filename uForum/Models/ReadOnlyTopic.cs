using System.Collections.Generic;
using Umbraco.Core.Persistence;

namespace uForum.Models
{
    /// <summary>
    /// A topic with all of it's data including comments and author names
    /// </summary>
    public class ReadOnlyTopic : Topic
    {
        [ResultColumn]
        public List<ReadOnlyComment> Comments { get; set; }

        [ResultColumn("AuthorName")]
        public string AuthorName { get; set; }

        [ResultColumn("LastReplyAuthorName")]
        public string LastReplyAuthorName { get; set; } 
    }
}