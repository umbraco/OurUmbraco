using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using Examine;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using ZipFile = System.IO.Compression.ZipFile;

namespace OurUmbraco.Documentation.Busineslogic.GithubSourcePull
{
    public class ZipDownloader
    {
        private const string DocumentationFolder = @"~\Documentation";
        private const string Config = @"~\config\githubpull.config";
        private readonly string _rootFolderPath = HostingEnvironment.MapPath(DocumentationFolder);
        private readonly string _configPath = HostingEnvironment.MapPath(Config);

        public string RootFolder { get; set; }
        public XmlDocument Configuration { get; set; }
        public bool IsProjectDocumentation { get; set; }

        public ZipDownloader(string rootFolder, XmlDocument configuration)
        {
            Configuration = configuration;
            RootFolder = rootFolder;
            IsProjectDocumentation = false;
        }

        public ZipDownloader()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(_configPath);

            Configuration = xmlDocument;
            RootFolder = _rootFolderPath;
            IsProjectDocumentation = false;
        }

        public ZipDownloader(string rootFolder, string configurationPath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(configurationPath);

            Configuration = xmlDocument;
            RootFolder = rootFolder;
            IsProjectDocumentation = false;
        }

        public ZipDownloader(int projectId)
        {
            var project = new umbraco.NodeFactory.Node(projectId);
            var githubRepo = project.GetProperty("documentationGitRepo");
            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(string.Format("<?xml version=\"1.0\"?><configuration><sources><add url=\"{0}/zipball/master\" folder=\"\" /></sources></configuration>", githubRepo));

            Configuration = xmlDocument;
            RootFolder = HostingEnvironment.MapPath(@"~" + project.Url.Replace("/", @"\") + @"\Documentation");
            IsProjectDocumentation = true;
        }

        /// <summary>
        /// This will ensure that the docs exist, this checks by the existence of the /Documentation/sitemap.js file
        /// </summary>
        public static void EnsureGitHubDocs(bool overwrite = false)
        {
            var rootFolderPath = HostingEnvironment.MapPath(DocumentationFolder);
            var configPath = HostingEnvironment.MapPath(Config);

            //Check if it exists, if it does then exit
            if (overwrite == false && File.Exists(Path.Combine(rootFolderPath, "sitemap.js")))
                return;

            if (Directory.Exists(rootFolderPath) == false)
                Directory.CreateDirectory(rootFolderPath);

            var unzip = new ZipDownloader(rootFolderPath, configPath) { IsProjectDocumentation = true };
            unzip.Run();
        }

        public void Run()
        {
            Trace.WriteLine("Started git sync", "Gitsyncer");

            var xmlNodeList = Configuration.SelectNodes("//add");
            if (xmlNodeList != null)
            {
                foreach (XmlNode node in xmlNodeList)
                {
                    if (node.Attributes != null)
                    {
                        var url = node.Attributes["url"].Value;
                        var folder = node.Attributes["folder"].Value;
                        var path = Path.Combine(RootFolder, folder);

                        Trace.WriteLine(string.Format("Loading: {0} to {1}", url, path), "Gitsyncer");

                        Process(url, folder);
                    }
                }
            }

            var finishEventArgs = new FinishEventArgs();
            FireOnFinish(finishEventArgs);
        }

        public SiteMapItem DocumentationSiteMap(string folder = "")
        {
            var path = Path.Combine(RootFolder, folder, "sitemap.js");
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<SiteMapItem>(json);
        }

        public void Process(string url, string foldername)
        {
            var zip = Download(url, foldername);
            RemoveExistingDocumentation(RootFolder);
            ZipFile.ExtractToDirectory(zip, RootFolder);

            var unzippedPath = RootFolder + "\\UmbracoDocs-master\\";
            foreach (var directory in new DirectoryInfo(unzippedPath).GetDirectories())
                Directory.Move(directory.FullName, RootFolder + "\\" + directory.Name);
            foreach (var file in new DirectoryInfo(unzippedPath).GetFiles())
                File.Move(file.FullName, RootFolder + "\\" + file.Name);
            Directory.Delete(unzippedPath, true);

            BuildSitemap(foldername);

            //YUCK, this is horrible but unfortunately the way that the doc indexes are setup are not with 
            // a consistent integer id per document. I'm sure we can make that happen but I don't have time right now.
            ExamineManager.Instance.IndexProviderCollection["documentationIndexer"].RebuildIndex();
        }

        public void BuildSitemap(string foldername)
        {
            var folder = new DirectoryInfo(Path.Combine(RootFolder, foldername));
            var root = GetFolderStructure(folder, folder.FullName, 0);

            var serializedRoot = JsonConvert.SerializeObject(root, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(folder.FullName, "sitemap.js"), serializedRoot);
        }

        public class SiteMapItem
        {
            public string name { get; set; }
            public string path { get; set; }
            public int level { get; set; }
            public int sort { get; set; }
            public bool hasChildren { get; set; }
            public List<SiteMapItem> directories { get; set; }

            //public string url => $"http://localhost:24292/documentation{this.path}/?altTemplate=Lesson";
            
            public string url
            {
                get
                {
                    return string.Format("https://our.umbraco.org/documentation{0}/?altTemplate=Lesson", this.path);
                }
            }
        }

        private SiteMapItem GetFolderStructure(DirectoryInfo dir, string rootPath, int level)
        {
            var list = new List<SiteMapItem>();

            var siteMapItem = new SiteMapItem
            {
                name = dir.Name.Replace("-", " "),
                path = dir.FullName.Substring(rootPath.Length).Replace('\\', '/'),
                level = level,
                sort = GetSort(dir.Name, level) ?? 100,
                directories = list,
                hasChildren = dir.GetDirectories().Any()
            };
            
            foreach (var child in dir.GetDirectories().Where(x => x.Name != "images"))
            {
                list.Add(GetFolderStructure(child, rootPath, level + 1));
            }

            siteMapItem.directories = list.OrderBy(x => x.sort).ToList();

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
                            return 3;
                        case "reference":
                            return 4;
                        case "tutorials":
                            return 5;
                        case "add-ons":
                            return 6;
                        case "umbraco-as-a-service":
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
                            return 3;
                        case "design":
                            return 4;

                        //Implementation
                        case "default-routing":
                            return 0;

                        //Extending
                        case "dashboards":
                            return 0;
                        case "section-trees":
                            return 1;
                        case "property-editors":
                            return 2;
                        case "macro-parameter-editors":
                            return 3;

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

                        //Tutorials
                        case "creating-basic-site":
                            return 0;

                        //Add ons
                        case "umbracoforms":
                            return 0;

                        //Umbraco Cloud
                        case "getting-started":
                            return 0;
                        case "set-up":
                            return 1;
                        case "deployment":
                            return 2;
                        case "troubleshooting":
                            return 3;
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

                        //Getting Started - Data
                        case "defining-content":
                            return 0;
                        case "creating-media":
                            return 1;
                        case "members":
                            return 2;
                        case "data-types":
                            return 3;

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
                        case "appalications":
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

                        //Reference - Templating
                        case "mvc":
                            return 0;
                        case "masterpages":
                            return 1;
                        case "macros":
                            return 2;

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

                        //Add Ons - UmbracoForms
                        case "installation":
                            return 0;
                        case "editor":
                            return 1;
                        case "developer":
                            return 2;

                        //Add ons - UmbracoCourrier
                        case "architechture":
                            return 3;

                        //UaaS - Getting Started
                        case "baselines":
                            return 0;
                        case "baseline-merge-conflicts":
                            return 1;

                        //UaaS - Set Up
                        case "working-locally":
                            return 0;
                        case "team-members":
                            return 1;
                        case "visual-studio":
                            return 2;
                        case "media":
                            return 3;

                    }
                    break;
            }
            return null;
        }

