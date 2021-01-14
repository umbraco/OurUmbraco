using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.Project.uVersion;
using umbraco.presentation.nodeFactory;
using Umbraco.Core;

namespace OurUmbraco.Project
{
    public static class ProjectExtensions
    {
        /// <summary>
        /// This tries to clean the string to convert to a Version instance based on the various ways we store version string values for projects
        /// </summary>
        /// <param name="version"></param>
        /// <param name="reduceToConfigured"></param>
        /// <returns>
        /// This compares the version passed in, then checks what the latest configured minor version is in uVersion.config with comparison
        /// to this minor version and uses that.
        /// </returns>
        public static System.Version GetFromUmbracoString(this string version, bool reduceToConfigured = true)
        {
            if (string.IsNullOrWhiteSpace(version)) return null;

            //need to clean up this string, it could be all sorts of things
            version = version.ToLower()
                .Split('-')[0]
                .Replace("saved", "")
                .Replace("nan", "")
                .Replace("v", "")
                .Replace(".x", "")
                .Trim(',');
            var versions = new UVersion();
            if (!version.Contains(".") && version.Length > 0)
            {
                //if there's no . then it stored the strange way like in uVersion.config
                var uVersion = versions.GetAllVersions().FirstOrDefault(x => x.Key == $"v{version}");
                
                if (uVersion != null)
                {
                    version = uVersion.FullVersion.ToString();
                }
                else
                {
                    //pad it out to 3 digits
                    int o;
                    version = version.PadRight(3, '0');
                    if (int.TryParse(version, out o))
                    {
                        //if it ends with '0', that means it's a X.X.X version
                        // if it does not end with '0', that means that the last 2 digits are the
                        // Minor part of the version
                        version = version.EndsWith("0")
                            ? string.Format("{0}.{1}.{2}", version[0], version[1], 0)
                            : string.Format("{0}.{1}.{2}", version[0], version.Substring(1), 0);
                    }
                }
            }

            if (!System.Version.TryParse(version, out var result))
                return null;

            if (!reduceToConfigured)
                return result;

            //Now we need to check what the latest configured major/minor version that corresponds to this one is and use that version.
            // This is so that search results are actually returned. Example, if running 7.4.3 but the latest configured minor is 7.4.0, then
            // no search results are returned because nothing would be tagged as compatible with 7.4.3, only 7.4.0

            // get all Version's configured order by latest
            var allAsVersion = versions.GetAllAsVersions().ToArray();
            //search for the latest compatible version to search on
            foreach (var v in allAsVersion)
                if (result > v)
                    return v;

            //we couldn't find anything, 
            //this will occur if the passed in version is not greater than any configured versions, in this case we have no choice 
            // but to return a very small version, we'll stick with 4.5 since this is the infamous hard coded value
            return new System.Version(4, 5, 0);
        }

        /// <summary>
        /// This returns a numerical version as a Right padded 3 digit combined long number. Example:
        /// 7.5.0 would be:
        ///     007005000 = 7005000
        /// 4.11.0 would be:
        ///     004011000 = 4011000
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static long GetNumericalValue(this System.Version version)
        {
            var asLong =
                version.Major.ToString().PadLeft(3, '0')
                + (version.Minor == -1 ? "0" : version.Minor.ToString()).PadLeft(3, '0')
                + (version.Build == -1 ? "0" : version.Build.ToString()).PadLeft(3, '0');
            return long.Parse(asLong);
        }

        public static string StripHtmlAndLimit(this String str, int chars)
        {
            str = str.StripHtml();

            if (str.Length > chars)
                str = str.Substring(0, chars);

            return str;

        }
        
        public static string CleanHtmlAttributes(this string description)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(description);
            var elementsWithAttribute = doc.DocumentNode.SelectNodes("//@*");
            if (elementsWithAttribute != null)
            {
                foreach (var element in elementsWithAttribute)
                {
                    var href = element.Attributes["href"];
                    var src = element.Attributes["src"];

                    element.Attributes.RemoveAll();
                    if (href != null)
                    {
                        element.Attributes.Add("href", href.Value);
                        element.Attributes.Add("target", "_blank");

                        if (href.Value.StartsWith("https://our.umbraco.") == false &&
                            href.Value.StartsWith("http://our.umbraco.") == false &&
                            href.Value.StartsWith("our.umbraco.") == false)
                        {
                            element.Attributes.Add("rel", "nofollow");
                        }
                    }

                    if (src != null)
                    {
                        element.Attributes.Add("src", src.Value);
                    }
                }
            }

            var preElements = doc.DocumentNode.SelectNodes("//pre");
            if (preElements != null)
            {
                foreach (var element in preElements)
                {
                    // Just take the very inner text and remove everything else leaving: <pre>the text</pre>
                    element.InnerHtml = string.Format("<code>{0}</code>", element.InnerText);
                }
                foreach (var element in preElements)
                {
                    // Wrap the <pre/> in <div class="body markdown-syntax" />
                    element.InnerHtml = WrapWithHtmlNode(element, "div", "body markdown-syntax").OuterHtml;
                }
            }

            string result;
            using (var stringWriter = new StringWriter())
            {
                doc.Save(stringWriter);
                result = stringWriter.ToString();
            }

            return result;
        }

        private static HtmlNode WrapWithHtmlNode(HtmlNode node, string name, string className = "")
        {
            var parent = node.ParentNode;
            var newParent = node.OwnerDocument.CreateElement(name);
            if (string.IsNullOrWhiteSpace(className) == false)
                newParent.Attributes.Add("class", className);

            parent.InsertBefore(newParent, node);

            var clone = node.CloneNode(true);
            newParent.AppendChild(clone);

            parent.RemoveChild(node);

            return newParent;
        }
    }
}