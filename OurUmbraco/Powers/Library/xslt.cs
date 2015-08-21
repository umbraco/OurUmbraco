using System.Xml;
using System.Xml.XPath;
using OurUmbraco.Powers.BusinessLogic;

namespace OurUmbraco.Powers.Library {
    [Umbraco.Core.Macros.XsltExtension("uPowers")]
    public class Xslt {

        //this will be changed to something abit less static before Upowers are released, if it ever is...
        public static XPathNodeIterator MemberKarma(int InLastNumberOfDays, int maxItems) {
            string SQL = Buddha.Buddha.TotalKarmaSQL(InLastNumberOfDays, maxItems);
            return Data.GetDataSet(SQL, "score");
        }
        
        public static XPathNodeIterator Reputation(int memberId) {
            XmlDocument xd = new XmlDocument();
            return new BusinessLogic.Reputation(memberId).ToXml(xd).CreateNavigator().Select(".");
        }


        public static XPathNodeIterator ItemsVotedFor(int memberId, string dataBaseTable)
        {
            XmlDocument xd = new XmlDocument();

            string sql = string.Format(@"SELECT TOP 1000 [id]
                                  ,sum([points]) as points
                                  ,sum([receiverPoints]) as receiverPoints
                            FROM {0}
                            where memberId = {1}
                            group by id
                            ORDER by receiverPoints DESC", dataBaseTable, memberId);

            return Data.GetDataSet(sql, "item");
        }


        public static XPathNodeIterator PopularItems(string dataBaseTable) {
            XmlDocument xd = new XmlDocument();
            return Data.GetDataSet("SELECT id, count(points) as count, AVG(points) as average, SUM(points) as score from " + dataBaseTable + " GROUP BY id ", "item");
        }

        public static XPathNodeIterator History(int itemKey, string dataBaseTable) {
            XmlDocument xd = new XmlDocument();
            string db = dataBaseTable.Split(' ')[0];
            return Data.GetDataSet("SELECT * from " + db + " where id = " + itemKey.ToString(), "vote");
        }

        public static int Score(int id, string dataBaseTable) {
            return BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT SUM(points) AS count FROM " + dataBaseTable + " WHERE (id = @id)",
                BusinessLogic.Data.SqlHelper.CreateParameter("@id", id)); 
        }

        public static bool HasVoted(int memberId, int id, string dataBaseTable) {
            return (BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT count(points) FROM " + dataBaseTable + " WHERE (id = @id) AND (memberId = @memberId)",
                BusinessLogic.Data.SqlHelper.CreateParameter("@id", id), BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId)) > 0);
        }
        
        public static int YourVote(int memberId, int id, string dataBaseTable) {
            return BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT sum(points) FROM " + dataBaseTable + " WHERE (id = @id) AND (memberId = @memberId)",
                BusinessLogic.Data.SqlHelper.CreateParameter("@id", id), BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId));
        }

		public static int GetExternalUrlData(int memberId, string url)
		{
			// get the Item Id of the Url
			var id = BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT id FROM externalUrls WHERE (@url = url)", BusinessLogic.Data.SqlHelper.CreateParameter("@url", url));

			// doesn't exist ... then create a new entry
			if (id == 0)
				id = BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("INSERT INTO externalUrls (memberId, url) VALUES (@memberId, @url); SELECT SCOPE_IDENTITY()", BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId), BusinessLogic.Data.SqlHelper.CreateParameter("@url", url));

			return id;
		}

		public static bool ExternalHasVoted(int receiverId, int itemId)
		{
			// get the current member's Id
			var memberId = umbraco.cms.businesslogic.member.Member.CurrentMemberId();

			// if the member is the same as the receiver - return false
			if (memberId == receiverId)
				return true;

			// check if member has already voted for the Url
			return (BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT count(points) FROM powersExternal WHERE (id = @id) AND (memberId = @memberId)", BusinessLogic.Data.SqlHelper.CreateParameter("@id", itemId), BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId)) > 0);
		}
    }
}
