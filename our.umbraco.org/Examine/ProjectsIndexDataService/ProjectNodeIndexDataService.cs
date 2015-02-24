using Examine;
using Examine.LuceneEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using our.Examine.DocumentationIndexDataService.Helper;

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

            return simpleDataSet;
        }

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var dataSets = new List<SimpleDataSet>();
            var i = 1; //unique id for each doc

            var projects = Umbraco.Web.UmbracoContext.Current.ContentCache.GetByXPath("//Community/Projects//Project [approved='1']");
           
            //index all projects
            foreach (var project in projects)
            {
                try
                {
                    var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
                    simpleDataSet = MapProjectToSimpleDataIndexItem(project, simpleDataSet, project.Id, indexType);
                    dataSets.Add(simpleDataSet);
                }
                catch (Exception ex)
                {
                   
                }

                i++;
            }

            return dataSets;
        }
    }
}
