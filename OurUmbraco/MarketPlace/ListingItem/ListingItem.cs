using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using OurUmbraco.MarketPlace.Interfaces;
using umbraco.cms.businesslogic.web;

namespace OurUmbraco.MarketPlace.ListingItem
{
    [Serializable]
    public class ListingItem : IListingItem
    {

        #region IListingItem Properties

        protected int _id;
        protected string _niceUrl;
        protected string _name;
        protected string _description;
        protected string _currentVersion;
        protected string _currentReleaseFile;
        protected string _defaultScreenshot;
        protected string _developmentStatus;
        protected IEnumerable<IProjectTag> _tags;
        protected int _category;
        protected string _logo;
        protected string _licenseType;
        protected string _licenseName;
        protected string _licenseUrl;
        protected ListingType _listingType;
        protected string _supportUrl;
        protected IEnumerable<IMediaFile> _packageFile;
        protected IEnumerable<IMediaFile> _hotFixes;
        protected IEnumerable<IMediaFile> _sourceFile;
        protected IEnumerable<IMediaFile> _documentationFile;
        protected IEnumerable<IMediaFile> _screenShots;
        protected IVendor _vendor;
        protected int _vendorId;
        protected double _price;
        protected string _gaCode;
        protected string _demonstrationUrl;
        protected string[] _umbracoVerionsSupported;
        protected string[] _netVersionsSupported;
        protected TrustLevel _trustLevelSupported;
        protected DateTime _termsAgreementDate;
        protected DateTime _createDate;
        protected Guid _packageGuid;
        protected bool _starterKitModule;
        protected bool _notAPackage;
        protected bool _openForCollab;
        protected string _sourceCodeUrl;
        protected bool _live;
        protected bool _stable;
        protected bool _approved;
        protected Func<int, int> _downloads;
        protected bool _disabled;
        protected Func<int, int> _karma;
        protected string _projectUrl;
        protected string _licenseKey;
        protected Func<Guid, int> _projectViews;

        #endregion

