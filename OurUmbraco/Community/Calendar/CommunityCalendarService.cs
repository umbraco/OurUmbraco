﻿using System;
using System.Collections.Generic;
using System.Linq;
using OurUmbraco.Community.Meetup;
using Skybrud.Essentials.Enums;
using Skybrud.Essentials.Time;
using Umbraco.Web;

namespace OurUmbraco.Community.Calendar
{

    public class CommunityCalendarService
    {

        public CommunityCalendarItem[] GetUpcomingItems(int parentId)
        {
            return GetAllItems(parentId).Where(x => x.StartDate >= EssentialsTime.Today).ToArray();
        }

        public CommunityCalendarItem[] GetItemsByYear(int year, int parentId)
        {
            return GetAllItems(parentId).Where(x => x.StartDate.Year == year).ToArray();
        }

        public CommunityCalendarItemType[] GetTypes()
        {
            return EnumUtils.GetEnumValues<CommunityCalendarItemType>();
        }

        private IEnumerable<CommunityCalendarItem> GetAllItems(int parentId)
        {
            var meetupService = new MeetupService();

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var parentNode = umbracoHelper.TypedContent(parentId);

            var items = new List<CommunityCalendarItem>();
            items.AddRange(parentNode.Children.Select(x => new ContentCalendarItem(x)));
            foreach (var meetup in meetupService.GetCachedUpcomingMeetups())
            {
                var nextMeetup = meetup.Events.OrderBy(x => x.DateTime).FirstOrDefault();
                
                if (nextMeetup == null) 
                    continue;
                
                var calendarItem = new CommunityCalendarItem()
                {
                        
                    Description = nextMeetup.Description,
                    Title = nextMeetup.Title,
                    Url = nextMeetup.Url
                        

                };
                items.Add(calendarItem);
            }
            
            return items.OrderBy(x => x.StartDate);
        }
    }
}