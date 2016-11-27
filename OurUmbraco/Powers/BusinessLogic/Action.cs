using System;
using System.Collections.Generic;
using System.Xml;
using umbraco.BusinessLogic;

namespace OurUmbraco.Powers.BusinessLogic
{
    public class Action
    {
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


        public string DataBaseTable
        {
            get
            {
                return "powers" + TypeAlias;
            }
        }

        private Events _e = new Events();

        public bool Perform(int performer, int itemId, string comment)
        {
            return Perform(performer, itemId, 0, comment);
        }

        public void ClearVotes(int performer, int itemId)
        {
            using (var sqlHelper = Application.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery(
                    "Delete FROM " + DataBaseTable + " WHERE memberId = @memberId AND id = @id",
                    sqlHelper.CreateParameter("@memberId", performer),
                    sqlHelper.CreateParameter("@id", itemId));
            }
        }

        public bool Perform(int performer, int itemId, int receiver, string comment)
        {

            ActionEventArgs e = new ActionEventArgs();
            e.PerformerId = performer;
            e.ItemId = itemId;
            e.ReceiverId = receiver;
            e.ActionType = TypeAlias;

            FireBeforePerform(e);



            if (!e.Cancel)
            {

                bool allowed = Allowed(performer, itemId, receiver, comment);

                if (allowed)
                {
                    using (var sqlHelper = Application.SqlHelper)
                    {
                        sqlHelper.ExecuteNonQuery(
                            "INSERT INTO " + DataBaseTable +
                            "(id, memberId, points, receiverId, receiverPoints, performerPoints, comment) VALUES(@id, @memberId, @points, @receiverId, @receiverPoints, @performerPoints, @comment)",
                            sqlHelper.CreateParameter("@id", e.ItemId),
                            sqlHelper.CreateParameter("@table", DataBaseTable),
                            sqlHelper.CreateParameter("@memberId", e.PerformerId),
                            sqlHelper.CreateParameter("@points", Weight),
                            sqlHelper.CreateParameter("@receiverid", e.ReceiverId),
                            sqlHelper.CreateParameter("@receiverPoints", ReceiverReward),
                            sqlHelper.CreateParameter("@performerPoints", PerformerReward),
                            sqlHelper.CreateParameter("@comment", comment + " ")
                        );
                    }

                    //the performer gets his share
                    if (PerformerReward != 0)
                    {
                        Reputation r = new Reputation(e.PerformerId);
                        r.Current = (r.Current + (PerformerReward));
                        r.Total = (r.Total + (PerformerReward));
                        r.Save();
                    }

                    //And maybe the author of the item gets a cut as well.. 
                    if (e.ReceiverId > 0 && ReceiverReward != 0)
                    {
                        Reputation pr = new Reputation(e.ReceiverId);
                        pr.Current = (pr.Current + (ReceiverReward));
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
                                using (var sqlHelper = Application.SqlHelper)
                                {
                                    //make sure the extra receivers also get inserted (but no points for item and performer)
                                    sqlHelper.ExecuteNonQuery(
                                        "INSERT INTO " + DataBaseTable +
                                        "(id, memberId, points, receiverId, receiverPoints, performerPoints, comment) VALUES(@id, @memberId, @points, @receiverId, @receiverPoints, @performerPoints, @comment)",
                                        sqlHelper.CreateParameter("@id", e.ItemId),
                                        sqlHelper.CreateParameter("@table", DataBaseTable),
                                        sqlHelper.CreateParameter("@memberId", e.PerformerId),
                                        sqlHelper.CreateParameter("@points", 0),
                                        sqlHelper.CreateParameter("@receiverid", r),
                                        sqlHelper.CreateParameter("@receiverPoints", ReceiverReward),
                                        sqlHelper.CreateParameter("@performerPoints", 0),
                                        sqlHelper.CreateParameter("@comment", comment + " "));
                                }
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

        public bool Allowed(int performer, int id, int receiver, string comment)
        {
            if (performer > 0 && (!MandatoryComment || (MandatoryComment && comment.Length >= _minCommentLenght)))
            {
                if (AllowedGroups != null && AllowedGroups.Length > 0)
                {
                    var allowed = false;
                    foreach (var group in AllowedGroups)
                        if (Library.Utils.IsMemberInGroup(group, performer))
                        {
                            allowed = true;
                            break;
                        }

                    if (!allowed)
                        return false;
                }

                var reputation = new Reputation(performer);
                using (var sqlHelper = Application.SqlHelper)
                {
                    var doneBefore = sqlHelper.ExecuteScalar<int>(
                                          "SELECT count(id) from " + DataBaseTable +
                                          " WHERE id = @id and memberId = @memberId",
                                          sqlHelper.CreateParameter("@id", id),
                                          sqlHelper.CreateParameter("@memberId", performer)
                                      ) > 0;

                    return MinimumReputation <= reputation.Current && !doneBefore && performer != receiver;
                }
            }

            return false;
        }

        public Action(string alias)
        {
            XmlNode x = config.GetKeyAsNode("/configuration/actions/action [@alias = '" + alias + "']");
            if (x != null)
            {
                Weight = int.Parse(x.Attributes.GetNamedItem("weight").Value);
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
            }

            Exists = false;
        }

        public static List<Action> GetAllActions()
        {
            XmlNode x = config.GetKeyAsNode("/configuration/actions");
            List<Action> l = new List<Action>();
            foreach (XmlNode cx in x.ChildNodes)
            {
                if (cx.Attributes.GetNamedItem("alias") != null)
                    l.Add(new Action(cx.Attributes.GetNamedItem("alias").Value));
            }

            return l;
        }

        /* EVENTS */
        public static event EventHandler<ActionEventArgs> BeforePerform;
        protected virtual void FireBeforePerform(ActionEventArgs e)
        {
            _e.FireCancelableEvent(BeforePerform, this, e);
        }
        public static event EventHandler<ActionEventArgs> AfterPerform;
        protected virtual void FireAfterPerform(ActionEventArgs e)
        {
            if (AfterPerform != null)
                AfterPerform(this, e);
        }

    }
}