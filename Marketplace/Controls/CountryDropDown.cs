using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Specialized;
using System.Globalization;

namespace Marketplace.Controls
{
    public class CountryDropDownList : DropDownList
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            GetCountries();
            
        }


        private void GetCountries()
        {

            Dictionary<string, string> dic = new Dictionary<string, string>();
            List<string> col = new List<string>();

            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {

                    RegionInfo ri = new RegionInfo(ci.LCID);
                    if (!dic.ContainsKey(ri.EnglishName))
                        dic.Add(ri.EnglishName, ri.TwoLetterISORegionName.ToLowerInvariant());

                    if (!col.Contains(ri.EnglishName))
                        col.Add(ri.EnglishName);
            }

            col.Sort();
            


            this.Items.Add(new ListItem("", ""));
            foreach (var i in dic.OrderBy(x => x.Key))
            {
                this.Items.Add(new ListItem(i.Key, i.Value));
            }

        }
    }
}