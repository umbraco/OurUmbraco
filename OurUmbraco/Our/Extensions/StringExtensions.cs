using System.Text.RegularExpressions;

namespace OurUmbraco.Our.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// taken from https://our.umbraco.org/projects/website-utilities/ezsearch/bugs-feedback-suggestions/59447-Special-Characters-in-Search-Term#comment-215504
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string MakeSearchQuerySafe(this string query)
        {
            if (query == null) return string.Empty;
            var regex = new Regex(@"[^\w\s]");
            return regex.Replace(query, " ").ToLowerInvariant();
        }
    }
}
