using System;

namespace OurUmbraco.Our.Models
{
    public class InstallationModel
    {
        private string _userAgent;
        private string _versionComment;

        public Guid InstallId { get; set; }
        public bool IsUpgrade { get; set; }
        public bool InstallCompleted { get; set; }
        public DateTime Timestamp { get; set; }
        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public int VersionPatch { get; set; }

        public string VersionComment
        {
            get
            {
                if (string.IsNullOrEmpty(_versionComment))
                    _versionComment = "";
                return _versionComment;
            }
            set { _versionComment = value; }
        }
        public string Error { get; set; }

        public string UserAgent
        {
            get
            {
                if (string.IsNullOrEmpty(_userAgent))
                    _userAgent = "blank";
                return _userAgent;
            }
            set { _userAgent = value; }
        }
        public string DbProvider { get; set; }
    }
}
