namespace OurUmbraco.Forum
{
    public class TopicEventArgs : System.ComponentModel.CancelEventArgs {
        public Models.Topic Topic { get; set; }
        public string CancellationReason { get; set; }
    }

    public class CommentEventArgs : System.ComponentModel.CancelEventArgs
    {
        public Models.Comment Comment { get; set; }
        public string CancellationReason { get; set; }
    }

    public class ForumEventArgs : System.ComponentModel.CancelEventArgs
    {
        public Models.Forum Forum { get; set; }
        public string CancellationReason { get; set; }
    }


    /* Events */
    public class CreateEventArgs : System.ComponentModel.CancelEventArgs { }
    public class UpdateEventArgs : System.ComponentModel.CancelEventArgs { }
    public class DeleteEventArgs : System.ComponentModel.CancelEventArgs { }
    public class MarkAsSpamEventArgs : System.ComponentModel.CancelEventArgs { }
    public class MarkAsHamEventArgs : System.ComponentModel.CancelEventArgs { }
    public class LockEventArgs : System.ComponentModel.CancelEventArgs { }
    public class MoveEventArgs : System.ComponentModel.CancelEventArgs { }
}
