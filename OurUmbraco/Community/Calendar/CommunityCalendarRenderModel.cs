namespace OurUmbraco.Community.Calendar
{

    public class CommunityCalendarRenderModel
    {

        public int Year { get; set; }

        public bool HasYear
        {
            get { return Year != null; }
        }

        public string SubTitle { get; set; }

        public CommunityCalendarItem[] Items { get; set; }

        public bool HasItems
        {
            get { return Items != null && Items.Length > 0; }
        }

    }

}