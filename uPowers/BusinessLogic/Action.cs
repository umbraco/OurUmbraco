using System;
using System.Collections.Generic;
using System.Xml;
using System.Web;
using umbraco.cms.businesslogic.member;

namespace uPowers.BusinessLogic {
    public class Action {
        public string Alias { get; set; }
        public string TypeAlias { get; set; }
        
        public int PerformerReward { get; set; }
        public int ReceiverReward { get; set; }

        public int Weight { get; set; }

        public bool MandatoryComment { get; set; }

        public int MinimumReputation { get; set; }
        public bool Exists { get; private set; }
        
        private static int _minCommentLenght = 50;

        public string[] AllowedGroups { get; set; }


        public string DataBaseTable { 
            get{
                return "powers" + TypeAlias;   
            }
        }

        private Events _e = new Events();

        public bool Perform(int performer, int itemId, string comment) {
            return Perform(performer, itemId, 0, comment);
        }

        public void ClearVotes(int performer, int itemId) {

            Data.SqlHelper.ExecuteNonQuery("Delete FROM " + DataBaseTable + " WHERE memberId = @memberId AND id = @id",
                Data.SqlHelper.CreateParameter("@memberId", performer),
                Data.SqlHelper.CreateParameter("@id", itemId));

        }

        public bool Perform(int performer, int itemId, int receiver, string comment) {
            
                ActionEventArgs e = new ActionEventArgs();
                e.PerformerId = performer;
                e.ItemId = itemId;
                e.ReceiverId = receiver;
                e.ActionType = TypeAlias;

                FireBeforePerform(e);
                
                

                if (!e.Cancel) {

                    bool allowed = Allowed(performer, itemId, receiver, comment);

                    if (allowed)
                    {
                    
                    Data.SqlHelper.ExecuteNonQuery("INSERT INTO " + DataBaseTable + "(id, memberId, points, receiverId, receiverPoints, performerPoints, comment) VALUES(@id, @memberId, @points, @receiverId, @receiverPoints, @performerPoints, @comment)",
                        Data.SqlHelper.CreateParameter("@id", e.ItemId),
                        Data.SqlHelper.CreateParameter("@table", DataBaseTable),
                        Data.SqlHelper.CreateParameter("@memberId", e.PerformerId),
                        Data.SqlHelper.CreateParameter("@points", Weight),
                        Data.SqlHelper.CreateParameter("@receiverid", e.ReceiverId),
                        Data.SqlHelper.CreateParameter("@receiverPoints", ReceiverReward),
                        Data.SqlHelper.CreateParameter("@performerPoints", PerformerReward),
                        Data.SqlHelper.CreateParameter("@comment", comment + " ")
                    );
                    
                    //the performer gets his share
                    if(PerformerReward != 0){
                        Reputation r = new Reputation(e.PerformerId);
                        r.Current = (r.Current + (PerformerReward) );
                        r.Total = (r.Total + (PerformerReward) );
                        r.Save();
                    }

                    //And maybe the author of the item gets a cut as well.. 
                    if (e.ReceiverId > 0 && ReceiverReward != 0) {
                        Reputation pr = new Reputation(e.ReceiverId);
                        pr.Current = (pr.Current + (ReceiverReward) );
                        pr.Total = (pr.Total + (ReceiverReward));
                        pr.Save();
                    }

                    //And maybe there are some additional receivers
                    if (e.ExtraReceivers != null)
                    {
                        foreach (int r in e.ExtraReceivers)
                        {
                            if (allowed)
                            {
                                //make sure the extra receivers also get inserted (but no points for item and performer)
                                Data.SqlHelper.ExecuteNonQuery("INSERT INTO " + DataBaseTable + "(id, memberId, points, receiverId, receiverPoints, performerPoints, comment) VALUES(@id, @memberId, @points, @receiverId, @receiverPoints, @performerPoints, @comment)",
                                    Data.SqlHelper.CreateParameter("@id", e.ItemId),
                                    Data.SqlHelper.CreateParameter("@table", DataBaseTable),
                                    Data.SqlHelper.CreateParameter("@memberId", e.PerformerId),
                                    Data.SqlHelper.CreateParameter("@points", 0),
                                    Data.SqlHelper.CreateParameter("@receiverid", r),
                                    Data.SqlHelper.CreateParameter("@receiverPoints", ReceiverReward),
                                    Data.SqlHelper.CreateParameter("@performerPoints", 0),
                                    Data.SqlHelper.CreateParameter("@comment", comment + " "));
                            }
                            Reputation pr = new Reputation(r);
                            pr.Current = (pr.Current + (ReceiverReward));
                            pr.Total = (pr.Total + (ReceiverReward));
                            pr.Save();
                        }
                    }


                    FireAfterPerform(e);
                    return true;
                }
            }

            return false;
        }

