using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.Providers;
using OurUmbraco.Wiki.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace OurUmbraco.Wiki.Extensions
{
    public static class MediaExtensions
    {
        public static string ToVersionString(this List<UmbracoVersion> versions)
        {
            var verStr = string.Empty;
            foreach (var ver in versions)
            {
                verStr += ver.Version + ", ";
            }

            return verStr.Trim().TrimEnd(',');
        }

        public static string ToVersionNameString(this List<UmbracoVersion> versions)
        {
            var verStr = string.Empty;
            foreach (var ver in versions)
            {
                verStr += ver.Name + ", ";
            }

            return verStr.Trim().TrimEnd(',');
        }

        public static void SetMinimumUmbracoVersion(this WikiFile mediaFile)
        {
            var fileType = (FileType)Enum.Parse(typeof(FileType), mediaFile.FileType);
            if (fileType != FileType.package)
                return;

            System.Version minimumUmbracoVersion = null;

            var extractor = new PackageExtraction();
            var filePath = IOHelper.MapPath(mediaFile.Path);
            var packageXml = extractor.ReadTextFileFromArchive(filePath, Umbraco.Core.Constants.Packaging.PackageXmlFileName);
            if (string.IsNullOrWhiteSpace(packageXml))
                return;

            var packageXmlDoc = XDocument.Parse(packageXml);

            // The XPath query will detect if the 'requirements' element has the attribute that we're looking for,
            // and if the child elements also exist. [LK:2016-06-12@CGRT16]
            var requirements = packageXmlDoc.XPathSelectElement("/umbPackage/info/package/requirements");
            if (requirements == null)
                return;
            if(requirements.Attribute("type") == null || requirements.Attribute("type").Value.ToLowerInvariant() != "strict")
                return;

            if(requirements.Element("major") == null  || requirements.Element("minor") == null || requirements.Element("patch") == null)
                return;

            var major = requirements.Element("major").Value;
            var minor = requirements.Element("minor").Value;
            var patch = requirements.Element("patch").Value;

            System.Version.TryParse(string.Format("{0}.{1}.{2}", major, minor, patch), out minimumUmbracoVersion);

            if (minimumUmbracoVersion != default(System.Version))
                mediaFile.MinimumVersionStrict = minimumUmbracoVersion.ToString(3);
        }
    }
}
