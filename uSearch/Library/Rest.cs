using System;
using System.Linq;
using Examine;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using UmbracoExamine;

namespace uSearch.Library
{
    public class ProjectExamineIndexer : ApplicationBase
    {
        public ProjectExamineIndexer()
        {
            var indexer = ExamineManager.Instance.IndexProviderCollection["ProjectIndexer"];

            // intercept when a project is indexed to add downloads/karma stats
            indexer.GatheringNodeData += indexer_GatheringNodeData;

            uPowers.BusinessLogic.Action.AfterPerform += Action_AfterPerform;
        }

        void Action_AfterPerform(object sender, uPowers.BusinessLogic.ActionEventArgs e)
        {
            if (e.ActionType != "project") 
                return;

            Log.Add(LogTypes.Debug, int.Parse(e.ItemId.ToString()), "Karma indexing starts");
            try
            {
                ExamineManager.Instance.IndexProviderCollection["ProjectIndexer"].ReIndexNode(new Document(e.ItemId).ToXDocument(true).Root, IndexTypes.Content);
            }
            catch (Exception ee)
            {
                Log.Add(LogTypes.Debug, int.Parse(e.ItemId.ToString()), "Karma indexing failed " + ee.ToString());
            }
        }

        void indexer_GatheringNodeData(object sender, IndexingNodeDataEventArgs e) { }
    }

    public static class ExamineHelpers
    {
        public static string BuildExamineString(this string term, int boost, string field)
        {
            var qs = string.Format("{0}:", field);
            qs += string.Format("\"{0}\"^{1} ", term, (boost + 30000));
            qs += string.Format("{0}:(+{1})^{2} ", field, term.Replace(" ", " +"), (boost + 5));
            qs += string.Format("{0}:({1})^{2} ", field, term, boost);

            return qs;
        }
    }
}
