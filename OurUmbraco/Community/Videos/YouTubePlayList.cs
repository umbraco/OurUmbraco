namespace OurUmbraco.Community.Videos
{
    public class YouTubeInfo
    {
        public Playlist[] PlayLists { get; set; }
    }

    public class Playlist
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}
