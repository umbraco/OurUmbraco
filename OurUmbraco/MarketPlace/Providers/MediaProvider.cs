using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using ICSharpCode.SharpZipLib.Zip;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.Models;
using OurUmbraco.Wiki.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace OurUmbraco.MarketPlace.Providers
{
    public class MediaProvider : IMediaProvider
    {
        public IEnumerable<IMediaFile> GetMediaForProjectByType(int projectId, FileType type)
        {
            var wikiFiles = WikiFile.CurrentFiles(projectId);

            var mediaFiles = new List<MediaFile>();

            foreach (var wikiFile in wikiFiles)
            {
                var mediaFile = new MediaFile
                {
                    Current = wikiFile.Current,
                    Archived = wikiFile.Archived,
                    CreateDate = wikiFile.CreateDate,
                    Name = wikiFile.Name,
                    Id = wikiFile.Id,
                    CreatedBy = wikiFile.CreatedBy,
                    DotNetVersion = wikiFile.DotNetVersion,
                    Downloads = wikiFile.Downloads,
                    FileType = (FileType)Enum.Parse(typeof(FileType), wikiFile.FileType),
                    FileVersion = wikiFile.NodeVersion,
                    Path = wikiFile.Path,
                    RemovedBy = wikiFile.RemovedBy,
                    SupportsMediumTrust = false,
                    UmbVersion = wikiFile.Versions,
                    Verified = wikiFile.Verified
                };

                if (mediaFiles.Contains(mediaFile) == false)
                    mediaFiles.Add(mediaFile);
            }

            return mediaFiles;
        }
        /// <summary>
        /// Get all the files that are associated with this project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<WikiFile> GetMediaFilesByProjectId(int projectId)
        {
            return WikiFile.CurrentFiles(projectId);
        }

        /// <summary>
        /// Get an media file by its Id
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public WikiFile GetFileById(int fileId)
        {
            return new WikiFile(fileId);
        }

        /// <summary>
        /// Save or update a media file back to the database.
        /// </summary>
        /// <param name="file"></param>
        public void SaveOrUpdate(WikiFile file)
        {
            var wf = file;
            wf.Save();
        }

        private static string GetFileTypeAsString(FileType file)
        {
            switch (file)
            {
                case FileType.screenshot:
                    return "screenshot";
                    break;
                case FileType.package:
                    return "package";
                    break;
                case FileType.hotfix:
                    return "hotfix";
                    break;
                case FileType.docs:
                    return "docs";
                    break;
                case FileType.source:
                    return "source";
                    break;
                default:
                    return "package";
                    break;
            }
        }

        public void Remove(WikiFile file)
        {
            file.Delete();
        }

        public WikiFile CreateFile(string fileName, Guid listingVersionGuid, Guid vendorGuid, HttpPostedFile file, FileType fileType, List<UmbracoVersion> v, string dotNetVersion, bool mediumTrust)
        {
            // we have to convert to the uWiki UmbracoVersion :(

            List<UmbracoVersion> vers = new List<UmbracoVersion>();

            foreach (var ver in v)
            {
                vers.Add(UmbracoVersion.AvailableVersions()[ver.Version]);
            }


            //Create the Wiki File
            var uWikiFile = WikiFile.Create(fileName, listingVersionGuid, vendorGuid, file, GetFileTypeAsString(fileType), vers);
            //return the IMediaFile

            //Convert to Deli Media file
            var mediaFile = GetFileById(uWikiFile.Id);

            // If upload is package, extract the package XML manifest and check the version number + type [LK:2016-06-12@CGRT16]
            if (fileType == FileType.package)
            {
                var minimumUmbracoVersion = GetMinimumUmbracoVersion(mediaFile);
                if (!string.IsNullOrWhiteSpace(minimumUmbracoVersion))
                {
                    mediaFile.Versions = new List<UmbracoVersion>() { new UmbracoVersion { Version = minimumUmbracoVersion } };
                }
            }

            mediaFile.DotNetVersion = dotNetVersion;
            SaveOrUpdate(mediaFile);
            return mediaFile;
        }

        private string GetMinimumUmbracoVersion(WikiFile mediaFile)
        {
            var extractor = new PackageExtraction();
            var filePath = IOHelper.MapPath(mediaFile.Path);
            var packageXml = extractor.ReadTextFileFromArchive(filePath, Constants.Packaging.PackageXmlFileName);
            if (string.IsNullOrWhiteSpace(packageXml))
            {
                return null;
            }

            var packageXmlDoc = XDocument.Parse(packageXml);

            // The XPath query will detect if the 'requirements' element has the attribute that we're looking for,
            // and if the child elements also exist. [LK:2016-06-12@CGRT16]
            var requirements = packageXmlDoc.XPathSelectElement("/umbPackage/info/package/requirements[@type='strict' and major and minor and patch]");
            if (requirements == null)
            {
                return null;
            }

            var major = requirements.Element("major").Value;
            var minor = requirements.Element("minor").Value;
            var patch = requirements.Element("patch").Value;

            return string.Join(".", new[] { major, minor, patch });
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


    }

    // [LK:2016-06-12] This code has been copied over from Umbraco v7.5.0, (coded during CGRT16)
    // Once Our has upgraded to v7.5+, then this code can be replaced with references to the core code.
    class PackageExtraction
    {
        public string ReadTextFileFromArchive(string packageFilePath, string fileToRead)
        {
            string retVal = null;
            bool fileFound = false;
            string foundDir = null;

            ReadZipfileEntries(packageFilePath, (entry, stream) =>
            {
                string fileName = Path.GetFileName(entry.Name);

                if (string.IsNullOrEmpty(fileName) == false &&
                    fileName.Equals(fileToRead, StringComparison.CurrentCultureIgnoreCase))
                {

                    foundDir = entry.Name.Substring(0, entry.Name.Length - fileName.Length);
                    fileFound = true;
                    using (var reader = new StreamReader(stream))
                    {
                        retVal = reader.ReadToEnd();
                        return false;
                    }
                }
                return true;
            });

            if (fileFound == false)
            {
                throw new FileNotFoundException(string.Format("Could not find file in package {0}", packageFilePath), fileToRead);
            }

            return retVal;
        }

        private static void CheckPackageExists(string packageFilePath)
        {
            if (string.IsNullOrEmpty(packageFilePath))
            {
                throw new ArgumentNullException("packageFilePath");
            }

            if (File.Exists(packageFilePath) == false)
            {
                if (File.Exists(packageFilePath) == false)
                    throw new ArgumentException(string.Format("Package file: {0} could not be found", packageFilePath));
            }

            string extension = Path.GetExtension(packageFilePath).ToLower();

            var alowedExtension = new[] { ".umb", ".zip" };

            // Check if the file is a valid package
            if (alowedExtension.All(ae => ae.Equals(extension) == false))
            {
                throw new ArgumentException(
                    string.Format("Error - file isn't a package. only extentions: \"{0}\" is allowed", string.Join(", ", alowedExtension)));
            }
        }

        private void ReadZipfileEntries(string packageFilePath, Func<ZipEntry, ZipInputStream, bool> entryFunc, bool skipsDirectories = true)
        {
            CheckPackageExists(packageFilePath);

            using (var fs = File.OpenRead(packageFilePath))
            {
                using (var zipInputStream = new ZipInputStream(fs))
                {
                    ZipEntry zipEntry;
                    while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (zipEntry.IsDirectory && skipsDirectories) continue;
                        if (entryFunc(zipEntry, zipInputStream) == false) break;
                    }

                    zipInputStream.Close();
                }
                fs.Close();
            }
        }
    }
}
