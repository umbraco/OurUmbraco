using umbraco.interfaces;

namespace OurUmbraco.Release.Controllers
{
    public static class NodeExtensions
    {
        public static string GetPropertyValue(this INode release, string propertyAlias)
        {
            return release.GetProperty(propertyAlias) == null ? "" : release.GetProperty(propertyAlias).Value;
        }
    }
}
