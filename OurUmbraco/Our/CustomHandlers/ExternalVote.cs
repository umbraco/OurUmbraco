using System;
using OurUmbraco.Powers.BusinessLogic;
using umbraco.BusinessLogic;
using Action = OurUmbraco.Powers.BusinessLogic.Action;

namespace OurUmbraco.Our.CustomHandlers
{
	public class ExternalVote : ApplicationBase
	{
		public ExternalVote()
		{
			Action.BeforePerform += new EventHandler<ActionEventArgs>(this.Action_BeforePerform);
		}

		void Action_BeforePerform(object sender, ActionEventArgs e)
		{
			var action = (Action)sender;
            
		    if (action.Alias != "ExternalVote")
                return;

		    using (var sqlHelper = Application.SqlHelper)
		    {
		        var memberId = sqlHelper.ExecuteScalar<int>("SELECT memberId FROM externalUrls WHERE (@id = id)", sqlHelper.CreateParameter("@id", e.ItemId));
		        e.ReceiverId = memberId;
		    }
		}
	}
}