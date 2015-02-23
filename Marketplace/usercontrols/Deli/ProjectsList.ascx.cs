using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Marketplace.Providers;
using Marketplace.Interfaces;
using Marketplace.Providers.Helpers;


namespace Marketplace.usercontrols.Deli
{
    public partial class ProjectsList : UserControl
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
        public int MaxPageSize { get { return _maxPageSize; } set { _maxPageSize = value; } }

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

        public string Tag { get; set; }

        public bool IsCategoryListing { get; set; }


        protected void Page_PreRender(object sender, EventArgs e)
        {
            Bind_Data();
        }

        protected void Bind_Data()
        {
            int count;

            var projectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            List<IListingItem> projects;

            Tag = (string.IsNullOrEmpty(Tag) == false && Tag != "all") ? Tag : string.Empty;

            var skip = PageNumber * _maxPageSize;
            var take = _maxPageSize;

            if (IsCategoryListing == false)
            {
                if (string.IsNullOrEmpty(Tag))
                {
                    count = projectsProvider.Count();
                    projects = projectsProvider.GetAllListings(skip, take, true).OrderByDescending(x => x.Downloads).ToList();
                }
                else
                {
                    count = projectsProvider.Count(Tag);
                    projects = projectsProvider.GetListingsByTag(Tag, skip, take, true).ToList();
                }
            }
            else
            {
                var categoryProvider = (ICategoryProvider)MarketplaceProviderManager.Providers["CategoryProvider"];
                var category = categoryProvider.GetCurrent();

                count = string.IsNullOrEmpty(ListingType) == false
                    ? projectsProvider.Count(category)
                    : projectsProvider.Count(category, ListingType.GetListingTypeFromString());

                Trace.Write("begin get category");

                projects = string.IsNullOrEmpty(ListingType)
                    ? projectsProvider.GetListingsByCategory(category, skip, take, true).ToList()
                    : projectsProvider.GetListingsByCategory(category, ListingType.GetListingTypeFromString(), skip, take, true).ToList();

                Trace.Write("end get category");
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

            Listing.DataSource = projects;
            Listing.DataBind();
        }
    }
}