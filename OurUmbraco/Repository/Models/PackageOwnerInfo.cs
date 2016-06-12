namespace OurUmbraco.Repository.Models
{
    public class PackageOwnerInfo
    {
        public string Owner { get; set; }
        public string OwnerAvatar { get; set; }
        public string[] Contributors { get; set; }
        public int Karma { get; set; }
    }
}