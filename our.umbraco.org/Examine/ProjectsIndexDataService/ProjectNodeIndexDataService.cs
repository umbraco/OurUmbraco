using Examine;
using Examine.LuceneEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using our.Examine.DocumentationIndexDataService.Helper;
using Umbraco.Core.Logging;

namespace our.ExamineServices
{
    public class ProjectNodeIndexDataService : ISimpleDataService
    {
        public SimpleDataSet MapProjectToSimpleDataIndexItem(IPublishedContent project, SimpleDataSet simpleDataSet, int index, string indexType)
        {
            simpleDataSet.NodeDefinition.NodeId = project.Id;
            simpleDataSet.NodeDefinition.Type = indexType;

            simpleDataSet.RowData.Add("body", umbraco.library.StripHtml( project.GetProperty("description").Value.ToString() )) ;
            simpleDataSet.RowData.Add("nodeName", project.Name);
            simpleDataSet.RowData.Add("updateDate", project.UpdateDate.SerializeForLucene());
            simpleDataSet.RowData.Add("nodeTypeAlias", "project");
            simpleDataSet.RowData.Add("url", project.Url );


            var karma = our.Utills.GetProjectTotalKarma(project.Id);
            var files = uWiki.Businesslogic.WikiFile.CurrentFiles(project.Id);
            var downloads = our.Utills.GetProjectTotalDownloadCount(project.Id);

            var image = files.Where(x => x.FileType == "screenshot").FirstOrDefault();
            var imageFile = "";
            if (image != null)
                imageFile = image.Path;

            //Clean up version data before its included in the index
            int o;
            var version = project.GetProperty("compatibleVersions").Value;
            var versions = version.ToString().ToLower()
                            .Replace("nan", "")
                            .Replace("saved", "")
                            .Replace("v", "")
                            .Trim(',').Split(',')
                            .Where(x => int.TryParse(x, out o))
                            .Select(x => (decimal.Parse(x.PadRight(3, '0') ) / 100));

            simpleDataSet.RowData.Add("karma", karma.ToString());
            simpleDataSet.RowData.Add("downloads", downloads.ToString());
            simpleDataSet.RowData.Add("image", imageFile);
            simpleDataSet.RowData.Add("versions", string.Join(",", versions));

            return simpleDataSet;
        }

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var dataSets = new List<SimpleDataSet>();
            var projects = Umbraco.Web.UmbracoContext.Current.ContentCache.GetByXPath("//Community/Projects//Project [projectLive='1']");
           
            //index all projects
            for (int i = 0; i < projects.Count(); i++)
            {
                var project = projects.ElementAt(i);
                try
                {
                    LogHelper.Debug(this.GetType(), "Indexing " + project.Name);

                    var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
                    simpleDataSet = MapProjectToSimpleDataIndexItem(project, simpleDataSet, project.Id, indexType);
                    dataSets.Add(simpleDataSet);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(this.GetType(), ex.Message, ex);
                }
            }

            return dataSets;
        }
    }
}
