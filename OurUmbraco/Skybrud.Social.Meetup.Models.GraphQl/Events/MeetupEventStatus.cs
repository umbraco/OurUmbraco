#pragma warning disable 1591

namespace Skybrud.Social.Meetup.Models.GraphQl.Events
{
    /// <summary>
    /// Enum class representing the status of an event.
    /// </summary>
    /// <see>
    ///     <cref>https://www.meetup.com/api/schema/#EventStatus</cref>
    /// </see>
    public enum MeetupEventStatus
    {
        Published,
        Draft,
        Cancelled,
        CancelledPerm,
        Autosched,
        Active,
        Past
    }
}