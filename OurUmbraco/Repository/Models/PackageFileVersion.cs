namespace OurUmbraco.Repository.Models
{
    public class PackageFileVersion
    {
        public int FileId { get; set; }
        public string MinUmbracoVersion { get; set;}
        public string PackageVersion { get; set; }
    }
}