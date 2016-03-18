using Umbraco.Core.Persistence;

namespace OurUmbraco.NotificationsWeb.Models
{
    [TableName("forumSubscribers")]
    public class ForumSubscriber
    {
        [Column("forumId")]
        public int ForumId { get; set; }

        [Column("memberID")]
        public int MemberId { get; set; }
    }
}
