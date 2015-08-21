using System;
using System.IO;
using System.Xml;
using umbraco.DataLayer;

namespace OurUmbraco.Powers.Buddha
{
    public class Buddha : IDisposable
    {
        public string ConnectionString { get; set; }
        public string WebRoot { get; set; }

        private XmlDocument _config;
        private ISqlHelper _sqlhelper;

        private Char sep = System.IO.Path.DirectorySeparatorChar;
       
        //settings
        private int _top = 25;
        public bool UseThreading { get; set; }
        private string karmaDir = "upowers";


        public Buddha(string connection, string root)
        {
            ConnectionString = connection;
            WebRoot = root;

            if (!Directory.Exists(WebRoot + sep + karmaDir))
                Directory.CreateDirectory(WebRoot + sep + karmaDir);

            _sqlhelper = DataLayerHelper.CreateSqlHelper(ConnectionString);

            _config = new XmlDocument();
            _config.Load(WebRoot + sep + "config" + sep + "uPowers.config");
        }

        //here we will calculate the /upowers/karma.xml file for all the overviews 
        public void CalculateKarma()
        {
            string file = WebRoot + sep + karmaDir + sep + "karma.xml";

            XmlDocument doc = new XmlDocument();
            string xml = BusinessLogic.Data.GetDataSetAsNode(ConnectionString, TotalKarmaSQL(365, 25), "karma").OuterXml;

            doc.LoadXml(xml);

            if (File.Exists(file))
                File.Delete(file);

            doc.Save(file);
            doc = null;
        }

        //here we do all the detail, and dig into each individual member
        public void CalculateKarmaHistory()
        {
            string memberSql = "SELECT nodeId from cmsMember";

            IRecordsReader rr = _sqlhelper.ExecuteReader(memberSql);

            while (rr.Read())
            {
                ProcessMember( rr.GetInt("nodeId") );
            }
        }

        //here we process the individual member
        public void ProcessMember(int memberId)
        {
            string karmaRoot = Path.Combine(WebRoot, karmaDir);
            string memberKarmaFile = Path.Combine(karmaRoot, memberId.ToString() + ".xml");

            XmlDocument doc = new XmlDocument();
            
            string xml = "<karma>" + BusinessLogic.Data.GetDataSetAsNode(ConnectionString, MemberKarmaSummarySQL(memberId), "summary").OuterXml;
            xml += BusinessLogic.Data.GetDataSetAsNode(ConnectionString, MemberKarmaHistorySQL(memberId), "history").OuterXml + "</karma>";

            doc.LoadXml(xml);

            string forumTopics = "SELECT count(id) from forumTopics WHERE memberID = " + memberId.ToString();
            string forumComments = "SELECT count(id) from forumComments WHERE memberID = " + memberId.ToString();

            int topicCount = _sqlhelper.ExecuteScalar<int>(forumTopics);
            int commentCount = _sqlhelper.ExecuteScalar<int>(forumComments);

            XmlNode karma = doc.SelectSingleNode("//karma");
            XmlNode forum = umbraco.xmlHelper.addTextNode(doc, "forum","");
            forum.AppendChild( umbraco.xmlHelper.addTextNode(doc, "topics", topicCount.ToString()));
            forum.AppendChild(umbraco.xmlHelper.addTextNode(doc, "comments", commentCount.ToString()));
            karma.AppendChild(forum);


            if (File.Exists(memberKarmaFile))
                File.Delete(memberKarmaFile);

            doc.Save(memberKarmaFile);
            doc = null; 
        }

        //for each type the member has karmapoints in, we will process the points
        public void ProcessType(string alias)
        {

        }
        
        public static string TotalKarmaSQL(int days, int topCount)
        {
            string where = string.Format("where DATEDIFF(DAY, date, GETDATE()) < {0}", days);
            string top = string.Format("TOP {0}", topCount);

            if (days <= 0)
                where = "";

            if (topCount <= 0)
                top = "";

            return string.Format(@"with score as(
                          SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints) as received
                          FROM [powersTopic]
                          {0}
                          group by receiverId
                          
                        UNION ALL
                        
                        SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints)as received
                        FROM [powersProject]
                        {0}
                        GROUP BY receiverId  
                        
                        UNION ALL
                        SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints)as received
                          FROM [powersComment]
                          {0}
                          GROUP BY receiverId
                       
                        UNION ALL
                         SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM [powersComment]
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersProject
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersTopic
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersWiki
                          {0}
                          GROUP BY memberId
                         )
                         
                         select {1} text as memberName, memberId, sum(performed) as performed, SUM(received) as received, (sum(received) + sum(performed)) as totalPoints from score
                           inner join umbracoNode ON memberId = id  
                        where memberId IS NOT NULL and memberId > 0
                         group by text, memberId order by totalPoints DESC", where, top);

        }

        public static string MemberKarmaSummarySQL(int memberId)
        {
            return string.Format(@"SELECT 'topic' as alias, receiverId AS memberId, 0 as performed, SUM(receiverPoints) as received
                          FROM [powersTopic]
                          where receiverId = {0}
                          group by receiverId
                          
                        UNION ALL
                        
                        SELECT 'project' as alias, receiverId AS memberId, 0 as performed, SUM(receiverPoints)as received
                        FROM [powersProject]
                        where receiverId = {0}
                        GROUP BY receiverId  
                        
                        UNION ALL
                        SELECT 'comment' as alias, receiverId AS memberId, 0 as performed, SUM(receiverPoints)as received
                          FROM [powersComment]
                          where receiverId = {0}
                          GROUP BY receiverId
                       
                        UNION ALL
                         SELECT 'comment' as alias, memberId, SUM(performerPoints)as performed, 0 as received
                          FROM [powersComment]
                          where memberId = {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT 'project' as alias, memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersProject
                          where memberId = {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT 'topic' as alias, memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersTopic
                          where memberId = {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT 'wiki' as alias, memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersWiki
                          where receiverId = {0}
                          GROUP BY memberId", memberId);
        }

        public static string MemberKarmaHistorySQL(int memberId)
        {
            return string.Format( @"
                        SELECT id, 'topic' as alias, receiverId AS memberId, 0 as performed, receiverPoints as received
                          FROM [powersTopic]
                          where receiverId = {0}
                          
                        UNION ALL
                        
                        SELECT id, 'project' as alias, receiverId AS memberId, 0 as performed,  receiverPoints as received
                        FROM [powersProject]
                        where receiverId = {0}
                       
                        
                        UNION ALL
                        SELECT id, 'comment' as alias, receiverId AS memberId, 0 as performed, receiverPoints as received
                          FROM [powersComment]
                          where receiverId = {0}
                       
                       
                        UNION ALL
                         SELECT id, 'comment' as alias, memberId, performerPoints as performed, 0 as received
                          FROM [powersComment]
                          where memberId = {0}
                        
                          
                        UNION ALL
                        SELECT id, 'project' as alias, memberId, performerPoints as performed, 0 as received
                          FROM powersProject
                          where memberId = {0}
                        
                          
                        UNION ALL
                        SELECT id, 'topic' as alias, memberId, performerPoints as performed, 0 as received
                          FROM powersTopic
                          where memberId = {0}
                       
                          
                        UNION ALL
                        SELECT id, 'wiki' as alias, memberId, performerPoints as performed, 0 as received
                          FROM powersWiki
                          where receiverId = {0}
                    ", memberId);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _config = null;
            _sqlhelper.Dispose();
            _sqlhelper = null;
        }

        #endregion
    }
}
