using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Strings;
using Skybrud.Social.Vimeo;
using Skybrud.Social.Vimeo.Models.Videos;
using Umbraco.Core.IO;

namespace OurUmbraco.Videos {

    public class VideosService
    {

        protected const string SaveDirectory = "~/App_Data/TEMP/";

        public void UpdateVimeoVideos(string username)
        {

            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentNullException("username");

            // Map the path to the directory
            string dir = IOHelper.MapPath(SaveDirectory);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Map the path to the JSON file
            string path = dir + "VimeoVideos_" + username + ".json";

            // Initialize a new service from an OAuth 2.0 access token
            var vimeo = VimeoService.CreateFromAccessToken("9307ee4ce73a77fbaf36db93cbef8d02");
            
            int page = 1;
            int maxPages = 10;

            // Initialize a list for all the videos
            var videos = new List<VimeoVideo>();

            // Create a loop for the pages
            while (page <= maxPages)
            {
                
                // Make the request to the Vimeo API
                var response = vimeo.Videos.GetVideos("umbraco", page, 100);

                // Append the videos of the response to the list
                videos.AddRange(response.Body.Data);

                // Break the loop if there are no further pages
                if (response.Body.Paging.Next == null) break;

                // Increment the page count
                page++;

            }

            // Save the videos as a JSON file
            JsonUtils.SaveJsonArray(path, videos);

        }

        public VimeoVideo[] GetVimeoVideosFromDisk(string username)
        {

            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentNullException("username");

            // Make the path to the JSON file
            string path = IOHelper.MapPath(SaveDirectory + "VimeoVideos_" + username + ".json");

            // Return an empty array if the file doesn't exist
            if (!File.Exists(path)) return new VimeoVideo[0];

            // Load and parse the JSON file
            return JsonUtils.LoadJsonArray(path, VimeoVideo.Parse);

        }

        public VideosCategory[] GroupVideosByCategoryAndYear(VimeoVideo[] videos) {

            Dictionary<string, VideosCategory> videosByTag = new Dictionary<string, VideosCategory>();

            foreach (var video in videos)
            {
                
                bool foundCategory = false;

                // Iterate through the tags of the video
                foreach (var tag in video.Tags)
                {

                    string[] pieces = tag.Canonical.ToLower().Split('-');

                    // Does the tag match the "our-{category}-{year}" syntax?
                    if (pieces.Length <= 2 || pieces[0] != "our") continue;

                    // Does the last piece match the year?
                    int year;
                    if (!Int32.TryParse(pieces[pieces.Length - 1], out year)) continue;
               
                    // Get the actual category name (the tag name, but without the "our" prefix and the year)
                    string categoryName = StringUtils.FirstCharToUpper(String.Join(" ", FixAcronyms(pieces.Skip(1).Take(pieces.Length - 2).ToArray())));

                    // Get the video category (or create it if not found)
                    VideosCategory category;
                    if (!videosByTag.TryGetValue(categoryName, out category))
                    {
                        videosByTag.Add(categoryName, category = new VideosCategory(categoryName));
                    }

                    // Append the video to the category
                    category.AppendVideo(year, video);
                    foundCategory = true;

                }

                // If we haven't found at least one matching category at this point, we append the video to the "Other" category
                if (!foundCategory)
                {
                    VideosCategory category;
                    if (!videosByTag.TryGetValue("Other", out category))
                    {
                        videosByTag.Add("Other", category = new VideosCategory("Other"));
                    }
                    category.AppendVideo(video.CreatedTime.Year, video);
                }

            }

            // Sort the groups in alphabetical order, but with "Other" as the last group
            return videosByTag.Values.OrderBy(x => (x.Name == "Other" ? "1" : "0") + x.Name).ToArray();

        }

        private string[] FixAcronyms(string[] pieces)
        {
            for (int i = 0; i < pieces.Length; i++)
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