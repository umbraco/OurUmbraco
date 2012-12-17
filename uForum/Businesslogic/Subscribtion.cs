using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.DataLayer;

namespace uForum.Businesslogic {
    public class Subscribtion {

        public int TopicId { get; set; }
        public int MemberId { get; set; }
        public DateTime Date { get; set; }

        public static List<Subscribtion> Subscriptions(int memberId) {
            List<Subscribtion> l = new List<Subscribtion>();
            IRecordsReader rr = Data.SqlHelper.ExecuteReader("SELECT * from forumTopicSubscribers where memberId = @memberId",
                Data.SqlHelper.CreateParameter("@memberId", memberId));

            while (rr.Read()) {
                Subscribtion s = new Subscribtion();
                s.MemberId = rr.GetInt("memberId");
                s.TopicId = rr.GetInt("topicId");
                s.Date = rr.GetDateTime("date");
                l.Add(s);
            }

            return l;
        }

        public static List<Subscribtion> Subscribers(int topicId) {
            List<Subscribtion> l = new List<Subscribtion>();
            IRecordsReader rr = Data.SqlHelper.ExecuteReader("SELECT * from forumTopicSubscribers where topicid = @topicId",
                Data.SqlHelper.CreateParameter("@topicId", topicId));

            while (rr.Read()) {
                Subscribtion s = new Subscribtion();
                s.MemberId = rr.GetInt("memberId");
                s.TopicId = rr.GetInt("topicId");
                s.Date = rr.GetDateTime("date");
                l.Add(s);
            }

            return l;
        }


        public static void Subscribe(int memberId, int topicId) {
            if (!IsASubscriber(memberId, topicId)) {
                Data.SqlHelper.ExecuteNonQuery("INSERT INTO forumTopicSubscribers(topicId, memberid) VALUES(@topicId, @memberId)",
                    Data.SqlHelper.CreateParameter("@topicId", topicId),
                    Data.SqlHelper.CreateParameter("@memberid", memberId));
            }
        }

        public static void Cancel(int memberId, int topicId) {
            Data.SqlHelper.ExecuteNonQuery("DELETE FROM forumTopicSubscribers where memberId = @memberId and topicId = @topicId",
            Data.SqlHelper.CreateParameter("@topicId", topicId),
            Data.SqlHelper.CreateParameter("@memberid", memberId));
        }

        public static bool IsASubscriber(int memberId, int topicId) {
            return (Data.SqlHelper.ExecuteScalar<int>("SELECT count(memberid) from forumTopicSubscribers where memberId = @memberId and topicId = @topicId",
                Data.SqlHelper.CreateParameter("@topicId", topicId),
                Data.SqlHelper.CreateParameter("@memberid", memberId)) > 0);

        }


    }

}
