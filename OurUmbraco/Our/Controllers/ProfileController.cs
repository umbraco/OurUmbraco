using System;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using OurUmbraco.Community.People;
using OurUmbraco.Our.Models;
using Skybrud.Social.GitHub.OAuth;
using Skybrud.Social.GitHub.Responses.Authentication;
using Skybrud.Social.GitHub.Responses.Users;
using Skybrud.Social.OAuth.Models;
using Skybrud.Social.OAuth.Responses;
using Skybrud.Social.Twitter.Models.Account;
using Skybrud.Social.Twitter.OAuth;
using Skybrud.Social.Twitter.Options.Account;
using Skybrud.Social.Twitter.Responses.Account;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class ProfileController: SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render()
        {
            var memberService = Services.MemberService;
            var member = memberService.GetById(Members.GetCurrentMemberId());
            var avatarService = new AvatarService();
            var avatarPath = avatarService.GetMemberAvatar(member);
            var avatarHtml = avatarService.GetImgWithSrcSet(avatarPath, member.Name, 100);

            var profileModel = new ProfileModel
            {
                Id = member.Id,
                Name = member.Name,
                Email = member.Email,
                Bio = member.GetValue<string>("profileText"),
                Location = member.GetValue<string>("location"),
                Company = member.GetValue<string>("company"),
                TwitterAlias = member.GetValue<string>("twitter"),
                Avatar = avatarPath,
                AvatarHtml = avatarHtml,
                GitHubUsername = member.GetValue<string>("github"),

                Latitude = member.GetValue<string>("latitude"), //TODO: Parse/cleanup bad data - auto remove it for user & resave the member?
                Longitude = member.GetValue<string>("longitude")
            };

            return PartialView("~/Views/Partials/Members/Profile.cshtml", profileModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkGitHub() {

            string rootUrl = Request.Url.GetLeftPart(UriPartial.Authority);

            GitHubOAuthClient client = new GitHubOAuthClient();
            client.ClientId = WebConfigurationManager.AppSettings["GitHubClientId"];
            client.ClientSecret = WebConfigurationManager.AppSettings["GitHubClientSecret"];
            client.RedirectUri = rootUrl + "/umbraco/surface/Profile/LinkGitHub";

            // Set the state (a unique/random value)
            string state = Guid.NewGuid().ToString();
            Session["GitHub_" + state] = "Unicorn rainbows";

            // Construct the authorization URL
            string authorizatioUrl = client.GetAuthorizationUrl(state);

            // Redirect the user to the OAuth dialog
            return Redirect(authorizatioUrl);

        }

        [HttpGet]
        public ActionResult LinkGitHub(string state, string code = null)
        {

            // Get the member of the current ID
            int memberId = Members.GetCurrentMemberId();
            if (memberId <= 0) return GetErrorResult("Oh noes! An error happened.");

            try
            {

                IPublishedContent profilePage = Umbraco.TypedContent(1057);
                if (profilePage == null) return GetErrorResult("Oh noes! This really shouldn't happen.");

                // Initialize the OAuth client
                GitHubOAuthClient client = new GitHubOAuthClient
                {
                    ClientId = WebConfigurationManager.AppSettings["GitHubClientId"],
                    ClientSecret = WebConfigurationManager.AppSettings["GitHubClientSecret"]
                };

                // Validate state - Step 1
                if (String.IsNullOrWhiteSpace(state))
                {
                    LogHelper.Info<ProfileController>("No OAuth state specified in the query string.");
                    return GetErrorResult("No state specified in the query string.");
                }

                // Validate state - Step 2
                string session = Session["GitHub_" + state] as string;
                if (String.IsNullOrWhiteSpace(session))
                {
                    LogHelper.Info<ProfileController>("Failed finding OAuth session item. Most likely the session expired.");
                    return GetErrorResult("Session expired? Please click the link below and try to link with your GitHub account again ;)");
                }

                // Remove the state from the session
                Session.Remove("GitHub_" + state);

                // Exchange the auth code for an access token
                GitHubTokenResponse accessTokenResponse;
                try
                {
                    accessTokenResponse = client.GetAccessTokenFromAuthorizationCode(code);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ProfileController>("Unable to retrieve access token from GitHub API", ex);
                    return GetErrorResult("Oh noes! An error happened.");
                }

                // Initialize a new service instance from the retrieved access token
                var service = Skybrud.Social.GitHub.GitHubService.CreateFromAccessToken(accessTokenResponse.Body.AccessToken);

                // Get some information about the authenticated GitHub user
                GitHubGetUserResponse userResponse;
                try
                {
                    userResponse = service.User.GetUser();
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ProfileController>("Unable to get user information from the GitHub API", ex);
                    return GetErrorResult("Oh noes! An error happened.");
                }

                // Get the GitHub username from the API response
                string githubUsername = userResponse.Body.Login;

                // Get a reference to the member searcher
                BaseSearchProvider searcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];

                // Initialize new search criteria for the GitHub username
                ISearchCriteria criteria = searcher.CreateSearchCriteria();
                criteria = criteria.RawQuery($"github:{githubUsername}");

                // Check if there are other members with the same GitHub username
                foreach (var result in searcher.Search(criteria))
                {
                    if (result.Id != memberId)
                    {
                        LogHelper.Info<ProfileController>("Failed setting GitHub username for user with ID " + memberId + ". Username is already used by member with ID " + result.Id + ".");
                        return GetErrorResult("Another member already exists with the same GitHub username.");
                    }
                }

                // Get the member from the member service
                var ms = ApplicationContext.Services.MemberService;
                var mem = ms.GetById(memberId);

                // Update the "github" property and save the value
                mem.SetValue("github", githubUsername);
                mem.SetValue("githubId", userResponse.Body.Id);
                mem.SetValue("githubData", userResponse.Body.JObject.ToString());
                ms.Save(mem);

                // Clear the runtime cache for the member
                ApplicationContext.ApplicationCache.RuntimeCache.ClearCacheItem("MemberData" + mem.Username);

                // Redirect the member back to the profile page
                return RedirectToUmbracoPage(1057);

            }
            catch (Exception ex)
            {
                LogHelper.Error<ProfileController>("Unable to link with GitHub user for member with ID " + memberId, ex);
                return GetErrorResult("Oh noes! An error happened.");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkTwitter()
        {

            string rootUrl = Request.Url.GetLeftPart(UriPartial.Authority);

            TwitterOAuthClient client = new TwitterOAuthClient();
            client.ConsumerKey = WebConfigurationManager.AppSettings["twitterConsumerKey"];
            client.ConsumerSecret = WebConfigurationManager.AppSettings["twitterConsumerSecret"];
            client.Callback = rootUrl + "/umbraco/surface/Profile/LinkTwitter";

            // Make the request to the Twitter API to get a request token
            SocialOAuthRequestTokenResponse response = client.GetRequestToken();

            // Get the request token from the response body
            TwitterOAuthRequestToken requestToken = (TwitterOAuthRequestToken) response.Body;

            // Save the token information to the session so we can grab it later
            Session[requestToken.Token] = requestToken;

            // Redirect the user to the authentication page at Twitter.com
            return Redirect(requestToken.AuthorizeUrl);

        }
        
        [HttpGet]
        public ActionResult LinkTwitter(string oauth_token, string oauth_verifier)
        {

            // Get the member of the current ID
            int memberId = Members.GetCurrentMemberId();
            if (memberId <= 0) return GetErrorResult("Oh noes! An error happened.");

            try
            {

                IPublishedContent profilePage = Umbraco.TypedContent(1057);
                if (profilePage == null) return GetErrorResult("Oh noes! This really shouldn't happen.");

                // Initialize the OAuth client
                TwitterOAuthClient client = new TwitterOAuthClient();
                client.ConsumerKey = WebConfigurationManager.AppSettings["twitterConsumerKey"];
                client.ConsumerSecret = WebConfigurationManager.AppSettings["twitterConsumerSecret"];

                // Grab the request token from the session
                SocialOAuthRequestToken requestToken = Session[oauth_token] as SocialOAuthRequestToken;
                if (requestToken == null) return GetErrorResult("Session expired? Please click the link below and try to link with your Twitter account again ;)");

                // Update the OAuth client with information from the request token
                client.Token = requestToken.Token;
                client.TokenSecret = requestToken.TokenSecret;

                // Make the request to the Twitter API to get the access token
                SocialOAuthAccessTokenResponse response = client.GetAccessToken(oauth_verifier);

                // Get the access token from the response body
                TwitterOAuthAccessToken accessToken = (TwitterOAuthAccessToken)response.Body;

                // Update the OAuth client properties
                client.Token = accessToken.Token;
                client.TokenSecret = accessToken.TokenSecret;

                // Initialize a new service instance from the OAUth client
                var service = Skybrud.Social.Twitter.TwitterService.CreateFromOAuthClient(client);

                // Get some information about the authenticated Twitter user
                TwitterAccount user;
                try
                {

                    // Initialize the options for the request (we don't need the status)
                    var options = new TwitterVerifyCrendetialsOptions
                    {
                        SkipStatus = true
                    };

                    // Make the request to the Twitter API
                    var userResponse = service.Account.VerifyCredentials(options);

                    // Update the "user" variable
                    user = userResponse.Body;

                }
                catch (Exception ex)
                {
                    LogHelper.Error<ProfileController>("Unable to get user information from the Twitter API", ex);
                    return GetErrorResult("Oh noes! An error happened.");
                }

                // Get a reference to the member searcher
                BaseSearchProvider searcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];

                // Initialize new search criteria for the Twitter screen name
                ISearchCriteria criteria = searcher.CreateSearchCriteria();
                criteria = criteria.RawQuery($"twitter:{user.ScreenName}");

                // Check if there are other members with the same Twitter screen name
                foreach (var result in searcher.Search(criteria))
                {
                    if (result.Id != memberId)
                    {
                        LogHelper.Info<ProfileController>("Failed setting Twitter screen name for user with ID " + memberId + ". Username is already used by member with ID " + result.Id + ".");
                        return GetErrorResult("Another member already exists with the same Twitter screen name.");
                    }
                }

                // Get the member from the member service
                var ms = ApplicationContext.Services.MemberService;
                var mem = ms.GetById(memberId);

                // Update the "twitter" property and save the value
                mem.SetValue("twitter", user.ScreenName);
                mem.SetValue("twitterId", user.IdStr);
                mem.SetValue("twitterData", user.JObject.ToString());
                ms.Save(mem);

                // Clear the runtime cache for the member
                ApplicationContext.ApplicationCache.RuntimeCache.ClearCacheItem("MemberData" + mem.Username);

                // Redirect the member back to the profile page
                return RedirectToUmbracoPage(1057);

            }
            catch (Exception ex)
            {
                LogHelper.Error<ProfileController>("Unable to link with Twitter user for member with ID " + memberId, ex);
                return GetErrorResult("Oh noes! An error happened.");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnlinkGitHub()
        {

            var ms = Services.MemberService;
            var mem = ms.GetById(Members.GetCurrentMemberId());
            mem.SetValue("github", "");
            ms.Save(mem);

            var memberPreviousUserName = mem.Username;

            ApplicationContext.ApplicationCache.RuntimeCache.ClearCacheItem("MemberData" + memberPreviousUserName);

            return RedirectToCurrentUmbracoPage();

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnlinkTwitter()
        {

            var ms = Services.MemberService;
            var mem = ms.GetById(Members.GetCurrentMemberId());
            mem.SetValue("twitter", "");
            ms.Save(mem);

            var memberPreviousUserName = mem.Username;

            ApplicationContext.ApplicationCache.RuntimeCache.ClearCacheItem("MemberData" + memberPreviousUserName);

            return RedirectToCurrentUmbracoPage();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleSubmit(ProfileModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var ms = Services.MemberService;
            var mem = ms.GetById(Members.GetCurrentMemberId());
            
            if (mem.Email != model.Email && ms.GetByEmail(model.Email) != null)
            {
                ModelState.AddModelError("Email", "A Member with that email already exists");
                return CurrentUmbracoPage();
            }

            var memberPreviousUserName = mem.Username;

            if (model.Password != model.RepeatPassword)
            {
                ModelState.AddModelError("Password", "Passwords need to match");
                ModelState.AddModelError("RepeatPassword", "Passwords need to match");
                return CurrentUmbracoPage();
            }
            
            mem.Name = model.Name ;
            mem.Email = model.Email;
            mem.Username = model.Email;
            mem.SetValue("profileText",model.Bio);
            mem.SetValue("location",model.Location);
            mem.SetValue("company",model.Company);
            
            // Assume it's valid lat/lon data posted - as its a hidden field that a Google Map will update the lat & lon of hidden fields when marker moved
            mem.SetValue("latitude", model.Latitude); 
            mem.SetValue("longitude", model.Longitude);
            
            var avatarService = new AvatarService();
            var avatarImage = avatarService.GetMemberAvatarImage(HostingEnvironment.MapPath($"~{model.Avatar}"));
            if (avatarImage != null && (avatarImage.Width < 400 || avatarImage.Height < 400))
            {
                // Save the rest of the data, but not the new avatar yet as it's too small
                ms.Save(mem);
                ModelState.AddModelError("Avatar", "Please upload an avatar that is at least 400x400 pixels");
                return CurrentUmbracoPage();
            }

            mem.SetValue("avatar", model.Avatar);
            ms.Save(mem);

            if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.RepeatPassword) && model.Password == model.RepeatPassword)
                ms.SavePassword(mem, model.Password);

            ApplicationContext.ApplicationCache.RuntimeCache.ClearCacheItem("MemberData" + memberPreviousUserName);
            TempData["success"] = true;

            return RedirectToCurrentUmbracoPage();
        }

        private ActionResult GetErrorResult(string message)
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<style> body { margin: 10px; font-family: sans-serif; } </style>");
            sb.AppendLine(message);
            sb.AppendLine("<p><a href=\"/member/profile/\">Return to your profile</a></p>");


            ContentResult result = new ContentResult();
            result.ContentType = "text/html";
            result.Content = sb.ToString();
            return result;

        }

    }
}
