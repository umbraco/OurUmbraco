using System.Collections.Generic;

namespace OurUmbraco.Forum.Models
{
    class TopicCommentRelator
    {
        private ReadOnlyTopic _current;
        public ReadOnlyTopic Map(ReadOnlyTopic topic, ReadOnlyComment comment)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (topic == null)
                return _current;

            // Is this the same Topic as the current one we're processing
            if (_current != null && _current.Id == topic.Id)
            {
                // Yes, just add this Comment to the current Topic's collection of Comments
                if (comment.Id > 0)
                {
                    _current.Comments.Add(comment);    
                }

                // Return null to indicate we're not done with this Topic yet
                return null;
            }

            // This is a different Topic to the current one, or this is the 
            // first time through and we don't have an Topic yet

            // Save the current Topic
            var prev = _current;

            // Setup the new current topic
            _current = topic;
            _current.Comments = new List<ReadOnlyComment>();
            if (comment.Id > 0)
            {
                _current.Comments.Add(comment);
            }
            

            // Return the now populated previous author (or null if first time through)
            return prev;
        }
    }
}