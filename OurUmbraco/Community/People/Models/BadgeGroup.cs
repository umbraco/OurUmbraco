using System.Collections.Generic;

namespace OurUmbraco.Community.People.Models
{
    public class BadgeGroup
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<BadgeMember> Members { get; set; }
    }
}