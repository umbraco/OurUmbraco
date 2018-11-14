using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using Devcorner.NIdenticon;
using Devcorner.NIdenticon.BrushGenerators;
using OurUmbraco.Our;
using OurUmbraco.Our.Extensions;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace OurUmbraco.Community.People
{
    public class AvatarService
    {
        public string GetMemberAvatar(IMember member)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var publishedMember = umbracoHelper.TypedMember(member.Id);
            return GetMemberAvatar(publishedMember);
        }

        public string GetMemberAvatar(IPublishedContent member)
        {
            var memberAvatarPath = MemberAvatarPath(member);
            if (memberAvatarPath == string.Empty)
                memberAvatarPath = GenerateIdenticon(member);

            return GetCleanImagePath(memberAvatarPath);
        }

        public string GetFakeAvatar(string hash, int minSize = 360)
        {
            var avatarPath = $"/media/avatar/fake-{hash}.png";
            var savePath = HostingEnvironment.MapPath($"~{avatarPath}");

            if (savePath == null)
                throw new InvalidOperationException();

            if (System.IO.File.Exists(savePath))
                return avatarPath;

            var identiconGenerator = GetIdenticonGenerator();
            using (var output = identiconGenerator.Create(hash))
                output.Save(savePath, ImageFormat.Png);

            return avatarPath;
        }

        public string GetRemoteAvatarUrl(string url, int size)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            var host = url.TrimStart("http://").TrimStart("https://");
            return $"/remote.axd?https://{host}?width={size}&upscale=true";
        }

        public string GetImgWithSrcSet(string avatarPath, string altText, int minSize, string cacheBuster = "")
        {
            var cleanImagePath = GetCleanImagePath(avatarPath);
            if (cacheBuster != string.Empty)
                cleanImagePath = $"{cleanImagePath}?v={cacheBuster}";

            var separator = cleanImagePath.Contains("?") ? "&" : "?";

            var image = $"<img src=\"{cleanImagePath}{separator}width={minSize}&height={minSize}&mode=crop&upscale=true\" " +
                        $"srcset=\"{cleanImagePath}{separator}width={minSize * 2}&height={minSize * 2}&mode=crop&upscale=true 2x, " +
                        $"{cleanImagePath}{separator}width={minSize * 3}&height={minSize * 3}&mode=crop&upscale=true 3x\" " +
                        $"alt=\"{altText}\" />";

            return image;
        }

        public string GetImgWithSrcSet(IPublishedContent member, string altText, int minSize, string cacheBuster = "")
        {
            var avatarPath = GetMemberAvatar(member);
            return GetImgWithSrcSet(avatarPath, altText, minSize, cacheBuster);
        }

        internal string MemberAvatarPath(IPublishedContent member)
        {
            try
            {
                var hasAvatar = member.HasValue("avatar");
                if (hasAvatar)
                {
                    var avatarPath = member.GetPropertyValue("avatar").ToString();
                    if (avatarPath.IsLocalPath() == false)
                        // Profiles with an avatar previously set to gravatar.com will get a new avatar
                        return string.Empty;

                    var path = HostingEnvironment.MapPath(avatarPath);
                    if (System.IO.File.Exists(path))
                        return GetCleanImagePath(avatarPath);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Utils>("Could not get MemberAvatarPath", ex);
            }

            return string.Empty;
        }

        internal Image GetMemberAvatarImage(string memberAvatarPath)
        {
            if (string.IsNullOrWhiteSpace(memberAvatarPath))
                return null;

            try
            {
                return memberAvatarPath.IsLocalPath()
                    ? Image.FromFile(memberAvatarPath.Replace("%20", " ")) 
                    : null;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Utils>($"Could not create Image object from {memberAvatarPath}", ex);
            }

            return null;
        }

        internal string GenerateIdenticon(IPublishedContent memberContent)
        {
            var avatarPath = string.Empty;
            if (memberContent != null)
            {
                var memberPublishedContent = (MemberPublishedContent) memberContent;
                if (memberPublishedContent != null && memberPublishedContent.Email != null)
                {
                    avatarPath = $"/media/avatar/{memberPublishedContent.Id}.png";
                    var emailHash = CreateMd5Hash(memberPublishedContent.Email);

                    var savePath = HostingEnvironment.MapPath($"~{avatarPath}");

                    if (savePath == null)
                        throw new InvalidOperationException();

                    if (System.IO.File.Exists(savePath))
                        return avatarPath;

                    var identiconGenerator = GetIdenticonGenerator();
                    using (var output = identiconGenerator.Create(emailHash))
                        output.Save(savePath, ImageFormat.Png);
                }
            }

            return avatarPath;
        }

        private IdenticonGenerator GetIdenticonGenerator()
        {
            var foreground = GetRandomColor();
            var background = GetRandomColor();
            while (background == foreground)
                background = GetRandomColor();

            var identiconGenerator = new IdenticonGenerator()
                .WithSize(360, 360)
                .WithBlocks(8, 8)
                .WithBackgroundColor(background)
                .WithAlgorithm("MD5")
                .WithBrushGenerator(new StaticColorBrushGenerator(foreground))
                .WithBlockGenerators(IdenticonGenerator.DefaultBlockGeneratorsConfig);

            return identiconGenerator;
        }

        internal Color GetRandomColor()
        {
            var colors = new List<Color>
            {
                ColorTranslator.FromHtml("#f1f1f1"),
                ColorTranslator.FromHtml("#2f6bff"),
                ColorTranslator.FromHtml("#2EB369"),
                ColorTranslator.FromHtml("#43cfcf"),
                ColorTranslator.FromHtml("#EB7439"),
                ColorTranslator.FromHtml("#fe6561"),
                ColorTranslator.FromHtml("#fe6561"),
                ColorTranslator.FromHtml("#56f272"),
                ColorTranslator.FromHtml("#a3db78")
            };
            return colors.RandomOrder().First();
        }


        // From: https://stackoverflow.com/a/24031467/5018
        public string CreateMd5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            input = input.ToLowerInvariant();
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var stringBuilder = new StringBuilder();
                foreach (var hashByte in hashBytes)
                    stringBuilder.Append(hashByte.ToString("X2"));

                return stringBuilder.ToString().ToLowerInvariant();
            }
        }

        internal static string GetCleanImagePath(string imgPath)
        {
            var cleanImagePath = imgPath.Replace(" ", "%20").TrimStart("~");
            if (cleanImagePath.Contains("?"))
                cleanImagePath = cleanImagePath.Substring(0, cleanImagePath.IndexOf("?", StringComparison.Ordinal));

            if (cleanImagePath.StartsWith("media/"))
                // add a / in front of media to make sure the path is relative
                cleanImagePath = $"/{cleanImagePath}";

            return cleanImagePath;
        }
    }
}
