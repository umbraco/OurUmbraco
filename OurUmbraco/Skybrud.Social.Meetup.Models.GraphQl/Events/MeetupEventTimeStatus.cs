#pragma warning disable 1591

namespace Skybrud.Social.Meetup.Models.GraphQl.Events
{
    /// <summary>
    /// Enum class representing the time status of an event.
    /// </summary>
    /// <see>
    ///     <cref>https://www.meetup.com/api/schema/#EventTimeStatus</cref>
    /// </see>
    public enum MeetupEventTimeStatus
    {
        Upcoming,
        Begun,
        Ended
    }
}