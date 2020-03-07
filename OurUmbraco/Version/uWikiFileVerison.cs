using System.Collections.Generic;
using System.Linq;
using OurUmbraco.Our.Extensions;
using OurUmbraco.Project.uVersion;

namespace OurUmbraco.Version
{
    public class UWikiFileVersion
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public string VoteDescription { get; set; }

        public List<UWikiFileVersion> GetAllVersions()
        {
            var versions = new UVersion();
            var allVersions = versions.GetAllVersions();
            var wikiFileVersions = new List<UWikiFileVersion>();
            foreach (var version in allVersions)
            {
                wikiFileVersions.Add(new UWikiFileVersion
                {
                    Name = version.Name,
                    Key = version.Key,
                    VoteDescription = version.FullVersion.VersionDescription()
                });
            }
            
            return wikiFileVersions;
        }

        public string DefaultKey()
        {
            var latestVersion = GetAllVersions().First();
            return latestVersion.Key;
        }
    }
}
