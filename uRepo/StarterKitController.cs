using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Xml;
using System.Xml.XPath;
using umbraco.NodeFactory;
using uVersion;

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
            Version version = null;
            if (umbracoVersion != null)
            {
                if (umbracoVersion.Contains("-"))
                    umbracoVersion = umbracoVersion.Substring(0, umbracoVersion.IndexOf("-", StringComparison.Ordinal));

                version = new Version(umbracoVersion);
            }

            var officialStarterKitGuidCollection =
                ConfigurationManager.AppSettings["UmbracoStarterKits"].Split(',').ToList();
            var starterKits = new List<StarterKit>();
            var allConfiguredVersions = UWikiFileVersion.GetAllVersions();

            foreach (var umbracoNode in umbracoNodes)
            {
                // If it's not in the official list, move on to the next package
                if (officialStarterKitGuidCollection.Contains(GetPropertyValue(umbracoNode, "packageGuid"),
                    StringComparer.OrdinalIgnoreCase) == false)
                    continue;

                // If the umbracoVersion is filled in then check if the kit is compatible the version requested
                if (umbracoVersion != null && VersionCompatible(umbracoNode, version, allConfiguredVersions) == false)
                    continue;

                var starterKit = new StarterKit
                {
                    Name = umbracoNode.Name,
                    Thumbnail = GetPropertyValue(umbracoNode, "defaultScreenshotPath"),
                    Id = GetPropertyValue(umbracoNode, "packageGuid"),
                    Description = GetPropertyValue(umbracoNode, "description"),
                    SortOrder = umbracoNode.SortOrder
                };

                starterKits.Add(starterKit);
            }

            return starterKits.OrderBy(s => s.SortOrder);
        }

        private bool VersionCompatible(Node umbracoNode, Version version, List<UWikiFileVersion> allConfiguredVersions)
        {
            var versionCompatible = false;

            var compatibleVersions =
                GetAllCompatibleVersions(GetPropertyValue(umbracoNode, "compatibleVersions"), allConfiguredVersions)
                    .ToList();

            // If there's no versions in the list, it's compatible with everything
            if (compatibleVersions.Any() == false)
            {
                versionCompatible = true;
            }
            else
            {
                foreach (var compatibleVersion in compatibleVersions)
                {
                    if (version >= compatibleVersion)
                        versionCompatible = true;
                }
            }
            
            return versionCompatible;
        }

        private static IEnumerable<Version> GetAllCompatibleVersions(string compatibleVersions, List<UWikiFileVersion> configuredVersions)
        {
            var compatibleVersionsList = new List<Version>();

            foreach (var compatibleVersion in compatibleVersions.Split(','))
            {
                var compVersion = compatibleVersion;
                var configuredVersion = configuredVersions.FirstOrDefault(x => x.Key == compVersion);

                if (configuredVersion == null)
                    continue;

                if (configuredVersion.Key != "nan")
                {
                    var voteDescription = configuredVersion.VoteDescription;
                    voteDescription = voteDescription.Replace("Version ", string.Empty).Replace(".x", ".0");
                    compatibleVersionsList.Add(new Version(voteDescription));
                }
            }

            return compatibleVersionsList;
        }

        private string GetPropertyValue(umbraco.NodeFactory.Node umbracoNode, string propertyAlias)
        {
            if (umbracoNode.GetProperty(propertyAlias) == null && string.IsNullOrWhiteSpace(umbracoNode.GetProperty(propertyAlias).Value))
                return string.Empty;

            return umbracoNode.GetProperty(propertyAlias).Value;
        }
    }
}
