using System;
using System.Collections.Generic;
using System.Web;
using OurUmbraco.Wiki.BusinessLogic;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IMediaProvider
    {
        IEnumerable<IMediaFile> GetMediaForProjectByType(int projectId,FileType type);
        List<WikiFile> GetMediaFilesByProjectId(int projectId);
        void SaveOrUpdate(WikiFile file);
        WikiFile GetFileById(int fileId);
        void Remove(WikiFile file);
        WikiFile CreateFile(string fileName, Guid listingVersionGuid, Guid vendorGuid, HttpPostedFile file, FileType fileType, List<UmbracoVersion> versions, string dotNetVersion, bool mediumTrust);
    }
}
