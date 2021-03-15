namespace OurUmbraco.Community.Nuget
{
    using System;

    using Newtonsoft.Json;

    public class NugetPackageInfo
    {
        public string PackageId { get; set; }

        public int TotalDownLoads { get; set; }

        public int AverageDownloadPerDay { get; set; }

        public DateTime PackageDate { get; set; }
    }
}
