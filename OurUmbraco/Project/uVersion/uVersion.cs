using System.Collections.Generic;
using System.Linq;
using OurUmbraco.Our.Extensions;
using OurUmbraco.Our.Services;
using Umbraco.Core;

namespace OurUmbraco.Project.uVersion
{
    public class UVersion
    {
        public string Key { get; private set; }
        public string Name { get; private set; }

        public System.Version FullVersion { get; private set; }
        
        public IEnumerable<UVersion> GetAllVersions()
        {
            var releasesService = new ReleasesService();
            var releases = releasesService.GetReleasesCache()
                .Where(x => x.FullVersion.Major >= 6 && x.FullVersion.Build == 0 && x.Released)
                .OrderByDescending(x => x.FullVersion).ToList();
            
            var versions = new List<UVersion>();
            foreach (var release in releases)
            {
                versions.Add(new UVersion
                {
                    Name = release.FullVersion.VersionName(),
                    Key = release.FullVersion.VersionKey(),
                    FullVersion = release.FullVersion
                });
            }
            
            return versions;
        }

        public IEnumerable<System.Version> GetAllAsVersions()
        {
            var versions = new UVersion();
            var all = versions.GetAllVersions()
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