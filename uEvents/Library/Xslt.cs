using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using System.Xml;

namespace uEvents.Library
{
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
            int contentType = DocumentType.GetByAlias("Event").Id;
            string property = "start";
            
            string sql = string.Format(@"SELECT distinct contentNodeId from cmsPropertyData
            inner join cmsPropertyType ON 
            cmspropertytype.contenttypeid = {0} and
            cmspropertytype.Alias = '{1}' and
            cmspropertytype.id = cmspropertydata.propertytypeid
            where dataDate > GETDATE()", contentType, property);

            ISqlHelper sqlhelper = umbraco.BusinessLogic.Application.SqlHelper;
            IRecordsReader rr = sqlhelper.ExecuteReader(sql);

            XmlDocument doc = new XmlDocument();
            XmlNode root = umbraco.xmlHelper.addTextNode(doc, "events", "");

            while (rr.Read()) 
            {
                XmlNode x = (XmlNode)umbraco.content.Instance.XmlContent.GetElementById( rr.GetInt("contentNodeId").ToString() );

                if (x != null)
                {
                    x = doc.ImportNode(x, true);
                    root.AppendChild(x);
                }    
            }
            rr.Close();
            rr.Dispose();
            
            return root.CreateNavigator().Select(".");
        }
    }
}
