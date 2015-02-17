using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace NotificationsWeb.Models
{
    [TableName("forumTopicSubscribers")]
    public class ForumTopicSubscribers
    {
        [Column("topicId")]
        public int TopicId { get; set; }

        [Column("memberID")]
        public int MemberId { get; set; }
    }
}
