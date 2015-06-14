using System;
using System.IO;
using System.Linq;
using System.Web;
using uDocumentation.Busineslogic;
using Umbraco.Core;
using Umbraco.Web.Routing;

namespace uDocumentation
{
    public class DocumentationContentFinder : ContentFinderByNiceUrl
    {
        public override bool TryFindContent(PublishedContentRequest contentRequest)
        {
            // eg / or /path/to/whatever
            var url = contentRequest.Uri.GetAbsolutePathDecoded();

            var mdRoot = "/" + MarkdownLogic.BaseUrl;
            if (url.StartsWith("/projects"))
                mdRoot = "/projects";

            // ensure it's a md url
            if (url.StartsWith(mdRoot) == false)
                return false; // not for us

            // find the root content
            var node = FindContent(contentRequest, mdRoot);
            if (node == null)
                return false;

            // kill those old urls
            foreach (var s in new []{ "master", "v480" })
                if (url.StartsWith(mdRoot + "/" + s))
                {
                    url = url.Replace(mdRoot + "/" + s, mdRoot);
                    contentRequest.SetRedirectPermanent(url);
                    return true;
                }

            // find the md file
            var mdFilepath = FindMarkdownFile(url);
            if (mdFilepath == null)
            {
                // clear the published content (that was set by FindContent) to cause a 404, and in
                // both case return 'true' because there's no point other finders try to handle the request
                contentRequest.PublishedContent = null;
                return true;
            }
 
            // set the context vars
            var httpContext = contentRequest.RoutingContext.UmbracoContext.HttpContext;
            httpContext.Items[MarkdownLogic.MarkdownPathKey] = mdFilepath;
            httpContext.Items["topicTitle"] = string.Join(" - ", httpContext.Request.RawUrl
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Reverse());

            // override the template
            const string altTemplate = "DocumentationSubpage";
            var templateIsSet = contentRequest.TrySetTemplate(altTemplate);
            //httpContext.Trace.Write("Markdown Files Handler",
            //    string.Format("Template changed to: '{0}' is {1}", altTemplate, templateIsSet));

            // be happy
            return true;
        }

        private static string FindMarkdownFile(string url /*, out string redirUrl*/)
        {
            //redirUrl = null;

            if (url.StartsWith("/"))
                url = url.Substring(1);

            var relpath = url.UnderscoreToDot().Replace('/', '\\').TrimEnd('\\');

            // note
            // the whole redirecting thing is moot because Umbraco normalizes urls
            // so even if redirecting to /foo/ it will end up as /foo... getting rid
            // of it

            if (url.EndsWith("/"))
            {
                var fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath, "\\index.md");
                if (File.Exists(fpath))
                    return fpath; // ok!
                // else it could be a normal file?
                fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath.Substring(0, relpath.Length - 1), ".md");
                if (File.Exists(fpath))
                    return fpath;
                //{
                //    // does not work for a directory but would work for a file
                //    redirUrl = "/" + url.Substring(0, url.Length - 1);
                //    return null;
                //}
            }
            else
            {
                var fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath, ".md");
                if (File.Exists(fpath))
                    return fpath; // ok!
                // else it could be a directory?
                fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath, "\\index.md");
                if (File.Exists(fpath))
                    return fpath;
                
                fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath, "\\readme.md");
                if (File.Exists(fpath))
                    return fpath;
                //{
                //    // does not match a file but would work for a directory, redirect
                //    redirUrl = "/" + url + "/";
                //    return null;
                //}
            }

            return null;
        }
    }

    public class DocumentationContentFinderInstaller : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase app, ApplicationContext context)
        {
            ContentFinderResolver.Current
                .InsertTypeBefore<ContentFinderByNiceUrl, DocumentationContentFinder>();
        }
    }
}
