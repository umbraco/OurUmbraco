using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Marketplace.usercontrols.Deli
{
    public partial class ListPagination : System.Web.UI.UserControl
    {
        public int NumberOfPages { get; set; }
        public int WindowSize { get; set; }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (NumberOfPages > 0)
            {
                if (WindowSize == 0)
                {
                    WindowSize = 5;
                }
                //list to hold the generated links
                List<HyperLink> linklist = new List<HyperLink>();
                //get currentpage
                int? currentPageNumber = 1;
                if (!string.IsNullOrEmpty(Request.QueryString["page"]))
                    currentPageNumber = Int32.Parse(Request.QueryString["page"]);

                //Create the new href retaining the current querystrings
                string url = Request.Url.ToString();
                if (!url.Contains("page"))
                {
                    if (url.Contains('?')) url += "&page=";
                    else url += "?page=";
                }
                else
                    url = url.Replace("page=" + currentPageNumber, "page=");


                if (String.IsNullOrEmpty(Request["ViewAll"]))
                {
                    //Create the numerical links to be added to the place holder
                    int start = 1;
                    int end = NumberOfPages;

                    //int midPage = (int)currentPageNumber;
                    //midPage = Math.Max(midPage, (WindowSize / 2) + (WindowSize % 2 == 0 ? 0 : 1));
                    //// make sure the middle of the view is not too close to the start.
                    //midPage = Math.Min(midPage, NumberOfPages - (WindowSize / 2));
                    //// make sure the middle of the view is not too close to the end.
                    //end = midPage + (WindowSize / 2);
                    //midPage += ((WindowSize % 2 == 0) ? 1 : 0); // this has to be incremented before calculating the From.
                    //start = midPage - (WindowSize / 2);

                    //start = Math.Max(start, 1);

                    if (currentPageNumber > 1)
                    {
                        HyperLink prevLink = new HyperLink();
                        prevLink.Text = " &lt; Previous ";
                        prevLink.NavigateUrl = url.Replace("page=", "page=" + (currentPageNumber - 1));
                        linklist.Add(prevLink);
                    }




                    //HyperLink startlink = CreateLink(url, 1, "first");
                    //linklist.Add(startlink);



                    for (int? i = start; i <= end; i++)
                    {
                            HyperLink PageLink = CreateLink(url, i);
                            linklist.Add(PageLink);

                        
                    }

                    //HyperLink endlink = CreateLink(url, NumberOfPages, "last");
                    //linklist.Add(endlink);



                    //check for next
                    if (currentPageNumber < NumberOfPages)
                    {
                        HyperLink nextLink = new HyperLink();
                        nextLink.Text = " Next &gt; ";
                        nextLink.NavigateUrl = url.Replace("page=", "page=" + (currentPageNumber + 1));
                        linklist.Add(nextLink);
                    }

                    Controls.Add(new Literal() { Text = "<ul class=\"deliPaging\">" });

                    foreach (HyperLink hl in linklist)
                    {
                        Controls.Add(new Literal() { Text = "<li>" });
                        Controls.Add(hl);
                        Controls.Add(new Literal() { Text = "</li>" });
                    }
                    Controls.Add(new Literal() { Text = "</ul>" });
                }
                else
                {
                    Controls.Add(new Literal() { Text = "<ul class=\"deliPaging\">" });
                    Controls.Add(new Literal() { Text = "<li class=\"switch\">" });
                    Controls.Add(new HyperLink() { Text = "Paged Results", NavigateUrl = url.Replace("&page=", "").Replace("ViewAll=true", "page=1") });
                    Controls.Add(new Literal() { Text = "</li>" });
                    Controls.Add(new Literal() { Text = "</ul>" });
                }
            }
        }

        private HyperLink CreateLink(string url, int? i, string text ="")
        {
            HyperLink PageLink = new HyperLink();
            if (!string.IsNullOrEmpty(text))
                PageLink.Text = " " + text + " ";
            else
                PageLink.Text = " " + i + " ";

            PageLink.NavigateUrl = url.Replace("page=", "page=" + i.ToString());

            //Check to see if link is current
            if (!string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                if (Int32.Parse(Request.QueryString["page"]) == i)
                {
                    PageLink.Attributes.Add("class", "selected");
                }
            }
            else if (i == 1)
            {
                PageLink.Attributes.Add("class", "selected");
            }
            return PageLink;
        }
    }
}