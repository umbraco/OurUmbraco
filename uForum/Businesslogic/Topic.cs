using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using System.ComponentModel;

namespace uForum.Businesslogic {
    public class Topic {

        public int Id { get; private set; }
        public int ParentId { get; private set; }
        public int MemberId { get; private set; }

        public string Title { get; set; }
        public string UrlName { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; private set; }
        public DateTime Updated { get; private set; }

        public int LatestReplyAuthor { get; private set; }
        public int LatestComment { get; private set; }

        public int Replies { get; private set; }

        public bool Locked { get; set; }
        private Events _e = new Events();

        public bool Exists {
            get {
                if (Id > 0)
                    return true;
                else
                    return false;
            }
        }

        public bool Editable(int memberId)
        {
            if (this.Exists == false || memberId == 0)
                return false;

            if (Library.Xslt.IsMemberInGroup("admin", memberId))
                return true;

            return memberId == MemberId;
        }

        public void Move(int newForumId) {
            MoveEventArgs e = new MoveEventArgs();
            FireBeforeMove(e);

            if (!e.Cancel) {
                Forum newF = new Forum(newForumId);
                Forum oldF = new Forum(ParentId);

                if (newF.Exists) {
                    ParentId = newForumId;
                    Save(true);

                    newF.Save();
                    oldF.Save();


                    FireAfterMove(e);
                }
            }
        }

        public void Delete() {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);
            if (!e.Cancel) {
                Forum f = new Forum(this.ParentId);

                Data.SqlHelper.ExecuteNonQuery("DELETE FROM forumTopics WHERE id = " + Id.ToString());
                Id = 0;


                f.Save();
                
                
                FireAfterDelete(e);
            }
        }

        public void Lock() {
            LockEventArgs e = new LockEventArgs();
            FireBeforeLock(e);

            if (!e.Cancel) {
                Data.SqlHelper.ExecuteNonQuery("UPDATE forumTopics SET locked = 1 WHERE id = " + Id.ToString());
                Id = 0;
                FireAfterLock(e);
            }  
        }

        public void Save() {
            Save(false);
        }

