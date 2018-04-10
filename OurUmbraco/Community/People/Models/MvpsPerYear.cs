using System.Collections.Generic;

namespace OurUmbraco.Community.People.Models
{
    public class MvpsPerYear
    {
        public int Year { get; set; }
        public List<MvpMember> Members { get; set; }
    }
}
