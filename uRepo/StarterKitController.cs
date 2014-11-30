using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Xml;
using System.Xml.XPath;

namespace uRepo
{
    public class StarterKitController : ApiController
    {
        public List<StarterKit> Get(string umbracoVersion = null)
        {
            var projects = new Packages();
            var starterKitsIterator = projects.GetProjects();
            var umbracoNodes = GetUmbracoNodes(starterKitsIterator);
            var starterKits = GetStarterKits(umbracoNodes, umbracoVersion);
            return starterKits.ToList();
        }

        private IEnumerable<umbraco.NodeFactory.Node> GetUmbracoNodes(XPathNodeIterator starterKitsIterator)
        {
            var nodes = new List<umbraco.NodeFactory.Node>();

            while (starterKitsIterator.MoveNext())
            {
                var current = starterKitsIterator.Current as IHasXmlNode;
                if (current == null) continue;

                var node = current.GetNode();
                var umbracoNode = new umbraco.NodeFactory.Node(node);

                if (node != null)
                    nodes.Add(umbracoNode);
            }

            return nodes;
        }


        private IEnumerable<StarterKit> GetStarterKits(IEnumerable<umbraco.NodeFactory.Node> umbracoNodes, string umbracoVersion)
        {
            var starterKits = new List<StarterKit>();

            var versionName = string.Empty;
            if (umbracoVersion != null)
            {
                if (umbracoVersion.Contains("-"))
                    umbracoVersion = umbracoVersion.Substring(0, umbracoVersion.IndexOf("-", StringComparison.Ordinal));

                versionName = string.Format("v{0}", umbracoVersion.Replace(".", string.Empty));
            }

            var officialStarterKitGuidCollection =
                ConfigurationManager.AppSettings["UmbracoStarterKits"].Split(',').ToList();
            
            foreach (var umbracoNode in umbracoNodes)
            {
                // If it's not in the official list, move on to the next package
                if (officialStarterKitGuidCollection.Contains(GetPropertyValue(umbracoNode, "packageGuid"), 
                    StringComparer.OrdinalIgnoreCase) == false)
                    continue;
                
                // If the umbracoVersion is filled in then check if the kit is compatible the version requested
                if (umbracoVersion != null)
                {
                    var versionCompatible = false;
                    var compatibleVersions = GetPropertyValue(umbracoNode, "compatibleVersions");

                    foreach (var compatibleVersion in compatibleVersions.Split(','))
                    {
                        if (compatibleVersion == versionName)
                            versionCompatible = true;
                    }

                    if(versionCompatible == false) 
                        continue;
                }

                var starterKit = new StarterKit
                {
                    Name = umbracoNode.Name,
                    Thumbnail = GetPropertyValue(umbracoNode, "defaultScreenshotPath"),
                    Id = umbracoNode.Id,
                    SortOrder = umbracoNode.SortOrder
                };

                starterKits.Add(starterKit);
            }

            return starterKits.OrderBy(s => s.SortOrder);
        }

        private string GetPropertyValue(umbraco.NodeFactory.Node umbracoNode, string propertyAlias)
        {
            if (umbracoNode.GetProperty(propertyAlias) == null && string.IsNullOrWhiteSpace(umbracoNode.GetProperty(propertyAlias).Value))
                return string.Empty;

            return umbracoNode.GetProperty(propertyAlias).Value;
        }
    }
}
