using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using Lucene.Net.Index;
using System.Collections;
using uSearch.Businesslogic;
using Lucene.Net.Analysis.Standard;

namespace uSearch.EventHandlers
{
    public class Video : umbraco.BusinessLogic.ApplicationBase
    {
        public Video() {
            //uSearch.Businesslogic.Indexer.AfterReIndex += new EventHandler<Businesslogic.ReIndexEventArgs>(Indexer_AfterReIndex);
        }


        public void Indexer_AfterReIndex(object sender, Businesslogic.ReIndexEventArgs e)
        {

            Businesslogic.Indexer i = (Businesslogic.Indexer)sender;
            Lucene.Net.Index.IndexWriter iw = i.ContentIndex(false);

            try
            {

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Re-indexing Video");

                string url = "http://umbraco.org/FullVideoXmlFeed.aspx";
                XPathNodeIterator xni = umbraco.library.GetXmlDocumentByUrl(url, 3600).Current.Select("//item");
                
                while (xni.MoveNext())
                {   
                    string content = umbraco.library.StripHtml( xni.Current.SelectSingleNode("./content").Value);
                    string name = xni.Current.SelectSingleNode("./title").Value;
                    string image = xni.Current.SelectSingleNode("./image").Value;
                    string id =  xni.Current.SelectSingleNode("./id").Value;
                    string link =  xni.Current.SelectSingleNode("./link").Value;

                    Hashtable fields = new Hashtable();

                    fields.Add("name", name);
                    fields.Add("content", content);
                    fields.Add("image", image );
                    fields.Add("id", id);
                    fields.Add("url", link);
 
                    i.AddToIndex("video_" + id.ToString(), "videos", fields, iw);

                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "adding " + name + " video");
                }
            } catch (Exception ex) {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
            }

            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "Re-indexing videos - DONE");


            iw.Optimize();
            iw.Close();
            
        }

    }
}