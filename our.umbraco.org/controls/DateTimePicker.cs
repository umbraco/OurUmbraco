using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using AjaxControlToolkit;
using System.Globalization;
using System.Threading;

namespace our.controls
{
    [ValidationProperty("SelectedDate")]
    public class DatePicker : System.Web.UI.WebControls.WebControl, System.Web.UI.INamingContainer
    {
        private System.Web.UI.WebControls.TextBox tb;
        private CalendarExtender ca;
        private System.Web.UI.WebControls.HiddenField hf;

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
            ca.SelectedDate = DateTime.Now.Date.AddDays(7);

            //this.Controls.Add(ca);
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Controls.Add(tb);
            this.Controls.Add(at);

            this.Controls.Add(hour);
            this.Controls.Add(colon);
            this.Controls.Add(minute);

            this.Controls.Add(ca);

            if (!SelectedDate.HasValue)
            {
                ca.SelectedDate = DateTime.Now.AddDays(5);

                hour.Text = "11";
                minute.Text = "00";
            }
        }

        private string ensuretwodigits(string digit)
        {
            int i = 0;

            if (int.TryParse(digit, out i))
                if (i < 10)
                    return "0" + digit;

            return digit;
        }

        public DateTime? SelectedDate
        {
            get
            {
                EnsureChildControls();
                DateTime selected;

                //string date = tb.Text + " " + ensuretwodigits(hour.Text) + ":" + ensuretwodigits(minute.Text);

                if (DateTime.TryParse(tb.Text , out selected))
                {

                    selected = selected.Subtract(  new TimeSpan( selected.Hour , selected.Minute, 0));
                    // then add back your assigned hours and minutes
                    selected = selected.AddHours(int.Parse(hour.Text)).AddMinutes(int.Parse(minute.Text));

                    return selected;
                }

                return null;
            }
            set
            {
                EnsureChildControls();

                if (value.HasValue)
                {

                    DateTimeFormatInfo di = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
                    tb.Text = ((DateTime)value.Value).ToString(di.ShortDatePattern);

                    hour.Text = ensuretwodigits( ((DateTime)value.Value).Hour.ToString());
                    minute.Text = ensuretwodigits( ((DateTime)value.Value).Minute.ToString());

                    ca.SelectedDate = value;
                }
                else
                {
                    ca.SelectedDate = DateTime.Now.AddDays(7);

                    hour.Text = "12";
                    minute.Text = "00";
                }
            }
        }
    }
}
