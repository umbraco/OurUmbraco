using System.Linq;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;
using Umbraco.Web;
using Relation = umbraco.cms.businesslogic.relation.Relation;
using RelationType = umbraco.cms.businesslogic.relation.RelationType;

namespace uEvents
{
    public class Event
    {
        private readonly IPublishedContent _content;

        public Event(int eventId)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            _content = umbracoHelper.TypedContent(eventId);
        }

        public Event(IPublishedContent eventDocument)
        {
            _content = eventDocument;
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
                return Relations.EventRelation.GetWaiting(_content.Id);
            }
        }
        

        public int Capacity
        {
            get
            {
                return _content.GetPropertyValue<int>("capacity");
            }
        }


        public int SignedUp
        {
            get
            {
                return Relations.EventRelation.GetSignedUp(_content.Id);
            }
        }


        public void Cancel(int memberid, string comment)
        {
            Relations.EventRelation.ClearRelations(memberid, _content.Id);
            
            syncCapacity();
        }


        public void SignUp(int memberid, string comment)
        {
            if (!IsFull)
            {
                Relations.EventRelation.RelateMemberToEvent(memberid, _content.Id, comment);
            }
            else
            {
                Relations.EventRelation.PutMemberOnWaitingList(memberid, _content.Id, comment);
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
                Log.Add(LogTypes.Debug, _content.Id, "bigger cap");

                //get people on the waitinglist and promote them to coming to event
                int diff = max - signedUp;
                RelationType coming = RelationType.GetByAlias("event");
                foreach (Relation r in Relations.EventRelation.GetPeopleWaiting(_content.Id, diff))
                {
                    Log.Add(LogTypes.Debug, _content.Id, "R:" + r.Id.ToString() );
                    r.RelType = coming;
                    r.Save();
                }
            }
            else
            {
                Log.Add(LogTypes.Debug, _content.Id, "smaller cap");

                //get people on the signedup list and put the people who signed up last on the waiting list.
                //remember to preserve the original sign-up date
                int diff = signedUp - max;
                RelationType waitinglist = RelationType.GetByAlias("waitinglist");
                foreach (Relation r in Relations.EventRelation.GetPeopleSignedUpLast(_content.Id, diff))
                {
                    Log.Add(LogTypes.Debug, _content.Id, "R:" + r.Id.ToString());
                    r.RelType = waitinglist;
                    r.Save();
                }
            }

        }

    }
        
    
}
