using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OurUmbraco.Repository
{
    [Serializable]
    [XmlType(Namespace = "http://packages.umbraco.org/webservices/")]
    public class Package : IComparable
    {
        private Guid repoGuid;
        public Guid RepoGuid
        {
            get { return repoGuid; }
            set { repoGuid = value; }
        }

        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private string icon;
        public string Icon
        {
            get { return icon; }
            set { icon = value; }
        }

        private string thumbnail;
        public string Thumbnail
        {
            get { return thumbnail; }
            set { thumbnail = value; }
        }

        private string documentation;
        public string Documentation
        {
            get { return documentation; }
            set { documentation = value; }
        }

        private string demo;
        public string Demo
        {
            get { return demo; }
            set { demo = value; }
        }

        private bool accepted;
        public bool Accepted
        {
            get { return accepted; }
            set { accepted = value; }
        }

        private bool isModule;
        public bool IsModule {
          get { return isModule; }
          set { isModule = value; }
        }


        private bool editorsPick = false;
        public bool EditorsPick
        {
            get { return editorsPick; }
            set { editorsPick = value; }
        }

        private bool m_protected;
        public bool Protected
        {
            get { return m_protected; }
            set { m_protected = value; }
        }


        private bool hasUpgrade;
        public bool HasUpgrade
        {
            get { return hasUpgrade; }
            set { hasUpgrade = value; }
        }

        private string upgradeVersion;
        public string UpgradeVersion
        {
            get { return upgradeVersion; }
            set { upgradeVersion = value; }
        }

        private string upgradeReadMe;
        public string UpgradeReadMe
        {
            get { return upgradeReadMe; }
            set { upgradeReadMe = value; }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public int CompareTo(object other)
        {
            return ((Package)other).text.CompareTo(this.text);
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://packages.umbraco.org/webservices/")]
    public enum SubmitStatus
    {
        Complete, Exists, NoAccess, Error
    }

    [Serializable]
    [XmlType(Namespace = "http://packages.umbraco.org/webservices/")]
    public class Category : IComparable
    {
        public Category()
        {
            packages = new List<Package>();
        }
        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private List<Package> packages;
        public List<Package> Packages
        {
            get { return packages; }
            set { packages = value; }
        }

        #region IComparable Members

        public int CompareTo(object other)
        {
            return ((Category)other).text.CompareTo(this.text);  
        }

        #endregion
    }
}
