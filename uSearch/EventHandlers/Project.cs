using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using System.Xml;
using umbraco.cms.businesslogic.web;
using System.Collections;

namespace uSearch.EventHandlers {
    public class ProjectHandler : umbraco.BusinessLogic.ApplicationBase {

        public ProjectHandler() {
            if (Businesslogic.Indexer.Active)
            {
                //this uses the standard umbraco event handlers...
                //no longer needed as we are using examine
                /*
                umbraco.cms.businesslogic.web.Document.AfterSave += new umbraco.cms.businesslogic.web.Document.SaveEventHandler(Document_AfterSave);
                Document.AfterDelete += new Document.DeleteEventHandler(Document_AfterDelete);

                Businesslogic.Indexer.AfterReIndex += new EventHandler<uSearch.Businesslogic.ReIndexEventArgs>(Indexer_AfterReIndex);
            */
                }
        }

        public void Indexer_AfterReIndex(object sender, uSearch.Businesslogic.ReIndexEventArgs e) {


            Businesslogic.Indexer i = (Businesslogic.Indexer)sender;
            Lucene.Net.Index.IndexWriter iw = i.ContentIndex(false);


            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Re-indexing projects");
            try {
                int projectRoot = 1113;
                XPathNodeIterator pages = umbraco.library.GetXmlNodeById(projectRoot.ToString()).Current.Select(".//node [@nodeTypeAlias = 'Project']");

                //umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, pages.Current.OuterXml);

                while (pages.MoveNext()) {
                    if (pages.Current is IHasXmlNode) {
                        XmlNode node = ((IHasXmlNode)pages.Current).GetNode();

                        string key = "project_" + node.Attributes["id"].Value;
                                                
                        Hashtable fields = new Hashtable();

                        fields.Add("id", node.Attributes["id"].Value);
                        fields.Add("name", node.Attributes["nodeName"].Value);

                        fields.Add("content", umbraco.library.StripHtml(umbraco.xmlHelper.GetNodeValue(node.SelectSingleNode("data [@alias = 'description']"))));
                        fields.Add("path", (node.Attributes["path"].Value.Replace("-1,", "").Replace(",", i.IndexerSettings.PathSplit)));


                        i.AddToIndex(key, "project", fields, iw);
                    }
                }

            } catch (Exception ex) {
                //umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
            }

            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Re-indexing projects - DONE");
            iw.Optimize();
            iw.Close();
            //umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Wiki index done");
        }


        void Document_AfterDelete(Document sender, umbraco.cms.businesslogic.DeleteEventArgs e) {
            new uSearch.Businesslogic.Indexer().RemoveFromIndex("project_" + sender.Id.ToString());
        }
        

        void Document_AfterSave(umbraco.cms.businesslogic.web.Document sender, umbraco.cms.businesslogic.SaveEventArgs e) {

            if(sender.ContentType.Alias == "Project"){
                
                Hashtable fields = new Hashtable();

                fields.Add("id", sender.Id);
                fields.Add("name", sender.Text);
                fields.Add("content", umbraco.library.StripHtml( sender.getProperty("description").Value.ToString() ));
                fields.Add("path", (sender.Path.Replace("-1,", "").Replace(",", new Businesslogic.Settings().PathSplit) ) );

                Businesslogic.Indexer i = new uSearch.Businesslogic.Indexer();
                i.AddToIndex("project_" + sender.Id.ToString(), "project", fields);     
            }
        }

    }
}
