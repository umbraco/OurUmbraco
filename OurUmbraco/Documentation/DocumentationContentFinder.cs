using System;
using System.IO;
using System.Linq;
using System.Web;
using OurUmbraco.Documentation.Busineslogic;
using Umbraco.Core;
using Umbraco.Web.Routing;
using System.Text.RegularExpressions;

namespace OurUmbraco.Documentation
{
    public class DocumentationContentFinder : ContentFinderByNiceUrl
    {
        public override bool TryFindContent(PublishedContentRequest contentRequest)
        {
            // eg / or /path/to/whatever
            var url = contentRequest.Uri.GetAbsolutePathDecoded();

            var mdRoot = "/" + MarkdownLogic.BaseUrl;
            if (url.StartsWith("/projects/umbraco-pro/contour/documentation"))
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
            
            //return the broken link doc page
            var is404 = false;
            if (mdFilepath == null)
            {
                mdFilepath = FindMarkdownFile("/documentation/broken-link");
                is404 = true;
            }
            if (mdFilepath == null)
            {                
                // clear the published content (that was set by FindContent) to cause a 404, and in
                // both case return 'true' because there's no point other finders try to handle the request
                contentRequest.PublishedContent = null;
                return true;
            }
 
            if (is404) contentRequest.SetIs404();

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

    public class Github
    {
        public static string MarkdownFileLink()
        {
            string branchName = "master";
            string baseUrl = "https://github.com/umbraco/UmbracoDocs/blob/" + branchName;

            var docUrl = HttpContext.Current.Items[MarkdownLogic.MarkdownPathKey].ToString();

            if (System.IO.File.Exists(docUrl))
            {
                //Need to get NEW key as needs to be the original MD filename 
                //from Github including .md & correct casing of file
                var originalUrl = HttpContext.Current.Items["umbOriginalUrl"].ToString();

                //Ensure beginning part of url is right case for GitHub URL
                if (originalUrl.StartsWith("/documentation/", StringComparison.InvariantCultureIgnoreCase))
                    // don't strip off the leading "/"
                    originalUrl = originalUrl.Substring("/documentation/".Length - 1);

                //Ensure parts of URL (after forward slash) begin with uppercase to match GitHub URLs.
                string expression = @"[\/]([a-z])";
                char[] charArray = originalUrl.ToCharArray();
                foreach (Match match in Regex.Matches(originalUrl, expression, RegexOptions.Singleline))
                {
                    charArray[match.Groups[1].Index] = Char.ToUpper(charArray[match.Groups[1].Index]);
                }
                originalUrl = new string(charArray);

                //If ends with / then it's an index.md file in a folder
                if (originalUrl.EndsWith("/"))
                {
                    //Add the word  after the /, so it's /index
                    originalUrl += "index";

                }

                //Append the .md file extension
                docUrl = baseUrl + string.Format("{0}{1}", originalUrl, ".md");

            }
            else
            {
                //MD file does not exist on disk - hide edit button
                docUrl = null;
            }

            return docUrl;
        }
    }
}
