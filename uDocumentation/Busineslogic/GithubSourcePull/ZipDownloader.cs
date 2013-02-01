using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using umbraco.BusinessLogic;

namespace uDocumentation.Busineslogic.GithubSourcePull
{
    public class ZipDownloader
    {
        public string RootFolder { get; set; }
        public XmlDocument Configuration { get; set; }
        public bool IsProjectDocumentation { get; set; }

        public ZipDownloader(string rootFolder, XmlDocument configuration)
        {
            Configuration = configuration;
            RootFolder = rootFolder;
            IsProjectDocumentation = false;
        }

        public ZipDownloader(string rootFolder, string configurationPath)
        {
            XmlDocument xd = new XmlDocument();
            xd.Load(configurationPath);

            Configuration = xd;
            RootFolder = rootFolder;
            IsProjectDocumentation = false;
        }

        public ZipDownloader(int projectId)
        {
            var project = new umbraco.NodeFactory.Node(projectId);
            var githubRepo = project.GetProperty("documentationGitRepo");
            var xd = new XmlDocument();

            xd.LoadXml(string.Format(
                "<?xml version=\"1.0\"?><configuration><sources><add url=\"{0}/zipball/master\" folder=\"\" /></sources></configuration>",
                githubRepo));

            Configuration = xd;
            RootFolder = HttpContext.Current.Server.MapPath(@"~" + project.Url.Replace("/",@"\") + @"\Documentation");
            IsProjectDocumentation = true;
        }

        public void Run()
        {
            Trace.WriteLine("Started git sync", "Gitsyncer");

            foreach (XmlNode node in Configuration.SelectNodes("//add"))
            {
                var url = node.Attributes["url"].Value;
                var folder = node.Attributes["folder"].Value;
                var path = Path.Combine(RootFolder, folder);

                Trace.WriteLine("Loading: " + url + " to " + path, "Gitsyncer");

                process(url, folder);
            }

            FinishEventArgs ev = new FinishEventArgs();
            
            FireOnFinish(ev);
        }

        public void process(string url, string foldername)
        {
            var zip = Download(url, foldername);
            unzip(zip, foldername, RootFolder);
        }

        private string Download(string url, string foldername)
        {
            var dir = new DirectoryInfo(Path.Combine(RootFolder, foldername));
            var dirsToCreate = new List<DirectoryInfo>();
            while (!dir.Exists)
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

            var path = Path.Combine(RootFolder, foldername + ".zip");
            
            if (File.Exists(path))
                File.Delete(path);

            WebClient Client = new WebClient();
            Client.DownloadFile(url, path);

            return path;
        }

        private void unzip(string path, string foldername, string rootFolder)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(path));
            ZipEntry theEntry;

            var stopDir = "\\Documentation";
            string serverFolder = rootFolder + "\\" + foldername;

            List<string> existingFiles = new List<string>();

            if (Directory.Exists(serverFolder))
            {
                var files = string.Join("|", Directory.GetFiles(serverFolder, "*.md", SearchOption.AllDirectories)).ToLower().Split('|');
                existingFiles.AddRange(files);
            }
            else
                Directory.CreateDirectory(serverFolder);

            try
            {
                bool stopDirSet = false;

                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (IsProjectDocumentation && !stopDirSet)
                    {
                        stopDir = Path.GetDirectoryName(theEntry.Name);
                        stopDirSet = true;
                    }

                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    HttpContext.Current.Trace.Write("git", theEntry.Name + "  " + fileName + " - " + directoryName);

                    if (directoryName.Contains(stopDir))
                    {
                        var startIndex = directoryName.LastIndexOf(stopDir) + stopDir.Length;
                        directoryName = directoryName.Substring(startIndex);

                        // create directory
                        Directory.CreateDirectory(serverFolder + directoryName);

                        if (fileName != String.Empty)
                        {
                            bool update = false;
                            var filepath = serverFolder + directoryName + "\\" + fileName;


                            if (existingFiles.Contains(filepath.ToLower()))
                            {
                                update = true;
                                existingFiles.Remove(filepath.ToLower());
                            }

                            FileStream streamWriter = File.Create(filepath);
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            streamWriter.Close();

                            if (update)
                            {
                                UpdateEventArgs ev = new UpdateEventArgs();
                                ev.FilePath = filepath;
                                FireOnUpdate(ev);
                            }
                            else
                            {
                                CreateEventArgs ev = new CreateEventArgs();
                                ev.FilePath = filepath;
                                FireOnCreate(ev);
                            }
                        }
                    }
                }

                s.Close();
                foreach (var file in Directory.GetFiles(rootFolder, "*.zip"))
                {
                    File.Delete(file);
                }

                foreach (var file in existingFiles)
                {
                    DeleteEventArgs ev = new DeleteEventArgs();
                    ev.FilePath = file;
                    FireOnDelete(ev);
                }

            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, -1, ex.ToString());
            }
        }

        private Events _e = new Events();

        public static event EventHandler<UpdateEventArgs> OnUpdate;
        protected virtual void FireOnUpdate(UpdateEventArgs e)
        {
            try
            {
                _e.FireCancelableEvent(OnUpdate, this, e);
            }
            catch (Exception ex) { }
        }


        public static event EventHandler<CreateEventArgs> OnCreate;
        protected virtual void FireOnCreate(CreateEventArgs e)
        {
            try
            {
                _e.FireCancelableEvent(OnCreate, this, e);
            }
            catch (Exception ex) { }
        }


        public static event EventHandler<DeleteEventArgs> OnDelete;
        protected virtual void FireOnDelete(DeleteEventArgs e)
        {
            try
            {
                _e.FireCancelableEvent(OnDelete, this, e);
            }
            catch (Exception ex) { }
        }

        public static event EventHandler<FinishEventArgs> OnFinish;
        protected virtual void FireOnFinish(FinishEventArgs e)
        {
            try
            {
                _e.FireCancelableEvent(OnFinish, this, e);
            }
            catch (Exception ex) { }
        }

    }
}