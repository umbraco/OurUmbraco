using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

            const string regexPattern = @"(\d*sort-)";
            var regex = new Regex(regexPattern);
            
            var siteMapItem = new SiteMapItem
            {
                Name = Regex.Replace(dir.Name, regexPattern, "").Replace("-", " "),
                Path = Regex.Replace(dir.FullName, regexPattern, "").Substring(rootPath.Length).Replace('\\', '/'),
                Level = level,
                Sort = GetSort(dir.Parent.Name + "/" + dir.Name, level) ?? 100,
                Directories = list,
                HasChildren = dir.GetDirectories().Any()
            };

            foreach (var child in dir.GetDirectories().Where(x => x.Name != "images" && x.Name != ".git" && x.Name != ".github" && x.Name != "Old-Courier-versions" && x.Name != "Umbraco-Uno" && x.Name != "UmbracoCourier"))
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
                        case "documentation/getting-started":
                            return 0;
                        case "documentation/fundamentals":
                            return 1;
                        case "documentation/implementation":
                            return 2;
                        case "documentation/extending":
                            return 3;
                        case "documentation/reference":
                            return 4;
                        case "documentation/tutorials":
                            return 5;
                        case "documentation/add-ons":
                            return 6;
                        case "documentation/umbraco-cloud":
                            return 7;
                        case "documentation/umbraco-heartcore":
                            return 8;
                        case "documentation/contribute":
                            return 9;
                    }
                    break;

                case 2:
                    switch (name.ToLowerInvariant())
                    {
                        //Getting-Started
                        case "getting-started/managing-an-umbraco-project":
                            return 0;
                        case "getting-started/editing-websites-with-umbraco":
                            return 1;
                        case "getting-started/creating-websites-with-umbraco":
                            return 2;
                        case "getting-started/developing-websites-with-umbraco":
                            return 3;
                        case "getting-started/hosting-an-umbraco-infrastructure":
                            return 4;
                        case "getting-started/where-can-i-get-help":
                            return 5;
                            
                        //Fundamentals
                        case "fundamentals/setup":
                            return 0;
                        case "fundamentals/backoffice":
                            return 1;
                        case "fundamentals/data":
                            return 2;
                        case "fundamentals/design":
                            return 3;
                        case "fundamentals/code":
                            return 4;

                        //Implementation
                        case "implementation/default-routing":
                            return 0;
                        case "implementation/custom-routing":
                            return 1;
                        case "implementation/controllers":
                            return 2;
                        case "implementation/data-persistence":
                            return 3;
                        case "implementation/rest-api":
                            return 4;

                        //Extending
                        case "extending/dashboards":
                            return 0;
                        case "extending/section-trees":
                            return 1;
                        case "extending/property-editors":
                            return 2;
                        case "extending/macro-parameter-editors":
                            return 3;
                        case "extending/health-check":
                            return 4;
                        case "extending/language-files":
                            return 5;

                        //Reference
                        case "reference/configuration":
                            return 0;
                        case "reference/configuration-for-umbraco-7-and-8":
                            return 1;
                        case "reference/templating":
                            return 2;
                        case "reference/querying":
                            return 3;
                        case "reference/routing":
                            return 4;
                        case "reference/searching":
                            return 5;
                        case "reference/events":
                            return 6;
                        case "reference/management":
                            return 7;
                        case "reference/plugins":
                            return 8;
                        case "reference/cache":
                            return 9;
                        case "reference/security":
                            return 10;
                        case "reference/common-pitfalls":
                            return 11;
                        case "reference/angular":
                            return 12;
                        case "reference/api-documentation":
                            return 13;
                        case "reference/debugging":
                            return 14;
                        case "reference/language-variation":
                            return 15;
                        case "reference/mapping":
                            return 16;
                        case "reference/notifications":
                            return 17;
                        case "reference/scheduling":
                            return 18;
                        case "reference/using-ioc":
                            return 19;
                        
                        //Tutorials
                        case "tutorials/creating-basic-site":
                            return 0;
                        case "tutorials/creating-a-custom-dashboard":
                            return 1;
                        case "tutorials/creating-a-property-editor":
                            return 2;
                        case "tutorials/multilanguage-setup":
                            return 3;
                        case "tutorials/starter-kit":
                            return 4;
                        case "tutorials/editors-manual":
                            return 5;

                        //Add ons
                        case "add-ons/umbracoforms":
                            return 0;
                        case "add-ons/umbraco-deploy":
                            return 1;
                        case "add-ons/umbracocourier":
                            return 2;

                        //Umbraco Cloud
                        case "umbraco-cloud/getting-started":
                            return 0;
                        case "umbraco-cloud/set-up":
                            return 1;
                        case "umbraco-cloud/deployment":
                            return 2;
                        case "umbraco-cloud/databases":
                            return 3;
                        case "umbraco-cloud/upgrades":
                            return 4;
                        case "umbraco-cloud/troubleshooting":
                            return 5;
                        case "umbraco-cloud/frequently-asked-questions":
                            return 6;
                        
                        //Umbraco Heartcore
                        case "umbraco-heartcore/getting-started-cloud":
                            return 0;
                        case "umbraco-heartcore/api-documentation":
                            return 1;
                        case "umbraco-heartcore/client-libraries":
                            return 2;
                        case "umbraco-heartcore/versions-and-updates":
                            return 3;
                    }
                    break;

                case 3:
                    switch (name.ToLowerInvariant())
                    {
                        //Fundamentals - Setup
                        case "setup/requirements":
                            return 0;
                        case "setup/install":
                            return 1;
                        case "setup/upgrading":
                            return 2;
                        case "setup/server-setup":
                            return 3;

                        //Fundamentals - Backoffice
                        case "backoffice/sections":
                            return 0;
                        case "backoffice/property-editors":
                            return 1;
                        case "backoffice/login":
                            return 2;

                        //fundamentals - Data
                        case "data/defining-content":
                            return 0;
                        case "data/creating-media":
                            return 1;
                        case "data/members":
                            return 2;
                        case "data/data-types":
                            return 3;
                        case "data/scheduled-publishing":
                            return 4;

                        //Fundamentals - Design
                        case "design/templates":
                            return 0;
                        case "design/rendering-content":
                            return 1;
                        case "design/rendering-media":
                            return 2;
                        case "design/stylesheets-javascript":
                            return 3;

                        //Fundamentals - Code
                        case "code/umbraco-services":
                            return 0;
                        case "code/subscribing-to-events":
                            return 1;
                        case "code/creating-forms":
                            return 2;

                        //Implementation - Default Routing
                        case "default-routing/inbound-pipeline":
                            return 0;
                        case "default-routing/controller-selection":
                            return 1;
                        case "default-routing/execute-request":
                            return 2;

                        //Reference - Configuration for umbraco 7 and 8
                        case "configuration-for-umbraco-7-and-8/404handlers":
                            return 0;
                        case "configuration-for-umbraco-7-and-8/applications":
                            return 1;
                        case "configuration-for-umbraco-7-and-8/baserestextensions":
                            return 2;
                        case "configuration-for-umbraco-7-and-8/dashboard":
                            return 3;
                        case "configuration-for-umbraco-7-and-8/embeddedmedia":
                            return 4;
                        case "configuration-for-umbraco-7-and-8/examineindex":
                            return 5;
                        case "configuration-for-umbraco-7-and-8/examinesettings":
                            return 6;
                        case "configuration-for-umbraco-7-and-8/filesystemproviders":
                            return 7;
                        case "configuration-for-umbraco-7-and-8/healthchecks":
                            return 8;
                        case "configuration-for-umbraco-7-and-8/serilog":
                            return 9;
                        case "configuration-for-umbraco-7-and-8/tinymceconfig":
                            return 10;
                        case "configuration-for-umbraco-7-and-8/trees":
                            return 11;
                        case "configuration-for-umbraco-7-and-8/umbracosettings":
                            return 12;
                        case "configuration-for-umbraco-7-and-8/webconfig":
                            return 13;

                        //Reference - Templating
                        case "templating/macros":
                            return 0;
                        case "templating/masterpages":
                            return 1;
                        case "templating/modelsbuilder":
                            return 2;
                        case "templating/mvc":
                            return 3;

                        //Reference - Querying
                        case "querying/dynamicpublishedcontent":
                            return 0;
                        case "querying/imembermanager":
                            return 1;
                        case "querying/ipublishedcontent":
                            return 2;
                        case "querying/ipublishedcontentquery":
                            return 3;
                        case "querying/itagquery":
                            return 4;
                        case "querying/membershiphelper":
                            return 5;
                        case "querying/umbracohelper":
                            return 6;

                        //Reference - Routing
                        case "routing/authorized":
                            return 0;
                        case "routing/iisrewriterules":
                            return 1;
                        case "routing/request-pipeline":
                            return 2;
                        case "routing/url-tracking":
                            return 3;
                        case "routing/webapi":
                            return 4;
                        
                        //Reference - Events
                        case "events/editormodel-events":
                            return 0;
                        case "events/memberservice-events":
                            return 1;
                            
                        //Tutorials - Basic site from scratch
                        case "creating-basic-site/getting-started":
                            return 0;
                        case "creating-basic-site/document-types":
                            return 1;
                        case "creating-basic-site/creating-your-first-template-and-content-node":
                            return 2;
                        case "creating-basic-site/css-and-images":
                            return 3;
                        case "creating-basic-site/displaying-the-document-type-properties":
                            return 4;
                        case "creating-basic-site/creating-master-template-part-1":
                            return 5;
                        case "creating-basic-site/creating-master-template-part-2":
                            return 6;
                        case "creating-basic-site/setting-the-navigation-menu":
                            return 7;
                        case "creating-basic-site/articles-parent-and-article-items":
                            return 8;
                        case "creating-basic-site/adding-language-variants":
                            return 9;
                        case "creating-basic-site/conclusions-where-next":
                            return 10;
                            
                        //Tutorials - Editor Manual
                        case "editors-manual/getting-started-with-umbraco":
                            return 0;
                        case "editors-manual/working-with-content":
                            return 1;
                        case "editors-manual/version-management":
                            return 2;
                        case "editors-manual/media-management":
                            return 3;
                        case "editors-manual/tips-and-tricks":
                            return 4;

                        //Add Ons - UmbracoForms
                        case "umbracoforms/installation":
                            return 0;
                        case "umbracoforms/editor":
                            return 1;
                        case "umbracoforms/developer":
                            return 2;

                       //Add ons - Umbraco Deploy
                        case "umbraco-deploy/get-started-with-deploy":
                            return 0; 
                        case "umbraco-deploy/installing-deploy":
                            return 1;
                        case "umbraco-deploy/deployment-workflow":
                            return 2;
                        case "umbraco-deploy/deploy-settings":
                            return 3;
                        case "umbraco-deploy/upgrades":
                            return 4;
                        case "umbraco-deploy/troubleshooting":
                            return 5;

                        //Umbraco Cloud - Getting Started
                        case "getting-started/project-overview":
                            return 0;
                        case "getting-started/environments":
                            return 1;
                        case "getting-started/the-umbraco-cloud-portal":
                            return 2;
                        case "getting-started/baselines":
                            return 3;
                        case "getting-started/migrate-existing-site":
                            return 4;

                        //Umbraco Cloud - Set Up
                        case "set-up/working-locally":
                            return 0;
                        case "set-up/visual-studio":
                            return 1;
                        case "set-up/working-with-visual-studio":
                            return 2;
                        case "set-up/working-with-uaas-cli":
                            return 3;
                        case "set-up/project-settings":
                            return 4;
                        case "set-up/team-members":
                            return 5;
                        case "set-up/media":
                            return 6;
                        case "set-up/smtp-settings":
                            return 7;
                        case "set-up/manage-hostnames":
                            return 8;
                        case "set-up/config-transforms":
                            return 9;
                        case "set-up/power-tools":
                            return 10;

                        //Umbraco Cloud - Deployment
                        case "deployment/local-to-cloud":
                            return 0;
                        case "deployment/cloud-to-cloud":
                            return 1;
                        case "deployment/content-transfer":
                            return 2;
                        case "deployment/restoring-content":
                            return 3;
                        case "deployment/deployment-webhook":
                            return 4;

                        //Umbraco Cloud - Troubleshooting
                        case "troubleshooting/deployments":
                            return 0;
                        case "troubleshooting/log-files":
                            return 1;
                        case "troubleshooting/faq":
                            return 2;
                        case "troubleshooting/courier":
                            return 3;

                    }
                    break;
            }
            return null;
        }
    }
}