        public void Save(bool silent) {
            if (Id == 0) {

                if (Library.Utills.IsMember(MemberId) &&  !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Body)) {

                    
                    CreateEventArgs e = new CreateEventArgs();
                    FireBeforeCreate(e);
                    if (!e.Cancel) {

                        UrlName = umbraco.cms.helpers.url.FormatUrl(Title);

                        Data.SqlHelper.ExecuteNonQuery("INSERT INTO forumTopics (parentId, memberId, title, urlName, body, latestReplyAuthor) VALUES(@parentId, @memberId, @title, @urlname, @body, @latestReplyAuthor)",
                            Data.SqlHelper.CreateParameter("@parentId", ParentId),
                            Data.SqlHelper.CreateParameter("@memberId", MemberId),
                            Data.SqlHelper.CreateParameter("@title", Title),
                            Data.SqlHelper.CreateParameter("@urlname", UrlName),
                            Data.SqlHelper.CreateParameter("@latestReplyAuthor", LatestReplyAuthor),    
                            Data.SqlHelper.CreateParameter("@body", Body)
                            );

                        Created = DateTime.Now;
                        Updated = DateTime.Now;
                        Id = Data.SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM forumTopics WHERE memberId = @memberId",
                            Data.SqlHelper.CreateParameter("@memberId", MemberId));

                        Forum f = new Forum(ParentId);

                        if (f.Exists) {
                            f.SetLatestTopic(Id);
                            f.SetLatestAuthor(MemberId);
                            f.LatestPostDate = DateTime.Now;
                            f.Save();
                        }

                        FireAfterCreate(e);
                    }
                }

            } else {

                UpdateEventArgs e = new UpdateEventArgs();
                FireBeforeUpdate(e);

                if (!e.Cancel) {

                    int totalComments = Data.SqlHelper.ExecuteScalar<int>("SELECT count(id) from forumComments where topicId = @id", Data.SqlHelper.CreateParameter("@id", Id));
                    LatestReplyAuthor = Data.SqlHelper.ExecuteScalar<int>("SELECT TOP 1 memberId FROM forumComments WHERE (topicId= @id) ORDER BY Created DESC ", Data.SqlHelper.CreateParameter("@id", Id));
                    LatestComment = Data.SqlHelper.ExecuteScalar<int>("SELECT TOP 1 id FROM forumComments WHERE (topicId= @id) ORDER BY Created DESC ", Data.SqlHelper.CreateParameter("@id", Id));

                    UrlName = umbraco.cms.helpers.url.FormatUrl(Title);
                    
                    if(!silent)
                        Updated = DateTime.Now;

                    Data.SqlHelper.ExecuteNonQuery("UPDATE forumTopics SET replies = @replies, parentId = @parentId, memberId = @memberId, title = @title, urlname = @urlname, body = @body, updated = @updated, locked = @locked, latestReplyAuthor = @latestReplyAuthor, latestComment = @latestComment WHERE id = @id",
                        Data.SqlHelper.CreateParameter("@parentId", ParentId),
                        Data.SqlHelper.CreateParameter("@memberId", MemberId),
                        Data.SqlHelper.CreateParameter("@title", Title),
                        Data.SqlHelper.CreateParameter("@urlname", UrlName),
                        Data.SqlHelper.CreateParameter("@body", Body),
                        Data.SqlHelper.CreateParameter("@id", Id),
                        Data.SqlHelper.CreateParameter("@updated", Updated),
                        Data.SqlHelper.CreateParameter("@latestReplyAuthor", LatestReplyAuthor),
                        Data.SqlHelper.CreateParameter("@latestComment", LatestComment),
                        Data.SqlHelper.CreateParameter("@locked", Locked),
                        Data.SqlHelper.CreateParameter("@replies", totalComments)
                        );

                    UpdateCommentsPosition();

                    FireAfterUpdate(e);
                }
            }
        }

        private void UpdateCommentsPosition() {
            string sql = @"SELECT id, position, created,
                          ROW_NUMBER() OVER (ORDER BY created) AS RowNumber
                          FROM forumComments where topicId = @Id";


            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader(sql, Data.SqlHelper.CreateParameter("@id", Id));

            while (dr.Read()) {
                int _cid = dr.GetInt("id");
                long _rn = dr.GetLong("RowNumber");
                
                Data.SqlHelper.ExecuteNonQuery("UPDATE forumComments SET position = @position WHERE id = @id",
                        Data.SqlHelper.CreateParameter("@id", _cid),
                        Data.SqlHelper.CreateParameter("@position", _rn));
            }

        }

        public List<Comment> Comments() {
            List<Comment> lc = new List<Comment>();

            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader(
                "SELECT * FROM forumComments WHERE topicId = " + Id.ToString()
            );

            try {
                //Sql effiecient way of fetching collection of comments instead of one by one.. 
                while (dr.Read()) {
                    Comment c = new Comment();
                    c.Id = dr.GetInt("id");
                    c.TopicId = dr.GetInt("topicId");
                    c.MemberId = dr.GetInt("memberId");

                    c.Body = dr.GetString("body");

                    c.Created = dr.GetDateTime("created");

                    lc.Add(c);
                }
            } catch (Exception ex) {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
            }

            dr.Close();
            dr.Dispose();

            return lc;
        }

        public XmlNode ToXml(XmlDocument d) {
            XmlNode tx = d.CreateElement("topic");
            
            tx.AppendChild(umbraco.xmlHelper.addTextNode(d, "title", Title));
            tx.AppendChild(umbraco.xmlHelper.addCDataNode(d, "body", Body));

            tx.AppendChild(umbraco.xmlHelper.addTextNode(d, "urlname", UrlName));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "id", Id.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "parentId", ParentId.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "memberId", MemberId.ToString()));

            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "latestReplyAuthor", LatestReplyAuthor.ToString()));
            
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "created", Created.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "updated", Updated.ToString()));

            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "locked", Locked.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "replies", Replies.ToString()));
            return tx;
        }

        public static Topic Create(int forumId, string title, string body, int memberId) {
            Topic t = new Topic();
            t.ParentId = forumId;
            t.Title = title;
            t.Body = body;
            t.MemberId = memberId;
            t.LatestReplyAuthor = memberId;
            t.Replies = 0;
            t.Save();
            
            return t;
        }

        public Topic() { }
        
        public Topic(int topicId) {
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader( "SELECT * FROM forumTopics WHERE id = " + topicId.ToString()  );

            if (dr.Read()) {

                Id = dr.GetInt("id");
                ParentId = dr.GetInt("parentId");
                MemberId = dr.GetInt("memberId");
                Replies = dr.GetInt("replies");
                Title = dr.GetString("title");
                Body = dr.GetString("body");
                LatestReplyAuthor = dr.GetInt("latestReplyAuthor");
                Created = dr.GetDateTime("created");
                Updated = dr.GetDateTime("updated");

                UrlName = dr.GetString("urlName");

                Locked = dr.GetBoolean("locked");
            }

            dr.Close();
            dr.Dispose();
        }

        public static Topic GetFromReader(umbraco.DataLayer.IRecordsReader dr) {

            Topic t = new Topic();
            t.Id = dr.GetInt("id");
            t.ParentId = dr.GetInt("parentId");
            t.MemberId = dr.GetInt("memberId");
            t.Replies = dr.GetInt("replies");
            t.Title = dr.GetString("title");
            t.Body = dr.GetString("body");
            t.LatestReplyAuthor = dr.GetInt("latestReplyAuthor");
            t.Created = dr.GetDateTime("created");
            t.Updated = dr.GetDateTime("updated");

            t.UrlName = dr.GetString("urlName");

            t.Locked = dr.GetBoolean("locked");

            return t;
        }

        //Collections
        public static List<Topic> TopicsInForum(int forumId, int topicsPerPage, int page) {
            List<Topic> lt = new List<Topic>();
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader(
                "SELECT TOP " + topicsPerPage.ToString() + " * FROM forumTopics WHERE parentId = " + forumId.ToString() + " ORDER BY updated DESC"
            );

            while (dr.Read() ) {
                lt.Add( GetFromReader(dr) );
            }

            dr.Close();
            dr.Dispose();

            return lt;
        }

        //Collections
        public static List<Topic> TopicsInForum(int forumId) {
            List<Topic> lt = new List<Topic>();
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader(
                "SELECT * FROM forumTopics WHERE parentId = " + forumId.ToString() + " ORDER BY updated DESC"
            );

            while (dr.Read()) {
                lt.Add(GetFromReader(dr));
            }

            dr.Close();
            dr.Dispose();

            return lt;
        }

        //Collections
        public static List<Topic> Latest(int amount) {
            List<Topic> lt = new List<Topic>();

            // 1057 is the profile node.  This is a hack way of hiding the forums from the latest list on the homepage added by PG

            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader(
                "SELECT TOP " + amount.ToString() + " forumTopics.* FROM forumTopics INNER JOIN ForumForums on forumTopics.ParentId = ForumForums.Id Where forumforums.parentId != 1057 ORDER BY updated DESC"
            );

            while (dr.Read()) {
                lt.Add(GetFromReader(dr));
            }

            dr.Close();
            dr.Dispose();

            return lt;
        }

        public static List<Topic> GetAll()
        {
            List<Topic> lt = new List<Topic>();
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader(
                "SELECT * FROM forumTopics"
            );

            while (dr.Read())
            {
                lt.Add(GetFromReader(dr));
            }

            dr.Close();
            dr.Dispose();

            return lt;
        }

        public static int TotalTopics()
        {
            return Data.SqlHelper.ExecuteScalar<int>("SELECT SUM(totalTopics) FROM [forumForums]");
        }

        /* Events */
        public static event EventHandler<CreateEventArgs> BeforeCreate;
        protected virtual void FireBeforeCreate(CreateEventArgs e) {
            _e.FireCancelableEvent(BeforeCreate, this, e);
        }
        public static event EventHandler<CreateEventArgs> AfterCreate;
        protected virtual void FireAfterCreate(CreateEventArgs e) {
            if (AfterCreate != null)
                AfterCreate(this, e);
        }


        public static event EventHandler<DeleteEventArgs> BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e) {
            _e.FireCancelableEvent(BeforeDelete, this, e);
        }
        public static event EventHandler<DeleteEventArgs> AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        public static event EventHandler<MoveEventArgs> BeforeMove;
        protected virtual void FireBeforeMove(MoveEventArgs e) {
            _e.FireCancelableEvent(BeforeMove, this, e);
        }
        public static event EventHandler<MoveEventArgs> AfterMove;
        protected virtual void FireAfterMove(MoveEventArgs e) {
            if (AfterMove != null)
                AfterMove(this, e);
        }

        public static event EventHandler<LockEventArgs> BeforeLock;
        protected virtual void FireBeforeLock(LockEventArgs e) {
            _e.FireCancelableEvent(BeforeLock, this, e);
        }
        public static event EventHandler<LockEventArgs> AfterLock;
        protected virtual void FireAfterLock(LockEventArgs e) {
            if (AfterLock != null)
                AfterLock(this, e);
        }


        public static event EventHandler<UpdateEventArgs> BeforeUpdate;
        protected virtual void FireBeforeUpdate(UpdateEventArgs e) {
            _e.FireCancelableEvent(BeforeUpdate, this, e);
        }
        public static event EventHandler<UpdateEventArgs> AfterUpdate;
        protected virtual void FireAfterUpdate(UpdateEventArgs e) {
            if (AfterUpdate != null)
                AfterUpdate(this, e);
        }
    }
}
