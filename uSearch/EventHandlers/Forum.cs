using System;
using System.Collections;
using System.Linq;
using System.Web;
using uForum.Businesslogic;
using Lucene.Net.Index;
using System.Collections.Generic;

namespace uSearch.EventHandlers {
    public class ForumHandler : umbraco.BusinessLogic.ApplicationBase {

        public ForumHandler()
        {

            if (Businesslogic.Indexer.Active)
            {
                
                /*
                 * no longer needed due to use of examine
                uForum.Businesslogic.Topic.AfterCreate += new EventHandler<uForum.Businesslogic.CreateEventArgs>(Topic_AfterCreate);
                uForum.Businesslogic.Comment.AfterCreate += new EventHandler<CreateEventArgs>(Comment_AfterCreate);

                uForum.Businesslogic.Comment.AfterDelete += new EventHandler<DeleteEventArgs>(Comment_AfterDelete);
                uForum.Businesslogic.Topic.AfterDelete += new EventHandler<DeleteEventArgs>(Topic_AfterDelete);

                uSearch.Businesslogic.Indexer.AfterReIndex += new EventHandler<uSearch.Businesslogic.ReIndexEventArgs>(Indexer_AfterReIndex);
                */
                //            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "sup?");
            }
            }


        public void Indexer_AfterReIndex(object sender, uSearch.Businesslogic.ReIndexEventArgs e) {

            Businesslogic.Indexer i = (uSearch.Businesslogic.Indexer)sender;
            IndexWriter iw = i.ContentIndex(false);

            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Re-indexing forum posts");

            try {

                List<Forum> forums = uForum.Businesslogic.Forum.Forums();
                foreach (Forum f in forums) {

                    try{
                    foreach (Topic t in Topic.TopicsInForum(f.Id)) {
                        
                        Hashtable fields = new Hashtable();

                        fields.Add("id", t.Id.ToString());
                        fields.Add("name", t.Title);
                        fields.Add("author", t.MemberId.ToString());
                        fields.Add("content", umbraco.library.StripHtml(t.Body));

                        i.AddToIndex("topic_" + t.Id.ToString(), "forumTopics", fields, iw);

                    }
                    }catch (Exception ex) {
                        umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
                    }
                }

            } catch (Exception ex) {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
                
            }

            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Re-indexing forum posts - DONE");

            iw.Optimize();
            iw.Close();
        }


        void Topic_AfterDelete(object sender, DeleteEventArgs e) {
            Topic t = (Topic)sender;
            Businesslogic.Indexer i = new uSearch.Businesslogic.Indexer();
            i.RemoveFromIndex("topic_" + t.Id.ToString());
        }

        void Comment_AfterDelete(object sender, DeleteEventArgs e) {
            Comment c = (Comment)sender;
            Businesslogic.Indexer i = new uSearch.Businesslogic.Indexer();
            i.RemoveFromIndex("comment_" + c.Id.ToString());
        }

        void Comment_AfterCreate(object sender, CreateEventArgs e) {
            Comment c = (Comment)sender;

            Hashtable fields = new Hashtable();

            fields.Add("id", c.Id.ToString());
            fields.Add("author", c.MemberId.ToString());
            fields.Add("content", umbraco.library.StripHtml(c.Body));

            Businesslogic.Indexer i = new uSearch.Businesslogic.Indexer();
            i.AddToIndex("comment_" + c.Id.ToString(), "forumComments", fields);
        }

        void Topic_AfterCreate(object sender, uForum.Businesslogic.CreateEventArgs e) {
            Topic t = (Topic)sender;

            Hashtable fields = new Hashtable();

            fields.Add("id", t.Id.ToString());
            fields.Add("name", t.Title);
            fields.Add("author", t.MemberId.ToString());
            fields.Add("content", umbraco.library.StripHtml(t.Body));
            
            Businesslogic.Indexer i = new uSearch.Businesslogic.Indexer();
            i.AddToIndex("topic_" + t.Id.ToString(), "forumTopics", fields);

            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "topic " + t.Id.ToString() + " added");
        }
    }
}
