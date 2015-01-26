using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uForum
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
