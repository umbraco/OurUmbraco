using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.web;

namespace uWiki.Businesslogic {
    public class WikiPage {

        public int ParentId { get; set; }
        public int NodeId { get; set; }
        public string Body { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public int Author { get; set; }
        public Guid Version { get; set; }
        public bool Exists { get; set; }
        public bool Locked { get; set; }


        public umbraco.cms.businesslogic.web.Document Node { get; set; }

        private Events _e = new Events();

        public WikiPage() {
            Exists = false;
        }

        public static WikiPage Create(int parentId, int authorId, string body, string title, string keywords) {
            WikiPage wp = new WikiPage();
            wp.Exists = false;
            wp.ParentId = parentId;
            wp.Author = authorId;
            wp.Body = body;
            wp.Title = title;
            wp.Keywords = keywords;
            wp.Save();

            return wp;
        }

        public WikiPage(int id) {
            umbraco.cms.businesslogic.web.Document doc = new umbraco.cms.businesslogic.web.Document(id);

            if (doc != null) {
                

                if (doc.ContentType.Alias == "WikiPage") {
                    Exists = true;
                    Body = doc.getProperty("bodyText").Value.ToString() ;
                    Locked = (doc.getProperty("umbracoNoEdit").Value.ToString() == "1");

                    if(doc.getProperty("keywords") != null)
                        Keywords = doc.getProperty("keywords").Value.ToString();
                    
                    Title = doc.Text;
                    Author = (int)doc.getProperty("author").Value;
                    Version = doc.Version;
                    Node = doc;
                    NodeId = doc.Id;
                    ParentId = Node.Parent.Id;
                }
            }
            
        }

        public void Save() {
            if (NodeId == 0) {

                if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Body)) {
                    CreateEventArgs e = new CreateEventArgs();
                    FireBeforeCreate(e);
                    if (!e.Cancel) {

                        Document childDoc = Document.MakeNew(Title, DocumentType.GetByAlias("WikiPage"), new umbraco.BusinessLogic.User(0), ParentId);
                        childDoc.getProperty("author").Value = Author;
                        childDoc.getProperty("bodyText").Value = Body;
                        childDoc.getProperty("keywords").Value = Keywords;
                        childDoc.Save();
                        childDoc.Publish(new umbraco.BusinessLogic.User(0));

                        umbraco.library.UpdateDocumentCache(childDoc.Id);

                        Node = childDoc;
                        NodeId = childDoc.Id;
                        Version = childDoc.Version;
                        Exists = true;

                        FireAfterCreate(e);
                    }
                }

            } else {

                UpdateEventArgs e = new UpdateEventArgs();
                FireBeforeUpdate(e);

                if (!e.Cancel) {

                    if (Node == null)
                        Node = new Document(NodeId);

                    Node.Text = Title;
                    Node.getProperty("author").Value = Author;
                    Node.getProperty("bodyText").Value = Body;
                    Node.getProperty("keywords").Value = Keywords;
                    Node.Save();
                    Node.Publish(new umbraco.BusinessLogic.User(0));

                    umbraco.library.UpdateDocumentCache(Node.Id);

                    FireAfterUpdate(e);
                }
            }
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
