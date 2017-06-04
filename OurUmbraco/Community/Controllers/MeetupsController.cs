using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Community.Models;
using Skybrud.Social.Meetup;
using Skybrud.Social.Meetup.Models.Events;
using Skybrud.Social.Meetup.Responses.Events;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Core.Cache;

namespace OurUmbraco.Community.Controllers {

    public class MeetupsController : SurfaceController {

        public ActionResult GetEvents() {

            MeetupEventsModel model = new MeetupEventsModel {
                Events = new MeetupEvent[0]
            };

            try {

                string configPath = Server.MapPath("~/config/MeetupUmbracoGroups.txt");
                if (!System.IO.File.Exists(configPath)) {
                    LogHelper.Debug<MeetupsController>("Config file was not found: " + configPath);
                    return PartialView("~/Views/Partials/Home/Meetups.cshtml", model);
                }

                // Get the alias (urlname) of each group from the config file
                string[] aliases = System.IO.File.ReadAllLines(configPath);

                model.Events =
                    ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<MeetupEvent[]>("UmbracoSearchedMeetups",
                        () => {

                            // Initialize a new service instance (we don't specify an API key since we're accessing public data) 
                            MeetupService service = new MeetupService();

                            List<MeetupEvent> aggregated = new List<MeetupEvent>();

                            foreach (string alias in aliases) {

                                try {
                                    
                                    // Make the call to the meetup.com API to get upcoming events
                                    MeetupGetEventsResponse res = service.Events.GetEvents(alias);

                                    // TODO: We should probably have some pagination, as the API only returns the first 20 events for a group (none of the groups currently have that much)

                                    // Append the events from the reasponse to the aggregated list
                                    aggregated.AddRange(res.Body);


                                } catch (Exception ex) {
                                    LogHelper.Error<MeetupsController>("Could not get events from meetup.com for group with alias: " + alias, ex);
                                }
                            
                            }

                            return aggregated.OrderBy(x => x.Time).ToArray();

                        }, TimeSpan.FromMinutes(30));

                
            } catch (Exception ex) {
                LogHelper.Error<MeetupsController>("Could not get events from meetup.com", ex);
            }

            return PartialView("~/Views/Partials/Home/Meetups.cshtml", model);

        }

    }

}