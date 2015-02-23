using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Marketplace.Interfaces;
using Marketplace.Providers;

namespace Marketplace.usercontrols.Deli
{
    public partial class NewestProjects : UserControl
    {
        public int TotalListings { get; set; }
        public int PageStartListingNumber { get; set; }
        public int PageEndListingNumber { get; set; }

        private string _listingType = "";
        public string ListingType
        {
            get { return _listingType; }
            set { _listingType = value; }
        }

        private int _maxPageSize = 20;
        public int MaxPageSize
        {
            get { return _maxPageSize; }
            set { _maxPageSize = value; }
        }

        private int _pageNumber;
        public int PageNumber
        {
            get
            {
                if (_pageNumber > 0)
                    return _pageNumber - 1;

                return _pageNumber;
            }
            set
            {
                _pageNumber = value;
            }
        }

        public bool paged { get; set; }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            Bind_Data();
        }

        protected void Bind_Data()
        {
            var projectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];

            var skip = PageNumber * _maxPageSize;
            var take = _maxPageSize;

            IEnumerable<IListingItem> projects;

            if (paged == false)
            {
                switch (_listingType)
                {
                    case "free":
                        projects = projectsProvider.GetListingsByLatest(0, take, Interfaces.ListingType.free, true);
                        break;

                    case "commercial":
                        projects = projectsProvider.GetListingsByLatest(0, take, Interfaces.ListingType.commercial, true);
                        break;

                    default:
                        projects = projectsProvider.GetListingsByLatest(0, take, true);
                        break;
                }

                ListingOpen.Visible = false;
                ListingClose.Visible = false;
                ProjectCounter.Visible = false;
                paging.Visible = false;
            }
            else
            {
                int count;

                switch (_listingType)
                {
                    case "free":
                        projects = projectsProvider.GetListingsByLatest(skip, take, Interfaces.ListingType.free, true);
                        count = projectsProvider.Count(Interfaces.ListingType.free);
                        break;

                    case "commercial":
                        projects = projectsProvider.GetListingsByLatest(skip, take, Interfaces.ListingType.commercial, true);
                        count = projectsProvider.Count(Interfaces.ListingType.commercial);
                        break;

                    default:
                        projects = projectsProvider.GetListingsByLatest(skip, take, true);
                        count = projectsProvider.Count();
                        break;
                }
                
                if (count > _maxPageSize)
                {
                    paging.Visible = true;

                    var numPages = (count / (decimal)_maxPageSize);
                    var intNumPages = Convert.ToInt32(numPages);

                    if (numPages % 1 > 0 && numPages % 1 <= (decimal)0.5)
                    {
                        intNumPages += 1;
                    }

                    paging.NumberOfPages = intNumPages;
                }

                var projectCount = projects.Count();

                if (projectCount > 0)
                {
                    ProjectCounter.Visible = true;
                    TotalListings = count;
                    PageStartListingNumber = skip + 1;
                    PageEndListingNumber = skip + projectCount;
                }
                else
                {
                    ProjectCounter.Visible = false;
                    Listing.Visible = false;
                    NoListings.Visible = true;
                }

                ListingOpen.Visible = true;
                ListingClose.Visible = true;
                WidgetTitle.Visible = false;
                WidgetFeatures.Visible = false;
            }

            Listing.DataSource = projects;
            Listing.DataBind();
        }
    }
}