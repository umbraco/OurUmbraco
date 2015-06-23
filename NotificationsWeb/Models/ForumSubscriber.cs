using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace NotificationsWeb.Models
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
