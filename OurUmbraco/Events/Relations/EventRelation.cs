using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.relation;
using umbraco.DataLayer;

namespace uEvents.Relations
{
    public class EventRelation
    {
        private static string _event = "event";
        private static string _waitinglist = "waitinglist";

          
        public static void RelateMemberToEvent(int memberId, int eventId, string comment)
        {
            if (!Relation.IsRelated(eventId, memberId))
            {
                RelationType rt = RelationType.GetByAlias(_event);
                Relation.MakeNew(eventId, memberId, rt, comment);
            }
        }

        public static void PutMemberOnWaitingList(int memberId, int eventId, string comment)
        {
            if (!Relation.IsRelated(eventId, memberId))
            {
                RelationType rt = RelationType.GetByAlias(_waitinglist);
                Relation.MakeNew(eventId, memberId, rt, comment);
            }
        }

        private static int GetRelations(string alias, int parentId)
        {
           
            ISqlHelper sqlhelper = umbraco.BusinessLogic.Application.SqlHelper;
            int result = sqlhelper.ExecuteScalar<int>(
                @"
                SELECT count(umbracoRelation.id)
                FROM umbracoRelation
                INNER JOIN umbracoRelationType ON umbracoRelationType.id = umbracoRelation.relType AND umbracoRelationType.alias = @alias
                where parentId = @parent
                "
                 ,
                    sqlhelper.CreateParameter("@parent", parentId),
                    sqlhelper.CreateParameter("@alias", alias)
                );
    
            return result;
        }

        public static int GetSignedUp(int eventId)
        {
            return GetRelations(_event, eventId);
        }

        public static int GetWaiting(int eventId)
        {
            return GetRelations(_waitinglist, eventId);
        }

        private static List<Relation> GetRelations(string alias, string sort, int parentId, int number)
        {
            List<Relation> retval = new List<Relation>();
            ISqlHelper sqlhelper = umbraco.BusinessLogic.Application.SqlHelper;

            IRecordsReader rr = sqlhelper.ExecuteReader(

                string.Format(@"
                SELECT TOP {0} umbracoRelation.id
                FROM umbracoRelation
                INNER JOIN umbracoRelationType ON umbracoRelationType.id = umbracoRelation.relType AND umbracoRelationType.alias = @alias
                where parentId = @parent
                ORDER BY datetime {1}                
                ", number, sort)
                 
                 ,
                    sqlhelper.CreateParameter("@parent", parentId),
                    sqlhelper.CreateParameter("@alias", alias)
                );

            while (rr.Read())
            {
                retval.Add( new Relation( rr.GetInt("id") ) );
            }
            rr.Close();
            rr.Dispose();

            return retval;
        }

        public static List<Relation> GetPeopleWaiting(int eventId, int number)
        {
            return GetRelations(_waitinglist, "ASC", eventId, number);
        }

        public static List<Relation> GetPeopleSignedUpLast(int eventId, int number)
        {
            return GetRelations(_event, "DESC", eventId, number);
        }


        public static void ClearRelations(int memberId, int eventId)
        {
            if (Relation.IsRelated(eventId, memberId))
            {
                foreach (Relation r in Relation.GetRelations(eventId))
                {
                    if (r.Child.Id == memberId)
                    {
                        r.Delete();
                        break;
                    }
                }
            }
        }
    }
}
