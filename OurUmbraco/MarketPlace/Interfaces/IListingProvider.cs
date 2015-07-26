using System;
using System.Collections.Generic;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IListingProvider
    {
        IEnumerable<IListingItem> GetAllListings(bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetAllListings(int skip, int take, bool optimized = false, bool all = false);
        IListingItem GetListing(int id, bool optimized = false, int projectKarma = -1);
        IListingItem GetListing(Guid guid, bool optimized = false);
        IListingItem GetCurrentListing();

        
        IEnumerable<IListingItem> GetListingsByCategory(ICategory category, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByCategory(ICategory category, int skip, int take, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByCategory(ICategory category, ListingType listingType, int skip, int take, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByTag(string tag, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByTag(string tag, int skip, int take, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByKarma(int skip, int take, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByPopularity(int skip, int take, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByPopularity(int skip, int take, ListingType listingType, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByLatest(int skip, int take, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByLatest(int skip, int take, ListingType listingType, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByVendor(int vendorId, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByDownload(int returnCount, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsByType(ListingType listingType, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetTopPaidListings(int skip = 0, int count = 5, bool optimized = false, bool all = false);
        IEnumerable<IListingItem> GetListingsForContributor(int memberId, bool optimized = false, bool all = false);
        int Count(bool all = false);
        int Count(ListingType listingType, bool all = false);
        int Count(string tag, bool all = false);
        int Count(ICategory category, bool all = false);
        int Count(ICategory category, ListingType listingType, bool all = false);
        int CountKarmaListings();
        void IncrementProjectViews(IListingItem listingItem);

        string GetListingUrl(int id);

        void SaveOrUpdate(IListingItem listingItem);
        void RemoveListingItem(IListingItem listingItem);

        IEnumerable<IMember> GetContributorsForListing(int p);
    }
}
