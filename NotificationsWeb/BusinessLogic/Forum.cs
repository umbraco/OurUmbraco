using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotificationsWeb.BusinessLogic
{
    public class Forum
    {
        public static void Subscribe(int forumId, int memberId)
        {

            if (!(BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT 1 FROM forumSubscribers WHERE forumId = @forumId and memberId = @memberId",
                BusinessLogic.Data.SqlHelper.CreateParameter("@forumId", forumId),
                BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId)) > 0))
            {

                Data.SqlHelper.ExecuteNonQuery(
                "INSERT INTO forumSubscribers (forumId, memberId) VALUES(@forumId, @memberId)",
                Data.SqlHelper.CreateParameter("@forumId", forumId),
                Data.SqlHelper.CreateParameter("@memberId", memberId));

            }
        }

        public static void UnSubscribe(int forumId, int memberId)
        {
            Data.SqlHelper.ExecuteNonQuery(
                "DELETE FROM forumSubscribers WHERE forumId = @forumId and memberId = @memberId",
                Data.SqlHelper.CreateParameter("@forumId", forumId),
                Data.SqlHelper.CreateParameter("@memberId", memberId));
        }

        public static void RemoveAllSubscriptions(int forumId)
        {
            Data.SqlHelper.ExecuteNonQuery(
               "DELETE FROM forumSubscribers WHERE forumId = @forumId",
               Data.SqlHelper.CreateParameter("@forumId", forumId));
        }
    }
}
