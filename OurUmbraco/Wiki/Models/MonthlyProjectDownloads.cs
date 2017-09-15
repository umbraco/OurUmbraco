using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurUmbraco.Wiki.Models
{
    public class MonthlyProjectDownloads
    {
        private readonly Dictionary<int, Dictionary<int, int>> _stats = new Dictionary<int, Dictionary<int, int>>();

        public void AddMonthlyStats(int year, int quarter, int downloads)
        {
            Dictionary<int, int> monthly;
            if (_stats.TryGetValue(year, out monthly) == false)
            {
                monthly = new Dictionary<int, int>();
                _stats[year] = monthly;
            }
            monthly[quarter] = downloads;
        }

        public int GetYearlyDownloads(int year)
        {
            Dictionary<int, int> monthly;
            if (_stats.TryGetValue(year, out monthly) == false)
                return 0;
            return monthly.Sum(x => x.Value);
        }

        public int GetMonthlyDownloads(int year, int month)
        {
            Dictionary<int, int> monthly;
            if (_stats.TryGetValue(year, out monthly) == false)
                return 0;
            int downloads;
            if (monthly.TryGetValue(month, out downloads) == false)
                return 0;
            return downloads;
        }

        public int GetLatestDownloads(DateTime now, int months)
        {
            var year = now.Year;
            var month = now.Month;

            //32 is more days than any month so we are being safe here
            var minYear = now.Subtract(TimeSpan.FromDays(months * 32)).Year - 1;

            var iterations = 0;
            var sum = 0;

            while (year > minYear)
            {  
                while (month > 0)
                {
                    //see if the current year is logged for this project
                    Dictionary<int, int> monthly;
                    if (_stats.TryGetValue(year, out monthly))
                    {
                        //then check if the current month is logged for this project
                        int downloads;
                        if (monthly.TryGetValue(month, out downloads))
                        {
                            sum += downloads;
                        }                        
                    }
                    month--;
                    iterations++;
                    //exit early once iterations match number of months
                    if (iterations >= months)
                        return sum;
                }

                year--;
                //reset
                month = 12;
            }
            
            return sum;
        }
    }
}