        public bool Allowed(int performer, int id, int receiver, string comment) {
            if (performer > 0 && (!MandatoryComment || (MandatoryComment && comment.Length >= _minCommentLenght) )) {


                if (AllowedGroups != null && AllowedGroups.Length > 0)
                {
                    bool allowed = false;
                    foreach(string group in AllowedGroups)
                        if(Library.Utills.IsMemberInGroup(group, performer)){
                            allowed = true;
                            break;
                        }

                    if(!allowed)
                        return false;
                }


                Reputation r = new Reputation(performer);
                
                bool doneBefore = Data.SqlHelper.ExecuteScalar<int>("SELECT count(id) from " + DataBaseTable + " WHERE id = @id and memberId = @memberId",
                    Data.SqlHelper.CreateParameter("@id", id), 
                    Data.SqlHelper.CreateParameter("@memberId", performer)
                    ) > 0;

                return ( (MinimumReputation <= r.Current) && !doneBefore && performer != receiver);
            } else
                return false;
        }

        public Action(string alias) {
            XmlNode x = config.GetKeyAsNode("/configuration/actions/action [@alias = '" + alias + "']");
            if (x != null) {
                Weight = int.Parse( x.Attributes.GetNamedItem("weight" ).Value);
                ReceiverReward = int.Parse(x.Attributes.GetNamedItem("receiverReward").Value);
                PerformerReward = int.Parse(x.Attributes.GetNamedItem("performerReward").Value);

                MinimumReputation = int.Parse(x.Attributes.GetNamedItem("minReputation").Value);
                Alias = x.Attributes.GetNamedItem("alias").Value;
                TypeAlias = x.Attributes.GetNamedItem("type").Value;
                

                bool _mandatoryComment = false;
                if (x.Attributes.GetNamedItem("mandatoryComment") != null)
                    bool.TryParse(x.Attributes.GetNamedItem("mandatoryComment").Value, out _mandatoryComment);


                if (x.Attributes.GetNamedItem("allowedGroups") != null)
                    AllowedGroups = x.Attributes.GetNamedItem("allowedGroups").Value.Split(',');

                MandatoryComment = _mandatoryComment;
                Exists = true;
            } else
                Exists = false;
        }

        public static List<Action> GetAllActions() {
            XmlNode x = config.GetKeyAsNode("/configuration/actions");
            List<Action> l = new List<Action>();            
            foreach (XmlNode cx in x.ChildNodes) {
                if (cx.Attributes.GetNamedItem("alias") != null)
                    l.Add( new Action( cx.Attributes.GetNamedItem("alias").Value ));
            }

            return l;
        }

        /* EVENTS */
        public static event EventHandler<ActionEventArgs> BeforePerform;
        protected virtual void FireBeforePerform(ActionEventArgs e) {
            _e.FireCancelableEvent(BeforePerform, this, e);
        }
        public static event EventHandler<ActionEventArgs> AfterPerform;
        protected virtual void FireAfterPerform(ActionEventArgs e) {
            if (AfterPerform != null)
                AfterPerform(this, e);
        }

    }
}