using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json.Serialization;
using OurUmbraco.Repository.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Repository.Controllers
{
    /// <summary>
    /// The package repository controller for querying packages
    /// </summary>
    /// <remarks>
    /// This is NOT auto-routed
    /// </remarks>
    [EnableCors(origins: "*", headers: "*", methods: "get")]
    [JsonCamelCaseFormatter]
    public class PackageRepositoryController : ApiController
    {       

        private Models.PacakgeDetails GetTestDetails()
        {
            return new Models.PacakgeDetails
            {
                Category = "Collaboration",
                Excerpt = "You will make me be president",
                Downloads = 123,
                Id = Guid.NewGuid(),
                Likes = 444,
                Name = "The Donald",
                Thumbnail = "https://our.umbraco.org/media/wiki/150283/635768313097111400_usightlylogopng.png?bgcolor=fff&height=154&width=281&format=png",
                Created = DateTime.Now,
                Compatibility = new List<PackageCompatibility>
                {
                    new PackageCompatibility
                    {
                        Percentage = 90,
                        Version = "7.5.0"
                    },
                    new PackageCompatibility
                    {
                        Percentage = 100,
                        Version = "7.4.0"
                    }
                },
                DotNetVersion = "4.5.0",
                LatestVersion = "1.2.0",
                LicenseName = "MIT",
                LicenseUrl = "https://opensource.org/licenses/MIT",
                MinimumVersion = "7.4.0",
                OwnerInfo = new PackageOwnerInfo
                {
                    Contributors = new[]
                    {
                        "Lee Kelleher",
                        "Matt Brailsford"
                    },
                    Karma = 1000000,
                    Owner = "Shannon Deminick"
                },
                Summary = "<p>This package is so great, I mean, it's really really good</p><p>It has lots of words because I know words, I have the best words</p>"
            };
        }

        private IEnumerable<Models.Package> GetTestData()
        {
            return new[]
            {
                new Models.Package
                {
                    Category = "Collaboration",
                    Excerpt = "You will make me be president",
                    Downloads = 123,
                    Id = Guid.NewGuid(),
                    Likes = 444,
                    Name = "The Donald",
                    Thumbnail = "https://our.umbraco.org/media/wiki/150283/635768313097111400_usightlylogopng.png?bgcolor=fff&height=154&width=281&format=png",                    
                    LatestVersion = "1.2.0",                    
                    MinimumVersion = "7.4.0",
                },
                new Models.Package
                {
                    Category = "Starter Kits",
                    Excerpt = "Another great package",
                    Downloads = 123,
                    Id = Guid.NewGuid(),
                    Likes = 444,
                    Name = "Kill IE6",
                    Thumbnail = "https://our.umbraco.org/media/wiki/9138/634697622367666000_offroadcode-100x100.png?bgcolor=fff&height=154&width=281&format=png"
                },
                new Models.Package
                {
                    Category = "Umbraco Pro",
                    Excerpt = "A package for doing great things",
                    Downloads = 123,
                    Id = Guid.NewGuid(),
                    Likes = 4,
                    Name = "Great!",
                    Thumbnail = "https://our.umbraco.org/media/wiki/50703/634782902373558000_cogworks.jpg?bgcolor=fff&height=154&width=281&format=png"
                },
                new Models.Package
                {
                    Category = "Collaboration",
                    Excerpt = "Super crazy awesome, this thing is just amazing",
                    Downloads = 13323,
                    Id = Guid.NewGuid(),
                    Likes = 44334,
                    Name = "Amazeballs",
                    Thumbnail = "https://our.umbraco.org/media/wiki/154472/635997115126742822_logopng.png?bgcolor=fff&height=154&width=281&format=png"
                },
                new Models.Package
                {
                    Category = "Starter Kits",
                    Excerpt = "A classic, you just can't get enough of this one",
                    Downloads = 12,
                    Id = Guid.NewGuid(),
                    Likes = 433,
                    Name = "uBeMeAndIBeU",
                    Thumbnail = "https://our.umbraco.org/media/wiki/152476/635917291068518788_pipeline-crm-logopng.png?bgcolor=fff&height=154&width=281&format=png"
                }
            };
        }
        
        public IEnumerable<Models.Category> GetCategories()
        {
            return new[]
            {
                new Models.Category
                {
                    Icon = "icon-male-and-female",
                    Name = "Collaboration"
                },
                new Models.Category
                {
                    Icon = "icon-molecular-network",
                    Name = "Backoffice extensions"
                },
                new Models.Category
                {
                    Icon = "icon-brackets",
                    Name = "Developer tools"
                },                
                new Models.Category
                {
                    Icon = "icon-wand",
                    Name = "Starter kits"
                },
                new Models.Category
                {
                    Icon = "icon-medal",
                    Name = "Umbraco Pro"
                },
                new Models.Category
                {
                    Icon = "icon-wrench",
                    Name = "Website utilities"
                }
            };
        }

        public IEnumerable<Models.Package> GetPopular(int maxResults)
        {
            return GetTestData().Take(maxResults);
        }

        public IEnumerable<Models.Package> GetLatest(int pageIndex, int pageSize, string category = null)
        {
            return GetTestData()
                .Where(x => string.IsNullOrWhiteSpace(category) || x.Category == category)
                .Skip(pageIndex*pageSize)
                .Take(pageSize);
        }

        public Models.PacakgeDetails Get(Guid id)
        {
            return GetTestDetails();
        }

    }
}
