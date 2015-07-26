using System;
using System.Collections.Generic;
using System.Web;
using OurUmbraco.Wiki.BusinessLogic;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IMediaProvider
    {
        IEnumerable<IMediaFile> GetMediaForProjectByType(int projectId,FileType type);
        IEnumerable<IMediaFile> GetMediaFilesByProjectId(int projectId);
        void SaveOrUpdate(IMediaFile file);
        IMediaFile GetFileById(int fileId);
        void Remove(IMediaFile file);
        IMediaFile CreateFile(string fileName, Guid listingVersionGuid, Guid vendorGuid, HttpPostedFile file, FileType fileType, List<UmbracoVersion> versions, string dotNetVersion, bool mediumTrust);
    }
}
