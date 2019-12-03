using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OurUmbraco.Documentation.Busineslogic.GithubSourcePull;
using OurUmbraco.Documentation.Models;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Web;
using OurUmbraco.Documentation.Busineslogic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OurUmbraco.Documentation.Controllers
{
    [PluginController("Documentation")]
    public class LessonsController : UmbracoApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The path in the documentation folder structure that you wish to start from & list descendants</param>
        /// <param name="userType">Future Use: Backoffice Usertype requesting lessons</param>
        /// <param name="allowedSections">Future Use: AllowedSections of BackOffice User</param>
        /// <param name="lang">Future Use: Backoffice Users langunage</param>
        /// <param name="version">Umbraco CMS Version as a string</param>
        /// <returns>
        /// A partial part of the documentation tree, that lists out folders
        /// This is used in the new Starter Kit in conjuction with 'Lessons'
        /// </returns>
        public List<DocumentationUpdater.SiteMapItem> GetDocsForPath(string path, string userType, string allowedSections, string lang, string version)
        {
            //Ensure path is not null & empty
            if (string.IsNullOrEmpty(path))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Path varibale is null or empty"),
                    ReasonPhrase = "Documentation Path is invalid"
                };

                throw new HttpResponseException(resp);
            }
            
            //Get the documentation Sitemap JS file that lives on disk everytime we fetch & unpack the markdown docs from GitHub
            var docs = new DocumentationUpdater();

            //Note: For now the folder param is NOT the folder/subtree but where the JSON file is stored to build up this model
            var allDocs = docs.DocumentationSiteMap();
            
            
            //Split path and ensure we can find each part of the path
            //In the array of nested objects
            var pathArray = path.Split('/').Where(x => string.IsNullOrEmpty(x) == false).ToArray();
            var currentDirectory = allDocs;

            for (int i = 0; i < pathArray.Length; i++)
            {
                var pathPart = "/" + string.Join("/", pathArray.Take(i + 1));
                currentDirectory = currentDirectory.Directories.First(x => x.Path == pathPart);
            }

            return currentDirectory.Directories.OrderBy(x=> x.Sort).ToList();
        }

        /// <summary>
        /// Gets Steps (Children of a lesson as rendered HTML from the markdown file)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<LessonStep> GetStepsForPath(string path)
        {
            var docs = new DocumentationUpdater();
            var rootFolder = global::Umbraco.Core.IO.IOHelper.MapPath("/Documentation/" + path);
            var mdFiles = System.IO.Directory.GetFiles(rootFolder, "*.md");

            var result = new List<LessonStep>();
            foreach(var fpath in mdFiles)
            {
                var content = System.IO.File.ReadAllText(fpath);
                var name = System.IO.Path.GetFileName(path);
                var md = new MarkdownLogic(fpath);
                var html = md.DoTransformation();

                result.Add(new LessonStep() { Name = name, Content = html });
            }


            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionAlias"></param>
        /// <param name="treeAlias"></param>
        public List<HelpDocument> GetContextHelpDocs(string sectionAlias, string treeAlias)
        {
            if (sectionAlias.ToLower() == "settings" && treeAlias.ToLower() == "documenttypes")
            {
                return new List<HelpDocument>
                {
                    new HelpDocument
                    {
                        Name = "Defining Content",
                        Description = "Here you'll find an explanation of how content is defined and quick guide for your first go at it (based on an empty installation).",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Data/Defining-content/"
                    }
                };
            }

            if (sectionAlias.ToLower() == "settings"  && (treeAlias.ToLower() == "templates" || treeAlias.ToLower() == "partialviews") )
            {
                return new List<HelpDocument>
                {
                    new HelpDocument
                    {
                        Name = "Templates",
                        Description = "Templating in Umbraco builds on the concept of Razor Views from asp.net MVC - if you already know this, then you are ready to create your first template - if not, this is a quick and handy guide.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Design/Templates/"
                    },
                    new HelpDocument
                    {
                        Name = "Basic Razor Syntax",
                        Description = "Shows how to perform common logical tasks in Razor like if/else, foreach loops, switch statements and using the @ character to separate code and markup.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Design/Templates/basic-razor-syntax"
                    },
                    new HelpDocument
                    {
                        Name = "Rendering Content",
                        Description = "The primary task of any template in Umbraco is to render the values of the current page or the result of query against the content cache.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Design/Rendering-Content/"
                    },
                    new HelpDocument
                    {
                        Name = "Rendering Media",
                        Description = "Templates can access items in the Media library, to assist in displaying rich content like galleries.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Design/Rendering-Media/"
                    },
                    new HelpDocument
                    {
                        Name = "Basic Razor Syntax",
                        Description = "Shows how to perform common logical tasks in Razor like if/else, foreach loops, switch statements and using the @ character to separate code and markup.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Design/Templates/basic-razor-syntax"
                    },
                    new HelpDocument
                    {
                        Name = "View/Razor Examples",
                        Description = "Lots of examples of using various techniques to render data in a view",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Reference/Templating/Mvc/examples"
                    }
                };
            }

            if (sectionAlias.ToLower() == "media")
            {
                return new List<HelpDocument>
                {
                    new HelpDocument
                    {
                        Name = "Creating Media",
                        Description = "Media in Umbraco is handled in much the same way as content. Instead of defining Document Types you define Media Types that act as the base for media items.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Data/Creating-Media/"
                    }
                };
            }

            if (sectionAlias.ToLower() == "developer" && treeAlias.ToLower() == "datatypes")
            {
                return new List<HelpDocument>
                {
                    new HelpDocument
                    {
                        Name = "Data-Types",
                        Description = "A Data Type defines the type of input for a property. So when adding a property (on Document Types, Media Types and Members) when selecting the Type you are selecting a Data Type. There are a number of preconfigured Data Types available in Umbraco and more can be added in the Developer section.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Data/Data-Types/"
                    },


                    new HelpDocument
                    {
                        Name = "Property Editors",
                        Description = "Extend Umbraco with your own property editors ",
                        Url = "https://our.umbraco.com/documentation/Extending/Property-Editors/",
                        Type = HelpDocType.Doc
                    }
                };
            }

            if (sectionAlias.ToLower() == "member")
            {
                return new List<HelpDocument>
                {
                    new HelpDocument
                    {
                        Name = "Members",
                        Description = "Members are used for registering and authenticating external users of an Umbraco installation (ie. forum members, intranet users and so forth). Unlike with Document Types and Media Types everything is done in the Members section both defining and creating, and editing members.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Data/Members/"
                    }
                };
            }

            if (sectionAlias.ToLower() == "developer")
            {
                return new List<HelpDocument>
                {
                    new HelpDocument
                    {
                        Name = "Data-Types",
                        Description = "A Data Type defines the type of input for a property. So when adding a property (on Document Types, Media Types and Members) when selecting the Type you are selecting a Data Type. There are a number of preconfigured Data Types available in Umbraco and more can be added in the Developer section.",
                        Type = HelpDocType.Doc,
                        Url = "https://our.umbraco.com/documentation/Getting-Started/Data/Data-Types/"
                    },


                    new HelpDocument
                    {
                        Name = "Macros",
                        Description = "A macro is a reusable piece of functionality that you can re-use throughout your site. Macros can be configured with parameters and be inserted into a Rich Text Editor.",
                        Url = "https://our.umbraco.com/Documentation/Reference/Templating/Macros/",
                        Type = HelpDocType.Doc
                    },

                     new HelpDocument
                    {
                        Name = "Xslt",
                        Description = "XSLT will soon be removed from Umbraco - read how you can transition away from it",
                        Url = "https://our.umbraco.com/Documentation/Reference/Templating/Macros/Xslt/",
                        Type = HelpDocType.Doc
                    }
                };
            }

            //Did not find anything for the combination
            return null;
        }
    }

    [DataContract(Name = "lessonStep")]
    public class LessonStep
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }
    }

    [DataContract(Name = "helpDocument")]
    public class HelpDocument
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public HelpDocType Type { get; set; }
    }

    public enum HelpDocType { Doc, Video }
}
