using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using Examine;
using OurUmbraco.Documentation.Busineslogic;
using OurUmbraco.Our.Models;
using umbraco;
using Umbraco.Web;

namespace OurUmbraco.Our
{
    public static class SearchResultExtensions
    {
        public static string GetTitle(this SearchResult result)
        {
            return HttpContext.Current.Server.HtmlEncode(result.Fields.ContainsKey("nodeName") ? result["nodeName"] : string.Empty);
        }

        public static List<SearchResultBreadcrumbModel> GetBreadcrumbs(this SearchResult result)
        {
            var currentContext = UmbracoContext.Current;
            var currentResult = currentContext.ContentCache.GetById(result.Id);
            var breadcrumbItems = new List<SearchResultBreadcrumbModel>();
            if (currentResult == null)
            {
                if (result.Fields.Keys.Contains("url"))
                {
                    if (result["url"].Contains("forum"))
                    {
                        breadcrumbItems.Add(new SearchResultBreadcrumbModel
                        {
                            Name = "Forum",
                            Url = "/forum/"
                        });

                        breadcrumbItems.Add(new SearchResultBreadcrumbModel
                        {
                            Name = result["nodeName"],
                            Url = result["url"]
                        });
                        return breadcrumbItems;
                    }

                    if (result["url"].Contains("Documentation"))
                    {
                        string baseurl = "/";
                        string directoryName = Path.GetDirectoryName(result["url"]);
                        directoryName = directoryName.Substring(1);
                        var strDirs = directoryName.Split('\\').ToList();
                        if (!result["url"].Contains("-v"))
                        {
                            if (!result["url"].EndsWith("/"))
                            {
                                strDirs.Add(result["url"].Substring(result["url"].LastIndexOf('/') + 1));
                            }
                        }

                        if (strDirs.Last() == "index")
                        {
                            strDirs.Remove(strDirs.Last());
                        }

                        foreach (var page in strDirs)
                        {
                            breadcrumbItems.Add(new SearchResultBreadcrumbModel
                            {
                                Name = page.RemoveDash().UnderscoreToDot().EnsureCorrectDocumentationText(),
                                Url = baseurl + page
                            });

                            baseurl += page + "/";
                        }
                    }
                    return breadcrumbItems;
                }

                return breadcrumbItems;
            }

            var ancestors = currentResult.Ancestors().ToList();

            foreach (var ancestor in ancestors)
            {
                breadcrumbItems.Add(new SearchResultBreadcrumbModel
                {
                    Name = ancestor.Name,
                    Url = ancestor.Url
                });
            }

            return breadcrumbItems;
        }

        public static string GetIcon(this SearchResult result)
        {
            var icon = "icon-Chat";
            if (result.Fields["nodeTypeAlias"] == "project")
            {
                icon = "icon-Box";
            }
            else if (result.Fields["nodeTypeAlias"] == "documentation")
            {
                icon = "icon-Book-alt";
            }

            return icon;
        }

        public static string SolvedClass(this SearchResult result)
        {
            if (result.Fields.ContainsKey("solved") && result.Fields["solved"] != "0")
            {
                return "solved";
            }

            return string.Empty;
        }

        public static string GetDate(this SearchResult result)
        {
            try
            {
                var createDate = GetFormattedDateTime(result["createDate"], "MMMM dd, yyyy");
                var updateDate = GetFormattedDateTime(result["updateDate"], "MMMM dd, yyyy");
                return
                    HttpContext.Current.Server.HtmlEncode(string.Format("Created: {0} - Last update: {1}", createDate,
                        updateDate));
            }
            catch (FormatException ex)
            {
                // Catches "String was not recognized as a valid DateTime." errors
                // TODO: Figure out why these errors occur..
            }
            catch (KeyNotFoundException ex)
            {
                // Catches "The given key was not present in the dictionary." errors for result["createDate"]
                // TODO: Figure out why these errors occur..
            }

            return string.Empty;
        }

        public static string GetFormattedDateTime(string date, string format)
        {
            try
            {
                var dateTime = new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2)));
                return dateTime.ToString(format);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string FullUrl(this SearchResult result)
        {
            if (result.Fields.ContainsKey("url"))
                return result["url"];

            if (result["__IndexType"] == "content")
                return library.NiceUrl(result.Id);

            if (result["__IndexType"] == "forum" && result.Fields.ContainsKey("parentId") && result.Fields.ContainsKey("urlName"))
            {
                var url = library.NiceUrl(int.Parse(result.Fields["parentId"]));
                return GlobalSettings.UseDirectoryUrls
                    ? string.Format("/{0}/{1}-{2}", url.Trim('/'), result.Fields["__NodeId"], result.Fields["urlName"])
                    : string.Format("/{0}/{1}-{2}.aspx", url.Substring(0, url.LastIndexOf('.')).Trim('/'), result.Fields["__NodeId"], result.Fields["urlName"]);
            }

            return "TODO";
        }

        public static IHtmlString GenerateBlurb(this SearchResult result, int noOfChars)
        {
            var text = string.Empty;

            switch (result["__IndexType"])
            {
                case "content":
                    if (result.Fields.ContainsKey("bodyText"))
                        text = result.Fields["bodyText"];
                    break;
                case "project":
                case "documentation":
                case "forum":
                    if (result.Fields.ContainsKey("body"))
                        text = result.Fields["body"];
                    break;
            }

            var helper = new UmbracoHelper(UmbracoContext.Current);

            return helper.Truncate(text, noOfChars);
        }

        public static string CssClassName(this SearchResult result)
        {
            string cssClass = string.Empty;

            if (result["__IndexType"] == "content")
            {
                switch (result.Fields["nodeTypeAlias"])
                {
                    case "WikiPage":
                        cssClass = " wiki ";
                        break;
                    case "Project":
                        cssClass = " project ";
                        break;
                }
            }
            else if (result["__IndexType"] == "documentation")
            {
                cssClass = " documentation ";
            }
            else if (result["__IndexType"] == "documents")
            {
                cssClass = " forum ";


                SqlConnection cn = new SqlConnection(umbraco.GlobalSettings.DbDSN);
                cn.Open();
                SqlCommand cmd = new SqlCommand("Select answer from forumTopics where id = @id", cn);
                cmd.Parameters.AddWithValue("@id", result.Id);
                int solved = 0;
                try
                {
                    int.TryParse(cmd.ExecuteScalar().ToString(), out solved);
                }
                catch { }
                cn.Close();

                if (solved != 0)
                    cssClass += "solution";
            }


            return cssClass;

        }

    }
}