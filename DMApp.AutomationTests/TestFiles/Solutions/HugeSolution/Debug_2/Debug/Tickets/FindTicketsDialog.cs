namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Tickets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Debug_2.Debug.Reservations;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class FindTicketsDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label findTicketsLabel = new Label("Find Ticket") { Style = TextStyle.Heading };
		private readonly Label ticketNameLabel = new Label("Name");
		private readonly Label ticketIdLabel = new Label("GUID");
		private readonly Button enterCurrentIdButton = new Button("Enter Current ID") { Width = 200 };
		private readonly TextBox ticketNameTextBox = new TextBox();
		private readonly TextBox ticketIdTextBox = new TextBox();
		private readonly Button findByTicketNameButton = new Button("Find By Name") { Width = 150 };
		private readonly Button findByTicketIdButton = new Button("Find By ID") { Width = 150 };

		private readonly List<ResponseSection> responseSections = new List<ResponseSection>();

		public FindTicketsDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Find Tickets";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private void Initialize()
		{
			findByTicketIdButton.Pressed += FindByTicketIdButton_Pressed;
			findByTicketNameButton.Pressed += FindByTicketNameButton_Pressed;
			enterCurrentIdButton.Pressed += (sender, args) => ticketIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;
		}

		private void FindByTicketNameButton_Pressed(object sender, EventArgs e)
		{
			var ticket = helpers.UserTaskManager.TicketingManager.GetTicketWithFieldValue("Name", ticketNameTextBox.Text);

			ShowRequestResult($"Ticket {ticket.ID}", ticket.ToJson());
		}

		private void FindByTicketIdButton_Pressed(object sender, EventArgs e)
		{
			var splitId = ticketIdTextBox.Text.Split('/');

			var ticket = helpers.UserTaskManager.TicketingManager.GetTicket(Convert.ToInt32(splitId[0]), Convert.ToInt32(splitId[1]));

			ShowRequestResult($"Ticket {ticket.ID}", ticket.ToJson());
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(findTicketsLabel, ++row, 0, 1, 5);

			AddWidget(ticketNameLabel, ++row, 0, 1, 2);
			AddWidget(ticketNameTextBox, row, 2);
			AddWidget(findByTicketNameButton, row, 3);

			AddWidget(ticketIdLabel, ++row, 0, 1, 2);
			AddWidget(ticketIdTextBox, row, 2);
			AddWidget(findByTicketIdButton, row, 3);
			AddWidget(enterCurrentIdButton, ++row, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			row++;
			foreach (var responseSection in responseSections)
			{
				responseSection.Collapse();
				AddSection(responseSection, row, 0);
				row += responseSection.RowCount;
			}
		}
	}
}
