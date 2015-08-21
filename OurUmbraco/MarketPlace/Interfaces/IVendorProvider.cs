using System;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IVendorProvider
    {
        IVendor GetVendorById(int id);
        IVendor GetVendorByGuid(Guid guid);
        IVendor GetVendorByEmail(string email);
        void SaveOrUpdate(IVendor member);
    }
}
