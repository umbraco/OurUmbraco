using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using OurUmbraco.Wiki.Extensions;
using OurUmbraco.Wiki.Models;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace OurUmbraco.Wiki.BusinessLogic
{
    public class WikiFile
    {
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
        public bool SupportsMediumTrust { get { return false; } }

        public List<UmbracoVersion> Versions { get; set; }
        public UmbracoVersion Version { get; set; }

        public string MinimumVersionStrict { get; set; }


        public void Delete()
        {
            if (File.Exists(HttpContext.Current.Server.MapPath(Path)))
                File.Delete(HttpContext.Current.Server.MapPath(Path));

            using (var sqlHelper = Application.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery("DELETE FROM wikiFiles where ID = @id",
                    sqlHelper.CreateParameter("@id", Id));
            }
        }

        public static IDictionary<int, MonthlyProjectDownloads> GetMonthlyDownloadStatsByProject(int projectId, DateTime from)
        {
            var sql = @"SELECT COUNT(*) count, DATEPART(m,[timestamp]) mon, YEAR([timestamp]) yr, projectId
FROM [projectDownload]
WHERE timestamp > @from";
            if (projectId > 0)
            {
                sql += " AND projectId = @projectId";
            }
            sql += @" GROUP BY DATEPART(m,[timestamp]), YEAR([timestamp]), projectId ORDER BY projectId, yr, mon";

            var result = new Dictionary<int, MonthlyProjectDownloads>();
            //Query is a forward read cursor so we won't allocate this all to memory twice
            var query = ApplicationContext.Current.DatabaseContext.Database.Query<dynamic>(
                sql, 
                projectId > 0 ? (object)new { from = from, projectId = projectId } : new { from = from });

            foreach (var q in query)
            {
                MonthlyProjectDownloads stats;
                if (result.TryGetValue(q.projectId, out stats) == false)
                {
                    stats = new MonthlyProjectDownloads();
                    result[q.projectId] = stats;
                }
                stats.AddMonthlyStats(q.yr, q.mon, q.count);
            }

            return result;
        }

        public static IDictionary<int, MonthlyProjectDownloads> GetMonthlyDownloadStatsByProject(DateTime from)
        {
            return GetMonthlyDownloadStatsByProject(0, from);
        }

        /// <summary>
        /// This is used to determine the 'now' of project indexing which is mostly used for testing to ensure that date values are not skewed by testing against old data
        /// </summary>
        /// <returns></returns>
        public static DateTime GetMostRecentDownloadDate()
        {
            return ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<DateTime>("SELECT MAX([timestamp]) FROM projectDownload");
        }

        /// <summary>
        /// Gets all wiki files for all nodes
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, IEnumerable<WikiFile>> CurrentFiles(IEnumerable<int> nodeIds)
        {
            var wikiFiles = new Dictionary<int, List<WikiFile>>();

            //we can only have 2000 (actually 2100) SQL parameters used at once, so we need to group them
            var nodeBatches = nodeIds.InGroupsOf(2000);

            foreach (var nodeBatch in nodeBatches)
            {
                foreach (var result in ApplicationContext.Current.DatabaseContext.Database.Query<dynamic>("SELECT * FROM wikiFiles WHERE nodeId IN (@nodeIds)", new { nodeIds = nodeBatch }))
                {
                    var file = new WikiFile
                    {
                        Id = result.id,
                        Path = result.path,
                        Name = result.name,
                        FileType = result.type,
                        RemovedBy = result.removedBy ?? 0,
                        CreatedBy = result.createdBy,
                        NodeVersion = result.version,
                        NodeId = result.nodeId,
                        CreateDate = result.createDate,
                        DotNetVersion = result.dotNetVersion,
                        Downloads = result.downloads,
                        Archived = result.archived,
                        Verified = result.verified,
                        Versions = GetVersionsFromString(result.umbracoVersion),
                        MinimumVersionStrict = result.minimumVersionStrict
                    };

                    file.Version = file.Versions.Any()
                        ? GetVersionsFromString(result.umbracoVersion)[0]
                        : UmbracoVersion.DefaultVersion();

                    if (wikiFiles.ContainsKey(result.nodeId))
                    {
                        var list = wikiFiles[result.nodeId];
                        list.Add(file);
                    }
                    else
                    {
                        wikiFiles.Add(result.nodeId, new List<WikiFile>(new[] { file }));
                    }
                }
            }

            return wikiFiles.ToDictionary(x => x.Key, x => (IEnumerable<WikiFile>)x.Value);

        }

        public string DotNetVersion { get; set; }

        public static List<WikiFile> CurrentFiles(int nodeId)
        {
            var wikiFiles = new List<WikiFile>();
            var wikiFileIds = new List<int>();
            using (var sqlHelper = Application.SqlHelper)
            using (var reader = sqlHelper.ExecuteReader("SELECT id FROM wikiFiles WHERE nodeId = @nodeid", sqlHelper.CreateParameter("@nodeId", nodeId)))
            {
                {
                    while (reader.Read())
                    {
                        var wikiFileId = reader.GetInt("id");
                        if(wikiFileIds.Contains(wikiFileId) == false)
                            wikiFileIds.Add(wikiFileId);
                    }
                }
            }

            foreach (var wikiFileId in wikiFileIds)
            {
                var wikiFile = new WikiFile(wikiFileId);
                if (wikiFiles.Contains(wikiFile) == false)
                    wikiFiles.Add(wikiFile);
            }

            return wikiFiles;
        }

        private readonly Events _events = new Events();

        private WikiFile() { }

        public static WikiFile Create(string name, Guid node, Guid memberGuid, HttpPostedFile file, string filetype, List<UmbracoVersion> versions, string dotNetVersion)
        {
            try
            {
                var filename = file.FileName;
                var extension = filename.Substring(filename.LastIndexOf('.') + 1);

                if (ExtensionNotAllowed(extension))
                    return null;

                var content = Content.GetContentFromVersion(node);

                var member = new Member(memberGuid);

                if (content != null)
                {
                    var wikiFile = new WikiFile
                    {
                        Name = name,
                        NodeId = content.Id,
                        NodeVersion = content.Version,
                        FileType = filetype,
                        CreatedBy = member.Id,
                        Downloads = 0,
                        Archived = false,
                        Versions = versions,
                        Version = versions[0],
                        Verified = false,
                        DotNetVersion = dotNetVersion
                    };

                    var path = string.Format("/media/wiki/{0}", content.Id);

                    if (Directory.Exists(HttpContext.Current.Server.MapPath(path)) == false)
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                    path = string.Format("{0}/{1}_{2}.{3}", path, DateTime.Now.Ticks, umbraco.cms.helpers.url.FormatUrl(filename), extension);

                    file.SaveAs(HttpContext.Current.Server.MapPath(path));

                    wikiFile.Path = path;

                    // Note: make sure to do this AFTER setting the path, else it will fail
                    wikiFile.SetMinimumUmbracoVersion();

                    wikiFile.Save();

                    return wikiFile;
                }

            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Debug, -1, ex.ToString());
            }

            return null;
        }
        public static WikiFile Create(string name, Guid node, Guid memberGuid, HttpPostedFile file, string filetype, List<UmbracoVersion> versions)
        {
            return Create(name, node, memberGuid, file, filetype, versions, string.Empty);
        }

        private static bool ExtensionNotAllowed(string extension)
        {
            return UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Contains(extension.ToLowerInvariant());
        }

        public static WikiFile Create(string fileName, string extension, Guid node, Guid memberGuid, byte[] file, string filetype, List<UmbracoVersion> versions)
        {
            return Create(fileName, extension, node, memberGuid, file, filetype, versions, null);
        }

        public static WikiFile Create(string fileName, string extension, Guid node, Guid memberGuid, byte[] file, string filetype, List<UmbracoVersion> versions, string dotNetVersion)
        {
            try
            {
                if (ExtensionNotAllowed(extension))
                    return null;

                var content = Content.GetContentFromVersion(node);
                var member = new Member(memberGuid);

                if (content != null)
                {
                    var wikiFile = new WikiFile();

                    if (dotNetVersion != null)
                    {
                        wikiFile.DotNetVersion = dotNetVersion;
                    }
                    
                    wikiFile.Name = fileName;
                    wikiFile.NodeId = content.Id;
                    wikiFile.NodeVersion = content.Version;
                    wikiFile.FileType = filetype;
                    wikiFile.CreatedBy = member.Id;
                    wikiFile.Downloads = 0;
                    wikiFile.Archived = false;
                    wikiFile.Versions = versions;
                    wikiFile.Version = versions[0];
                    wikiFile.Verified = false;

                    var path = string.Format("/media/wiki/{0}", content.Id);

                    if (Directory.Exists(HttpContext.Current.Server.MapPath(path)) == false)
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                    fileName = fileName.Substring(0, fileName.LastIndexOf('.') + 1);
                    path = string.Format("{0}/{1}_{2}.{3}", path, DateTime.Now.Ticks, umbraco.cms.helpers.url.FormatUrl(fileName), extension);

                    using (var fileStream = new FileStream(HttpContext.Current.Server.MapPath(path), FileMode.Create))
                        fileStream.Write(file, 0, file.Length);

                    wikiFile.Path = path;
                    wikiFile.SetMinimumUmbracoVersion();
                    wikiFile.Save();

                    return wikiFile;
                }
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Debug, -1, ex.ToString());
            }

            return null;
        }

        public void Save()
        {
            if (Id == 0)
            {
                var e = new FileCreateEventArgs();
                FireBeforeCreate(e);
                if (e.Cancel)
                    return;

                using (var sqlHelper = Application.SqlHelper)
                {
                    sqlHelper.ExecuteNonQuery(
                        "INSERT INTO wikiFiles (path, name, createdBy, createDate, [current], nodeId, version, type, downloads, archived, umbracoVersion, verified, dotNetVersion, minimumVersionStrict) VALUES(@path, @name, @createdBy, @createDate, @current, @nodeId, @nodeVersion, @type, @downloads, @archived, @umbracoVersion, @verified, @dotNetVersion, @minimumVersionStrict)",
                        sqlHelper.CreateParameter("@path", Path),
                        sqlHelper.CreateParameter("@name", Name),
                        sqlHelper.CreateParameter("@createdBy", CreatedBy),
                        sqlHelper.CreateParameter("@createDate", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")),
                        
                        // Note: for some reason this has to be set to 1 else package uploads will fail
                        // Interestingly, this property is never actually used, we use the currentFile
                        // Note2: HACK! This is stored as a type `bit` and you need to pass "1" instead of a boolean 🤷‍♂️
                        sqlHelper.CreateParameter("@current", "1"),
                        
                        sqlHelper.CreateParameter("@nodeId", NodeId),
                        sqlHelper.CreateParameter("@type", FileType),
                        sqlHelper.CreateParameter("@nodeVersion", NodeVersion),
                        sqlHelper.CreateParameter("@downloads", Downloads),
                        sqlHelper.CreateParameter("@archived", Archived),
                        sqlHelper.CreateParameter("@umbracoVersion", ToVersionString(Versions)),
                        sqlHelper.CreateParameter("@verified", Verified),
                        sqlHelper.CreateParameter("@dotNetVersion",
                            string.IsNullOrWhiteSpace(DotNetVersion) ? "" : DotNetVersion),
                        sqlHelper.CreateParameter("@minimumVersionStrict",
                            string.IsNullOrWhiteSpace(MinimumVersionStrict) ? "" : MinimumVersionStrict)
                    );
                    
                    Id = sqlHelper.ExecuteScalar<int>(
                            "SELECT MAX(id) FROM wikiFiles WHERE createdBy = @createdBy",
                            sqlHelper.CreateParameter("@createdBy", CreatedBy));
                }
                FireAfterCreate(e);
            }
            else
            {
                var e = new FileUpdateEventArgs();
                FireBeforeUpdate(e);

                if (e.Cancel)
                    return;

                using (var sqlHelper = Application.SqlHelper)
                {
                    sqlHelper.ExecuteNonQuery(
                        "UPDATE wikiFiles SET path = @path, name = @name, type = @type, [current] = @current, removedBy = @removedBy, version = @version, downloads = @downloads, archived = @archived, umbracoVersion = @umbracoVersion, verified = @verified, dotNetVersion = @dotNetVersion, minimumVersionStrict = @minimumVersionStrict WHERE id = @id",
                        sqlHelper.CreateParameter("@path", Path),
                        sqlHelper.CreateParameter("@name", Name),
                        sqlHelper.CreateParameter("@type", FileType),
                        sqlHelper.CreateParameter("@current", Current),
                        sqlHelper.CreateParameter("@removedBy", RemovedBy),
                        sqlHelper.CreateParameter("@version", NodeVersion),
                        sqlHelper.CreateParameter("@id", Id),
                        sqlHelper.CreateParameter("@downloads", Downloads),
                        sqlHelper.CreateParameter("@archived", Archived),
                        sqlHelper.CreateParameter("@umbracoVersion", ToVersionString(Versions)),
                        sqlHelper.CreateParameter("@verified", Verified),
                        sqlHelper.CreateParameter("@dotNetVersion",
                            string.IsNullOrWhiteSpace(DotNetVersion) ? "" : DotNetVersion),
                        sqlHelper.CreateParameter("@minimumVersionStrict",
                            string.IsNullOrWhiteSpace(MinimumVersionStrict) ? "" : MinimumVersionStrict)
                    );
                }
                FireAfterUpdate(e);
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
                using (var sqlHelper = Application.SqlHelper)
                {
                    //try find one based on the specific version first
                    var id = sqlHelper.ExecuteScalar<int>(
                        "Select TOP 1 id from wikiFiles where nodeId = @nodeid and type = @packageType and (umbracoVersion like @umbracoVersion) ORDER BY createDate DESC",
                        sqlHelper.CreateParameter("@nodeid", nodeid),
                        sqlHelper.CreateParameter("@packageType", fileType),
                        sqlHelper.CreateParameter("@umbracoVersion", "%" + umbracoVersion + "%")
                    );

                    if (id > 0)
                        return new WikiFile(id);

                    //if a version specific file wasnt found try and find one based on nan
                    id = sqlHelper.ExecuteScalar<int>(
                        "Select TOP 1 id from wikiFiles where nodeId = @nodeid and type = @packageType and (umbracoVersion like '%nan%') ORDER BY createDate DESC",
                        sqlHelper.CreateParameter("@nodeid", nodeid),
                        sqlHelper.CreateParameter("@packageType", fileType)
                    );

                    if (id > 0)
                        return new WikiFile(id);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public WikiFile(int id)
        {
            using (var sqlHelper = Application.SqlHelper)
            using (var reader = sqlHelper.ExecuteReader("SELECT * FROM wikiFiles WHERE id = @fileId", sqlHelper.CreateParameter("@fileId", id)))
            {
                {
                    if (reader.Read())
                    {
                        Id = reader.GetInt("id");
                        Path = reader.GetString("path");
                        Name = reader.GetString("name");
                        FileType = reader.GetString("type");
                        RemovedBy = reader.GetInt("removedBy");
                        CreatedBy = reader.GetInt("createdBy");
                        NodeVersion = reader.GetGuid("version");
                        NodeId = reader.GetInt("nodeId");
                        CreateDate = reader.GetDateTime("createDate");
                        Current = reader.GetBoolean("current");
                        Downloads = reader.GetInt("downloads");
                        Archived = reader.GetBoolean("archived");
                        Verified = reader.GetBoolean("verified");
                        DotNetVersion = reader.GetString("dotNetVersion");
                        Versions = GetVersionsFromString(reader.GetString("umbracoVersion"));
                        Version = Versions.Any()
                            ? GetVersionsFromString(reader.GetString("umbracoVersion"))[0]
                            : UmbracoVersion.DefaultVersion();
                        MinimumVersionStrict = reader.GetString("minimumVersionStrict");
                    }
                    else
                    {
                        HttpContext.Current.Response.StatusCode = 404;
                        HttpContext.Current.Response.End();
                    }
                }
            }
        }

        public void UpdateDownloadCounter()
        {
            UpdateDownloadCount(this.Id, false, true);
        }

        public void UpdateDownloadCounter(bool ignoreCookies, bool isPackage)
        {
            UpdateDownloadCount(this.Id, ignoreCookies, isPackage);
        }

        public static List<UmbracoVersion> GetVersionsFromString(string p)
        {
            var umbracoVersions = new List<UmbracoVersion>();
            var availableVersions = UmbracoVersion.AvailableVersions();

            foreach (var ver in p.Split(','))
            {
                if (availableVersions.ContainsKey(ver))
                    umbracoVersions.Add(availableVersions[ver]);
            }

            return umbracoVersions;
        }

        public static string ToVersionString(List<UmbracoVersion> versions)
        {
            var umbracoVersions = string.Empty;

            foreach (var ver in versions)
                umbracoVersions += string.Format("{0},", ver.Version);

            return umbracoVersions.TrimEnd(',');

        }

        public void UpdateDownloadCount(int fileId, bool ignoreCookies, bool isPackage)
        {
            var cookie = HttpContext.Current.Request.Cookies["ProjectFileDownload" + fileId];

            if (cookie != null && ignoreCookies == false)
                return;

            var downloads = 0;
            var projectId = 0;

            using (var sqlHelper = Application.SqlHelper)
            {
                using (var reader = sqlHelper.ExecuteReader("Select downloads, nodeId from wikiFiles where id = @id;", sqlHelper.CreateParameter("@id", fileId)))
                {
                    if (reader.Read())
                    {
                        downloads = reader.GetInt("downloads");
                        projectId = reader.GetInt("nodeId");
                    }
                }

                downloads = downloads + 1;

                sqlHelper.ExecuteNonQuery(
                    "update wikiFiles set downloads = @downloads where id = @id;",
                    sqlHelper.CreateParameter("@id", fileId),
                    sqlHelper.CreateParameter("@downloads", downloads));

                var totalDownloads =
                    sqlHelper.ExecuteScalar<int>(
                        "Select SUM(downloads) from wikiFiles where nodeId = @projectId;",
                        sqlHelper.CreateParameter("@projectId", projectId));

                if (isPackage)
                {
                    var memberHelper = new Umbraco.Web.Security.MembershipHelper(Umbraco.Web.UmbracoContext.Current);
                    var memberId = memberHelper.GetCurrentMemberId();
                    var currentMemberId = memberId == -1 ? 0 : memberId;

                    //update download count update
                    sqlHelper.ExecuteNonQuery(
                        @"insert into projectDownload(projectId,memberId,timestamp) 
                        values((select nodeId from wikiFiles where id = @id) ,@memberId, getdate())",
                        sqlHelper.CreateParameter("@id", fileId),
                        sqlHelper.CreateParameter("@memberId", currentMemberId));
                }

                var e = new FileDownloadUpdateEventArgs { ProjectId = projectId, Downloads = totalDownloads };
                FireAfterDownloadUpdate(e);
            }

            cookie = new HttpCookie("ProjectFileDownload" + fileId) { Expires = DateTime.Now.AddHours(1) };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public byte[] ToByteArray()
        {
            var path = HttpContext.Current.Server.MapPath(this.Path);

            byte[] packageByteArray;

            if(File.Exists(path) == false)
                throw new InvalidOperationException("The file " + path + " does not exist on the server");

            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                packageByteArray = new byte[fileStream.Length];

                fileStream.Read(packageByteArray, 0, (int)fileStream.Length);
            }

            return packageByteArray;
        }

        public XmlNode ToXml(XmlDocument d)
        {
            XmlNode toXml = d.CreateElement("wikiFile");

            toXml.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "id", Id.ToString()));
            toXml.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "name", Name));
            toXml.Attributes.Append(umbraco.xmlHelper.addAttribute(d, "created", CreateDate.ToString()));

            toXml.AppendChild(umbraco.xmlHelper.addCDataNode(d, "path", Path));
            toXml.AppendChild(umbraco.xmlHelper.addCDataNode(d, "verified", Verified.ToString()));

            return toXml;
        }

        /* EVENTS */
        public static event EventHandler<FileCreateEventArgs> BeforeCreate;
        protected virtual void FireBeforeCreate(FileCreateEventArgs e)
        {
            _events.FireCancelableEvent(BeforeCreate, this, e);
        }
        public static event EventHandler<FileCreateEventArgs> AfterCreate;
        protected virtual void FireAfterCreate(FileCreateEventArgs e)
        {
            if (AfterCreate != null)
                AfterCreate(this, e);
        }

        public static event EventHandler<FileRemoveEventArgs> BeforeRemove;
        protected virtual void FireBeforeDelete(FileRemoveEventArgs e)
        {
            _events.FireCancelableEvent(BeforeRemove, this, e);
        }
        public static event EventHandler<FileRemoveEventArgs> AfterRemove;
        protected virtual void FireAfterDelete(FileRemoveEventArgs e)
        {
            if (AfterRemove != null)
                AfterRemove(this, e);
        }

        public static event EventHandler<FileUpdateEventArgs> BeforeUpdate;
        protected virtual void FireBeforeUpdate(FileUpdateEventArgs e)
        {
            _events.FireCancelableEvent(BeforeUpdate, this, e);
        }
        public static event EventHandler<FileUpdateEventArgs> AfterUpdate;
        protected virtual void FireAfterUpdate(FileUpdateEventArgs e)
        {
            if (AfterUpdate != null)
                AfterUpdate(this, e);
        }

        public static event EventHandler<FileDownloadUpdateEventArgs> AfterDownloadUpdate;
        protected virtual void FireAfterDownloadUpdate(FileDownloadUpdateEventArgs e)
        {
            if (AfterDownloadUpdate != null)
                AfterDownloadUpdate(this, e);
        }
    }
}
