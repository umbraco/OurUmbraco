using Umbraco.Core.Persistence;

namespace OurUmbraco.NotificationsWeb.Models
{
    [TableName("forumTopicSubscribers")]
    public class ForumTopicSubscriber
    {
        [Column("topicId")]
        public int TopicId { get; set; }

        [Column("memberID")]
        public int MemberId { get; set; }
    }
}
