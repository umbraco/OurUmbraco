using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic;
using umbraco.BusinessLogic;
using System.Xml;

namespace uWiki.Businesslogic {
    public class WikiFile {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string FileType { get; set; }
        public Guid NodeVersion { get; set; }
        public int CreatedBy { get; set; }
        public int RemovedBy { get; set; }
        public int NodeId { get; set; }
        
        public DateTime CreateDate { get; set; }
        public bool Current { get; set; }
        public int Downloads { get; set; }
        public bool Archived { get; set; }

        public bool Verified { get; set; }

        public List<UmbracoVersion> Versions { get; set; }
        public UmbracoVersion Version { get; set; }


        public void Delete() {

            if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(Path)))
                System.IO.File.Delete(HttpContext.Current.Server.MapPath(Path));

            Data.SqlHelper.ExecuteNonQuery("DELETE FROM wikiFiles where ID = @id", Data.SqlHelper.CreateParameter("@id", Id));

        }


        public static List<WikiFile> CurrentFiles(int nodeId) {
            List<WikiFile> wfl = new List<WikiFile>();


            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader("SELECT id FROM wikiFiles WHERE nodeId = @nodeid", Data.SqlHelper.CreateParameter("@nodeId", nodeId));

            while (dr.Read()) {
                wfl.Add(new WikiFile(dr.GetInt("id")));
            }

            return wfl;
        }


        private Events _e = new Events();

        private WikiFile() { }
        public static WikiFile Create(string name, Guid node, Guid member, HttpPostedFile file, string filetype, List<UmbracoVersion> versions) {

            try {
                Content d = Document.GetContentFromVersion(node);
                Member m = new Member(member);

                if (d != null && m != null) {

                    WikiFile wf = new WikiFile();
                    wf.Name = name;
                    wf.NodeId = d.Id;
                    wf.NodeVersion = d.Version;
                    wf.FileType = filetype;
                    wf.CreatedBy = m.Id;
                    wf.Downloads = 0;
                    wf.Archived = false;
                    wf.Versions = versions;
                    wf.Version = versions[0];
                    wf.Verified = false;


                    string path = "/media/wiki/" + d.Id.ToString();

                    if (!System.IO.Directory.Exists(HttpContext.Current.Server.MapPath(path)))
                        System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                    string filename = file.FileName;
                    string extension = filename.Substring(filename.LastIndexOf('.')+1);
                    filename = filename.Substring(0, filename.LastIndexOf('.') + 1);

                    path = path + "/" + DateTime.Now.Ticks.ToString() + "_" + umbraco.cms.helpers.url.FormatUrl(filename) + "." + extension;

                    file.SaveAs(HttpContext.Current.Server.MapPath(path));

                    wf.Path = path;

                    wf.Save();

                    return wf;
                }

                
            } catch (Exception ex) {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
                
            }

            return null;
        }

        //public static WikiFile Create(string name, string ext, Guid node, Guid member, byte[] file, string filetype)
        //{
        //    return Create(name, ext, node, member, file, filetype, UmbracoVersion.AvailableVersions()["v45"]);
        //}

        public static WikiFile Create(string name, string ext, Guid node, Guid member, byte[] file, string filetype, List<UmbracoVersion> versions)
        {

            try
            {
                Content d = Document.GetContentFromVersion(node);
                Member m = new Member(member);

                if (d != null && m != null)
                {

                    WikiFile wf = new WikiFile();
                    wf.Name = name;
                    wf.NodeId = d.Id;
                    wf.NodeVersion = d.Version;
                    wf.FileType = filetype;
                    wf.CreatedBy = m.Id;
                    wf.Downloads = 0;
                    wf.Archived = false;
                    wf.Versions = versions;
                    wf.Version = versions[0];
                    wf.Verified = false;

                    string path = "/media/wiki/" + d.Id.ToString();

                    if (!System.IO.Directory.Exists(HttpContext.Current.Server.MapPath(path)))
                        System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                    string filename = name;
                    string extension = ext;

                    filename = filename.Substring(0, filename.LastIndexOf('.') + 1);
                    path = path + "/" + DateTime.Now.Ticks.ToString() + "_" + umbraco.cms.helpers.url.FormatUrl(filename) + "." + extension;

                    System.IO.FileStream fs1 = null;
                    fs1 = new FileStream( HttpContext.Current.Server.MapPath(path) , FileMode.Create);
                    fs1.Write(file, 0, file.Length);
                    fs1.Close();
                    fs1 = null;

                    wf.Path = path;
                    wf.Save();

                    return wf;
                }


            }
            catch (Exception ex)
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());

            }

            return null;
        }

        public void Save() {
            if (Id == 0) {
                    FileCreateEventArgs e = new FileCreateEventArgs();
                    FireBeforeCreate(e);
                    if (!e.Cancel) {

                        Data.SqlHelper.ExecuteNonQuery("INSERT INTO wikiFiles (path, name, createdBy, nodeId, version, type, downloads, archived, umbracoVersion, verified) VALUES(@path, @name, @createdBy, @nodeId, @nodeVersion, @type, @downloads, @archived, @umbracoVersion, @verified)",
                            Data.SqlHelper.CreateParameter("@path", Path),
                            Data.SqlHelper.CreateParameter("@name", Name),
                            Data.SqlHelper.CreateParameter("@createdBy", CreatedBy),
                            Data.SqlHelper.CreateParameter("@nodeId", NodeId),
                            Data.SqlHelper.CreateParameter("@type", FileType),
                            Data.SqlHelper.CreateParameter("@nodeVersion", NodeVersion),
                            Data.SqlHelper.CreateParameter("@downloads", Downloads),
                            Data.SqlHelper.CreateParameter("@archived", Archived),
                            Data.SqlHelper.CreateParameter("@umbracoVersion", ToVersionString(Versions)),
                            Data.SqlHelper.CreateParameter("@verified", Verified)
                            );
                      

                        CreateDate = DateTime.Now;


                        Id = Data.SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM wikiFiles WHERE createdBy = @createdBy", Data.SqlHelper.CreateParameter("@createdBy", CreatedBy));


                        FireAfterCreate(e);

                    }
                

            } else {

                FileUpdateEventArgs e = new FileUpdateEventArgs();
                FireBeforeUpdate(e);

                if (!e.Cancel) {
                    Data.SqlHelper.ExecuteNonQuery("UPDATE wikiFiles SET path = @path, name = @name, type = @type, [current] = @current, removedBy = @removedBy, version = @version, downloads = @downloads, archived = @archived, umbracoVersion = @umbracoVersion, verified = @verified WHERE id = @id",
                        Data.SqlHelper.CreateParameter("@path", Path),
                        Data.SqlHelper.CreateParameter("@name", Name),
                        Data.SqlHelper.CreateParameter("@type", FileType),
                        Data.SqlHelper.CreateParameter("@current", Current),
                        Data.SqlHelper.CreateParameter("@removedBy", RemovedBy),
                        Data.SqlHelper.CreateParameter("@version", NodeVersion),
                        Data.SqlHelper.CreateParameter("@id", Id),
                        Data.SqlHelper.CreateParameter("@downloads", Downloads),
                        Data.SqlHelper.CreateParameter("@archived", Archived),
                        Data.SqlHelper.CreateParameter("@umbracoVersion", ToVersionString(Versions)),
                        Data.SqlHelper.CreateParameter("@verified",Verified)
                        );
                    FireAfterUpdate(e);
                }
            }
        }



        //available umbraco version strings: v4, v45, v3
        public static WikiFile FindPackageForUmbracoVersion(int nodeid, string umbracoVersion)
        {
            return FindRelatedFileForUmbracoVersion(nodeid, umbracoVersion, "package");
        }

        public static WikiFile FindPackageDocumentationForUmbracoVersion(int nodeid, string umbracoVersion)
        {
            return FindRelatedFileForUmbracoVersion(nodeid, umbracoVersion, "docs");
        }

        private static WikiFile FindRelatedFileForUmbracoVersion(int nodeid, string umbracoVersion, string fileType)
        {
            try
            {
                int id = 0;

                //try find one based on the specific version first
                id = Application.SqlHelper.ExecuteScalar<int>(
                    "Select TOP 1 id from wikiFiles where nodeId = @nodeid and type = @packageType and (umbracoVersion like @umbracoVersion) ORDER BY createDate DESC",
                    Application.SqlHelper.CreateParameter("@nodeid", nodeid),
                    Application.SqlHelper.CreateParameter("@packageType", fileType),
                    Application.SqlHelper.CreateParameter("@umbracoVersion", "%" + umbracoVersion + "%")
                    );

                if (id > 0)
                    return new WikiFile(id);

                //if a version specific file wasnt found try and find one based on nan
                id = Application.SqlHelper.ExecuteScalar<int>(
                    "Select TOP 1 id from wikiFiles where nodeId = @nodeid and type = @packageType and (umbracoVersion like '%nan%') ORDER BY createDate DESC",
                    Application.SqlHelper.CreateParameter("@nodeid", nodeid),
                    Application.SqlHelper.CreateParameter("@packageType", fileType)
                    );

                if (id > 0)
                    return new WikiFile(id);

            }
            catch
            {
                return null;
            }

            return null;
        }

        public WikiFile(int id) {
            umbraco.DataLayer.IRecordsReader dr = Data.SqlHelper.ExecuteReader("SELECT * FROM wikiFiles WHERE id = " + id.ToString());

            if (dr.Read()) {
                Id = dr.GetInt("id");
                Path = dr.GetString("path");
                Name = dr.GetString("name");
                FileType = dr.GetString("type");
                RemovedBy = dr.GetInt("removedBy");
                CreatedBy = dr.GetInt("createdBy");
                NodeVersion = dr.GetGuid("version");
                NodeId = dr.GetInt("nodeId");
                CreateDate = dr.GetDateTime("createDate");
                Current = dr.GetBoolean("current");
                Downloads = dr.GetInt("downloads");
                Archived = dr.GetBoolean("archived");
                Verified = dr.GetBoolean("verified");
                Versions = GetVersionsFromString(dr.GetString("umbracoVersion"));
                Version = Versions.Any() ? GetVersionsFromString(dr.GetString("umbracoVersion"))[0] : UmbracoVersion.DefaultVersion();
            } else
                throw new ArgumentException(string.Format("No node exists with id '{0}'", Id));

            dr.Close();

        }

        public void UpdateDownloadCounter()
        {
            UpdateDownloadCount(this.Id, false,true);
        }

        public void UpdateDownloadCounter(bool ignoreCookies, bool isPackage)
        {
            UpdateDownloadCount(this.Id, ignoreCookies, isPackage);
        }

        public static List<UmbracoVersion> GetVersionsFromString(string p)
        {
            var verArray = p.Split(',');
            var verList = new List<UmbracoVersion>();
            foreach (var ver in verArray)
            {
                if (UmbracoVersion.AvailableVersions().ContainsKey(ver))
                    verList.Add(UmbracoVersion.AvailableVersions()[ver]);
            }
            return verList;
        }

        public static string ToVersionString(List<UmbracoVersion> Versions)
        {
            var stringVers = string.Empty;
            foreach (var ver in Versions)
            {
                stringVers += ver.Version + ",";
            }

            return stringVers.TrimEnd(',');

        }

        public static void UpdateDownloadCount(int fileId, bool ignoreCookies, bool isPackage)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["ProjectFileDownload" + fileId];
            if (cookie == null || ignoreCookies)
            {
                int downloads = 0;
                downloads = Application.SqlHelper.ExecuteScalar<int>(
                    "Select downloads from wikiFiles where id = @id;",
                    Application.SqlHelper.CreateParameter("@id", fileId));

                downloads = downloads + 1;

                Application.SqlHelper.ExecuteNonQuery(
                    "update wikiFiles set downloads = @downloads where id = @id;",
                     Application.SqlHelper.CreateParameter("@id", fileId),
                     Application.SqlHelper.CreateParameter("@downloads", downloads));


                if (isPackage)
                {
                    int _currentMember = 0;
                    Member m = Member.GetCurrentMember();
                    if (m != null)
                        _currentMember = m.Id;

                    //update download count update
                   Application.SqlHelper.ExecuteNonQuery(
                           @"insert into projectDownload(projectId,memberId,timestamp) 
                        values((select nodeId from wikiFiles where id = @id) ,@memberId, getdate())",
                                Application.SqlHelper.CreateParameter("@id", fileId),
                                Application.SqlHelper.CreateParameter("@memberId", _currentMember));
                }

                cookie = new HttpCookie("ProjectFileDownload" + fileId);
                cookie.Expires = DateTime.Now.AddHours(1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public byte[] ToByteArray()
        {
            byte[] packageByteArray = new byte[0];
            string path = HttpContext.Current.Server.MapPath(this.Path);
            
            System.IO.FileStream fs1 = null;
            fs1 = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

            packageByteArray = new byte[fs1.Length];
            fs1.Read(packageByteArray, 0, (int)fs1.Length);

            fs1.Close();

            return packageByteArray;
        }


        public XmlNode ToXml(XmlDocument d)
        {
            XmlNode tx = d.CreateElement("wikiFile");

            

            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "id", Id.ToString()));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "name", Name));
            tx.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "created", CreateDate.ToString()));

            tx.AppendChild(umbraco.xmlHelper.addCDataNode(d, "path", Path));
            tx.AppendChild(umbraco.xmlHelper.addCDataNode(d, "verified", Verified.ToString()));

            return tx;
        }

        /* EVENTS */
        public static event EventHandler<FileCreateEventArgs> BeforeCreate;
        protected virtual void FireBeforeCreate(FileCreateEventArgs e) {
            _e.FireCancelableEvent(BeforeCreate, this, e);
        }
        public static event EventHandler<FileCreateEventArgs> AfterCreate;
        protected virtual void FireAfterCreate(FileCreateEventArgs e) {
            if (AfterCreate != null)
                AfterCreate(this, e);
        }

        public static event EventHandler<FileRemoveEventArgs> BeforeRemove;
        protected virtual void FireBeforeDelete(FileRemoveEventArgs e) {
            _e.FireCancelableEvent(BeforeRemove, this, e);
        }
        public static event EventHandler<FileRemoveEventArgs> AfterRemove;
        protected virtual void FireAfterDelete(FileRemoveEventArgs e) {
            if (AfterRemove != null)
                AfterRemove(this, e);
        }

        public static event EventHandler<FileUpdateEventArgs> BeforeUpdate;
        protected virtual void FireBeforeUpdate(FileUpdateEventArgs e) {
            _e.FireCancelableEvent(BeforeUpdate, this, e);
        }
        public static event EventHandler<FileUpdateEventArgs> AfterUpdate;
        protected virtual void FireAfterUpdate(FileUpdateEventArgs e) {
            if (AfterUpdate != null)
                AfterUpdate(this, e);
        }

    }
}
