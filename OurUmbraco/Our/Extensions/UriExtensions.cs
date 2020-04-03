using System;

namespace OurUmbraco.Our.Extensions
{
    public static class UriExtensions
    {
        internal static string CleanPathAndQuery(this Uri uri)
        {
            //sometimes the request path may have double slashes so make sure to normalize this
            return uri.PathAndQuery.Replace("//", "/");
        }
    }
}