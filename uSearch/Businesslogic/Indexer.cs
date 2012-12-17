using System;

using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Web;

namespace uSearch.Businesslogic
{
	/// <summary>
	/// Summary description for Indexer.
	/// </summary>
	public class Indexer
	{
        public const bool Active = true;


        private HttpContext _context { get; set; }
        private Events _e = new Events();
        public Settings IndexerSettings = new Settings();

		public Indexer()
		{
            _context = HttpContext.Current;

			//
			// TODO: Add constructor logic here
			//
		}

        public Indexer(HttpContext context)
		{
            IndexerSettings = new Settings(context);
        }


        public void AddToIndex(string uniqueKey, string contentType, Hashtable fields) {

            RemoveFromIndex(uniqueKey);

            IndexWriter writer = new IndexWriter(IndexerSettings.IndexDirectory, new StandardAnalyzer(), false);

            AddToIndex(uniqueKey, contentType, fields, writer);

            writer.Optimize();
            writer.Close();
        }

        

        public void AddToIndex(string uniqueKey, string contentType, Hashtable fields, IndexWriter writer) {

            //RemoveFromIndex(uniqueKey);
                       
            Document d = new Document();
            d.Add(new Field("uniqueKey", uniqueKey, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            d.Add(new Field("contentType", contentType, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));

            IDictionaryEnumerator id = fields.GetEnumerator();
            while (id.MoveNext()) {
                if (id.Key.ToString() != "uniqueKey" && id.Key.ToString() != "contentType")
                    d.Add(new Field(id.Key.ToString(), id.Value.ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            }

            writer.AddDocument(d);
        }

        public void AsyncReindex() {
            ThreadPool.QueueUserWorkItem(delegate { ReIndex(); });
        }

        public void ReIndex() {
            ReIndexEventArgs e = new ReIndexEventArgs();
            FireBeforeReIndex(e);

            if (!e.Cancel) {
                // Create new index
                IndexWriter w = ContentIndex(true);
                w.Close();

                //this is a totally blank reindex, so we don't index anything by default.
                //Everything is handled by event handlers. 
                //we should really get a async event on umbraco's reindexing so we could just tag along with that one 
                //instead of setting up our own UI

                FireAfterReIndex(e);
            }         
        }


        public IndexWriter ContentIndex(bool ForceRecreation) {
            if (!ForceRecreation && System.IO.Directory.Exists(IndexerSettings.IndexDirectory) &&
                new System.IO.DirectoryInfo(IndexerSettings.IndexDirectory).GetFiles().Length > 1)
                return new IndexWriter(IndexerSettings.IndexDirectory, new StandardAnalyzer(), false);
            else {
                IndexWriter iw = new IndexWriter(IndexerSettings.IndexDirectory, new StandardAnalyzer(), true);
                return iw;
            }
        }
		
		public bool RemoveFromIndex(string key) 
		{
			try 
			{
                IndexReader ir = IndexReader.Open(IndexerSettings.IndexDirectory, false);
                ir.DeleteDocuments(new Term("uniqueKey", key.ToString()));
				ir.Close();
				return true;
			} 
			catch(Exception ex) 
			{
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "[uSearch]" + ex.Message);
				return false;
			}
		}

        /* EVENTS */
        public static event EventHandler<IndexEventArgs> BeforeAddToIndex;
        protected virtual void FireBeforeAddToIndex(IndexEventArgs e) {
            _e.FireCancelableEvent(BeforeAddToIndex, this, e);
        }
        public static event EventHandler<IndexEventArgs> AfterAddToIndex;
        protected virtual void FireAfterAddToIndex(IndexEventArgs e) {
            if (AfterAddToIndex != null)
                AfterAddToIndex(this, e);
        }

        public static event EventHandler<IndexEventArgs> BeforeRemoveFromIndex;
        protected virtual void FireBeforeRemoveFromIndex(IndexEventArgs e) {
            _e.FireCancelableEvent(BeforeRemoveFromIndex, this, e);
        }
        public static event EventHandler<IndexEventArgs> AfterRemoveFromIndex;
        protected virtual void FireAfterRemoveFromIndex(IndexEventArgs e) {
            if (AfterRemoveFromIndex != null)
                AfterRemoveFromIndex(this, e);
        }


        public static event EventHandler<ReIndexEventArgs> BeforeReIndex;
        protected virtual void FireBeforeReIndex(ReIndexEventArgs e) {
            _e.FireCancelableEvent(BeforeReIndex, this, e);
        }
        public static event EventHandler<ReIndexEventArgs> AfterReIndex;
        protected virtual void FireAfterReIndex(ReIndexEventArgs e) {
            if (AfterReIndex != null)
                AfterReIndex(this, e);
        }
	}
}
