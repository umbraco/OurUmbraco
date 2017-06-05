using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Community.Models;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Social.Meetup;
using Skybrud.Social.Meetup.Models.Events;
using Skybrud.Social.Meetup.Models.Groups;
using Skybrud.Social.Meetup.Responses.Events;
using Skybrud.Social.Meetup.Responses.Groups;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Core.Cache;

namespace OurUmbraco.Community.Controllers {

    public class MeetupsController : SurfaceController {

        public ActionResult GetEvents() {

            MeetupEventsModel model = new MeetupEventsModel {
                Items = new MeetupItem[0]
            };

            try {

                string configPath = Server.MapPath("~/config/MeetupUmbracoGroups.txt");
                if (!System.IO.File.Exists(configPath)) {
                    LogHelper.Debug<MeetupsController>("Config file was not found: " + configPath);
                    return PartialView("~/Views/Partials/Home/Meetups.cshtml", model);
                }

                // Get the alias (urlname) of each group from the config file
                string[] aliases = System.IO.File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();

                model.Items =
                    ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<MeetupItem[]>("UmbracoSearchedMeetups",
                        () => {

                            // Initialize a new service instance (we don't specify an API key since we're accessing public data) 
                            MeetupService service = new MeetupService();

                            List<MeetupItem> items = new List<MeetupItem>();

                            foreach (string alias in aliases) {

                                try {

                                    // Get information about the group
                                    MeetupGroup group = service.Groups.GetGroup(alias).Body;

                                    if (group.JObject.HasValue("next_event")) {

                                        string nextEventId = group.JObject.GetString("next_event.id");

                                        // Make the call to the Meetup.com API to get upcoming events
                                        MeetupGetEventsResponse res = service.Events.GetEvents(alias);

                                        // Get the next event(s)
                                        MeetupEvent nextEvent = res.Body.FirstOrDefault(x => x.Id == nextEventId);
                                        
                                        // Append the first event of the group
                                        if (nextEvent != null) items.Add(new MeetupItem(group, nextEvent));

                                    }

                                } catch (Exception ex) {
                                    LogHelper.Error<MeetupsController>("Could not get events from meetup.com for group with alias: " + alias, ex);
                                }
                            
                            }

                            return items.OrderBy(x => x.Event.Time).ToArray();

                        }, TimeSpan.FromMinutes(30));

                
            } catch (Exception ex) {
                LogHelper.Error<MeetupsController>("Could not get events from meetup.com", ex);
            }

            return PartialView("~/Views/Partials/Home/Meetups.cshtml", model);

        }

    }

}