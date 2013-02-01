using System;
using System.IO;
using System.Web;
using System.Web.UI;
using uDocumentation.Busineslogic;

namespace uDocumentation.usercontrols
{
    public partial class DocumentationShowMarkdown : UserControl
    {
        private string _markdownFilePath;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (AddHeader)
            {
                string path = HttpContext.Current.Request.Url.AbsolutePath;
                int index = path.EndsWith("/")
                                ? path.TrimEnd('/').LastIndexOf('/') + 1
                                : path.LastIndexOf('/') + 1;
                string urlPath = path.Substring(index);
                lblHeader.Text = string.Concat("<h1>", urlPath.RemoveDash().UnderscoreToDot().TrimStart('/').TrimEnd('/'), "</h1>");
            }

            

            MarkdownLogic ml = new MarkdownLogic(MarkdownFilePath, VersionFromSession) { PrefixLinks = PrefixLinks };
            lblMarkdownOutput.Text = ml.DoTransformation();
        }

        public bool PrefixLinks { get; set; }

        public bool AddHeader { get; set; }

        public string MarkdownFilePath
        {
            get
            {
                if (umbraco.NodeFactory.Node.GetCurrent().Parent.NodeTypeAlias == "Project")
                {
                    string readmePath = string.Concat(HttpRuntime.AppDomainAppPath, umbraco.NodeFactory.Node.GetCurrent().Url.Substring(1).Replace("/", @"\") + @"\readme.md");
                    if (File.Exists(readmePath))
                        return readmePath;
                    else
                        return string.Concat(HttpRuntime.AppDomainAppPath, umbraco.NodeFactory.Node.GetCurrent().Url.Substring(1).Replace("/", @"\") + @"\index.md");
                }
                if (string.IsNullOrEmpty(_markdownFilePath))
                {
                    return HttpContext.Current.Items[MarkdownLogic.MarkdownPathKey].ToString();
                }

                string formattedPath = string.Format(_markdownFilePath, VersionFromSession);
                //Try to resolve relative filepath
                if (!File.Exists(formattedPath))
                {
                    string absolutePath = string.Concat(HttpRuntime.AppDomainAppPath, formattedPath);
                    if (File.Exists(absolutePath))
                        return absolutePath;
                }

                return formattedPath;
            }
            set { _markdownFilePath = value; }
        }

        public string VersionFromSession
        {
            get
            {
                if (Session[MarkdownLogic.VersionSession] == null || Session[MarkdownLogic.VersionSession].ToString() == string.Empty)
                {
                    Session[MarkdownLogic.VersionSession] = DefaultVersion.Instance.Number;
                }

                return Session[MarkdownLogic.VersionSession].ToString();
            }
            set { Session[MarkdownLogic.VersionSession] = value; }
        }
    }
}