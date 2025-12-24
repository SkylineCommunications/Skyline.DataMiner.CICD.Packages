namespace Debug_2.Debug.NonLiveUserTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Debug_2.Debug.Tickets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UpdateNonLiveOrderUserTasksDialog : DebugDialog
	{
		private readonly Label header = new Label("Update Non-Live Order User Tasks") { Style = TextStyle.Title };

		private readonly GetTicketSection getTicketSection;

		private readonly Button updateUserTasksAndPropertiesForCompletedNonLiveOrders = new Button("Update user tasks and properties for completed non-live orders");
		private readonly Label explanationLabel = new Label("Feature created for DCP195594");

		private readonly Button updateSpecificUserTaskValuesButton = new Button("Update certain user task values");
		private readonly Label updateSpecificUserTaskValuesLabel = new Label("Feature created for DCP212455");

		public UpdateNonLiveOrderUserTasksDialog(Helpers helpers) : base(helpers)
		{
			getTicketSection = new GetTicketSection(helpers);

			getTicketSection.RegenerateUiRequired += (s, e) =>
			{
				getTicketSection.RegenerateUi();
				GenerateUi();
			};

			getTicketSection.AddPropertyExistenceFilter("Non Live Order Name", true);

			getTicketSection.UiEnabledStateChangeRequired += Section_UiEnabledStateChangeRequired;

			updateUserTasksAndPropertiesForCompletedNonLiveOrders.Pressed += UpdateUserTasksAndPropertiesForCompletedNonLiveOrders_Pressed;

			updateSpecificUserTaskValuesButton.Pressed += UpdateSpecificUserTaskValuesButton_Pressed;

			GenerateUi();
		}

		private void UpdateSpecificUserTaskValuesButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				var nonLiveUserTaskTickets = getTicketSection.SelectedTickets.ToList();

				var userTasks = helpers.NonLiveUserTaskManager.CreateNonLiveUserTasks(nonLiveUserTaskTickets);

				var notFoundNonLiveOrders = new List<string>();
				var succeededUpdatedUserTasks = new List<NonLiveUserTask>();

				foreach (var userTask in userTasks)
				{
					if (!Int32.TryParse(userTask.IngestExportForeignKey.Split('/').FirstOrDefault(), out int nonLiveOrderAgentId) || !Int32.TryParse(userTask.IngestExportForeignKey.Split('/').LastOrDefault(), out int nonLiveOrderTicketId) || !helpers.NonLiveOrderManager.TryGetNonLiveOrder(nonLiveOrderAgentId, nonLiveOrderTicketId, out var nonLiveOrder))
					{
						notFoundNonLiveOrders.Add(userTask.IngestExportForeignKey);
						continue;
					}

					userTask.DeadlineDate = nonLiveOrder.Deadline;

					if (nonLiveOrder is Project projectOrder)
					{
						userTask.DeliveryDate = projectOrder.MaterialDeliveryTime;
					}
					else if (nonLiveOrder is Ingest ingestOrder)
					{
						userTask.DeliveryDate = ingestOrder.DeliveryTime;
					}

					userTask.AddOrUpdate(helpers);

					succeededUpdatedUserTasks.Add(userTask);
				}

				ShowRequestResult($"Updated deadline and delivery time on non-live user tasks", $"Updated deadline and delivery time on non-live user tasks:\n{string.Join("\n", succeededUpdatedUserTasks.Select(u => u.ID))}");
				ShowRequestResult($"Unable to find Non-Live orders for user tasks", string.Join("\n", notFoundNonLiveOrders));
				GenerateUi();
			}	
		}

		private void UpdateUserTasksAndPropertiesForCompletedNonLiveOrders_Pressed(object sender, EventArgs e)
		{
			var completedNonLiveOrders = helpers.NonLiveOrderManager.GetAllCompletedNonLiveOrders();

			foreach (var nonLiveOrder in completedNonLiveOrders)
			{
				helpers.NonLiveUserTaskManager.AddOrUpdateUserTasks(nonLiveOrder);
			}

			ShowRequestResult($"Updated user tasks for {completedNonLiveOrders.Count()} completed non-live orders", string.Join("\n", completedNonLiveOrders.Select(u => $"{u.ShortDescription}")));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(header, ++row, 0, 1, 5);

			AddSection(getTicketSection, ++row, 0);
			row += getTicketSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);

			//AddWidget(updateUserTasksAndPropertiesForCompletedNonLiveOrders, ++row, 0);
			//AddWidget(explanationLabel, row, 1);

			AddWidget(updateSpecificUserTaskValuesButton, ++row, 0);
			AddWidget(updateSpecificUserTaskValuesLabel, row, 1);

			AddResponseSections(row);
		}

		protected override void HandleEnabledUpdate()
		{
			base.HandleEnabledUpdate();

			getTicketSection.IsEnabled = IsEnabled;
		}
	}
}
