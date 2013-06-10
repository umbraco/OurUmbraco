using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.XPath;

namespace uForum.Library {
    /// <summary>
    /// This class contains raw xml from the database server, so extra coloumns addded etc, will be included in these feeds.
    /// These are uncached feeds, so should only be used with macros that 
    /// </summary>
    public class RawXml {

        public static XPathNodeIterator TopicsWithParticipation(int memberId, int maxItems, int page) {
            
            // Check for paging
            int pageSize = maxItems;
            int pageStart = page;

            int pageEnd = ((page + 1) * pageSize);

            if (pageStart > 0)
            {
                pageStart = (page * pageSize) + 1;
            }

            string sql = @"WITH Topics  AS
                        (
                            SELECT forumTopics.*, ROW_NUMBER() OVER (ORDER BY updated DESC) AS RowNumber 
                            from forumTopics 
                            
                            where id IN(
                            SELECT forumTopics.id
                            FROM [forumTopics]
                            LEFT JOIN forumComments ON forumComments.topicId = forumTopics.id
                            where forumTopics.memberId = " + memberId + " OR forumComments.memberId = " + memberId + @"
                            )
                        )
                        
                        SELECT	id, parentId, RowNumber, memberId, title, body, created, updated, locked, latestReplyAuthor, latestComment, replies, score, urlname, answer
                        FROM	Topics
                        WHERE	RowNumber BETWEEN " + pageStart + " AND " + pageEnd + @"  
                        ORDER BY RowNumber ASC;";
            
            return Businesslogic.Data.GetDataSet(sql, "topic");
        }

        public static XPathNodeIterator CountTopicsWithParticipation(int memberId)
        {

            string sql = @"SELECT count(DISTINCT forumTopics.id)
                            FROM [forumTopics]
                            LEFT JOIN forumComments ON forumComments.topicId = forumTopics.id
                            where forumTopics.memberId = " + memberId + " OR forumComments.memberId = " + memberId;
            
            var x =  Businesslogic.Data.GetDataSet(sql, "yourTopicsCount");
            return x;
        }

        public static XPathNodeIterator TopicsWithAuthor(int memberId) {
            return Businesslogic.Data.GetDataSet("SELECT * FROM forumTopics where memberid = " + memberId.ToString(), "topic");
        }

        public static XPathNodeIterator Comment(int commentId)
        {
            return Businesslogic.Data.GetDataSet("SELECT * FROM forumComments where id = " + commentId.ToString(), "comment");
        }

        public static XPathNodeIterator Topic(int topicId)
        {
            var topic = uForum.Businesslogic.Topic.GetTopic(topicId);
            var topicXml = topic.ToXml(new XmlDocument());
            return topicXml.CreateNavigator().Select(".");
//            return Businesslogic.Data.GetDataSet("SELECT * FROM forumTopics where id = " + topicId.ToString(), "topic");
        }

        public static XPathNodeIterator Forum(int forumId) {
            return Businesslogic.Data.GetDataSet("SELECT * FROM forumForums where id = " + forumId.ToString(), "forum");
        }

        public static XPathNodeIterator Topics(int forumId, int maxItems, int page) {

            // Check for paging
            int pageSize = maxItems;
            int pageStart = page;

            int pageEnd = ((page + 1) * pageSize);

            if (pageStart > 0) {
                pageStart = (page * pageSize) + 1;
            }
            

            string sql = @"WITH Topics  AS
                        (
                            SELECT	id, parentId, memberId, title, body, created, updated, locked, latestReplyAuthor, latestComment, replies, score, urlname, answer,
		                            ROW_NUMBER() OVER (ORDER BY updated DESC) AS RowNumber
                            FROM	dbo.forumTopics
                            WHERE parentId = " + forumId.ToString() + @"
                        )
                        SELECT	id, parentId, RowNumber, memberId, title, body, created, updated, locked, latestReplyAuthor, latestComment, replies, score, urlname, answer
                        FROM	Topics
                        WHERE	RowNumber BETWEEN " + pageStart.ToString() + " AND " + pageEnd.ToString() + @"  
                        ORDER BY RowNumber ASC;";


            return Businesslogic.Data.GetDataSet(sql, "topic");
        }

