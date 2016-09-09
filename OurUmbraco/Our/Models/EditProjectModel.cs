using Umbraco.Core.Models;

namespace OurUmbraco.Our.Models
{
    public class EditProjectModel
    {
        public IPublishedContent Content { get; set; }
        public bool HasAccess { get; set; }
    }
}