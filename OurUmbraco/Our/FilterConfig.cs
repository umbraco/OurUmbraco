using System.Web.Mvc;
using OurUmbraco.Our.ErrorHandler;

namespace OurUmbraco.Our
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AiHandleErrorAttribute());
        }
    }
}