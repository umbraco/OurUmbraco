using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//WB Added
using Examine;
using Examine.LuceneEngine;
using System.Data;
using System.Xml;
using System.Text;
using uForum.Models;
using uForum.Services;
using System.Globalization;
using our.Examine.DocumentationIndexDataService.Helper;

namespace our
{
   
    /// <summary>
    /// The data service used by the LuceneEngine in order for it to reindex all data
    /// </summary>
    public class ForumDataService : ISimpleDataService
    {
        private static int m_CurrentId = 0;

        private static readonly object m_Locker = new object();


        #region ISimpleDataService Members

        /// <summary>
        /// Returns a list of type SimpleDataSet based on the SampleData.xml data
        /// </summary>
        /// <param name="indexType"></param>
        /// <returns></returns>
        /// 
        

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var data = new List<SimpleDataSet>();

            using(var ts = new TopicService())
            using (var cs = new CommentService())
            {
                foreach (var currentTopic in ts.GetAll().Take(1000))
                {
                    //First generate the accumulated comment text:
                    string commentText = String.Empty;

                    foreach (var currentComment in cs.GetComments(currentTopic.Id))
                        commentText += umbraco.library.StripHtml(currentComment.Body);

                    var body = currentTopic.Body + commentText;

                    //Add the item to the index..
                    data.Add(new SimpleDataSet()
                    {
                        //Create the node definition, ensure that it is the same type as referenced in the config
                        NodeDefinition = new IndexedNode()
                        {
                            NodeId = currentTopic.Id,
                            Type = "forum"
                        },
                        //add the data to the row
                        RowData = new Dictionary<string, string>() 
                        {
                            { "nodeName", SanitizeXmlString(currentTopic.Title.Replace("<![CDATA[", string.Empty).Replace("]]>",string.Empty))},
                            { "body", SanitizeXmlString(umbraco.library.StripHtml(body.Replace("<![CDATA[", string.Empty).Replace("]]>",string.Empty)))},
                            { "updateDate", currentTopic.Updated.SerializeForLucene()},
                            { "url", currentTopic.Url},
                            { "nodeTypeAlias","forum"},


                            { "Created", currentTopic.Created.ToString()},
                            { "LatestComment", currentTopic.LatestComment.ToString()},
                            { "LatestReplyAuthor", currentTopic.LatestReplyAuthor.ToString()},
                            { "Locked", currentTopic.Locked.ToString()},
                            { "MemberId", currentTopic.MemberId.ToString()},
                            { "ParentId", currentTopic.ParentId.ToString()},
                            { "Replies", currentTopic.Replies.ToString()},
                            { "UrlName", currentTopic.UrlName.ToString()}
                        }
                    });

                    // 
                }

                return data;
            }

            
        }

        public SimpleDataSet CreateNewDocument()
        {
            lock (m_Locker)
            {
                //JobDetailItem jobDetails = new JobDetailItem();
                Topic forumTopic = new Topic();

                //First generate the accumulated comment text:
                string commentText = String.Empty;
                return new SimpleDataSet()
                {
                    NodeDefinition = new IndexedNode() { NodeId = (++m_CurrentId), Type = "forum" },
                    RowData = new Dictionary<string, string>() 
                    {
                        { "Title", SanitizeXmlString(forumTopic.Title.Replace("<![CDATA[", string.Empty).Replace("]]>",string.Empty))},
                        { "Body", SanitizeXmlString(umbraco.library.StripHtml(forumTopic.Body.Replace("<![CDATA[", string.Empty).Replace("]]>",string.Empty)))},
                        { "Created", forumTopic.Created.ToString()},
                        { "LatestComment", forumTopic.LatestComment.ToString()},
                        { "LatestReplyAuthor", forumTopic.LatestReplyAuthor.ToString()},
                        { "Locked", forumTopic.Locked.ToString()},
                        { "MemberId", forumTopic.MemberId.ToString()},
                        { "ParentId", forumTopic.ParentId.ToString()},
                        { "Replies", forumTopic.Replies.ToString()},
                        { "UrlName", forumTopic.UrlName.ToString()},
                        {"nodeTypeAlias","forum"},
                        { "updateDate", forumTopic.Updated.SerializeForLucene()},
                        { "CommentsContent", SanitizeXmlString(commentText.Replace("<![CDATA[", string.Empty).Replace("]]>",string.Empty))}
                    }
                };
            }
        }

        public SimpleDataSet CreateNewDocument(int id)
        {
            lock (m_Locker)
            {
                //JobDetailItem jobDetails = new JobDetailItem();
                using(var ts = new TopicService())
                using (var cs = new CommentService())
                {

                    var forumTopic = ts.GetById(id);

                    //First generate the accumulated comment text:
                    string commentText = String.Empty;

                    foreach (var currentComment in cs.GetComments(forumTopic.Id))
                        commentText += umbraco.library.StripHtml(currentComment.Body);

                    return new SimpleDataSet()
                    {
                        NodeDefinition = new IndexedNode() { NodeId = (id), Type = "forum" },
                        RowData = new Dictionary<string, string>() 
                        {
                            { "Title", SanitizeXmlString(forumTopic.Title.Replace("<![CDATA[", string.Empty).Replace("]]>",string.Empty))},
                            { "Body", SanitizeXmlString(umbraco.library.StripHtml(forumTopic.Body.Replace("<![CDATA[", string.Empty).Replace("]]>",string.Empty)))},
                            { "Created", forumTopic.Created.ToString()},
                            { "LatestComment", forumTopic.LatestComment.ToString()},
                            { "LatestReplyAuthor", forumTopic.LatestReplyAuthor.ToString()},
                            { "Locked", forumTopic.Locked.ToString()},
                            { "MemberId", forumTopic.MemberId.ToString()},
                            { "ParentId", forumTopic.ParentId.ToString()},
                            { "Replies", forumTopic.Replies.ToString()},
                            { "UrlName", forumTopic.UrlName.ToString()},
                            {"nodeTypeAlias","forum"},
                            { "updateDate", forumTopic.Updated.SerializeForLucene()},
                            { "CommentsContent", SanitizeXmlString(commentText.Replace("<![CDATA[", string.Empty).Replace("]]>",string.Empty))}
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Remove illegal XML characters from a string.
        /// </summary>
        public string SanitizeXmlString(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            StringBuilder buffer = new StringBuilder(xml.Length);

            foreach (char c in xml)
            {
                if (IsLegalXmlChar(c))
                {
                    buffer.Append(c);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Whether a given character is allowed by XML 1.0.
        /// </summary>
        public bool IsLegalXmlChar(int character)
        {
            return
            (
                 character == 0x9 /* == '\t' == 9   */          ||
                 character == 0xA /* == '\n' == 10  */          ||
                 character == 0xD /* == '\r' == 13  */          ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }


        #endregion


    }
}


