using System;
using System.Collections.Generic;
using OurUmbraco.Wiki.BusinessLogic;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IMediaFile
    {
      int Id { get; set; }
      string Name { get; set; }
      string Path { get; set; }
      FileType FileType { get; set; }
      Guid FileVersion { get; set; }
      //Guid ProjectGuid { get; set; }
      int CreatedBy { get; set; }
      int ListingItemId { get; set; }
      DateTime CreateDate { get; set; }
      Boolean Current { get; set; }
      int RemovedBy { get; set; }
      int Downloads { get; set; }
      Boolean Archived { get; set; }
      List<UmbracoVersion> UmbVersion { get; set; }
      String DotNetVersion { get; set; }
      Boolean SupportsMediumTrust { get; set; }
      Boolean Verified { get; set; }
    }
}
