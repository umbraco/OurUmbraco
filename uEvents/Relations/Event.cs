using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.relation;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;

namespace uEvents
{
    public class Event
    {
        private Document n;
        public Event(int eventId)
        {
            n = new Document(eventId);
        }

        public Event(Document eventDocument)
        {
            n = eventDocument;
        }

        public bool IsFull {
            get
            {
                return this.SignedUp >= this.Capacity; 
            }
        }

        public int OnWaitingList
        {
            get
            {
                return Relations.EventRelation.GetWaiting(n.Id);
            }
        }
        

        public int Capacity
        {
            get
            {
                int retval = 0;
                int.TryParse(n.getProperty("capacity").Value.ToString(), out retval);
                return retval;
            }
        }


        public int SignedUp
        {
            get
            {
                return Relations.EventRelation.GetSignedUp(n.Id);
            }
        }


        public void Cancel(int memberid, string comment)
        {
            Relations.EventRelation.ClearRelations(memberid, n.Id);
            
            syncCapacity();
        }


        public void SignUp(int memberid, string comment)
        {
            if (!IsFull)
            {
                Relations.EventRelation.RelateMemberToEvent(memberid, n.Id, comment);
            }
            else
            {
                Relations.EventRelation.PutMemberOnWaitingList(memberid, n.Id, comment);
            }
        }

        public void syncCapacity()
        {
            int max = this.Capacity;
            int signedUp = this.SignedUp;
            int waiting = this.OnWaitingList;

            //2 scenarios, either the room is now smaller or bigger
            if (max >= signedUp)
            {
                Log.Add(LogTypes.Debug, n.Id, "bigger cap");

                //get people on the waitinglist and promote them to coming to event
                int diff = max - signedUp;
                RelationType coming = RelationType.GetByAlias("event");
                foreach (Relation r in Relations.EventRelation.GetPeopleWaiting(n.Id, diff))
                {
                    Log.Add(LogTypes.Debug, n.Id, "R:" + r.Id.ToString() );
                    r.RelType = coming;
                    r.Save();
                }
            }
            else
            {
                Log.Add(LogTypes.Debug, n.Id, "smaller cap");

                //get people on the signedup list and put the people who signed up last on the waiting list.
                //remember to preserve the original sign-up date
                int diff = signedUp - max;
                RelationType waitinglist = RelationType.GetByAlias("waitinglist");
                foreach (Relation r in Relations.EventRelation.GetPeopleSignedUpLast(n.Id, diff))
                {
                    Log.Add(LogTypes.Debug, n.Id, "R:" + r.Id.ToString());
                    r.RelType = waitinglist;
                    r.Save();
                }
            }

        }

    }
        
    
}
