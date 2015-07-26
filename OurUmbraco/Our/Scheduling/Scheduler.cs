using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace OurUmbraco.Our.Scheduling
{
    /// <summary>
    /// Used to do the scheduling for tasks, publishing, etc...
    /// </summary>
    /// <remarks>
    /// All tasks are run in a background task runner which is web aware and will wind down the task correctly instead of killing it completely when
    /// the app domain shuts down.
    /// </remarks>
    internal sealed class Scheduler : ApplicationEventHandler
    {
        private static Timer _youTrackTimer;
        private static volatile bool _started;
        private static readonly object Locker = new object();

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (umbracoApplication.Context == null)
                return;

            //subscribe to app init so we can subsribe to the application events
            UmbracoApplicationBase.ApplicationInit += (sender, args) =>
            {
                var app = (HttpApplication)sender;

                //subscribe to the end of a successful request (a handler actually executed)
                app.PostRequestHandlerExecute += (o, eventArgs) =>
                {
                    if (_started == false)
                    {
                        lock (Locker)
                        {
                            if (_started == false)
                            {
                                _started = true;
                                LogHelper.Debug<Scheduler>(() => "Initializing the scheduler");
                                
                                // note
                                // must use the single-parameter constructor on Timer to avoid it from being GC'd
                                // also make the timer static to ensure further GC safety
                                // read http://stackoverflow.com/questions/4962172/why-does-a-system-timers-timer-survive-gc-but-not-system-threading-timer

                                // ping/keepalive - no need for a background runner - does not need to be web aware, ok if the app domain dies
                                _youTrackTimer = new Timer(state => YouTrackSync.Start());
                                _youTrackTimer.Change(60000, 600000);
                            }
                        }
                    }
                };
            };
        }
    }
}
