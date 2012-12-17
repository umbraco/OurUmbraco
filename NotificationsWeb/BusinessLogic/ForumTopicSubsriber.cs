using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotificationsWeb.BusinessLogic
{
    public class ForumTopicSubsriber
    {

        public static void Subscribe(int topicId, int memberId)
        {
            umbraco.DataLayer.IRecordsReader dr =
                Data.SqlHelper.ExecuteReader("SELECT FROM forumTopicSubscribers WHERE topicId = @topicId and memberId = @memberId",
                Data.SqlHelper.CreateParameter("@topicId", topicId),
                Data.SqlHelper.CreateParameter("@memberId", topicId));

            if (!dr.Read())
            {
                Data.SqlHelper.ExecuteNonQuery(
                "INSERT INTO forumTopicSubscribers (topicId, memberId) VALUES(@topicId, @memberId)",
                Data.SqlHelper.CreateParameter("@topicId", topicId),
                Data.SqlHelper.CreateParameter("@memberId", topicId));

            }
        }

        public static void UnSubscribe(int topicId, int memberId)
        {
            Data.SqlHelper.ExecuteNonQuery(
                "DELETE FROM forumTopicSubscribers WHERE topicId = @topicId and memberId = @memberId",
                Data.SqlHelper.CreateParameter("@topicId", topicId),
                Data.SqlHelper.CreateParameter("@memberId", topicId));
        }
    }
}
