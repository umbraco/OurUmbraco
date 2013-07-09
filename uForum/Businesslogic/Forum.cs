using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml;
using Joel.Net;
using umbraco.cms.businesslogic.member;
using umbraco.presentation.nodeFactory;
using umbraco.BusinessLogic;

namespace uForum.Businesslogic
{

    /// <summary>
    /// Strictly a helper class for making the code a bit more transparent
    /// It simply combines info from NodeFactory (forum info) and the custom Topic / Comment tables
    /// </summary>
    public class Forum
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        public string Title { get; private set; }
        public string Description { get; set; }
        public DateTime LatestPostDate { get; set; }
        public bool Exists { get; private set; }

        public int TotalTopics { get; set; }
        public int TotalComments { get; set; }
        public int SortOrder { get; set; }

        public Comment LatestComment
        {
            get
            {
                if (_latestCommentID > 0)
                    return new Comment(_latestCommentID);
                else
                    return null;
            }
        }
        public Topic LatestTopic
        {
            get
            {
                if (_latestTopicID > 0)
                    return Topic.GetTopic(_latestTopicID);
                else
                    return null;
            }
        }

        public Member LatestAuthor
        {
            get
            {
                if (_latestAuthorID > 0)
                    return Library.Utills.GetMember(_latestAuthorID);
                else
                    return null;
            }
        }


        private int _latestCommentID;
        private int _latestTopicID;
        private int _latestAuthorID;

        public List<Forum> SubForums { get; set; }

        private Events _e = new Events();

        public Forum() { }


        public static Forum Create(int forumId, int parentId, int sortOrder)
        {
            Forum f = new Forum();
            f.Id = forumId;
            f.ParentId = parentId;
            f.SortOrder = sortOrder;
            f.Exists = false;
            f.Save();
            return f;
        }

        public void Delete()
        {

            Data.SqlHelper.ExecuteNonQuery("DELETE FROM forumForums where ID = @id",
                        Data.SqlHelper.CreateParameter("@id", Id));

        }

        public void Save()
        {
            if (!Exists)
            {

                CreateEventArgs e = new CreateEventArgs();
                FireBeforeCreate(e);
                if (!e.Cancel)
                {
                    Data.SqlHelper.ExecuteNonQuery("INSERT INTO forumForums (id, parentId, sortOrder) VALUES(@id, @parentId, @sortOrder)",
                    Data.SqlHelper.CreateParameter("@id", Id), Data.SqlHelper.CreateParameter("@parentId", ParentId), Data.SqlHelper.CreateParameter("@sortOrder", SortOrder)
                    );

                    FireAfterCreate(e);
                }
            }
            else
            {

                UpdateEventArgs e = new UpdateEventArgs();
                FireBeforeUpdate(e);

                if (!e.Cancel)
                {

                    TotalTopics = Data.SqlHelper.ExecuteScalar<int>("SELECT count(*) from forumTopics where (forumTopics.isSpam IS NULL OR forumTopics.isSpam != 1) AND parentId = @id", Data.SqlHelper.CreateParameter("@id", Id));
                    TotalComments = Data.SqlHelper.ExecuteScalar<int>("SELECT COUNT(forumComments.id) FROM forumTopics INNER JOIN forumComments ON forumComments.topicId = forumTopics.id WHERE ((forumTopics.isSpam IS NULL OR forumTopics.isSpam != 1) AND forumTopics.parentId = @id) AND (forumComments.isSpam IS NULL OR forumComments.isSpam != 1) ", Data.SqlHelper.CreateParameter("@id", Id));

                    if (TotalTopics > 0)
                    {
                        _latestTopicID = Data.SqlHelper.ExecuteScalar<int>("SELECT TOP 1 id FROM forumTopics WHERE (forumTopics.isSpam IS NULL OR forumTopics.isSpam != 1) AND (forumTopics.parentId = @id) ORDER BY Updated DESC ", Data.SqlHelper.CreateParameter("@id", Id));
                        _latestAuthorID = Data.SqlHelper.ExecuteScalar<int>("SELECT TOP 1 latestReplyAuthor FROM forumTopics WHERE (forumTopics.isSpam IS NULL OR forumTopics.isSpam != 1) AND (forumTopics.parentId = @id) ORDER BY Updated DESC ", Data.SqlHelper.CreateParameter("@id", Id));

                        LatestPostDate = Data.SqlHelper.ExecuteScalar<DateTime>("SELECT TOP 1 updated FROM forumTopics WHERE (forumTopics.isSpam IS NULL OR forumTopics.isSpam != 1) AND (forumTopics.parentId = @id) ORDER BY Updated DESC", Data.SqlHelper.CreateParameter("@id", Id));

                        if (TotalComments > 0)
                        {
                            _latestCommentID = Data.SqlHelper.ExecuteScalar<int>("SELECT TOP 1 id FROM forumComments WHERE (forumComments.isSpam IS NULL OR forumComments.isSpam != 1) AND (topicId = @id) ORDER BY Created DESC ", Data.SqlHelper.CreateParameter("@id", _latestTopicID));
                        }
                    }



                    Data.SqlHelper.ExecuteNonQuery(@"UPDATE forumForums 
                        SET latestComment = @latestComment, latestTopic = @latestTopic, 
                        latestAuthor = @latestAuthor, totalTopics = @totalTopics, 
                        totalComments = @totalComments, latestPostDate = @latestPostDate,
                        sortOrder = @sortOrder
                        WHERE id = @id",
                        Data.SqlHelper.CreateParameter("@latestComment", _latestCommentID),
                        Data.SqlHelper.CreateParameter("@latestTopic", _latestTopicID),
                        Data.SqlHelper.CreateParameter("@latestAuthor", _latestAuthorID),
                        Data.SqlHelper.CreateParameter("@totalTopics", TotalTopics),
                        Data.SqlHelper.CreateParameter("@totalComments", TotalComments),
                        Data.SqlHelper.CreateParameter("@latestPostDate", LatestPostDate),
                        Data.SqlHelper.CreateParameter("@sortOrder", SortOrder),
                        Data.SqlHelper.CreateParameter("@id", Id)
                        );
                    FireAfterUpdate(e);
                }
            }
        }


