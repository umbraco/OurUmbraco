using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Examine;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace OurUmbraco.Documentation
{
    public class DocumentationUpdater
    {
        private const string DocumentationFolder = @"~\Documentation";
        private readonly string _rootFolderPath = HostingEnvironment.MapPath(DocumentationFolder);
        
        /// <summary>
        /// This will ensure that the docs exist, this checks by the existence of the /Documentation/sitemap.js file
        /// </summary>
        public void EnsureGitHubDocs()
        {
            if (Directory.Exists(_rootFolderPath) == false)
                Directory.CreateDirectory(_rootFolderPath);

            if (Directory.Exists(Path.Combine(_rootFolderPath, ".git")))
            {
                using (var repo = new LibGit2Sharp.Repository(_rootFolderPath))
                {
                    var options = new PullOptions { FetchOptions = new FetchOptions() };
                    var signature = new Signature("Our Umbraco", "our@umbraco.org", new DateTimeOffset(DateTime.Now));
                    Commands.Pull(repo, signature, options);
                }
            }
            else
            {
                // clone if the repo doesn't yet exist
                LibGit2Sharp.Repository.Clone("https://github.com/umbraco/UmbracoDocs", _rootFolderPath);
            }

            BuildSitemap(_rootFolderPath);

            //YUCK, this is horrible but unfortunately the way that the doc indexes are setup are not with 
            // a consistent integer id per document. I'm sure we can make that happen but I don't have time right now.
            ExamineManager.Instance.IndexProviderCollection["documentationIndexer"].RebuildIndex();
        }

        public SiteMapItem DocumentationSiteMap()
        {
            var path = Path.Combine(_rootFolderPath, "sitemap.js");
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<SiteMapItem>(json);
        }

        public void BuildSitemap(string foldername)
        {
            var folder = new DirectoryInfo(Path.Combine(_rootFolderPath, foldername));
            var root = GetFolderStructure(folder, folder.FullName, 0);

            var serializedRoot = JsonConvert.SerializeObject(root, Formatting.Indented);
            File.WriteAllText(Path.Combine(folder.FullName, "sitemap.js"), serializedRoot);
        }

        public class SiteMapItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public int Level { get; set; }
            public int Sort { get; set; }
            public bool HasChildren { get; set; }
            public List<SiteMapItem> Directories { get; set; }

            public string Url => $"https://our.umbraco.com/documentation{Path}/?altTemplate=Lesson";
        }

        private SiteMapItem GetFolderStructure(DirectoryInfo dir, string rootPath, int level)
        {
            var list = new List<SiteMapItem>();

            var siteMapItem = new SiteMapItem
            {
                Name = dir.Name.Replace("-", " "),
                Path = dir.FullName.Substring(rootPath.Length).Replace('\\', '/'),
                Level = level,
                Sort = GetSort(dir.Name, level) ?? 100,
                Directories = list,
                HasChildren = dir.GetDirectories().Any()
            };

            foreach (var child in dir.GetDirectories().Where(x => x.Name != "images" && x.Name != ".git" && x.Name != ".github" && x.Name != "Old-Courier-versions"))
                list.Add(GetFolderStructure(child, rootPath, level + 1));

            siteMapItem.Directories = list.OrderBy(x => x.Sort).ToList();

            return siteMapItem;
        }

        private int? GetSort(string name, int level)
        {
            switch (level)
            {
                case 1:
                    switch (name.ToLowerInvariant())
                    {
                        case "getting-started":
                            return 0;
                        case "implementation":
                            return 1;
                        case "extending":
                            return 2;
                        case "reference":
                            return 3;
                        case "tutorials":
                            return 4;
                        case "add-ons":
                            return 5;
                        case "umbraco-cloud":
                            return 6;
                        case "umbraco-heartcore":
                            return 7;
                    }
                    break;

                case 2:
                    switch (name.ToLowerInvariant())
                    {
                        //Getting Started
                        case "setup":
                            return 0;
                        case "backoffice":
                            return 1;
                        case "data":
                            return 2;
                        case "design":
                            return 3;
                        case "code":
                            return 4;

                        //Implementation
                        case "default-routing":
                            return 0;
                        case "custom-routing":
                            return 1;
                        case "controllers":
                            return 2;
                        case "data-persistence":
                            return 3;
                        case "rest-api":
                            return 4;

                        //Extending
                        case "dashboards":
                            return 0;
                        case "section-trees":
                            return 1;
                        case "property-editors":
                            return 2;
                        case "macro-parameter-editors":
                            return 3;
                        case "healthcheck":
                            return 4;
                        case "language-files":
                            return 5;

                        //Reference
                        case "config":
                            return 0;
                        case "templating":
                            return 1;
                        case "querying":
                            return 2;
                        case "routing":
                            return 3;
                        case "searching":
                            return 4;
                        case "events":
                            return 5;
                        case "management":
                            return 6;
                        case "plugins":
                            return 7;
                        case "cache":
                            return 8;
                        case "packaging":
                            return 9;
                        case "security":
                            return 10;
                        case "common-pitfalls":
                            return 11;

                        //Tutorials
                        case "creating-basic-site":
                            return 0;
                        case "creating-a-custom-dashboard":
                            return 1;
                        case "creating-a-property-editor":
                            return 2;
                        case "multilanguage-setup":
                            return 3;
                        case "starter-kit":
                            return 4;
                        case "editors-manual":
                            return 5;

                        //Add ons
                        case "umbracoforms":
                            return 0;
                        case "umbracocourier":
                            return 1;

                        //Umbraco Cloud
                        case "getting-started":
                            return 0;
                        case "set-up":
                            return 1;
                        case "deployment":
                            return 2;
                        case "databases":
                            return 3;
                        case "upgrades":
                            return 4;
                        case "troubleshooting":
                            return 5;
                        case "frequently-asked-questions":
                            return 6;
                        
                    }
                    break;

                case 3:
                    switch (name.ToLowerInvariant())
                    {
                        //Getting Started - Setup
                        case "requirements":
                            return 0;
                        case "install":
                            return 1;
                        case "upgrading":
                            return 2;
                        case "server-setup":
                            return 3;

                        //Getting Started - Backoffice
                        case "sections":
                            return 0;
                        case "property-editors":
                            return 1;
                        case "login":
                            return 2;

                        //Getting Started - Data
                        case "defining-content":
                            return 0;
                        case "creating-media":
                            return 1;
                        case "members":
                            return 2;
                        case "data-types":
                            return 3;
                        case "scheduled-publishing":
                            return 4;

                        //Getting Started - Design
                        case "templates":
                            return 0;
                        case "rendering-content":
                            return 1;
                        case "rendering-media":
                            return 2;
                        case "stylesheets-javascript":
                            return 3;

                        //Getting Started - Code
                        case "umbraco-services":
                            return 0;
                        case "subscribing-to-events":
                            return 1;
                        case "creating-forms":
                            return 2;

                        //Implementation - Default Routing
                        case "inbound-pipeline":
                            return 0;
                        case "controller-selection":
                            return 1;
                        case "execute-request":
                            return 2;

                        //Reference - Config
                        case "webconfig":
                            return 0;
                        case "404handlers":
                            return 1;
                        case "applications":
                            return 2;
                        case "embeddedmedia":
                            return 3;
                        case "examineindex":
                            return 4;
                        case "examinesettings":
                            return 5;
                        case "filesystemproviders":
                            return 6;
                        case "baserestextensions":
                            return 7;
                        case "tinymceconfig":
                            return 8;
                        case "trees":
                            return 9;
                        case "umbracosettings":
                            return 10;
                        case "dashboard":
                            return 11;
                        case "healthchecks":
                            return 12;

                        //Reference - Templating
                        case "mvc":
                            return 0;
                        case "masterpages":
                            return 1;
                        case "macros":
                            return 2;
                        case "modelsbuilder":
                            return 3;

                        //Reference - Querying
                        case "ipublishedcontent":
                            return 0;
                        case "dynamicpublishedcontent":
                            return 1;
                        case "umbracohelper":
                            return 2;
                        case "membershiphelper":
                            return 3;

                        //Reference - Routing
                        case "authorized":
                            return 0;
                        case "request-pipeline":
                            return 1;
                        case "webapi":
                            return 2;
                        case "iisrewriterules":
                            return 3;
                        case "url-tracking":
                            return 4;
                            
                        //Tutorials - Basic site from scratch
                        case "getting-started":
                            return 0;
                        case "document-types":
                            return 1;
                        case "creating-your-first-template-and-content-node":
                            return 2;
                        case "css-and-javascript":
                            return 3;
                        case "outputting-the-document-type-properties":
                            return 4;
                        case "creating-master-template-part-1":
                            return 5;
                        case "creating-master-template-part-2":
                            return 6;
                        case "master-template-the-navigation-menu":
                            return 7;
                        case "articles-parent-and-article-items":
                            return 8;
                        case "conclusions-where-next":
                            return 9;
                            
                        //Tutorials - Editor Manual
                        case "introduction":
                            return 0;
                        case "getting-started-with-umbraco":
                            return 1;
                        case "working-with-content":
                            return 2;
                        case "version-management":
                            return 3;
                        case "media-management":
                            return 4;
                        case "tips-and-tricks":
                            return 5;

                        //Add Ons - UmbracoForms
                        case "installation":
                            return 0;
                        case "editor":
                            return 1;
                        case "developer":
                            return 2;

                        //Add ons - UmbracoCourier
                        case "architechture":
                            return 1;

                        //Umbraco Cloud - Getting Started
                        case "project-overview":
                            return 0;
                        case "environments":
                            return 1;
                        case "the-umbraco-cloud-portal":
                            return 2;
                        case "baselines":
                            return 3;
                        case "migrate-existing-site":
                            return 4;

                        //Umbraco Cloud - Set Up
                        case "working-locally":
                            return 0;
                        case "visual-studio":
                            return 1;
                        case "working-with-visual-studio":
                            return 2;
                        case "working-with-uaas-cli":
                            return 3;
                        case "project-settings":
                            return 4;
                        case "team-members":
                            return 5;
                        case "media":
                            return 6;
                        case "smtp-settings":
                            return 7;
                        case "manage-hostnames":
                            return 8;
                        case "config-transforms":
                            return 9;
                        case "power-tools":
                            return 10;

                        //Umbraco Cloud - Deployment
                        case "local-to-cloud":
                            return 0;
                        case "cloud-to-cloud":
                            return 1;
                        case "content-transfer":
                            return 2;
                        case "restoring-content":
                            return 3;
                        case "deployment-webhook":
                            return 4;

                        //Umbraco Cloud - Troubleshooting
                        case "deployments":
                            return 0;
                        case "log-files":
                            return 1;
                        case "faq":
                            return 2;
                        case "courier":
                            return 3;

                    }
                    break;
            }
            return null;
        }
    }
}
