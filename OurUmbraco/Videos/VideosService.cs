using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Strings;
using Skybrud.Social.Vimeo;
using Skybrud.Social.Vimeo.Models.Videos;
using Umbraco.Core.IO;

namespace OurUmbraco.Videos
{
    public class VideosService
    {
        protected const string SaveDirectory = "~/App_Data/TEMP/";

        public void UpdateVimeoVideos(string username)
        {
            if (ConfigurationManager.AppSettings["VimeoAccessToken"] == "it's a secret.. and no, this is not the secret, this key will transformed on the build server")
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException("username");

            // Map the path to the directory
            var savePath = IOHelper.MapPath(SaveDirectory);
            if (Directory.Exists(savePath) == false)
                Directory.CreateDirectory(savePath);

            // Map the path to the JSON file
            var path = savePath + "VimeoVideos_" + username + ".json";

            // Initialize a new service from an OAuth 2.0 access token
            var vimeo = VimeoService.CreateFromAccessToken(ConfigurationManager.AppSettings["VimeoAccessToken"]);

            var page = 1;
            const int maxPages = 10;

            // Initialize a list for all the videos
            var videos = new List<VimeoVideo>();

            // Create a loop for the pages
            while (page <= maxPages)
            {
                // Make the request to the Vimeo API
                var response = vimeo.Videos.GetVideos(username, page, 100);

                // Append the videos of the response to the list
                videos.AddRange(response.Body.Data);

                // Download the thumnails for each video
                foreach (var video in videos)
                {
                    var thumbnail = video.Pictures.Sizes.FirstOrDefault(x => x.Width >= 350);

                    const string mediaRoot = "~/media/Vimeo";
                    var thumbnailFile = IOHelper.MapPath($"{mediaRoot}/{video.Id}.jpg");

                    var mediaPath = IOHelper.MapPath(mediaRoot);
                    if (Directory.Exists(mediaPath) == false)
                        Directory.CreateDirectory(mediaPath);

                    if (File.Exists(thumbnailFile))
                        continue;

                    using (var client = new WebClient())
                        client.DownloadFile(thumbnail.Link, thumbnailFile);
                }

                // Break the loop if there are no further pages
                if (response.Body.Paging.Next == null)
                    break;

                // Increment the page count
                page++;
            }

            // Save the videos as a JSON file
            JsonUtils.SaveJsonArray(path, videos);
        }

        public VimeoVideo[] GetVimeoVideosFromDisk(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException("username");

            // Make the path to the JSON file
            var path = IOHelper.MapPath(SaveDirectory + "VimeoVideos_" + username + ".json");

            // Return an empty array if the file doesn't exist
            if (!File.Exists(path))
                return new VimeoVideo[0];

            // Load and parse the JSON file
            return JsonUtils.LoadJsonArray(path, VimeoVideo.Parse);
        }

        public VideosCategory[] GroupVideosByCategoryAndYear(VimeoVideo[] videos)
        {
            var videosByTag = new Dictionary<string, VideosCategory>();

            foreach (var video in videos)
            {
                // Iterate through the tags of the video
                foreach (var tag in video.Tags)
                {
                    var pieces = tag.Canonical.ToLower().Split('-');

                    // Does the tag match the "our-{category}-{year}" syntax?
                    if (pieces.Length <= 2 || pieces[0] != "our")
                        continue;

                    // Does the last piece match the year?
                    int year;
                    if (int.TryParse(pieces[pieces.Length - 1], out year) == false)
                        continue;

                    // Get the actual category name (the tag name, but without the "our" prefix and the year)
                    var categoryName = StringUtils.FirstCharToUpper(string.Join(" ", FixAcronyms(pieces.Skip(1).Take(pieces.Length - 2).ToArray())));

                    // Get the video category (or create it if not found)
                    VideosCategory category;
                    if (videosByTag.TryGetValue(categoryName, out category) == false)
                        videosByTag.Add(categoryName, category = new VideosCategory(categoryName));

                    // Append the video to the category
                    category.AppendVideo(year, video);
                }
            }

            // Sort the groups in alphabetical order, but with "Other" as the last group
            return videosByTag.Values.OrderBy(x => (x.Name == "Other" ? "1" : "0") + x.Name).ToArray();
        }

        private static string[] FixAcronyms(string[] pieces)
        {
            for (var i = 0; i < pieces.Length; i++)
            {
                switch (pieces[i])
                {
                    case "cms":
                        pieces[i] = "CMS";
                        break;
                    case "hq":
                        pieces[i] = "HQ";
                        break;
                }
            }
            return pieces;
        }
    }
}
