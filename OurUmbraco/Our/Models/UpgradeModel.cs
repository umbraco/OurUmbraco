namespace OurUmbraco.Our.Models
{
    public class UpgradeModel
    {
        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public int VersionPatch { get; set; }
        public string VersionComment { get; set; }
    }
}