        public static XPathNodeIterator CommentsByDate(int topicId, int maxItems, int page, string order) {
            // Check for paging
            int pageSize = maxItems;
            int pageStart = page;

            int pageEnd = ((page + 1) * pageSize);

            if (pageStart > 0) {
                pageStart = (page * pageSize) + 1;
            }
            
                        
            string sql = @"WITH Comments  AS
                            (
	                            SELECT	id, topicId, memberId, body, created, score, position,
			                            ROW_NUMBER() OVER (ORDER BY created " + order  + @") AS RowNumber
	                            FROM	dbo.forumComments
	                            WHERE topicId = " + topicId.ToString() + @"
                            )
                            SELECT	id, topicId, memberId, body, created, score, position, RowNumber
                            FROM	Comments
                            WHERE	RowNumber BETWEEN " + pageStart.ToString() + " AND " + pageEnd.ToString() + @" 
                            ORDER BY RowNumber ASC;";
            
            return Businesslogic.Data.GetDataSet(sql, "comment");
        }

        public static XPathNodeIterator CommentsByScore(int topicId, int maxItems, int page, string order) {

            // Check for paging
            int pageSize = maxItems;
            int pageStart = page;

            int pageEnd = ((page + 1) * pageSize);

            if (pageStart > 0) {
                pageStart = (page * pageSize) + 1;
            }


            string sql = @"WITH Comments  AS
                            (
	                            SELECT	id, topicId, memberId, body, created, score, position,
			                            ROW_NUMBER() OVER (ORDER BY score " + order + @") AS RowNumber
	                            FROM	dbo.forumComments
	                            WHERE topicId = " + topicId.ToString() + @"
                            )
                            SELECT	id, topicId, memberId, body, created, score, position, RowNumber
                            FROM	Comments
                            WHERE	RowNumber BETWEEN " + pageStart.ToString() + " AND " + pageEnd.ToString() + @" 
                            ORDER BY RowNumber ASC;";

            return Businesslogic.Data.GetDataSet(sql, "comment");
        }

        public static XPathNodeIterator AllForums() {
            return Businesslogic.Data.GetDataSet("SELECT * FROM forumForums", "forum");
        }

        public static XPathNodeIterator Forums(int parentId) {
            return Businesslogic.Data.GetDataSet("SELECT * FROM forumForums where parentId = " + parentId.ToString(), "forum");
        }


        public static XPathNodeIterator LatestTopicsSinceDate(int maxItems, int page, DateTime sinceDate) {

            // Check for paging
            int pageSize = maxItems;
            int pageStart = page;

            int pageEnd = ((page + 1) * pageSize);

            if (pageStart > 0) {
                pageStart = (page * pageSize) + 1;
            }


            string sql = @"WITH Topics  AS
                        (
                            SELECT	dbo.forumTopics.id, dbo.forumTopics.parentId, dbo.forumTopics.memberId, dbo.forumTopics.title, dbo.forumTopics.body, dbo.forumTopics.created, dbo.forumTopics.updated, dbo.forumTopics.locked, dbo.forumTopics.latestReplyAuthor, dbo.forumTopics.replies, dbo.forumTopics.score, dbo.forumTopics.urlname, dbo.forumTopics.answer, dbo.forumTopics.latestComment,
		                            ROW_NUMBER() OVER (ORDER BY dbo.forumTopics.updated DESC) AS RowNumber 
                            FROM	dbo.forumTopics INNER JOIN dbo.ForumForums on dbo.forumTopics.ParentId = dbo.ForumForums.Id WHERE dbo.forumforums.parentId != 1057 AND dbo.ForumTopics.updated > " + sinceDate.ToShortDateString() + @"
                        )
                        SELECT	id, parentId, RowNumber, memberId, title, body, created, updated, locked, latestReplyAuthor, replies, score, urlname, answer, latestComment
                        FROM	Topics
                        WHERE	RowNumber BETWEEN " + pageStart.ToString() + " AND " + pageEnd.ToString() + @"  
                        ORDER BY RowNumber ASC;";


            return Businesslogic.Data.GetDataSet(sql, "topic");
        }


        public static XPathNodeIterator LatestTopics(int maxItems, int page) {
            return LatestTopicsSinceDate(maxItems, page, DateTime.MinValue);
        }
    }
}
