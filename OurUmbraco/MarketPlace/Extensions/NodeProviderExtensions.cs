using System;
using System.Collections.Generic;
using System.Linq;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.NodeListing;
using OurUmbraco.MarketPlace.Providers;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace OurUmbraco.MarketPlace.Extensions
{
    public static class NodeProviderExtensions
    {
        public static ListingType GetPropertyAsListingType(this IPublishedContent content, string field)
        {
            if (content.GetProperty(field) != null)
            {
                switch (content.GetPropertyValue<string>(field))
                {
                    case "promoted":
                        return ListingType.promoted;
                        break;
                    case "commercial":
                        return ListingType.commercial;
                    default:
                        return ListingType.free;
                        break;
                }
            }

            // if its not set we can assume that its free.
            return ListingType.free;
        }

        public static TrustLevel GetPropertyAsTrustLevel(this IPublishedContent content, string field)
        {
            if (content.GetProperty(field) != null)
            {
                switch (content.GetPropertyValue<string>(field))
                {
                    case "medium":
                        return TrustLevel.medium;
                        break;
                    default:
                        return TrustLevel.full;
                        break;
                }
            }
            return TrustLevel.full;
        }

        public static string GetListingTypeAsString(this ListingType listingType)
        {
            switch (listingType)
            {
                case ListingType.free:
                    return "free";
                    break;
                case ListingType.commercial:
                    return "commercial";
                    break;
                case ListingType.promoted:
                    return "promoted";
                    break;
                default:
                    return "free";
                    break;
            }
        }

        public static IEnumerable<IListingItem> ToIListingItemList(this IEnumerable<IPublishedContent> content, bool optimized)
        {
            var items = new List<IListingItem>();
            foreach (var c in content)
            {
                yield return c.ToIListingItem(optimized);
            }
        }

        public static IListingItem ToIListingItem(this IPublishedContent content, bool optimized)
        {
            return new NodeListingProvider().GetListing(content, optimized);
        }

        public static IEnumerable<ICategory> ToICategoryList(this IEnumerable<IPublishedContent> contents)
        {
            List<ICategory> items = new List<ICategory>();
            foreach (var c in contents)
            {
                yield return c.ToICategory();
            }
        }

        public static ICategory ToICategory(this IPublishedContent c)
        {
            var cat = new Category
            {
                Id = c.Id,
                CategoryGuid = new Guid(c.GetPropertyValue<string>("categoryGuid")),
                Name = c.Name,
                Description = c.GetPropertyValue<string>("description"),
                Image = c.GetPropertyAsMediaItem("icon"),
                HQOnly = c.GetPropertyValue<string>("hqOnly") == "1",
                Url = c.Url,
                ProjectCount = c.Descendants().Count(p => p.GetPropertyValue<int>("projectLive") == 1)
            };

            return cat;
        }

        public static string GetPropertyAsMediaItem(this IPublishedContent content, string field)
        {
            if (string.IsNullOrEmpty(content.GetPropertyValue<string>(field)) == false)
            {
                var mItem = new umbraco.cms.businesslogic.media.Media(int.Parse(content.GetPropertyValue<string>(field)));

                var propertyValue = string.Empty;
                var property = mItem.getProperty("umbracoFile");
                if (property != null)
                    propertyValue = property.Value.ToString();

                return propertyValue;
            }
            return string.Empty;
        }
    }
}
