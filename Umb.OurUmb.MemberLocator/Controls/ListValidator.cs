using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using System.Text;
using System.Web.UI;


namespace Umb.OurUmb.MemberLocator.Controls
{
    public class ListValidator : BaseValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListValidator"/> class.
        /// </summary>
        public ListValidator()
        {

        }

        /// <summary>
        /// Determines whether the control specified by the <see cref="P:System.Web.UI.WebControls.BaseValidator.ControlToValidate"/> property is a valid control.
        /// </summary>
        /// <returns>
        /// true if the control specified by <see cref="P:System.Web.UI.WebControls.BaseValidator.ControlToValidate"/> is a valid control; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.Web.HttpException">
        /// No value is specified for the <see cref="P:System.Web.UI.WebControls.BaseValidator.ControlToValidate"/> property.
        /// - or -
        /// The input control specified by the <see cref="P:System.Web.UI.WebControls.BaseValidator.ControlToValidate"/> property is not found on the page.
        /// - or -
        /// The input control specified by the <see cref="P:System.Web.UI.WebControls.BaseValidator.ControlToValidate"/> property does not have a <see cref="T:System.Web.UI.ValidationPropertyAttribute"/> attribute associated with it; therefore, it cannot be validated with a validation control.
        /// </exception>
        protected override bool ControlPropertiesValid()
        {
            Control ctrl = FindControl(ControlToValidate) as ListControl;
            return (ctrl != null);
        }

        /// <summary>
        /// When overridden in a derived class, this method contains the code to determine whether the value in the input control is valid.
        /// </summary>
        /// <returns>
        /// true if the value in the input control is valid; otherwise, false.
        /// </returns>
        protected override bool EvaluateIsValid()
        {
            return this.CheckIfItemIsChecked();
        }

        /// <summary>
        /// Checks if item is checked.
        /// </summary>
        /// <returns></returns>
        protected bool CheckIfItemIsChecked()
        {
            ListControl listItemValidate = ((ListControl)this.FindControl(this.ControlToValidate));
            foreach (ListItem listItem in listItemValidate.Items)
            {
                if (listItem.Selected == true)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            // Determines whether the validation control can be rendered
            // for a newer ("uplevel") browser.
            // check if client-side validation is enabled.
            if (this.DetermineRenderUplevel() && this.EnableClientScript)
            {
                Page.ClientScript.RegisterExpandoAttribute(this.ClientID, "evaluationfunction", "CheckIfListChecked");
                this.CreateJavaScript();
            }
            base.OnPreRender(e);
        }

        /// <summary>
        /// Creates the java script.
        /// </summary>
        protected void CreateJavaScript()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<script type=""text/javascript"">function CheckIfListChecked(ctrl){");
            sb.Append(@"var chkBoxList = document.getElementById(document.getElementById(ctrl.id).controltovalidate);");
            sb.Append(@"var chkBoxCount= chkBoxList.getElementsByTagName(""input"");");
            sb.Append(@"for(var i=0;i<chkBoxCount.length;i++){");
            sb.Append(@"if(chkBoxCount.item(i).checked){");
            sb.Append(@"return true; }");
            sb.Append(@"}return false; ");
            sb.Append(@"}</script>");
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "JSScript", sb.ToString());
        }
    }
}
