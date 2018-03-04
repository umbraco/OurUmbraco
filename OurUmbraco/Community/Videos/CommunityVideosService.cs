using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Skybrud.Essentials.Json;
using Skybrud.Social.Google.Common;
using Skybrud.Social.Google.YouTube.Models.Videos;
using Skybrud.Social.Google.YouTube.Options.PlaylistItems;
using Skybrud.Social.Google.YouTube.Options.Playlists;
using Skybrud.Social.Google.YouTube.Options.Videos;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace OurUmbraco.Community.Videos
{

    public class CommunityVideosService
    {

        protected GoogleService Api { get; private set; }

        public CommunityVideosService()
        {
            Api = GoogleService.CreateFromServerKey(ConfigurationManager.AppSettings["GoogleServerKey"]);
        }

        private bool ExistsOnDiskAndIsUpToDate(string id)
        {

            string dir = IOHelper.MapPath("~/App_Data/TEMP/YouTube/");
            string path = dir + "Video_" + id + ".json";
            return File.Exists(path) && File.GetLastWriteTimeUtc(path) >= DateTime.UtcNow.AddDays(-1);

        }

        public YouTubeVideo LoadYouTubeVideo(string videoId)
        {
            string dir = IOHelper.MapPath("~/App_Data/TEMP/YouTube/");
            string path = dir + "Video_" + videoId + ".json";
            return JsonUtils.LoadJsonObject(path, YouTubeVideo.Parse);
        }

        public YouTubeVideo[] LoadYouTubePlaylistVideos(string playlistId)
        {
            string dir = IOHelper.MapPath("~/App_Data/TEMP/YouTube/");
            string path = dir + "Playlist_" + playlistId + "_Videos.json";
            return JsonUtils.LoadJsonArray(path, YouTubeVideo.Parse);
        }

        public void UpdateYouTubePlaylistVideos(string playlistId)
        {

            // Make sure we have a TEMP directory
            string dir = IOHelper.MapPath("~/App_Data/TEMP/YouTube/");
            Directory.CreateDirectory(dir);

            // Make an initial request to get information about the playlist
            var response1 = Api.YouTube.Playlists.GetPlaylists(new YouTubeGetPlaylistListOptions
            {
                Ids = new[] { playlistId }
            });

            // Get a reference to the playlist (using "Single" as there should be exactly one playlist)
            var playlist = response1.Body.Items.Single();

            // Save the playlist to the disk
            JsonUtils.SaveJsonObject(dir + "Playlist_" + playlist.Id + ".json", playlist);

            // List of all video IDs
            List<string> ids = new List<string>();

            // Initialize the options for getting the playlist items
            var playlistItemsOptions = new YouTubeGetPlaylistItemListOptions(playlistId)
            {

                // Maximum allowed value is 50
                MaxResults = 50

            };

            int page = 0;
            while (page < 10)
            {

                // Get the playlist items
                var response2 = Api.YouTube.PlaylistItems.GetPlaylistItems(playlistItemsOptions);

                // Append each video ID to the list
                foreach (var item in response2.Body.Items)
                {
                    ids.Add(item.VideoId);
                }

                // Break the loop if there are no additional pages
                if (String.IsNullOrWhiteSpace(response2.Body.NextPageToken))
                {
                    break;
                }

                // Update the options with the page token
                playlistItemsOptions.PageToken = response2.Body.NextPageToken;

                page++;

            }

            // Iterate through groups of IDs (maximum 50 items per group)
            foreach (var group in ids.Where(x => !ExistsOnDiskAndIsUpToDate(x)).InGroupsOf(50))
            {

                // Initialize the video options
                var videosOptions = new YouTubeGetVideoListOptions
                {
                    Ids = group.ToArray(),
                    Part = YouTubeVideoParts.Snippet + YouTubeVideoParts.ContentDetails + YouTubeVideoParts.Statistics
                };

                // Make a request to the APi to get video information
                var res3 = Api.YouTube.Videos.GetVideos(videosOptions);

                // Iterate through the videos
                foreach (var video in res3.Body.Items)
                {

                    // Save the video to the disk
                    string path = dir + "Video_" + video.Id + ".json";
                    JsonUtils.SaveJsonObject(path, video);

                }

            }

            // Load the videos from the individual files, and save them to a common file
            JsonUtils.SaveJsonArray(dir + "Playlist_" + playlistId + "_Videos.json", ids.Select(LoadYouTubeVideo).WhereNotNull());

        }

    }

}