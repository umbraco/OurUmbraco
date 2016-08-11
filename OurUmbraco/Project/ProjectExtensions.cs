using System;
using System.Linq;
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
            //need to clean up this string, it could be all sorts of things
            version = version.ToLower()
                .Replace("saved", "")
                .Replace("nan", "")
                .Replace("v", "")
                .Replace(".x", "")
                .Trim(',');
            if (!version.Contains(".") && version.Length > 0 && version.Length <= 3)
            {
                //if there's no . then it stored the strange way like in uVersion.config

                //pad it out to 3 digits
                version = version.PadRight(3, '0');
                int o;
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

            System.Version result;
            if (!System.Version.TryParse(version, out result))
                return null;

            if (!reduceToConfigured)
                return result;

            //Now we need to check what the latest configured major/minor version that corresponds to this one is and use that version.
            // This is so that search results are actually returned. Example, if running 7.4.3 but the latest configured minor is 7.4.0, then
            // no search results are returned because nothing would be tagged as compatible with 7.4.3, only 7.4.0

            //get all Version's configured order by latest
            var all = UVersion.GetAllAsVersions().ToArray();
            //search for the latest compatible version to search on
            foreach (var v in all)
            {
                if (result > v)
                {
                    return v;
                }
            }
            
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
                + (version.Minor == -1 ? "0" : version.Minor.ToString()).PadRight(3, '0')
                + (version.Build == -1 ? "0" : version.Build.ToString()).PadRight(3, '0');
            return long.Parse(asLong);
        }

        public static string StripHtmlAndLimit(this String str, int chars)
        {
            str = umbraco.library.StripHtml(str);

            if (str.Length > chars)
                str = str.Substring(0, chars);

            return str;

        }
        
        public static string BuildExamineString(this string term, int boost, string field, bool andSearch)
        {
            term = Lucene.Net.QueryParsers.QueryParser.Escape(term);
            var terms = term.Trim().Split(' ');
            var qs = field + ":";
            qs += "\"" + term + "\"^" + (boost + 30000).ToString() + " ";
            qs += field + ":(+" + term.Replace(" ", " +") + ")^" + (boost + 5).ToString() + " ";
            if (!andSearch)
            {
                qs += field + ":(" + term + ")^" + boost.ToString() + " ";
            } return qs;
        }

    }
}