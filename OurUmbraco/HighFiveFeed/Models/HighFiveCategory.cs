using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurUmbraco.HighFiveFeed.Models
{
    public class HighFiveCategory
    {

        public HighFiveCategory(int id, string text)
        {
            Id = id;
            CategoryText = text;
        }
        public int Id { get; set; }
        public string CategoryText { get; set; }


    }
}