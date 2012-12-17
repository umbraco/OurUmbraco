using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.web;
using System.Xml.XPath;
using umbraco.presentation.nodeFactory;
using System.Xml;
using uSearch.Businesslogic;

namespace uSearch.EventHandlers {
    public class WikiHandler : umbraco.BusinessLogic.ApplicationBase {
        public WikiHandler() {

            if (Businesslogic.Indexer.Active)
            {
                //this uses the standard umbraco event handlers...
                /* no longer needed because we are using examine.
                umbraco.cms.businesslogic.web.Document.AfterSave += new umbraco.cms.businesslogic.web.Document.SaveEventHandler(Document_AfterSave);

                Document.AfterDelete += new Document.DeleteEventHandler(Document_AfterDelete);
                Document.AfterMoveToTrash += new Document.MoveToTrashEventHandler(Document_AfterMoveToTrash);

                Businesslogic.Indexer.AfterReIndex += new EventHandler<uSearch.Businesslogic.ReIndexEventArgs>(Indexer_AfterReIndex);
                 * */
            }
        }

        void Document_AfterMoveToTrash(Document sender, umbraco.cms.businesslogic.MoveToTrashEventArgs e)
        {
            new uSearch.Businesslogic.Indexer().RemoveFromIndex("wiki_" + sender.Id.ToString());
        }

        public void Indexer_AfterReIndex(object sender, uSearch.Businesslogic.ReIndexEventArgs e) {

            Businesslogic.Indexer i = (Businesslogic.Indexer)sender;
            Lucene.Net.Index.IndexWriter iw = i.ContentIndex(false);

            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Re-indexing wiki pages");

            try {
                int wikiRoot = 1054;

                XPathNodeIterator pages = umbraco.library.GetXmlNodeById( wikiRoot.ToString() ).Current.Select(".//node [@nodeTypeAlias = 'WikiPage']");

                while (pages.MoveNext()) {
                    if (pages.Current is IHasXmlNode) {
                        XmlNode node = ((IHasXmlNode)pages.Current).GetNode();

                        string key = "wiki_" + node.Attributes["id"].Value;

                        Hashtable fields = new Hashtable();

                        fields.Add("id", node.Attributes["id"].Value);
                        fields.Add("name", node.Attributes["nodeName"].Value);

                        fields.Add("content", umbraco.library.StripHtml(umbraco.xmlHelper.GetNodeValue(node.SelectSingleNode("data [@alias = 'bodyText']"))));
                        fields.Add("path", (node.Attributes["path"].Value.Replace("-1,", "").Replace(",", i.IndexerSettings.PathSplit)));


                        i.AddToIndex(key, "wiki", fields);
                    }
                }

            } catch (Exception ex) {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
            }

            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Re-indexing wiki pages - DONE");
        }


        void Document_AfterDelete(Document sender, umbraco.cms.businesslogic.DeleteEventArgs e) {
            new uSearch.Businesslogic.Indexer().RemoveFromIndex("wiki_" + sender.Id.ToString());
        }


        void Document_AfterSave(umbraco.cms.businesslogic.web.Document sender, umbraco.cms.businesslogic.SaveEventArgs e) {

            if(sender.ContentType.Alias == "WikiPage"){
                
                Hashtable fields = new Hashtable();

                fields.Add("id", sender.Id);
                fields.Add("name", sender.Text);
                fields.Add("content", umbraco.library.StripHtml( sender.getProperty("bodyText").Value.ToString() ));
                fields.Add("path", (sender.Path.Replace("-1,", "").Replace(",", new Businesslogic.Settings().PathSplit) ) );

                Businesslogic.Indexer i = new uSearch.Businesslogic.Indexer();
                i.AddToIndex("wiki_" + sender.Id.ToString(), "wiki", fields);     
            }
        }

     }
}
