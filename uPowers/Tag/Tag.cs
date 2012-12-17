using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace uPowers.BusinessLogic {
    public class Tag {
        
        private Events _e = new Events();

        public bool SubmitTag(int id, string type, int memberId) {
            bool retval = false;
            TagEventArgs e = new TagEventArgs();
            FireBeforeSubmitTag(e);
            if (!e.Cancel) {
                
                //do the tag, tag tag tag

                retval = true;
                FireAfterSubmitTag(e);
            }

            return retval;
        }

        


        public static event EventHandler<TagEventArgs> BeforeSubmitTag;
        protected virtual void FireBeforeSubmitTag(TagEventArgs e) {
            _e.FireCancelableEvent(BeforeSubmitTag, this, e);
        }

        public static event EventHandler<TagEventArgs> AfterSubmitTag;
        protected virtual void FireAfterSubmitTag(TagEventArgs e) {
            _e.FireCancelableEvent(AfterSubmitTag, this, e);
        }
    }
}
