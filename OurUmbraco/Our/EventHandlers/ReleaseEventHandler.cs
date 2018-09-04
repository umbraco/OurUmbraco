using System.Linq;
using Hangfire;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace OurUmbraco.Our.EventHandlers
{
    public class ReleaseEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Publishing += ContentService_Publishing;
        }

        private static void ContentService_Publishing(Umbraco.Core.Publishing.IPublishingStrategy sender, Umbraco.Core.Events.PublishEventArgs<Umbraco.Core.Models.IContent> e)
        {
            foreach (var item in e.PublishedEntities)
            {
                if (item.ContentType.Alias != "Release")
                    continue;

                var releaseStatus = item.Properties.FirstOrDefault(x => x.Alias == "recommendedRelease");
                if (releaseStatus?.Value != null && releaseStatus.Value.ToString() == "1")
                    RecurringJob.Trigger("ReleasesService.GenerateReleasesCache");
            }
        }
    }
}
