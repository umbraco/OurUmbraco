using System;
using System.Web;
using System.Web.UI;
using ClientDependency.Core.Controls;

namespace our
{
    /// <summary>
    /// This should not be necessary but we are mixing webforms with mvc sub forms and normally that should work or be done, without
    /// this we'd get a ysod on postback with validatoin errors.
    /// </summary>
    [ParseChildren(typeof(ClientDependencyPath), ChildrenAsProperties = true)]
    public class HackedCdfLoader : ClientDependencyLoader
    {
        public class FormlessPage : Page
        {
            public override void VerifyRenderingInServerForm(Control control) { }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data. </param>
        protected override void OnInit(EventArgs e)
        {
            IHttpHandler currHandler = null;

            try
            {
                //add this object to the context and validate the context type
                if (Context != null)
                {
                    currHandler = Context.Handler;


                    if (!Context.Items.Contains(ContextKey))
                    {
                        //The context needs to be a page
                        var page = Context.Handler as Page;
                        if (page == null)
                        {
                            //Urgg... this is the worst. This is required because we have html mvc forms inside of the webforms html form
                            // otherwise we'll get a ysod
                            Context.Handler = new FormlessPage();
                        }

                        Context.Items[ContextKey] = this;
                    }
                }

                base.OnInit(e);

            }
            finally
            {
                if (Context != null)
                {
                    Context.Handler = currHandler;
                }
            }
            
        }
    }
}