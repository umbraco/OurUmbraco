using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using uDocumentation.Busineslogic;
using umbraco.NodeFactory;

namespace uDocumentation.usercontrols
{
    public partial class DocumentationEditButton : UserControl
    {
        private readonly Node _currentNode  = Node.GetCurrent();
        public bool ShowButton              = false;
        public static string BaseUrl        = "https://github.com/umbraco/Umbraco4Docs/blob/master/Documentation/";

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do same check as masterpage - that DocType/Node alias is Projects
            if (_currentNode.NodeTypeAlias == "Projects")
            {
                //We don;t want the button to show, so stop excuting...
                return;
            }

            //We got this far so we can show button
            ShowButton = true;

            

            //Get the documentation link to the md file to edit on GitHub
            var docUrl = string.Empty;

            //Get the MarkDown path key from the Context Keys
            //This is the full url - C:/inetpub/wwwroot/OurUmbraco/OurUmbraco.Site/documentation/installation/index.md
            docUrl = HttpContext.Current.Items[MarkdownLogic.MarkdownPathKey].ToString();

            //Make sure file exists on disk
            if (File.Exists(docUrl))
            {
                //Need to get NEW key as needs to be the original MD filename 
                //from Github including .md & correct casing of file



            }
            else
            {
                //MD file does not exist on disk - hide edit button
                ShowButton = false;
            }

            //Show or hide the link/button
            editLink.Visible = ShowButton;

            if (ShowButton)
            {
                //Onbly bother setting the link url if it's visible
                //NOTE: GitHub URLs are case sensitive
                editLink.HRef = string.Format("{0}{1}", BaseUrl, docUrl);
            }
           
        }

    }
}