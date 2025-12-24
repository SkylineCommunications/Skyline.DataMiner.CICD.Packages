namespace Debug_2.Debug.Tickets
{
	using System;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class FindTicketsWithFiltersDialog : DebugDialog
	{
		private TicketingGatewayHelper ticketingHelper;

		private readonly GetTicketSection getTicketsSection;

		private readonly Button showJsonButton = new Button("Show Tickets JSON");
		private readonly Button deleteButton = new Button("Delete Tickets");
		private readonly CheckBox confirmDeleteChecbox = new CheckBox("Confirm");

		public FindTicketsWithFiltersDialog(Helpers helpers) : base(helpers)
		{
			getTicketsSection = new GetTicketSection(helpers);

			Title = "Find Tickets with Filters";
			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			ticketingHelper = new TicketingGatewayHelper { HandleEventsAsync = false };
			ticketingHelper.RequestResponseEvent += (sender, args) => args.responseMessage = Engine.SendSLNetSingleResponseMessage(args.requestMessage);

			getTicketsSection.RegenerateUiRequired += GetTicketsSection_RegenerateUi;
			getTicketsSection.UiEnabledStateChangeRequired += Section_UiEnabledStateChangeRequired;

			showJsonButton.Pressed += ShowJsonButton_Pressed;

			deleteButton.Pressed += DeleteButton_Pressed;
		}

		private void DeleteButton_Pressed(object sender, EventArgs e)
		{
			if (!confirmDeleteChecbox.IsChecked) return;

			if (ticketingHelper.RemoveTickets(out string error, getTicketsSection.SelectedTickets.ToArray()))
			{
				ShowRequestResult($"Removed tickets", string.Join(", ", getTicketsSection.SelectedTickets.Select(t => t.ID)));
			}
			else
			{
				ShowRequestResult($"Error while removing tickets", error);
			}

			RegenerateUi();
		}

		private void GetTicketsSection_RegenerateUi(object sender, EventArgs e)
		{
			RegenerateUi();
		}

		private void ShowJsonButton_Pressed(object sender, EventArgs e)
		{
			ShowRequestResult("Serialized Tickets", string.Join("\n", getTicketsSection.SelectedTickets.Select(r => JsonConvert.SerializeObject(r))));
			RegenerateUi();
		}

		private void RegenerateUi()
		{
			getTicketsSection.RegenerateUi();
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddSection(getTicketsSection, new SectionLayout(++row, 0));
			row += getTicketsSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(showJsonButton, ++row, 0);
			AddWidget(deleteButton, ++row, 0);
			AddWidget(confirmDeleteChecbox, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddResponseSections(row);
		}
	}
}
