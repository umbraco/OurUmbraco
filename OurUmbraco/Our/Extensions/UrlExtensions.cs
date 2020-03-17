using System;
using System.Web.Mvc;
using OurUmbraco.Our.Models;

namespace OurUmbraco.Our.Extensions
{
    public static class UrlExtensions
    {
        public static string GetProfileUrl(this UrlHelper helper, MemberData member)
        {
            return "/members/" + (member.HasGitHubUsername ? member.GitHubUsername : "id:" + member.Id) + "/";
        }

        public static string GetProfileUrl(this UrlHelper helper, ProfileModel member)
        {
            return "/members/" + (member.HasGitHubUsername ? member.GitHubUsername : "id:" + member.Id) + "/";
        }

        public static string GetProfileUrlWithDomain(this UrlHelper helper, MemberData member)
        {
            var rootUrl = helper.RequestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            return rootUrl + "/members/" + (member.HasGitHubUsername ? member.GitHubUsername : "id:" + member.Id) + "/";
        }

        public static string GetProfileUrlWithDomain(this UrlHelper helper, ProfileModel member)
        {
            var rootUrl = helper.RequestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            return rootUrl + "/members/" + (member.HasGitHubUsername ? member.GitHubUsername : "id:" + member.Id) + "/";
        }

        public static string GetReleaseUrl(this UrlHelper helper, Release release)
        {
            return "/download/releases/" + release.Version.Replace(".", string.Empty);
        }

        public static string GetReleaseUrl(this UrlHelper helper, Release release, bool download)
        {
            return GetReleaseUrl(helper, release) + "?fromdownload=" + download;
        }
    }
}