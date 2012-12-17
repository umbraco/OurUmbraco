using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace uPowers.BusinessLogic{
    public class Vote {
        public int Id { get; set; }
        public string Type { get; set; }
        public int MemberID { get; set; }
        public int Amount { get; set; }

        private Events _e = new Events();
        public static bool SubmitVote(int id, string type, int memberId, int amount) {
            Vote v = new Vote();
            v.Id = id;
            v.Type = type;
            v.MemberID = memberId;
            v.Amount = amount;

            return v.Submit();
        }

        public bool Submit() {
            bool retval = false;

            VoteEventArgs e = new VoteEventArgs();
            FireBeforeSubmitVote(e);
            
            if (!e.Cancel) {


                retval = true;
                FireAfterSubmitVote(e);
            }

            return retval;

        }


        public static List<Vote> AllVotesFromID(int id) {
            List<Vote> vl = new List<Vote>();
            return vl;
        }

        public static List<Vote> AllVotesFromIDAndType(int id, string type) {
            List<Vote> vl = new List<Vote>();
            return vl;
        }


        public static List<Vote> AllVotesFromMember(int MemberId) {
            List<Vote> vl = new List<Vote>();
            return vl;
        }

        public static List<Vote> AllVotesFromMember(int MemberId, string type) {
            List<Vote> vl = new List<Vote>();
            return vl;
        }

        public XmlNode ToXml(XmlDocument xd) {
            return xd.CreateNode("", "", "");
        }

        public static event EventHandler<VoteEventArgs> BeforeSubmitVote;
        protected virtual void FireBeforeSubmitVote(VoteEventArgs e) {
            _e.FireCancelableEvent(BeforeSubmitVote, this, e);
        }

        public static event EventHandler<VoteEventArgs> AfterSubmitVote;
        protected virtual void FireAfterSubmitVote(VoteEventArgs e) {
            _e.FireCancelableEvent(AfterSubmitVote, this, e);
        }
    }


    
}
