using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using Examine;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Umbraco.Core.Logging;

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
            var rootFolderPath = HttpContext.Current.Server.MapPath(DocumentationFolder);
            var configPath = HttpContext.Current.Server.MapPath(Config);

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

        public dynamic DocumentationSiteMap(string folder = "")
        {
            var path = Path.Combine(RootFolder, folder, "sitemap.js");
            var json = File.ReadAllText(path);
            dynamic documentationSiteMap = JsonConvert.DeserializeObject<dynamic>(json);
            return documentationSiteMap;
        }

        public void Process(string url, string foldername)
        {
            var zip = Download(url, foldername);
            Unzip(zip, foldername, RootFolder);
            BuildSitemap(foldername);

            //YUCK, this is horrible but unfortunately the way that the doc indexes are setup are not with 
            // a consistent integer id per document. I'm sure we can make that happen but I don't have time right now.
            ExamineManager.Instance.IndexProviderCollection["documentationIndexer"].RebuildIndex();
        }

        public void BuildSitemap(string foldername)
        {
            var folder = new DirectoryInfo(Path.Combine(RootFolder, foldername));
            dynamic root = GetFolderStructure(folder, folder.FullName, 0);

            var serializedRoot = JsonConvert.SerializeObject(root, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(folder.FullName, "sitemap.js"), serializedRoot);
        }

        private class SiteMapItem
        {
            public string name { get; set; }
            public string path { get; set; }
            public int level { get; set; }
            public int sort { get; set; }
            public bool hasChildren { get; set; }
            public List<SiteMapItem> directories { get; set; }
        }

        private SiteMapItem GetFolderStructure(DirectoryInfo dir, string rootPath, int level)
        {
            var siteMapItem = new SiteMapItem
            {
                name = dir.Name.Replace("-", " "),
                path = dir.FullName.Substring(rootPath.Length).Replace('\\', '/'),
                level = level,
                sort = GetSort(dir.Name, level) ?? 100
            };

            if (dir.GetDirectories().Any() == false)
                return siteMapItem;

            var list = new List<SiteMapItem>();
            foreach (var child in dir.GetDirectories().Where(x => x.Name != "images"))
            {
                list.Add(GetFolderStructure(child, rootPath, level + 1));
            }

            siteMapItem.hasChildren = true;
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

        private void Unzip(string path, string foldername, string rootFolder)
        {
            using (var zipInputStream = new ZipInputStream(File.OpenRead(path)))
            {
                var stopDir = "\\Documentation";
                var serverFolder = string.Format("{0}\\{1}", rootFolder, foldername);

                if (Directory.Exists(serverFolder))
                {
                    foreach (var folder in Directory.GetDirectories(serverFolder))
                        Directory.Delete(folder, true);

                    foreach (var mdfile in Directory.GetFiles(serverFolder, "*.md"))
                        File.Delete(mdfile);
                }
                else
                {
                    Directory.CreateDirectory(serverFolder);
                }

                try
                {
                    var stopDirSet = false;

                    ZipEntry theEntry;
                    while ((theEntry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (IsProjectDocumentation && !stopDirSet)
                        {
                            stopDir = Path.GetDirectoryName(theEntry.Name);
                            stopDirSet = true;
                        }

                        var directoryName = Path.GetDirectoryName(theEntry.Name);
                        var fileName = Path.GetFileName(theEntry.Name);

                        HttpContext.Current.Trace.Write("git", theEntry.Name + "  " + fileName + " - " + directoryName);

                        if (directoryName != null && stopDir != null && directoryName.Contains(stopDir))
                        {
                            var startIndex = directoryName.LastIndexOf(stopDir, StringComparison.Ordinal) +
                                             stopDir.Length;
                            directoryName = directoryName.Substring(startIndex);

                            // create directory
                            Directory.CreateDirectory(serverFolder + directoryName);

                            if (fileName == string.Empty)
                                continue;

                            var filepath = serverFolder + directoryName + "\\" + fileName;

                            using (var streamWriter = File.Create(filepath))
                            {
                                var data = new byte[2048];
                                while (true)
                                {
                                    var size = zipInputStream.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            var createEventArgs = new CreateEventArgs { FilePath = filepath };
                            FireOnCreate(createEventArgs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ZipDownloader>("Error processing documentation", ex);
                }
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
