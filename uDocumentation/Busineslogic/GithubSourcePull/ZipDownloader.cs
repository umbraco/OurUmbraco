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
        public ZipDownloader(string rootFolder, XmlDocument configuration)
        {
            Configuration = configuration;
            RootFolder = rootFolder;
        }

        public ZipDownloader(string rootFolder, string configurationPath)
        {
            XmlDocument xd = new XmlDocument();
            xd.Load(configurationPath);

            Configuration = xd;
            RootFolder = rootFolder;
        }

        public void Run()
        {
            Trace.WriteLine("Started git sync", "Gitsyncer");

            foreach(XmlNode node in Configuration.SelectNodes("//add")){
                var url = node.Attributes["url"].Value;
                var folder = node.Attributes["folder"].Value;
                var path = Path.Combine(RootFolder, folder);

                Trace.WriteLine("Loading: " + url + " to " + path, "Gitsyncer");

                process(url, folder);
            }   
        }

        public void process(string url, string foldername)
        {
            var zip = Download(url, foldername);
            unzip(zip, foldername, RootFolder);
        }

        private string Download(string url, string foldername)
        {
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
                while ((theEntry = s.GetNextEntry()) != null)
                {
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

                            if(update){
                                UpdateEventArgs ev = new UpdateEventArgs();
                                ev.FilePath = filepath;
                                FireOnUpdate(ev);
                            }else{
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
            _e.FireCancelableEvent(OnUpdate, this, e);
        }
       

        public static event EventHandler<CreateEventArgs> OnCreate;
        protected virtual void FireOnCreate(CreateEventArgs e)
        {
            _e.FireCancelableEvent(OnCreate, this, e);
        }
       

        public static event EventHandler<DeleteEventArgs> OnDelete;
        protected virtual void FireOnDelete(DeleteEventArgs e)
        {
            _e.FireCancelableEvent(OnDelete, this, e);
        }
        
    }
}