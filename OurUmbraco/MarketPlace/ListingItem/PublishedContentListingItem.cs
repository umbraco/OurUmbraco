using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using OurUmbraco.MarketPlace.Extensions;
using OurUmbraco.MarketPlace.Interfaces;
using umbraco;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace OurUmbraco.MarketPlace.ListingItem
{
    /// <summary>
    /// A listing item that is assocated with a published content item
    /// </summary>
    [Serializable]
    public class PublishedContentListingItem : IListingItem
    {
        public PublishedContentListingItem(IPublishedContent content)
        {
            if (content == null) throw new ArgumentNullException("content");
            PublishedContent = content;

            //set simple properties based on the content item
            Id = content.Id;
            NiceUrl = library.NiceUrl(Id);
            Name = content.Name;
            Description = content.GetPropertyValue<string>("description", "");
            CurrentVersion = content.GetPropertyValue<string>("version", "");
            CurrentReleaseFile = content.GetPropertyValue<string>("file", "");
            DefaultScreenshot = content.GetPropertyValue<string>("defaultScreenshotPath", "");
            DevelopmentStatus = content.GetPropertyValue<string>("status", "");
            ListingType = content.GetPropertyAsListingType("listingType");
            GACode = content.GetPropertyValue<string>("gaCode", "");
            CategoryId = content.GetPropertyValue<int>("category");
            Stable = content.GetPropertyValue<bool>("stable");
            Live = content.GetPropertyValue<bool>("projectLive");
            LicenseName = content.GetPropertyValue<string>("licenseName", "");
            LicenseUrl = content.GetPropertyValue<string>("licenseUrl", "");
            ProjectUrl = content.GetPropertyValue<string>("websiteUrl", "");
            SupportUrl = content.GetPropertyValue<string>("supportUrl", "");
            SourceCodeUrl = content.GetPropertyValue<string>("sourceUrl", "");
            DemonstrationUrl = content.GetPropertyValue<string>("demoUrl", "");
            OpenForCollab = content.GetPropertyValue<bool>("openForCollab", false);
            NotAPackage = content.GetPropertyValue<bool>("notAPackage", false);
            ProjectGuid = new Guid(content.GetPropertyValue<string>("packageGuid"));
            Approved = content.GetPropertyValue<bool>("approved", false);
            UmbracoVerionsSupported = content.GetPropertyValue<string>("compatibleVersions", "").Split(';');
            NETVersionsSupported = (content.GetPropertyValue<string>("dotNetVersion", "") != null) ? content.GetPropertyValue<string>("dotNetVersion", "").Split(';') : "".Split(';');
            TrustLevelSupported = content.GetPropertyAsTrustLevel("trustLevelSupported");
            TermsAgreementDate = content.GetPropertyValue<DateTime>("termsAgreementDate");
            CreateDate = content.CreateDate;
            VendorId = content.GetPropertyValue<int>("owner");
            Logo = content.GetPropertyValue<string>("logo", "");
            LicenseKey = content.GetPropertyValue<string>("licenseKey", "");
        }

        public PublishedContentListingItem()
        {
            
        }

        public IPublishedContent PublishedContent { get; set; }

        public int Id { get; set; }
        public string NiceUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CurrentVersion { get; set; }
        public string CurrentReleaseFile { get; set; }
        public string DefaultScreenshot { get; set; }
        public string DevelopmentStatus { get; set; }

        [XmlIgnore]
        public IEnumerable<IProjectTag> Tags { get; set; }
        public int CategoryId { get; set; }
        public string Logo { get; set; }
        public string LicenseName { get; set; }
        public string LicenseUrl { get; set; }
        public ListingType ListingType { get; set; }
        public string SupportUrl { get; set; }

        [XmlIgnore]
        public IEnumerable<IMediaFile> PackageFile { get; set; }

        [XmlIgnore]
        public IEnumerable<IMediaFile> HotFixes { get; set; }

        [XmlIgnore]
        public IEnumerable<IMediaFile> SourceFile { get; set; }

        [XmlIgnore]
        public IEnumerable<IMediaFile> DocumentationFile { get; set; }

        [XmlIgnore]
        public IVendor Vendor { get; set; }
        public int VendorId { get; set; }
        public double Price { get; set; }
        public string GACode { get; set; }

        [XmlIgnore]
        public IEnumerable<IMediaFile> ScreenShots { get; set; }
        public string DemonstrationUrl { get; set; }
        public string[] UmbracoVerionsSupported { get; set; }
        public string[] NETVersionsSupported { get; set; }
        public TrustLevel TrustLevelSupported { get; set; }
        public DateTime TermsAgreementDate { get; set; }
        public Guid ProjectGuid { get; set; }
        public bool StarterKitModule { get; set; }
        public bool NotAPackage { get; set; }
        public bool OpenForCollab { get; set; }
        public string SourceCodeUrl { get; set; }
        public bool Live { get; set; }
        public bool Stable { get; set; }
        public bool Approved { get; set; }
        public int Downloads { get; set; }
        public bool Disabled { get; set; }
        public int Karma { get; set; }
        public string ProjectUrl { get; set; }
        public string LicenseKey { get; set; }
        public int ProjectViews { get; set; }
        public Guid Version { get; set; }
        public DateTime CreateDate { get; set; }
        
    }
}
