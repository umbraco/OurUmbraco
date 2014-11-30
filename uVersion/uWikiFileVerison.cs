using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace uVersion
{
    public class UWikiFileVersion
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public string VoteDescription { get; set; }
        public bool Exists { get; set; }


        public UWikiFileVersion(string name)
        {
            XmlNode x = config.GetKeyAsNode("/configuration/versions/version [@name = '" + name + "']");
            if (x != null)
            {
                Name = x.Attributes.GetNamedItem("name").Value;
                Description = x.Attributes.GetNamedItem("description").Value;
                VoteDescription = x.Attributes.GetNamedItem("voteDescription") == null 
                    ? string.Empty 
                    : x.Attributes.GetNamedItem("voteDescription").Value;
                Key = x.Attributes.GetNamedItem("key").Value;



                Exists = true;
            }
            else
                Exists = false;
        }

        public static List<UWikiFileVersion> GetAllVersions()
        {
            XmlNode x = config.GetKeyAsNode("/configuration/versions");
            List<UWikiFileVersion> l = new List<UWikiFileVersion>();
            foreach (XmlNode cx in x.ChildNodes)
            {
                if (cx.Attributes != null && cx.Attributes.GetNamedItem("name") != null)
                    l.Add(new UWikiFileVersion(cx.Attributes.GetNamedItem("name").Value));
            }

            return l;
        }

        public static string DefaultKey()
        {
            XmlNode x = config.GetKeyAsNode("/configuration");
                if (x.Attributes.GetNamedItem("default") != null)
                   return x.Attributes.GetNamedItem("default").Value;
            return null;
        }
    }
}
