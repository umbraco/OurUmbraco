using System.Collections.Generic;
using Umbraco.Core.Models;

namespace OurUmbraco.Forum.Models
{
    public class TopicMember
    {
        public IPublishedContent Member { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
