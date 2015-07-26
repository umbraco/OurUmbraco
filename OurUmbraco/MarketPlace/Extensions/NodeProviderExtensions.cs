using OurUmbraco.MarketPlace.Interfaces;
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
    }
}
