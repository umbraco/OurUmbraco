using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using OurUmbraco.MarketPlace.Extensions;
using OurUmbraco.MarketPlace.Interfaces;
using Umbraco.Web;

namespace OurUmbraco.MarketPlace.Providers
{
    public class UmbracoCategoryProvider : ICategoryProvider
    {
        public IEnumerable<ICategory> GetAllCategories()
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var content = umbracoHelper.TypedContentAtRoot().First(x => string.Equals(x.DocumentTypeAlias, "community", StringComparison.CurrentCultureIgnoreCase))
                .Children.First(x => string.Equals(x.DocumentTypeAlias, "projects", StringComparison.CurrentCultureIgnoreCase));
            var contents = content.Descendants().Where(x => x.DocumentTypeAlias == "ProjectGroup");

            return contents.ToICategoryList();
        }

        public ICategory GetCurrent()
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var content = umbracoHelper.TypedContent(HttpContext.Current.Items["pageID"]);

            if (content.DocumentTypeAlias != "ProjectGroup")
            {
                throw new Exception("Content is not of the correct type");
            }

            return content.ToICategory();
        }

        public ICategory GetCategory(int id)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var content = umbracoHelper.TypedContent(id);

            if (content.DocumentTypeAlias != "ProjectGroup")
            {
                throw new Exception("Content is not of the correct type");
            }

            return content.ToICategory();
        }

        public ICategory GetCategory(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
