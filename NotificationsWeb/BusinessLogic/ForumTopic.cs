using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uForum.Services;

namespace NotificationsWeb.BusinessLogic
{
    public class ForumTopic
    {

        public static void Subscribe(int topicId, int memberId)
        {

            if(!(BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT 1 FROM forumTopicSubscribers WHERE topicId = @topicId and memberId = @memberId",
                BusinessLogic.Data.SqlHelper.CreateParameter("@topicId", topicId),
                BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId)) > 0))
            {

                Data.SqlHelper.ExecuteNonQuery(
                "INSERT INTO forumTopicSubscribers (topicId, memberId) VALUES(@topicId, @memberId)",
                Data.SqlHelper.CreateParameter("@topicId", topicId),
                Data.SqlHelper.CreateParameter("@memberId", memberId));

            }
        }

        public static void UnSubscribe(int topicId, int memberId)
        {
            Data.SqlHelper.ExecuteNonQuery(
                "DELETE FROM forumTopicSubscribers WHERE topicId = @topicId and memberId = @memberId",
                Data.SqlHelper.CreateParameter("@topicId", topicId),
                Data.SqlHelper.CreateParameter("@memberId", memberId));
        }

    }
}
