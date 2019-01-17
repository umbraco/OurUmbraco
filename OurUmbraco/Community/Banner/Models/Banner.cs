using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace OurUmbraco.Community.Banner.Models
{
    public class Banner : PublishedContentModel
    {
        public Banner(IPublishedContent content) : base(content) { }
        public bool All { get; internal set; }
        public IEnumerable<string> Continents { get; internal set; }
        public IEnumerable<string> Countries { get; internal set; }
        public string Link { get; internal set; }
        public IPublishedContent Image { get; internal set; }
    }
}
