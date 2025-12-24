using System;
using System.Linq;
using Library.UI.NonLive;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace UpdateComments_2
{
	public class NonLiveOrderUpdateCommentHandler
	{
		private readonly Helpers helpers;
		private readonly InteractiveController interactiveController;

		private NonLiveOrder nonLiveOrder;
		private UpdateNonLiveOrderCommentsDialog updateCommentsDialog;

		private NonLiveOrderUpdateCommentHandler(Helpers helpers, InteractiveController interactiveController)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));

			this.interactiveController = interactiveController;
		}

		public static void Run(Helpers helpers, InteractiveController interactiveController)
		{
			var nonLiveOrderUpdateCommentHandler = new NonLiveOrderUpdateCommentHandler(helpers, interactiveController);

			nonLiveOrderUpdateCommentHandler.Run();
		}

		private void Run()
		{
			GetNonLiveOrder();

			updateCommentsDialog = new UpdateNonLiveOrderCommentsDialog(helpers, nonLiveOrder);

			updateCommentsDialog.SaveButton.Pressed += UpdateNonLiveCommentsDialog_SaveButton_Pressed;

			interactiveController.Run(updateCommentsDialog);
		}

		private void GetNonLiveOrder()
		{
			string scriptInput = helpers.Engine.GetScriptParam("ID").Value;

			var ticketIds = TicketingManager.ParseTicketIdsFromScriptInput(scriptInput);

			var ticketId = ticketIds.First();

			nonLiveOrder = helpers.NonLiveOrderManager.GetNonLiveOrder(ticketId.DataMinerID, ticketId.TID);
		}

		private void UpdateNonLiveCommentsDialog_SaveButton_Pressed(object sender, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets.YleValueWidgetChangedEventArgs e)
		{
			nonLiveOrder.OperatorComment = updateCommentsDialog.OperatorComments;
			nonLiveOrder.IsilonBackupFileLocation = updateCommentsDialog.IsilonBackupFileLocation;

			var user = helpers.ContractManager.GetUserInfo(helpers.Engine.UserLoginName).User;

			helpers.NonLiveOrderManager.AddOrUpdateNonLiveOrder(nonLiveOrder, user, out string ticketId);

			var messageDialog = new MessageDialog(helpers.Engine, "Update Successful") { Title = "Update Successful" };
			messageDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("Update Successful");

			interactiveController.ShowDialog(messageDialog);
		}
	}
}
