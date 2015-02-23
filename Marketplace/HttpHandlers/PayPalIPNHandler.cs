using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketplace.HttpHandlers
{
    public class PayPalIPNHandler : IHttpHandler
    {
        #region IHttpHandler Members

        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            PayPalIPN ipn = new PayPalIPN();
            ipn.Process(context, false);
        }

        #endregion
    }

    public class PayPalIPNHandlerTEST : IHttpHandler
    {
        #region IHttpHandlerTEST Members

        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            PayPalIPN ipn = new PayPalIPN();
            ipn.Process(context, true);
        }

        #endregion
    }
}