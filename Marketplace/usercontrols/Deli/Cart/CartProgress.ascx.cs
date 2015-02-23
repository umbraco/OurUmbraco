using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Marketplace.usercontrols.Deli.Cart
{
    public partial class CartProgress : System.Web.UI.UserControl
    {
        public int CurrentStepIndex { get; set; }

        private string[] stepArray;

        protected void Page_Load(object sender, EventArgs e)
        {
            stepArray = new string[] { "Your Cart", "Your Details", "Review Your Order", "Checkout", "Order Complete" };
            var steps = stepArray.Select(x => new
            {
                StepName = x,
                StepClass = GetStepClass(x)
            });

            ProgressIndicator.DataSource = steps;
            ProgressIndicator.DataBind();
        }

        private object GetStepClass(string x)
        {
            var index = stepArray.ToList().IndexOf(x);
            var cssClass = "";
            if (index == 0)
            {
                cssClass += "first ";
            }
            if (index < CurrentStepIndex)
            {
                cssClass += "complete";
            }
            if (index == CurrentStepIndex)
            {
                cssClass += "current";
            }
            return cssClass;
        }
    }

}