        public void SetLatestTopic(int topicId)
        {
            _latestTopicID = topicId;
        }

        public void SetLatestComment(int commentId)
        {
            _latestCommentID = commentId;
        }

        public void SetLatestAuthor(int Id)
        {
            _latestAuthorID = Id;
        }

        public Forum(int forumID)
        {
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader("SELECT * FROM forumForums WHERE ID = @id", Data.SqlHelper.CreateParameter("@id", forumID));

            if (dr.Read())
            {
                Node n = new Node(dr.GetInt("id"));

                Forum f = new Forum();
                Id = dr.GetInt("id");
                Title = n.Name;

                if (n.GetProperty("forumDescription") != null)
                    Description = n.GetProperty("forumDescription").Value;

                Exists = true;
                TotalComments = dr.GetInt("totalComments");
                TotalTopics = dr.GetInt("totalTopics");

                //Latest private vals
                _latestAuthorID = dr.GetInt("latestAuthor");
                _latestCommentID = dr.GetInt("latestComment");
                _latestTopicID = dr.GetInt("latestTopic");
                LatestPostDate = dr.GetDateTime("latestPostDate");

            }
            else
                Exists = false;

            dr.Close();
            dr.Dispose();
        }

        public static int TotalTopicsAndComments(int memberId)
        {
            string sql = @"SELECT Count(forumTopics.id)
                            FROM [forumTopics]
                            LEFT JOIN forumComments ON forumComments.topicId = forumTopics.id
                            where forumTopics.memberId = " + memberId + " OR forumComments.memberId = " + memberId + @";";

            return Data.SqlHelper.ExecuteScalar<int>(sql);
        }

        public static List<Forum> Forums() { return Forums(0); }
        public static List<Forum> Forums(int parentId)
        {

            List<Forum> lt = new List<Forum>();

            umbraco.DataLayer.IRecordsReader dr;

            if (parentId > 0)
            {
                dr = Data.SqlHelper.ExecuteReader(
                    "SELECT * FROM forumForums WHERE parentID = @parentId ORDER by sortOrder ASC", Data.SqlHelper.CreateParameter("@parentId", parentId)
                );
            }
            else
            {
                dr = Data.SqlHelper.ExecuteReader(
                    "SELECT * FROM forumForums ORDER by sortOrder ASC"
                );
            }


            while (dr.Read())
            {
                try
                {
                    Node n = new Node(dr.GetInt("id"));

                    if (n != null)
                    {
                        Forum f = new Forum();

                        f.Id = dr.GetInt("id");
                        f.ParentId = dr.GetInt("parentId");
                        f.Title = n.Name;

                        if (n.GetProperty("forumDescription") != null)
                            f.Description = n.GetProperty("forumDescription").Value;

                        f.Exists = true;
                        f.SortOrder = dr.GetInt("SortOrder");

                        //Latest private vals
                        f._latestAuthorID = dr.GetInt("latestAuthor");
                        f._latestCommentID = dr.GetInt("latestComment");
                        f._latestTopicID = dr.GetInt("latestTopic");

                        f.LatestPostDate = dr.GetDateTime("latestPostDate");
                        f.TotalComments = dr.GetInt("totalComments");
                        f.TotalTopics = dr.GetInt("totalTopics");

                        lt.Add(f);
                    }
                }
                catch (Exception ex)
                {
                    Log.Add(LogTypes.Debug, -1, ex.ToString());
                }
            }
            dr.Close();
            dr.Dispose();

            return lt;
        }