        private string Download(string url, string foldername)
        {
            var dir = new DirectoryInfo(Path.Combine(RootFolder, foldername));
            var dirsToCreate = new List<DirectoryInfo>();

            while (dir != null && dir.Exists == false)
            {
                dirsToCreate.Add(dir);
                dir = dir.Parent;
            }

            if (dirsToCreate.Any())
            {
                dirsToCreate.Reverse();
                foreach (var d in dirsToCreate)
                    d.Create();
            }

            var path = Path.Combine(RootFolder, foldername + "archive.zip");

            if (File.Exists(path))
                File.Delete(path);

            using (var client = new WebClient())
            {
                client.DownloadFile(url, path);
            }

            return path;
        }
        
        private static void RemoveExistingDocumentation(string folder)
        {
            if (Directory.Exists(folder))
            {
                foreach (var directory in Directory.GetDirectories(folder))
                    Retry.Do(() => Directory.Delete(directory, true), TimeSpan.FromSeconds(1), 5);

                foreach (var mdfile in Directory.GetFiles(folder, "*.md"))
                    Retry.Do(() => File.Delete(mdfile), TimeSpan.FromSeconds(1), 5);
            }
            else
            {
                Directory.CreateDirectory(folder);
            }
        }

        private readonly Events _events = new Events();

        public static event EventHandler<UpdateEventArgs> OnUpdate;
        protected virtual void FireOnUpdate(UpdateEventArgs e)
        {
            try
            {
                _events.FireCancelableEvent(OnUpdate, this, e);
            }
            catch (Exception ex)
            {
                LogHelper.Error<ZipDownloader>("Error firing update handler", ex);
            }
        }

        public static event EventHandler<CreateEventArgs> OnCreate;
        protected virtual void FireOnCreate(CreateEventArgs e)
        {
            try
            {
                _events.FireCancelableEvent(OnCreate, this, e);
            }
            catch (Exception ex)
            {
                LogHelper.Error<ZipDownloader>("Error firing create handler", ex);
            }
        }

        public static event EventHandler<DeleteEventArgs> OnDelete;
        protected virtual void FireOnDelete(DeleteEventArgs e)
        {
            try
            {
                _events.FireCancelableEvent(OnDelete, this, e);
            }
            catch (Exception ex)
            {
                LogHelper.Error<ZipDownloader>("Error firing delte handler", ex);
            }
        }

        public static event EventHandler<FinishEventArgs> OnFinish;
        protected virtual void FireOnFinish(FinishEventArgs e)
        {
            try
            {
                _events.FireCancelableEvent(OnFinish, this, e);
            }
            catch (Exception ex)
            {
                LogHelper.Error<ZipDownloader>("Error firing finish handler", ex);
            }
        }
    }
}
