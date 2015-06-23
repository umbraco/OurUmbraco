using System;
using umbraco.BusinessLogic;
using uPowers.BusinessLogic;

namespace our.CustomHandlers
{
	public class ExternalVote : ApplicationBase
	{
		public ExternalVote()
		{
			uPowers.BusinessLogic.Action.BeforePerform += new EventHandler<ActionEventArgs>(this.Action_BeforePerform);
		}

		void Action_BeforePerform(object sender, ActionEventArgs e)
		{
			uPowers.BusinessLogic.Action a = (uPowers.BusinessLogic.Action)sender;

			if (a.Alias == "ExternalVote")
			{
				var memberId = uPowers.BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT memberId FROM externalUrls WHERE (@id = id)", uPowers.BusinessLogic.Data.SqlHelper.CreateParameter("@id", e.ItemId));
				e.ReceiverId = memberId;
			}
		}
	}
}