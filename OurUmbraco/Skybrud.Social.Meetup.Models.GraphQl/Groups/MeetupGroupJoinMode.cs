namespace Skybrud.Social.Meetup.Models.GraphQl.Groups
{
    /// <summary>
    /// Enum class representing the join mode of a group.
    /// </summary>
    /// <see>
    ///     <cref>https://www.meetup.com/api/schema/#GroupJoinMode</cref>
    /// </see>
    public enum MeetupGroupJoinMode
    {
        /// <summary>
        /// Indicates that members must be approved by an organizer.
        /// </summary>
        Approval,

        /// <summary>
        /// Indicates that the group is not currently accepting new members.
        /// </summary>
        Closed,

        /// <summary>
        /// Indicates that any meetup member can join.
        /// </summary>
        Open
    }
}