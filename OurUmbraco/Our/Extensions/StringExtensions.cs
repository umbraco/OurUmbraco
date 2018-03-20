using System.Collections.Generic;
using System.Linq;
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

        public static List<Badge> GetBadges(this IEnumerable<string> roles)
        {
            var badges = new List<Badge>();
            var rolesWithoutMvp = roles.Where(x => x.ToLowerInvariant().StartsWith("MVP".ToLowerInvariant()) == false).ToList();
            var numberOfMvps = roles.Count(x => x.ToLowerInvariant().StartsWith("MVP ".ToLowerInvariant()));

            if (numberOfMvps != 0)
            {
                var name = "MVP";
                if (numberOfMvps > 1)
                    name = string.Format("{0} {1}x", name, numberOfMvps);

                badges.Add(new Badge {Name = name, Link = "/community/most-valuable-people/", CssClass = "mvp" });
            }
            
            foreach (var role in rolesWithoutMvp)
                badges.Add(new Badge { Name = role, CssClass = role.ToLowerInvariant(), Link = "/community/badges/" });

            return badges;
        }
    }

    public class Badge
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public string CssClass { get; set; }
    }
}
