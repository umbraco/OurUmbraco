using System;

namespace OurUmbraco.Repository.Models
{
    internal class PackageVersionSupport : IEquatable<PackageVersionSupport>
    {
        private readonly int _fileId;

        public PackageVersionSupport(int fileId, System.Version packageVersion, System.Version minUmbracoVersion)
        {
            _fileId = fileId;
            MinUmbracoVersion = minUmbracoVersion;
            PackageVersion = packageVersion;
        }

        public int FileId
        {
            get { return _fileId; }
        }

        public System.Version MinUmbracoVersion { get; private set; }
        public System.Version PackageVersion { get; private set; }

        public bool Equals(PackageVersionSupport other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _fileId == other._fileId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PackageVersionSupport) obj);
        }

        public override int GetHashCode()
        {
            return _fileId;
        }

        public static bool operator ==(PackageVersionSupport left, PackageVersionSupport right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PackageVersionSupport left, PackageVersionSupport right)
        {
            return !Equals(left, right);
        }
    }
}