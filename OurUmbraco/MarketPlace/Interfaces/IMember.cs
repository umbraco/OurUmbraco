using System;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IMember
    {
        int Id { get; set; }
        Guid UniqueId { get; set; }
        string Name { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        bool IsVerified { get; set; }
        bool IsDeliVendor { get; set; }
        DateTime LastLoginDate { get; set; }
        int ReputationTotal {get;set;}
        int ReputationCurrent {get;set;}
        int ForumPosts{get;set;}
        string Location {get;set;}
        string Company{get;set;}
        string Twitter{get;set;}
		string Flickr{get;set;}
        string Latitude{get;set;}
        string Longitude{get;set;}
        DateTime LastMeetupSuggestDate{get;set;}
        int LastMeetupTopicId{get;set;}
        string Profile{get;set;}
        string Avatar{get;set;}
        string LatestIp{get;set;}
        bool Blocked{get;set;}
        bool BugMeNot{get;set;}
        string[] Groups{get;set;}
        DateTime TermsOfServiceAcceptanceDate{get;set;}
        bool CreatedByDeli { get; set; }
        int Threshold{get;set;}
        string CompanyVATNumber { get; set; }
        bool VatInvalid { get; set; }
        string CompanyCountry { get; set; }
        string CompanyAddress { get; set; }
        string CompanyInvoiceEmail { get; set; }


    }
}
