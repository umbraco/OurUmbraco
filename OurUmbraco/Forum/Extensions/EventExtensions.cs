using System;
using System.ComponentModel;

namespace OurUmbraco.Forum.Extensions
{
        public static class EventExtensions
        {
            public static void Raise<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
            {
                if (handler != null) handler(sender, args);
            }

            //returns true if there is no handler or if the handler doesnt set Cancel to true
            public static bool RaiseAndContinue<T>(this EventHandler<T> handler, object sender, T args) where T : CancelEventArgs
            {
                if (handler == null)
                    return true;

                handler(sender, args);
                return (args.Cancel != true);
            }
        
    }
}
