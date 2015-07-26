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
			Action a = (Action)sender;

			if (a.Alias == "ExternalVote")
			{
				var memberId = OurUmbraco.Powers.BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT memberId FROM externalUrls WHERE (@id = id)", OurUmbraco.Powers.BusinessLogic.Data.SqlHelper.CreateParameter("@id", e.ItemId));
				e.ReceiverId = memberId;
			}
		}
	}
}