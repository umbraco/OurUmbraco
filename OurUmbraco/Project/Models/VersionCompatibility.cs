using System.Runtime.Serialization;

namespace OurUmbraco.Project.Models
{
    [DataContract]
    public class VersionCompatibility
    {
        [DataMember(Name = "percentage")]
        public int Percentage { get; set; }
        [DataMember(Name = "smiley")]
        public string Smiley { get; set; }
        [DataMember(Name = "version")]
        public string Version { get; set; }
    }
}