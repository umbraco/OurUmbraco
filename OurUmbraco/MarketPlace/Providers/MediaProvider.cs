using System;
using System.Collections.Generic;
using System.Web;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.Models;
using OurUmbraco.Wiki.BusinessLogic;

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
            var MediaFile = GetFileById(uWikiFile.Id);
            MediaFile.DotNetVersion = dotNetVersion;
            SaveOrUpdate(MediaFile);
            return MediaFile;
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
}
