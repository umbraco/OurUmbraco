using System;

namespace OurUmbraco.Announcements.Models
{
    public class AnnouncementDisplayModel
    {
        public Guid Id { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public string AnnouncementType { get; set; }
        public string CssClass { get; set; }
        public string Area { get; set; }
        public bool Permanent { get; set; }
        public string GeoLocation { get; set; }

    }
}
