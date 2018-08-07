using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Umbraco.Core;

namespace OurUmbraco.Project.uVersion
{
    public class UVersion
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Exists { get; set; }

        public UVersion(string name)
        {
            var xmlNode = UVersionConfig.GetKeyAsNode("/configuration/versions/version [@voteName = '" + name + "']");
            if (xmlNode?.Attributes != null)
            {
                Name = name;
                Key = xmlNode.Attributes.GetNamedItem("key").Value;
                Description = xmlNode.Attributes.GetNamedItem("voteDescription").Value;
                Exists = true;
            }
            else
            {
                Exists = false;
            }
        }

        public static List<UVersion> GetAllVersions()
        {
            var xmlNode = UVersionConfig.GetKeyAsNode("/configuration/versions");
            var allVersions = new List<UVersion>();
            foreach (XmlNode cx in xmlNode.ChildNodes)
            {
                if (cx.Attributes?.GetNamedItem("vote") != null && cx.Attributes.GetNamedItem("vote").Value == "true")
                    if (cx.Attributes.GetNamedItem("voteName") != null)
                        allVersions.Add(new UVersion(cx.Attributes.GetNamedItem("voteName").Value));
            }

            return allVersions;
        }

        public static IEnumerable<System.Version> GetAllAsVersions()
        {
            var all = UVersion.GetAllVersions()
                .Select(x => x.Name.Replace(".x", ""))
                .Select(x =>
                {
                    return System.Version.TryParse(x, out var version) ? version : null;
                })
                .WhereNotNull()
                .OrderByDescending(x => x);
            return all;
        }
    }
}