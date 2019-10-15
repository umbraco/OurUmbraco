using Umbraco.Core.Models;

namespace OurUmbraco.Our.Models
{
    public class ActivationEmailModel
    {
        public IMember Member { get; }
        public string Id { get; }
        public string Name { get; }
        public string ActivationUrl => $"https://our.umbraco.com/member/activate/?id={Id}";

        public ActivationEmailModel(IMember member)
        {
            Member = member;
            Id = $"{member.ProviderUserKey}";
            Name = member.Name;
        }
    }
}
