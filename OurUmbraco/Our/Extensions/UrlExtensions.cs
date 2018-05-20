
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

    }

}