        #region IListingItem Members

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string NiceUrl
        {
            get { return _niceUrl; }
            set { _niceUrl = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }
        public string CurrentVersion
        {
            get
            {
                return this._currentVersion;
            }
            set
            {
                this._currentVersion = value;
            }
        }

        public string CurrentReleaseFile
        {
            get
            {
                return this._currentReleaseFile;
            }
            set
            {
                this._currentReleaseFile = value;
            }
        }

        public string DefaultScreenshot
        {
            get
            {
                return this._defaultScreenshot;
            }
            set
            {
                _defaultScreenshot = value;
            }
        }

        public string DevelopmentStatus
        {
            get
            {
                return _developmentStatus;
            }
            set
            {
                _developmentStatus = value;
            }
        }

        [XmlIgnore]
        public IEnumerable<IProjectTag> Tags
        {
            get
            {
                return this._tags;
            }
            set
            {
                this._tags = value;
            }
        }
        public int CategoryId
        {
            get
            {
                return this._category;
            }
            set
            {
                this._category = value;
            }
        }
        public string Logo
        {
            get
            {
                return this._logo;
            }
            set
            {
                this._logo = value;
            }
        }

        public string LicenseName
        {
            get
            {
                return this._licenseName;
            }
            set
            {
                this._licenseName = value;
            }
        }
        public string LicenseUrl
        {
            get
            {
                return this._licenseUrl;
            }
            set
            {
                this._licenseUrl = value;
            }
        }
        public ListingType ListingType
        {
            get
            {
                return this._listingType;
            }
            set
            {
                this._listingType = value;
            }
        }
        public string SupportUrl
        {
            get
            {
                return this._supportUrl;
            }
            set
            {
                this._supportUrl = value;
            }
        }

        [XmlIgnore]
        public IEnumerable<IMediaFile> PackageFile
        {
            get
            {
                return this._packageFile;
            }
            set
            {
                this._packageFile = value;
            }
        }

        [XmlIgnore]
        public IEnumerable<IMediaFile> HotFixes
        {
            get
            {
                return this._hotFixes;
            }
            set
            {
                this._hotFixes = value;
            }
        }

        [XmlIgnore]
        public IEnumerable<IMediaFile> SourceFile
        {
            get
            {
                return this._sourceFile;
            }
            set
            {
                this._sourceFile = value;
            }
        }

        [XmlIgnore]
        public IEnumerable<IMediaFile> DocumentationFile
        {
            get
            {
                return this._documentationFile;
            }
            set
            {
                this._documentationFile = value;
            }
        }

        [XmlIgnore]
        public IVendor Vendor
        {
            get
            {
                return this._vendor;
            }
            set
            {
                this._vendor = value;
            }
        }

        public int VendorId
        {
            get { return _vendorId; }
            set { _vendorId = value; }
        }


        public double Price
        {
            get
            {
                return this._price;
            }
            set
            {
                this._price = value;
            }
        }
        public string GACode
        {
            get
            {
                return this._gaCode;
            }
            set
            {
                this._gaCode = value;
            }
        }

        [XmlIgnore]
        public IEnumerable<IMediaFile> ScreenShots
        {
            get
            {
                return this._screenShots;
            }
            set
            {
                this._screenShots = value;
            }
        }
        public string DemonstrationUrl
        {
            get
            {
                return this._demonstrationUrl;
            }
            set
            {
                this._demonstrationUrl = value;
            }
        }
        public string[] UmbracoVerionsSupported
        {
            get
            {
                return this._umbracoVerionsSupported;
            }
            set
            {
                this._umbracoVerionsSupported = value;
            }
        }
        public string[] NETVersionsSupported
        {
            get
            {
                return this._netVersionsSupported;
            }
            set
            {
                this._netVersionsSupported = value;
            }
        }
        public TrustLevel TrustLevelSupported
        {
            get
            {
                return this._trustLevelSupported;
            }
            set
            {
                this._trustLevelSupported = value;
            }
        }
        public DateTime TermsAgreementDate
        {
            get
            {
                return this._termsAgreementDate;
            }
            set
            {
                this._termsAgreementDate = value;
            }
        }
        public Guid ProjectGuid
        {
            get { return _packageGuid; }
            set { _packageGuid = value; }
        }
        public bool StarterKitModule
        {
            get { return _starterKitModule; }
            set { _starterKitModule = value; }
        }
        public bool NotAPackage
        {
            get { return _notAPackage; }
            set { _notAPackage = value; }
        }
        public bool OpenForCollab
        {
            get { return _openForCollab; }
            set { _openForCollab = value; }
        }
        public string SourceCodeUrl
        {
            get { return _sourceCodeUrl; }
            set { _sourceCodeUrl = value; }
        }
        public bool Live
        {
            get { return _live; }
            set { _live = value; }
        }
        public bool Stable
        {
            get { return _stable; }
            set { _stable = value; }
        }
        public bool Approved
        {
            get { return _approved; }
            set { _approved = value; }
        }

        public int Downloads
        {
            get { return _downloads.Invoke(Id); }
        }

        public Boolean Disabled
        {
            get { return _disabled; }
            set { _disabled = value; }
        }

        public int Karma
        {
            get { return _karma.Invoke(Id); }
        }

        public string ProjectUrl
        {
            get { return _projectUrl; }
            set { _projectUrl = value; }

        }

        public string LicenseKey
        {
            get { return _licenseKey; }
            set { _licenseKey = value; }

        }


        public int ProjectViews
        {
            get { return _projectViews.Invoke(ProjectGuid); }

        }

        public Guid Version
        {
            get
            {
                var Doc = new Document(Id);
                return Doc.Version;
            }
        }

        #endregion

        public DateTime CreateDate
        {
            get { return _createDate; }
            set { _createDate = value; }
        }


        public ListingItem(Func<int, int> downloads, Func<int, int> karma)
        {
            _karma = karma;
            _downloads = downloads;
        }
    }
}
