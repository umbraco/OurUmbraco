using System;
using OurUmbraco.Release;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace OurUmbraco.Our.Scheduling
{
    internal class YouTrackSync
    {
        public static void Start()
        {
            using (DisposableTimer.DebugDuration<YouTrackSync>(() => "YouTrack import executing", () => "YouTrack import complete"))
            {
                try
                {
                    var import = new Import();
                    import.SaveAllToFile();
                }
                catch (Exception ex)
                {
                    LogHelper.Error<YouTrackSync>("Error importing from YouTrack", ex);
                }
            }
        }
    }
}