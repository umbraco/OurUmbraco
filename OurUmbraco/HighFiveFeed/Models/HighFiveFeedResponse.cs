using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurUmbraco.HighFiveFeed.Models
{
    public class HighFiveFeedResponse
    {
        public HighFiveFeedResponse()
        {
            HighFives = new List<HighFiveResponse>();
        }
        public int Count { get; set; }
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }

        public List<HighFiveResponse> HighFives { get; set;}
    }
}
