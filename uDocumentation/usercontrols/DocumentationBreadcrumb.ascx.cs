using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using uDocumentation.Busineslogic;

namespace uDocumentation.usercontrols
{
    public partial class DocumentationBreadcrumb : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public string GetBreadcrumb()
        {
            StringBuilder sbResult = new StringBuilder();
            StringBuilder sbBcUrl = new StringBuilder();
            string rootUrl = "/";
            string rootName = "Home";

            sbResult.Append("<li><a href=\"" + rootUrl + "\">" + rootName + "</a></li>");
            sbBcUrl.Append("/");

            string absolutePath = HttpContext.Current.Request.Url.AbsolutePath;
            string directoryName = Path.GetDirectoryName(absolutePath);

            directoryName = directoryName.Substring(1);
            string[] strDirs = directoryName.Split('\\');

            //Added nofollow to link because we can't ensure all parts of the url path to be an actual page
            //main concern being the version part, ie. v501.
            foreach (string strDirName in strDirs)
            {
                sbResult.Append("<li><a href=\"" + sbBcUrl + strDirName + "\" rel=\"nofollow\">" +
                                strDirName
                                .RemoveDash()
                                .UnderscoreToDot()
                                .EnsureCorrectDocumentationText() + "</a></li>");
                sbBcUrl.Append(strDirName + "/");
            }

            if (!absolutePath.EndsWith("/"))
            {
                sbResult.AppendFormat("<li>{0}</li>",
                                      absolutePath.Substring(absolutePath.LastIndexOf('/') + 1)
                                      .RemoveDash()
                                      .UnderscoreToDot()
                                      .EnsureCorrectDocumentationText());
            }

            return sbResult.ToString();
        }
    }
}