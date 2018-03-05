using System.Collections.Generic;
using System.Linq;
using Skybrud.Social.Vimeo.Models.Videos;

namespace OurUmbraco.Videos
{
    public class VideosCategory
    {

        readonly List<VimeoVideo> _videos = new List<VimeoVideo>();
        readonly Dictionary<int, List<VimeoVideo>> _years = new Dictionary<int, List<VimeoVideo>>();

        public string Name { get; private set; }

        public int[] Years { get { return _years.Keys.OrderByDescending(x => x).ToArray(); } }

        public List<VimeoVideo> this[int year]
        {
            get
            {
                List<VimeoVideo> list;
                return _years.TryGetValue(year, out list) ? list : new List<VimeoVideo>();
            }
        }

        /// <summary>
        /// Gets all videos of the category (across all years).
        /// </summary>
        public IEnumerable<VimeoVideo> Videos
        {
            get { return _videos.OrderByDescending(x => x.CreatedTime); }
        }

        public VideosCategory(string name)
        {
            Name = name;
        }

        public void AppendVideo(int year, VimeoVideo video) {

            // Get or create the list of videos for "year"
            List<VimeoVideo> list;
            if (!_years.TryGetValue(year, out list)) {
                _years.Add(year, list = new List<VimeoVideo>());
            }

            list.Add(video);
            _videos.Add(video);

        }

        public bool HasYear(int year)
        {
            return _years.ContainsKey(year);
        }

        public bool TryGetYear(int year, out List<VimeoVideo> videos)
        {
            return _years.TryGetValue(year, out videos);
        }

    }

}