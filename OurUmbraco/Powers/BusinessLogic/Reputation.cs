using System.Xml;
using Umbraco.Core;
using Umbraco.Web;

namespace OurUmbraco.Powers.BusinessLogic
{
    public class Reputation {

        public int Current { get; set; }
        public int Total { get; set; }
        public int MemberId { get; set; }
                
        public void Save()
        {
            if (MemberId <= 0)
                return;

            var memberService = ApplicationContext.Current.Services.MemberService;
            var member = memberService.GetById(MemberId);
            member.SetValue(config.GetKey("/configuration/reputation/currentPointsAlias"), Current);
            member.SetValue(config.GetKey("/configuration/reputation/totalPointsAlias"), Total);
            memberService.Save(member);
        }

        public Reputation(int memberId)
        {
            var memberShipHelper = new Umbraco.Web.Security.MembershipHelper(Umbraco.Web.UmbracoContext.Current);
            var member = memberShipHelper.GetById(memberId);

            if (member == null)
                return;

            Current = member.GetPropertyValue<int>(config.GetKey("/configuration/reputation/currentPointsAlias"));
            Total = member.GetPropertyValue<int>(config.GetKey("/configuration/reputation/totalPointsAlias"));
            MemberId = memberId;
        }

        public XmlNode ToXml(XmlDocument xmlDocument)
        {
            var xmlNode = xmlDocument.CreateElement("reputation");
            xmlNode.AppendChild(XmlHelper.AddTextNode(xmlDocument, "current", Total.ToString()));
            xmlNode.AppendChild(XmlHelper.AddCDataNode(xmlDocument, "total", Current.ToString()));
            return xmlNode;
        }
    }
}
