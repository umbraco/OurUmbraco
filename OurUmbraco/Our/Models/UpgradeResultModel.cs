using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OurUmbraco.Our.Models
{
    public enum UpgradeType
    {
        None,
        Patch,
        Minor,
        Major,
        Critical,
        Error,
        OutOfSync
    }

    public class UpgradeResultModel
    {
        [JsonProperty(PropertyName = "upgradeType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UpgradeType UpgradeType { get; set; }
        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }
        [JsonProperty(PropertyName = "upgradeUrl")]
        public string UpgradeUrl { get; set; }

        public UpgradeResultModel(UpgradeType upgradeType, string comment, string upgradeUrl)
        {
            UpgradeType = upgradeType;
            Comment = comment;
            UpgradeUrl = upgradeUrl;
        }
    }
}
