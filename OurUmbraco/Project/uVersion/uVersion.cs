using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Umbraco.Core;

namespace OurUmbraco.Project.uVersion
{
    public class UVersion
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Exists { get; set; }


        public UVersion(string name)
        {
            XmlNode x = UVersionConfig.GetKeyAsNode("/configuration/versions/version [@voteName = '" + name + "']");
            if (x != null) {
                Name = x.Attributes.GetNamedItem("voteName").Value;
                Description = x.Attributes.GetNamedItem("voteDescription").Value;
                

                
                Exists = true;
            } else
                Exists = false;
        }

        public static List<UVersion> GetAllVersions()
        {
            XmlNode x = UVersionConfig.GetKeyAsNode("/configuration/versions");
            var l = new List<UVersion>();
            foreach (XmlNode cx in x.ChildNodes)
            {
                if (cx.Attributes != null && cx.Attributes.GetNamedItem("vote") != null && cx.Attributes.GetNamedItem("vote").Value == "true")
                    if (cx.Attributes.GetNamedItem("voteName") != null)
                        l.Add(new UVersion(cx.Attributes.GetNamedItem("voteName").Value));
            }

            return l;
        }

        public static IEnumerable<System.Version> GetAllAsVersions()
        {
            var all = UVersion.GetAllVersions()
                .Select(x => x.Name.Replace(".x", ""))
                .Select(x =>
                {
                    System.Version v;
                    return System.Version.TryParse(x, out v) ? v : null;
                })
                .WhereNotNull()
                .OrderByDescending(x => x);
            return all;
        }
    }
}