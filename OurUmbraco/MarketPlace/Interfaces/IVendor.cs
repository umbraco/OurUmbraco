using System;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IVendor
    {
        IMember Member { get; set; }
        string VendorCompanyName { get; set; }
        string VendorDescription { get; set; }
        IMediaFile VendorLogo { get; set; }
        string VendorUrl { get; set; }
        string SupportUrl { get; set; }
        string BillingContactEmail { get; set; }
        string SupportContactEmail { get; set; }
        string VendorCountry { get; set; }
        string BaseCurrency { get; set; }
        string PayPalAccount { get; set; }
        string IBAN { get; set; }
        string SWIFT { get; set; }
        string BSB { get; set; }
        string AccountNumber { get; set; }
        string VATNumber { get; set; }
        string TaxId { get; set; }
        int EconomicId { get; set; }
        DateTime DeliTermsAgreementDate { get; set; }
    }
}
