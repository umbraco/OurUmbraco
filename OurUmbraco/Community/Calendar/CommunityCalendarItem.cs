using System;
using Skybrud.Essentials.Locations;
using Skybrud.Essentials.Time;

namespace OurUmbraco.Community.Calendar
{

    public class CommunityCalendarItem
    {

        public CommunityCalendarItemType Type { get; protected set; }

        public EssentialsDateTime StartDate { get; set; }

        public EssentialsDateTime EndDate { get; set; }

        public bool HasEndDate
        {
            get { return EndDate != null; }
        }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public bool HasSubTitle
        {
            get { return !String.IsNullOrWhiteSpace(SubTitle); }
        }

        public string Url { get; set; }

        public bool HasUrl
        {
            get { return !String.IsNullOrWhiteSpace(Url); }
        }

        public string LocationText { get; set; }

        public bool HasLocationText
        {
            get { return !String.IsNullOrWhiteSpace(LocationText); }
        }

        public ILocation Location { get; set; }

        public bool HasLocation
        {
            get { return Location != null; }
        }

        public string Description { get; set; }

        public bool HasDescription
        {
            get { return !String.IsNullOrWhiteSpace(Description); }
        }
        
        public CommunityCalendarItem()
        {
            Type = CommunityCalendarItemType.Other;
        }

        public CommunityCalendarItem(CommunityCalendarItemType type)
        {
            Type = type;
        }

    }

}