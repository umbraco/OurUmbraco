using System;
using OurUmbraco.Community.People;
using Skybrud.Essentials.Security;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace OurUmbraco.Community.Calendar
{
    public class ContentCalendarItem : CommunityCalendarItem
    {
        public ContentCalendarItem(IPublishedContent contentItem)
        {
            if (Enum.TryParse(contentItem.GetPropertyValue<string>("calendarItemType"), out CommunityCalendarItemType itemType) == false)
            {
                itemType = CommunityCalendarItemType.Other;
            }

            Type = itemType;
            Title = contentItem.Name;
            SubTitle = contentItem.GetPropertyValue<string>("subTitle");
            Description = contentItem.GetPropertyValue<string>("bodyText");
            StartDate = contentItem.GetPropertyValue<DateTime>("start");
            EndDate = contentItem.GetPropertyValue<DateTime>("end");
            LocationText = contentItem.GetPropertyValue<string>("location");
            Url = contentItem.GetPropertyValue<string>("url");
            
            var avatarService = new AvatarService();
            string img;

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var iconPropertyValue = contentItem.GetPropertyValue<int>("icon");
            var icon = umbracoHelper.TypedMedia(iconPropertyValue);

            if (string.IsNullOrEmpty(icon?.Url))
            {
                var fakeHash = SecurityUtils.GetMd5Hash(contentItem.Id + "");

                var avatarPath = avatarService.GetFakeAvatar(fakeHash);
                img = avatarService.GetImgWithSrcSet(avatarPath, contentItem.Name, 60);
            }
            else
            {
                img = avatarService.GetImgWithSrcSet(icon.Url, contentItem.Name, 60);
            }

            Icon = img;
        }
    }
}