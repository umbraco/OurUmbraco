using System;
using System.Globalization;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

namespace OurUmbraco.Events.controls
{
    [ValidationProperty("SelectedDate")]
    public class DatePicker : System.Web.UI.WebControls.WebControl, System.Web.UI.INamingContainer
    {
        private System.Web.UI.WebControls.TextBox tb;
        private CalendarExtender ca;

        private System.Web.UI.WebControls.TextBox hour;
        private System.Web.UI.WebControls.TextBox minute;
        private System.Web.UI.WebControls.Literal colon;
        private System.Web.UI.WebControls.Literal at;
        public DatePicker()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            colon = new Literal();
            colon.ID = "lt_" + this.ID;
            colon.Text = ":";

            at = new Literal();
            at.ID = "lt_at_" + this.ID;
            at.Text = "@";

            minute = new System.Web.UI.WebControls.TextBox();
            minute.ID = "tb_minute" + this.ID;
            minute.CssClass = "minute";

            hour = new System.Web.UI.WebControls.TextBox();
            hour.ID = "tb_hour" + this.ID;
            hour.CssClass = "hour";

            tb = new System.Web.UI.WebControls.TextBox();
            tb.ID = "tb" + this.ID;
            tb.CssClass = "calendar";
            //this.Controls.Add(tb);

            ca = new CalendarExtender();
            DateTimeFormatInfo di = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
            ca.Format = di.ShortDatePattern;
            ca.TargetControlID = tb.ID;
            ca.ID = "ce" + this.ID;
            //this.Controls.Add(ca);
        }

       

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Controls.Add(tb);
            this.Controls.Add(hour);
            this.Controls.Add(colon);
            this.Controls.Add(minute);

            this.Controls.Add(ca);
        }


        public DateTime? SelectedDate
        {
            get
            {
                EnsureChildControls();
                DateTime selected;

                if (DateTime.TryParse(tb.Text, out selected))
                {
                    selected = selected.Subtract(new TimeSpan(int.Parse(hour.Text), int.Parse(minute.Text), 0));

                    // then add back your assigned hours and minutes
                    selected = selected.AddHours(int.Parse(hour.Text)).AddMinutes(int.Parse(minute.Text));
                   
                    return selected;
                }

                return null;
            }
            set
            {
                EnsureChildControls();
                if (value != null)
                {

                    DateTimeFormatInfo di = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
                    tb.Text = ((DateTime)value).ToString(di.ShortDatePattern);
                    hour.Text = ((DateTime)value).Hour.ToString();
                    minute.Text = ((DateTime)value).Minute.ToString();

                    ca.SelectedDate = value;
                }
                else
                {
                    hour.Text = "12";
                    minute.Text = "00";
                }
            }
        }
    }
}
