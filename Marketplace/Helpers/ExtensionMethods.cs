using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OurUmbraco.MarketPlace.Interfaces;

namespace uProject.Helpers
{
    public static class ExtensionMethods
    {
        public static string ListingTypeAsString(this ListingType listingType)
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

        public static string FileTypeAsString(this FileType fileType)
        {
            switch (fileType)
            {
                case FileType.screenshot:
                    return "screenshot";
                    break;
                case FileType.package:
                    return "package";
                    break;
                case FileType.docs:
                    return "docs";
                    break;
                case FileType.source:
                    return "source";
                    break;
                default:
                    return "package";
                    break;
            }

        }

        public static string LicenseTypeAsString(this LicenseType licenseType)
        {
            switch (licenseType)
            {
                case LicenseType.Domain:
                    return "Domain";
                    break;
                case LicenseType.IP:
                    return "IP";
                    break;
                case LicenseType.Unlimited:
                    return "Unlimited";
                    break;
                case LicenseType.SourceCode:
                    return "Source Code";
                    break;
                default:
                    return "Domain";
                    break;
            }
        }
    }
}