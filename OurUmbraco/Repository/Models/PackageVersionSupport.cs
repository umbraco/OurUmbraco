using System;

namespace OurUmbraco.Repository.Models
{
    public class PackageVersionSupport : IEquatable<PackageVersionSupport>
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
            return _fileId == other._fileId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PackageVersionSupport && Equals((PackageVersionSupport)obj);
        }

        public override int GetHashCode()
        {
            return _fileId;
        }

        public static bool operator ==(PackageVersionSupport left, PackageVersionSupport right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PackageVersionSupport left, PackageVersionSupport right)
        {
            return !left.Equals(right);
        }
    }
}