        public XmlNode ToXml(XmlDocument d, bool includeLatestData)
        {
            XmlNode tx = d.CreateElement("forum");

            tx.AppendChild(umbraco.xmlHelper.addTextNode(d, "title", Title));
            tx.AppendChild(umbraco.xmlHelper.addCDataNode(d, "description", Description));

            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "id", Id.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "LatestTopic", _latestTopicID.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "LatestComment", _latestCommentID.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "LatestAuthor", _latestAuthorID.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "SortOrder", SortOrder.ToString()));

            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "TotalTopics", TotalTopics.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "TotalComments", TotalComments.ToString()));

            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "LatestPostDate", LatestPostDate.ToString("s")));

            //if it's not needed you can save some db queries... 
            if (includeLatestData)
            {
                //add latest data as xml
                XmlNode l = umbraco.xmlHelper.addTextNode(d, "latest", "");
                if (_latestAuthorID > 0)
                    l.AppendChild(LatestAuthor.ToXml(d, false));

                if (_latestCommentID > 0)
                    l.AppendChild(LatestComment.ToXml(d));

                if (_latestTopicID > 0 && LatestTopic != null)
                    l.AppendChild(LatestTopic.ToXml(d));

                tx.AppendChild(l);
            }

            return tx;

        }

        public static bool IsSpam(int memberId, string body, string commentType)
        {
            var akismetApiKey = ConfigurationManager.AppSettings["AkismetApiKey"];

            var akismetApi = new Akismet(akismetApiKey, "http://our.umbraco.org", "Test/1.0");
            if (akismetApi.VerifyKey() == false)
                throw new Exception("Akismet API key could not be verified");

            var member = new Member(memberId);

            var comment = new AkismetComment
                          {
                              Blog = "http://our.umbraco.org",
                              UserIp = HttpContext.Current.Request.UserHostAddress,
                              CommentAuthor = member.Text,
                              CommentAuthorEmail = member.Email,
                              CommentType = commentType,
                              CommentContent = body,
                              UserAgent = HttpContext.Current.Request.UserAgent
                          };

            return akismetApi.CommentCheck(comment) || TextContainsSpam(body);
        }

        private static bool TextContainsSpam(string text)
        {
            var spamWords = ConfigurationManager.AppSettings["uForumSpamWords"];
            return spamWords.Split(',').Any(spamWord => text.ToLowerInvariant().Contains(spamWord.Trim().ToLowerInvariant()));
        }

        /* Events */
        public static event EventHandler<CreateEventArgs> BeforeCreate;
        protected virtual void FireBeforeCreate(CreateEventArgs e)
        {
            _e.FireCancelableEvent(BeforeCreate, this, e);
        }
        public static event EventHandler<CreateEventArgs> AfterCreate;
        protected virtual void FireAfterCreate(CreateEventArgs e)
        {
            if (AfterCreate != null)
                AfterCreate(this, e);
        }

        public static event EventHandler<DeleteEventArgs> BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e)
        {
            _e.FireCancelableEvent(BeforeDelete, this, e);
        }
        public static event EventHandler<DeleteEventArgs> AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        public static event EventHandler<UpdateEventArgs> BeforeUpdate;
        protected virtual void FireBeforeUpdate(UpdateEventArgs e)
        {
            _e.FireCancelableEvent(BeforeUpdate, this, e);
        }
        public static event EventHandler<UpdateEventArgs> AfterUpdate;
        protected virtual void FireAfterUpdate(UpdateEventArgs e)
        {
            if (AfterUpdate != null)
                AfterUpdate(this, e);
        }

    }
}