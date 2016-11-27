using System.Xml;
using System.Xml.XPath;
using OurUmbraco.Events.Relations;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;

namespace OurUmbraco.Events.Library
{
    [Umbraco.Core.Macros.XsltExtension("uEvents")]
    public class Xslt
    {
        public static bool isSignedUp(int eventId, int memberId)
        {
            return umbraco.cms.businesslogic.relation.Relation.IsRelated(eventId, memberId);
        }

        public static bool isFull(int eventId)
        {
            Event e = new Event(eventId);
            return e.IsFull;
        }

        public static void SignUp(int memberId, int eventId)
        {
            Event e = new Event(eventId);
            e.SignUp(memberId, "no comment");
        }

        public static void Cancel(int memberId, int eventId)
        {
            Event e = new Event(eventId);
            e.Cancel(memberId, "no comment");
        }

        public static XPathNodeIterator UpcomingEvents()
        {
            var contentType = DocumentType.GetByAlias("Event").Id;
            var property = "start";

            var sql = string.Format(@"SELECT distinct contentNodeId from cmsPropertyData
            inner join cmsPropertyType ON 
            cmspropertytype.contenttypeid = {0} and
            cmspropertytype.Alias = '{1}' and
            cmspropertytype.id = cmspropertydata.propertytypeid
            where dataDate > GETDATE()", contentType, property);

            using (var sqlhelper = umbraco.BusinessLogic.Application.SqlHelper)
            using (var reader = sqlhelper.ExecuteReader(sql))
            {
                var xmlDocument = new XmlDocument();
                var root = umbraco.xmlHelper.addTextNode(xmlDocument, "events", "");

                while (reader.Read())
                {
                    var contentNodeId = reader.GetInt("contentNodeId").ToString();
                    var xmlNode = (XmlNode)umbraco.content.Instance.XmlContent.GetElementById(contentNodeId);

                    if (xmlNode == null) continue;

                    xmlNode = xmlDocument.ImportNode(xmlNode, true);
                    root.AppendChild(xmlNode);
                }

                return root.CreateNavigator().Select(".");
            }
        }
    }
}
