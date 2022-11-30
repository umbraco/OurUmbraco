using System;
using System.IO;
using System.Linq;
using System.Web;
using OurUmbraco.Documentation.Busineslogic;
using Umbraco.Core;
using Umbraco.Web.Routing;
using System.Collections.Generic;
using System.Globalization;
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
            foreach (var s in new[] { "master", "v480" })
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
            httpContext.Items["topicTitle"] = string.Join(" - ", httpContext.Request.Url.AbsolutePath
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
                const string defaultFileName = "index.md";
                var fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath, "\\", defaultFileName); // Example: D:\Dev\OurUmbracov7\OurUmbraco.Site\documentation\extending\backoffice-search\index.md
                if (File.Exists(fpath))
                {
                    return fpath; // ok!
                }
                else
                {
                    // is it a special sort order?
                    var findMarkdownFile = GetFilePathForFile(fpath);
                    if (findMarkdownFile != null) 
                        return findMarkdownFile;
                }
                    
                // else it could be a normal file?
                fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath.Substring(0, relpath.Length - 1), ".md"); // Example: D:\Dev\OurUmbracov7\OurUmbraco.Site\documentation\extending\backoffice-search.md
                if (File.Exists(fpath))
                {
                    return fpath;
                }
                else
                { 
                    // is it a special sort order?
                    var findMarkdownFile = GetFilePathForFile(fpath);
                    if (findMarkdownFile != null) 
                        return findMarkdownFile;
                }
            }
            else
            {
                var fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath, ".md"); // Example: D:\Dev\OurUmbracov7\OurUmbraco.Site\documentation\extending\backoffice-search.md
                if (File.Exists(fpath))
                {
                    return fpath; // ok!
                }
                else
                {
                    // is it a special sort order?
                    var findMarkdownFile = GetFilePathForFile(fpath);
                    if (findMarkdownFile != null) 
                        return findMarkdownFile;
                }

                // else it could be a directory?
                fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath, "\\index.md"); // Example: D:\Dev\OurUmbracov7\OurUmbraco.Site\documentation\extending\backoffice-search\index.md
                if (File.Exists(fpath))
                {
                    return fpath;
                }
                else
                {
                    // is it a special sort order?
                    var findMarkdownFile = GetFilePathForFile(fpath);
                    if (findMarkdownFile != null) 
                        return findMarkdownFile;
                }


                fpath = string.Concat(HttpRuntime.AppDomainAppPath, relpath, "\\readme.md"); // Example: D:\Dev\OurUmbracov7\OurUmbraco.Site\documentation\extending\backoffice-search\readme.md
                if (File.Exists(fpath))
                {
                    return fpath;
                }
                else
                {
                    var findMarkdownFile = GetFilePathForFile(fpath);
                    if (findMarkdownFile != null) 
                        return findMarkdownFile;
                }
            }

            return null;
        }

        private static string GetFilePathForFile(string path)
        {
            const string regexPattern = @"(\d*sort-)";
            var regex = new Regex(regexPattern);

            var directory = path.Substring(0, path.LastIndexOf("\\", StringComparison.Ordinal));
            var fileName = path.Substring(path.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            
            // Build a valid file path if possible
            if (Directory.Exists(directory) == false)
            {
                var segments = directory
                    .Replace(HttpRuntime.AppDomainAppPath, "")
                    .Split('\\');

                directory = $"{HttpRuntime.AppDomainAppPath}\\{segments[0]}"; // will be "documentation"
                foreach (var segment in segments.Skip(1))
                {
                    var parentDirectory = directory;
                    directory = $"{directory}\\{segment}";
                    
                    var directories = Directory
                        .GetDirectories(parentDirectory)
                        .Where(x => regex.IsMatch(x.Split('\\').Last()));
                
                    var lastSegment = directory.Split('\\').Last();
                    foreach (var dir in directories)
                    {
                        var foundLastSegment = dir.Split('\\').Last();
                        foundLastSegment = Regex.Replace(foundLastSegment, regexPattern, "");
                        if (lastSegment.InvariantEquals(foundLastSegment) == false) 
                            continue;
                        
                        directory = dir;
                    }
                }
            }

            // Directory might have been updated, if it still doesn't exist - cancel the search
            if (Directory.Exists(directory) == false)
            {
                return null;
            }
            
            var filePath = $"{directory}\\{fileName}";
            if (File.Exists(filePath))
            {
                return filePath;
            }
            
            var files = Directory.GetFiles(directory, "*.md")
                .Where(p => regex.IsMatch(p))
                .ToList();

            foreach (var file in files)
            {
                var lastSegment = file.Split('\\').LastOrDefault();
                if (lastSegment == null)
                    continue;

                lastSegment = Regex.Replace(lastSegment, regexPattern, "");
                if (lastSegment.InvariantEquals(fileName))
                    return file;
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
        public static string MarkdownFileEditLink()
        {
            string branchName = "legacy-docs";
            string baseUrl = "https://github.com/umbraco/UmbracoDocs/edit/" + branchName;

            var docUrl = HttpContext.Current.Items[MarkdownLogic.MarkdownPathKey].ToString();
            
            //Need to get NEW key as needs to be the original MD filename 
            //from Github including .md & correct casing of file
            //var originalUrl = HttpContext.Current.Items["umbOriginalUrl"].ToString();
            string originalUrl;
            if (!TryGetExactPath(docUrl, out originalUrl))
            {
                // MD file does not exist on disk - hide edit button
                return null;
            }

            // Ensure beginning part of url is right case for GitHub URL
            var docFolderPosition = originalUrl.IndexOf(@"\documentation\", StringComparison.InvariantCultureIgnoreCase);
            if (docFolderPosition > -1)
            {
                // don't strip off the leading "/"
                originalUrl = originalUrl
                    .Substring(docFolderPosition + @"\documentation\".Length - 1)
                    .Replace('\\', '/');
            }

            // If ends with / then it's an index.md file in a folder
            if (originalUrl.EndsWith("/"))
            {
                // Add the word  after the /, so it's /index
                originalUrl += "index.md";
            }

            // Append the base and file url together
            docUrl = baseUrl + originalUrl;



            return docUrl;
        }

        public static string GithubIssueString(string title = null)
        {
            var githubIssueLink = "https://github.com/umbraco/UmbracoDocs/issues/new";

            var queryStringSeparator = "?";

            if (title != null)
            {
                githubIssueLink = githubIssueLink + "?title=" + Uri.EscapeDataString(title);
                queryStringSeparator = "&";
            }

            githubIssueLink = $"{githubIssueLink}{queryStringSeparator}body={Uri.EscapeDataString($"\n\n\n***This is the page with issues: {MarkdownFileEditLink()}***")}";

            return githubIssueLink;
        }

        /// <summary>
        /// Gets the exact case used on the file system for an existing file or directory.
        /// </summary>
        /// <param name="path">A relative or absolute path.</param>
        /// <param name="exactPath">The full path using the correct case if the path exists.  Otherwise, null.</param>
        /// <returns>True if the exact path was found.  False otherwise.</returns>
        /// <remarks>
        /// This supports drive-lettered paths and UNC paths, but a UNC root
        /// will be returned in title case (e.g., \\Server\Share).
        /// Original from http://stackoverflow.com/a/29578292/97615
        /// </remarks>
        public static bool TryGetExactPath(string path, out string exactPath)
        {
            bool result = false;
            exactPath = null;

            // DirectoryInfo accepts either a file path or a directory path, and most of its properties work for either.
            // However, its Exists property only works for a directory path.
            DirectoryInfo directory = new DirectoryInfo(path);
            if (File.Exists(path) || directory.Exists)
            {
                var parts = new List<string>();

                DirectoryInfo parentDirectory = directory.Parent;
                while (parentDirectory != null)
                {
                    FileSystemInfo entry = parentDirectory.EnumerateFileSystemInfos(directory.Name).First();
                    parts.Add(entry.Name);

                    directory = parentDirectory;
                    parentDirectory = directory.Parent;
                }

                // Handle the root part (i.e., drive letter or UNC \\server\share).
                string root = directory.FullName;
                if (root.Contains(':'))
                {
                    root = root.ToUpper();
                }
                else
                {
                    string[] rootParts = root.Split('\\');
                    root = string.Join("\\", rootParts.Select(part => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(part)));
                }

                parts.Add(root);
                parts.Reverse();
                exactPath = Path.Combine(parts.ToArray());
                result = true;
            }

            return result;
        }
    }
}
