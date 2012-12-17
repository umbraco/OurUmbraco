using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uForum.Businesslogic;

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


        public static List<uForum.Businesslogic.Forum> GetSubscribedForums(int memberId)
        {
            List<uForum.Businesslogic.Forum> lt = new List<uForum.Businesslogic.Forum>();
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader(
                "SELECT forumId FROM forumSubscribers WHERE memberId = " + memberId.ToString()
            );

            while (dr.Read())
            {
                lt.Add(new uForum.Businesslogic.Forum(dr.GetInt("forumId")));
            }
            dr.Close();
            dr.Dispose();

            return lt;
        }

        public static List<Topic> GetSubscribedTopics(int memberId)
        {
            List<Topic> lt = new List<Topic>();
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader(
                "SELECT topicId FROM forumTopicSubscribers WHERE memberId = " + memberId.ToString()
            );

            while (dr.Read())
            {
                lt.Add(new Topic(dr.GetInt("topicId")));
            }
            dr.Close();
            return lt;
        }
    }
}
