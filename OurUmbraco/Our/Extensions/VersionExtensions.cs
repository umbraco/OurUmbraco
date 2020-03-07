namespace OurUmbraco.Our.Extensions
{
    public static class VersionExtensions
    {
        public static string VersionKey(this System.Version semVersion)
        {
            return $"v{semVersion.Major}{semVersion.Minor}0";
        }

        public static string VersionName(this System.Version semVersion)
        {
            return $"Version {semVersion.Major}.{semVersion.Minor}.x";
        }

        public static string VersionDescription(this System.Version semVersion)
        {
            var versionName = semVersion.VersionName().ToLowerInvariant();
            return $"Compatible with {versionName}";
        }
    }
}