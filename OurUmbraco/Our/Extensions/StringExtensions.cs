using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OurUmbraco.Our.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// taken from https://our.umbraco.com/projects/website-utilities/ezsearch/bugs-feedback-suggestions/59447-Special-Characters-in-Search-Term#comment-215504
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string MakeSearchQuerySafe(this string query)
        {
            if (query == null) return String.Empty;
            var regex = new Regex(@"[^\w\s]");
            return regex.Replace(query, " ").ToLowerInvariant();
        }

        public static List<Badge> GetBadges(this IEnumerable<string> roles)
        {
            var excludeRoles = new List<string>
            {
                "teamumbraco".ToLowerInvariant()
            };

            var rolesList = roles.Except(excludeRoles).ToList();
            var rolesWithoutMvp = rolesList.Where(x => x.ToLowerInvariant().StartsWith("MVP".ToLowerInvariant()) == false).ToList();
            var numberOfMvps = rolesList.Count(x => x.ToLowerInvariant().StartsWith("MVP ".ToLowerInvariant()));

            var badges = new List<Badge>();
            if (numberOfMvps != 0)
            {
                var name = "MVP";
                if (numberOfMvps > 1)
                    name = $"{name} {numberOfMvps}x";

                badges.Add(new Badge
                {
                    Name = name,
                    Link = "/community/most-valuable-people/",
                    CssClass = "mvp"
                });
            }

            foreach (var role in rolesWithoutMvp)
                badges.Add(new Badge
                {
                    Name = role,
                    CssClass = role.ToLowerInvariant(),
                    Link = $"/community/badges/#{role.ToLowerInvariant()}"
                });

            return badges;
        }

        // From: https://stackoverflow.com/a/42260733/5018
        internal static bool IsLocalPath(this string path)
        {
            Uri uri;
            try
            {
                uri = new Uri(path);
            }
            catch (Exception ex)
            {
                // It's not a URI, so assume it's something local
                return true;
            }

            return uri.IsFile;
        }

        /// <summary>
        /// Return the version number as a int to deal with minor versions with single digits
        /// </summary>
        /// <param name="ver"></param>
        /// <returns></returns>
        public static System.Version AsFullVersion(this string ver)
        {
            return new System.Version(ver);
        }

        public static string StateIcon(this string issueState)
        {
            switch (issueState.ToLowerInvariant().Replace(" ", "").Replace("'", ""))
            {
                case "backlog":
                case "maturing":
                case "submitted":
                case "estimation":
                case "sprint-backlog":
                    return "icon-Checkbox-dotted";
                case "open":
                    return "icon-Checkbox-empty";
                case "review":
                case "inprogress":
                case "in-progress":
                    return "icon-Paper-plane-alt";
                case "duplicate":
                    return "icon-Multiple-windows";
                case "cantreproduce":
                    return "icon-Enter";
                case "obsolete":
                    return "icon-Scull";
                case "closed":
                    return "icon-Stop-alt";
                case "reopened":
                    return "icon-Undo";
                case "workaroundposted":
                    return "icon-Redo";
                case "fixed":
                case "resolved":
                    return "icon-Check";
                default:
                    return "";
            }
        }

        public static DateTime DateFromMonthYear(this string monthYear)
        {
            var firstPrMonth = int.Parse(monthYear.Substring(4, 2));
            var firstPrYear = int.Parse(monthYear.Substring(0, 4));
            var firstPrDate = new DateTime(firstPrYear, firstPrMonth, 01);
            return firstPrDate;
        }
    }

    public class Badge
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public string CssClass { get; set; }
    }
}
