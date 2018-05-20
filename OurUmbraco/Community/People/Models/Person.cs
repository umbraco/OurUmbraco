using Umbraco.Core.Models;

namespace OurUmbraco.Community.People.Models
{
    public class Person
    {
        public string Username { get; set; }
        public int MemberId { get; set; }

        public Person(IMember member)
        {
            Username = member.Name;
            MemberId = member.Id;
        }
    }
}
