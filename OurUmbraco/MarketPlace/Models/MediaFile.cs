using System;
using System.Collections.Generic;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.Wiki.BusinessLogic;

namespace OurUmbraco.MarketPlace.Models
{
    public class MediaFile : IMediaFile
    {

        protected int _id;
        protected string _name;
        protected string _path;
        protected FileType _fileType;
        protected Guid _fileVersion;
        protected int _createdBy;
        protected int _listingItemId;
        //protected Guid _projectGuid;
        protected DateTime _createDate;
        protected Boolean _current;
        protected int _removedBy;
        protected int _downloads;
        protected Boolean _archived;
        protected List<UmbracoVersion> _umbVersion;
        protected string _dotNetVersion;
        protected Boolean _supportsMediumTrust;
        protected Boolean _verified;
        protected string _minimumUmbracoVersion;
        
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
            }
        }

        public FileType FileType
        {
            get
            {
                return _fileType;
            }
            set
            {
                _fileType = value;
            }
        }

        public Guid FileVersion
        {
            get
            {
                return _fileVersion;
            }
            set
            {
                _fileVersion = value;
            }
        }

        public int CreatedBy
        {
            get
            {
                return _createdBy;
            }
            set
            {
                _createdBy = value;
            }
        }

        public int ListingItemId
        {
            get
            {
                return _listingItemId;
            }
            set
            {
                _listingItemId = value;
            }
        }

        public DateTime CreateDate
        {
            get
            {
                return _createDate;
            }
            set
            {
                _createDate = value;
            }
        }

        public bool Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
            }
        }

        public int RemovedBy
        {
            get
            {
                return _removedBy;
            }
            set
            {
                _removedBy = value;
            }
        }

        public int Downloads
        {
            get
            {
                return _downloads;
            }
            set
            {
                _downloads = value;
            }
        }

        public bool Archived
        {
            get
            {
                return _archived;
            }
            set
            {
                _archived = value;
            }
        }

        public List<UmbracoVersion> UmbVersion
        {
            get
            {
                return _umbVersion;
            }
            set
            {
                _umbVersion = value;
            }
        }

        public string DotNetVersion
        {
            get { return _dotNetVersion; }
            set { _dotNetVersion = value; }
        }

        public bool SupportsMediumTrust
        {
            get { return _supportsMediumTrust; }
            set { _supportsMediumTrust = value; }
        }

        public bool Verified
        {
            get
            {
                return _verified;
            }
            set
            {
                _verified = value;
            }
        }

        public string MinimumUmbracoVersion
        {
            get
            {
                return _minimumUmbracoVersion;
            }
            set
            {
                _minimumUmbracoVersion = value;
            }
        }
    }
}
