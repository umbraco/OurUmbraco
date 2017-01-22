using System;
using System.Collections.Generic;

namespace OurUmbraco.MarketPlace.Interfaces
{
    //TODO: All of this data should probably be in the Lucene index! 
    // Then we can use that index for querying and returning these results. Alternatively, another DB table 
    // should be created to store all of this normalized data.
    public interface IListingItem
    {
        int Id { get; set; }
        string NiceUrl { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string CurrentVersion { get; set; }
        string CurrentReleaseFile { get; set; }
        string DefaultScreenshot { get; set; }
        string DevelopmentStatus { get; set; }
        IEnumerable<IProjectTag> Tags { get;set; }
        int CategoryId { get; set; }
        string Logo { get; set; }
        string LicenseName { get; set; }
        string LicenseUrl { get; set; }
        ListingType ListingType { get; set; }
        string SupportUrl { get; set; }
        IEnumerable<IMediaFile> PackageFile { get; set; }
        IEnumerable<IMediaFile> HotFixes { get; set; }
        IEnumerable<IMediaFile> SourceFile { get; set; }
        IEnumerable<IMediaFile> DocumentationFile { get; set; }
        IVendor Vendor { get; set; }
        int VendorId { get; set; }
        double Price { get; set; }
        string GACode { get; set; }
        IEnumerable<IMediaFile> ScreenShots { get; set; }
        string DemonstrationUrl { get; set; }
        string[] UmbracoVerionsSupported { get; set; }
        string[] NETVersionsSupported { get; set; }
        TrustLevel TrustLevelSupported { get; set; }
        DateTime TermsAgreementDate { get; set; }
        DateTime CreateDate { get; set; }
        Guid ProjectGuid { get; set; }
        bool Stable { get; set; }
        bool Approved { get; set; }
        bool StarterKitModule { get; set; }
        bool NotAPackage { get; set; }
        bool OpenForCollab { get; set; }
        bool Live { get; set; }
        bool Disabled { get; set; }
        string SourceCodeUrl { get; set; }
        string NuGetPackageUrl { get; set; }
        int Downloads { get; }
        int Karma { get; }
        string ProjectUrl { get; set; }
        Guid Version { get; }
        string LicenseKey { get; set; }
        int ProjectViews { get; }
    }
}
