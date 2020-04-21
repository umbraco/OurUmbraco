using System.Collections.Generic;
using OurUmbraco.Version;

namespace OurUmbraco.Wiki.BusinessLogic
{
    public class UmbracoVersion
    {
        public string Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public UmbracoVersion() { }
        public UmbracoVersion(string version, string name, string description)
        {
            this.Version = version;
            this.Name = name;
            this.Description = description;
        }

        public static UmbracoVersion DefaultVersion()
        {
            var versions = new UWikiFileVersion();
            return AvailableVersions()[versions.DefaultKey()];
        }

        public static Dictionary<string, UmbracoVersion> AvailableVersions()
        {
            Dictionary<string, UmbracoVersion> Versions = new Dictionary<string, UmbracoVersion>();
            
            //load the wikiFileVersions from the wikiFileVersions.config file
            var versions = new UWikiFileVersion();
            var wikiFileVersions = versions.GetAllVersions();

            foreach (var v in wikiFileVersions)
            {
                Versions.Add(v.Key, new UmbracoVersion(v.Key, v.Name, v.Description));
            }
            //Versions.Add("v5", new UmbracoVersion("v5", "Version 5.0.x", "Compatible with version 5.0.x"));
            //Versions.Add("v491", new UmbracoVersion("v491", "Version 4.9.1", "Compatible with version 4.9.1"));
            //Versions.Add("v49", new UmbracoVersion("v49", "Version 4.9.x", "Compatible with version 4.9.x"));
            //Versions.Add("v48", new UmbracoVersion("v48", "Version 4.8.x", "Compatible with version 4.8.x"));
            //Versions.Add("v47", new UmbracoVersion("v47", "Version 4.7.x", "Compatible with version 4.7.x"));
            //Versions.Add("v46", new UmbracoVersion("v46", "Version 4.6.x", "Compatible with version 4.6.x"));
            //Versions.Add("v45", new UmbracoVersion("v45", "Version 4.5.x", "Compatible with version 4.5 using the new XML schema"));
            //Versions.Add("v45l", new UmbracoVersion("v45l", "Version 4.5.x - Legacy Schema Only", "Compatible with version 4.5 but only using the old XML schema"));
            //Versions.Add("v4", new UmbracoVersion("v4", "Version 4.0.x", "Compatible with version 4.0.x or 4.5 using the legacy schema"));
            //Versions.Add("v31", new UmbracoVersion("v31", "Version 3.x", "Only compatible with Umbraco version 3.x and incompatible with the version 4 API"));
            //Versions.Add("nan", new UmbracoVersion("nan", "Not version dependant", "Works with all versions of umbraco, as it does not contain any version dependencies"));
            
            return Versions;
        }
    }
}