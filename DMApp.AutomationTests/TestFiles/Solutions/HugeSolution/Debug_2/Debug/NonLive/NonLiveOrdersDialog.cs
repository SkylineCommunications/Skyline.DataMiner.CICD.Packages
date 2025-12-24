namespace Debug_2.Debug.NonLive
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class NonLiveOrdersDialog : DebugDialog
	{
		private readonly Label actionsLabel = new Label("Actions") { Style = TextStyle.Title };

		private readonly GetNonLiveOrdersSection getNonLiveOrdersSection;

		private readonly YleButton addOrUpdateUserTasksButton = new YleButton("Add or Update User Tasks") { Style = ButtonStyle.CallToAction };

		public NonLiveOrdersDialog(Helpers helpers) : base(helpers)
		{
			Title = "Non-Live Orders";

			getNonLiveOrdersSection = new GetNonLiveOrdersSection(helpers);
			
			getNonLiveOrdersSection.RegenerateUiRequired += (o, e) => RegenerateUi();
			getNonLiveOrdersSection.UiEnabledStateChangeRequired += Section_UiEnabledStateChangeRequired;

			addOrUpdateUserTasksButton.Pressed += AddOrUpdateUserTasksButton_Pressed;

			GenerateUi();
		}

		private void AddOrUpdateUserTasksButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			try
			{
				var nonLiveOrders = getNonLiveOrdersSection.SelectedNonLiveOrders;

				foreach (var nonLiveOrder in nonLiveOrders)
				{
					helpers.NonLiveUserTaskManager.AddOrUpdateUserTasks(nonLiveOrder);
				}

				ShowRequestResult("Added or Updated user tasks");
				RegenerateUi();
			}
			catch(Exception ex)
			{
				ShowRequestResult("Exception occurred", ex.ToString());
				RegenerateUi();
			}
		}

		private void RegenerateUi()
		{
			Clear();
			getNonLiveOrdersSection.RegenerateUi();
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 5);

			AddSection(getNonLiveOrdersSection, ++row, 0);

			row += getNonLiveOrdersSection.RowCount;

			AddWidget(actionsLabel, ++row, 0, 1, 5);
			AddWidget(addOrUpdateUserTasksButton, ++row, 0);

			foreach (var responseSection in responseSections)
			{
				AddSection(responseSection, ++row, 0);
				row += responseSection.RowCount;
			}
		}
	}
}