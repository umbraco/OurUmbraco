namespace OurUmbraco.Community.Nuget
{
    public class NugetPackageInfo
    {
        public string Name { get; set; }
        public string PackageId { get; set; }

        public int TotalDownLoads { get; set; }

        public int AverageDownloadPerDay { get; set; }
    }
}
