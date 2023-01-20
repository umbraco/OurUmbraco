using YamlDotNet.Serialization;

namespace OurUmbraco.Our.Models
{
    public class YamlMetaData
    {
        public string VersionTo { get; internal set; }
        public string Keywords { get; internal set; }
        public string Tags { get; internal set; }
        public string VersionFrom { get; internal set; }
        public string AssetId { get; set; }
        /// <summary>
        /// The related umbraco product, omit this property in case of CMS
        /// </summary>
        public string Product { get; set; }
        public string Complexity { get; set; }
        public string Audience { get; set; }
        [YamlMember(Alias = "Meta.Title")]
        public string MetaTitle { get; set; }
        [YamlMember(Alias = "Meta.Description")]
        public string MetaDescription { get; set; }
        /// <summary>
        /// A space separated list of Topic IDs
        /// </summary>
        public string Topics { get; set; }
        public string VersionRemoved { get; internal set; }
        public string NeedsV8Update { get; set; }
        
        [YamlMember(Alias = "Meta.RedirectLink")]
        public string RedirectLink { get; set; }
        
    }
}