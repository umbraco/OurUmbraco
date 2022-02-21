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
                        case "fundamentals":
                            return 1;
                        case "implementation":
                            return 2;
                        case "extending":
                            return 3;
                        case "reference":
                            return 4;
                        case "tutorials":
                            return 5;
                        case "add-ons":
                            return 6;
                        case "umbraco-uno":
                            return 7;
                        case "umbraco-cloud":
                            return 8;
                        case "umbraco-heartcore":
                            return 9;
                        case "contribute":
                            return 10;
                    }
                    break;

                case 2:
                    switch (name.ToLowerInvariant())
                    {
                        //Getting-Started
                        case "managing-an-umbraco-project":
                            return 0;
                        case "editing-websites-with-umbraco":
                            return 1;
                        case "creating-websites-with-umbraco":
                            return 2;
                        case "developing-websites-with-umbraco":
                            return 3;
                        case "hosting-an-umbraco-infrastructure":
                            return 4;
                        case "where-can-i-get-help":
                            return 5;
                            
                        //Fundamentals
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
                        case "angular":
                            return 12;
                        case "api-documentation":
                            return 13;
                        case "debugging":
                            return 14;
                        case "language-variation":
                            return 15;
                        case "mapping":
                            return 16;
                        case "notifications":
                            return 17;
                        case "scheduling":
                            return 18;
                        case "using-loc":
                            return 19;
                        case "v9-config":
                            return 20;
                        
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
                        case "umbraco-deploy":
                            return 1;
                        case "umbracocourier":
                            return 2;

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
                        
                        //Umbraco Heartcore
                        case "getting-started-cloud":
                            return 0;
                        case "api-documenation":
                            return 1;
                        case "client-libraries":
                            return 2;
                        case "versions-and-updates":
                            return 3;
                    }
                    break;

                case 3:
                    switch (name.ToLowerInvariant())
                    {
                        //Fundamentals - Setup
                        case "requirements":
                            return 0;
                        case "install":
                            return 1;
                        case "upgrading":
                            return 2;
                        case "server-setup":
                            return 3;

                        //Fundamentals - Backoffice
                        case "sections":
                            return 0;
                        case "property-editors":
                            return 1;
                        case "login":
                            return 2;

                        //fundamentals - Data
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

                        //Fundamentals - Design
                        case "templates":
                            return 0;
                        case "rendering-content":
                            return 1;
                        case "rendering-media":
                            return 2;
                        case "stylesheets-javascript":
                            return 3;

                        //Fundamentals - Code
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
                        case "404handlers":
                            return 0;
                        case "applications":
                            return 1;
                        case "baserestextensions":
                            return 2;
                        case "dashboard":
                            return 3;
                        case "embeddedmedia":
                            return 4;
                        case "examineindex":
                            return 5;
                        case "examinesettings":
                            return 6;
                        case "filesystemproviders":
                            return 7;
                        case "healthchecks":
                            return 8;
                        case "serilog":
                            return 9;
                        case "tinymceconfig":
                            return 10;
                        case "trees":
                            return 11;
                        case "umbracosettings":
                            return 12;
                        case "webconfig":
                            return 13;

                        //Reference - Templating
                        case "macros":
                            return 0;
                        case "masterpages":
                            return 1;
                        case "modelsbuilder":
                            return 2;
                        case "mvc":
                            return 3;

                        //Reference - Querying
                        case "dynamicpublishedcontent":
                            return 0;
                        case "imembermanager":
                            return 1;
                        case "ipublishedcontent":
                            return 2;
                        case "ipublishedcontentquery":
                            return 3;
                        case "itagquery":
                            return 4;
                        case "membershiphelper":
                            return 5;
                        case "umbracohelper":
                            return 6;

                        //Reference - Routing
                        case "authorized":
                            return 0;
                        case "iisrewriterules":
                            return 1;
                        case "request-pipeline":
                            return 2;
                        case "url-tracking":
                            return 3;
                        case "webapi":
                            return 4;
                        
                        //Reference - Events
                        case "editormodel-events":
                            return 0;
                        case "memberservice-events":
                            return 1;
                            
                        //Reference - V9 Config
                        case "basicauthsettings":
                            return 0;
                        case "connectionstringssettings":
                            return 1;
                        case "contentsettings":
                            return 2;
                        case "debugsettings":
                            return 3;
                        case "examinesettings":
                            return 4;
                        case "exceptionfiltersettings":
                            return 5;
                        case "globalsettings":
                            return 6;
                        case "healthchecks":
                            return 7;
                        case "hostingsettings":
                            return 8;
                        case "imagingsettings":
                            return 9;
                        case "keepalivesettings":
                            return 10;
                        case "loggingsettings":
                            return 11;
                        case "maximumuploadsizesettings":
                            return 12;
                        case "modelsbuildersettings":
                            return 13;
                        case "nucachesettings":
                            return 14;
                        case "packagemigrationsettings":
                            return 15;
                        case "pluginssettings":
                            return 16;
                        case "requesthandlersettings":
                            return 17;
                        case "richtexteditorsettings":
                            return 18;
                        case "runtimeminificationsettings":
                            return 19;
                        case "runtimesettings":
                            return 20;
                        case "securitysettings":
                            return 21;
                        case "serilog":
                            return 22;
                        case "tourssettings":
                            return 23;
                        case "typefindersettings":
                            return 24;
                        case "umbracosettings":
                            return 25;
                        case "unattendedsettings":
                            return 26;
                        case "webroutingsettings":
                            return 27;
                            
                        //Tutorials - Basic site from scratch
                        case "getting-started":
                            return 0;
                        case "document-types":
                            return 1;
                        case "creating-your-first-template-and-content-node":
                            return 2;
                        case "css-and-images":
                            return 3;
                        case "displaying-the-document-type-properties":
                            return 4;
                        case "creating-master-template-part-1":
                            return 5;
                        case "creating-master-template-part-2":
                            return 6;
                        case "setting-the-navigation-menu":
                            return 7;
                        case "articles-parent-and-article-items":
                            return 8;
                        case "adding-language-variants":
                            return 9;
                        case "conclusions-where-next":
                            return 10;
                            
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

                       //Add ons - Umbraco Deploy
                        case "get-started-with-deploy":
                            return 0; 
                        case "installing-deploy":
                            return 1;
                        case "deployment-workflow":
                            return 2;
                        case "deploy-settings":
                            return 3;
                        case "upgrades":
                            return 4;
                        case "troubleshooting":
                            return 5;

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
