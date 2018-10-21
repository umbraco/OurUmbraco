using Skybrud.Essentials.Locations;
using Skybrud.Essentials.Time;

namespace OurUmbraco.Community.Calendar
{
    public class CommunityCalendarItem
    {
        public CommunityCalendarItemType Type { get; protected set; }

        public EssentialsDateTime StartDate { get; set; }

        public EssentialsDateTime EndDate { get; set; }

        public bool HasEndDate => EndDate != null;

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public bool HasSubTitle => string.IsNullOrWhiteSpace(SubTitle) == false;

        public string Url { get; set; }

        public string Icon { get; set; }

        public bool HasIcon => string.IsNullOrWhiteSpace(Icon) == false;

        public bool HasUrl => string.IsNullOrWhiteSpace(Url) == false;

        public string LocationText { get; set; }

        public bool HasLocationText => string.IsNullOrWhiteSpace(LocationText) == false;

        public ILocation Location { get; set; }

        public bool HasLocation => Location != null;

        public string Description { get; set; }

        public bool HasDescription => string.IsNullOrWhiteSpace(Description) == false;

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