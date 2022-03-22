namespace Skybrud.Social.Meetup.Models.GraphQl.Events
{
    /// <summary>
    /// Enum class representing the type of an event.
    /// </summary>
    /// <see>
    ///     <cref>https://www.meetup.com/api/schema/#EventType</cref>
    /// </see>
    public enum MeetupEventType
    {
        /// <summary>
        /// Indicates a online event.
        /// </summary>
        Online,

        /// <summary>
        /// Indicates a physical event.
        /// </summary>
        Physical
    }
}