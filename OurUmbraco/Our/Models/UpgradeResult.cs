using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OurUmbraco.Our.Models
{
    public class UpgradeResult
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

        [JsonProperty(PropertyName = "upgradeType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UpgradeType CurrentUpgradeType { get; set; }
        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }
        [JsonProperty(PropertyName = "upgradeUrl")]
        public string UpgradeUrl { get; set; }

        public UpgradeResult(UpgradeType upgradeType, string comment, string upgradeUrl)
        {
            CurrentUpgradeType = upgradeType;
            Comment = comment;
            UpgradeUrl = upgradeUrl;
        }
    }
}
