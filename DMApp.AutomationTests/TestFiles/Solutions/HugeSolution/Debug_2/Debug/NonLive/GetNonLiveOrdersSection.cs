namespace Debug_2.Debug.NonLive
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Debug_2.Debug.Tickets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class GetNonLiveOrdersSection : YleSection
	{
		private readonly GetTicketSection getTicketSection;
		private readonly HowToSection howToSection;

		private readonly Label additionalFilteringLabel = new Label("Additional filtering") { Style = TextStyle.Title };

		private readonly CheckBox withoutDeleteTasksCheckBox = new CheckBox("Only Non-Live Orders without delete tasks");

		private readonly Button getSelectedNonLiveOrdersButton = new Button("Get Selected Non-Live Orders") { Style = ButtonStyle.CallToAction };
		private readonly CollapseButton showSelectedNonLiveOrdersButton;
		private readonly TextBox selectedNonLiveOrdersTextBox = new TextBox { IsMultiline = true, MinWidth = 500 };

		public GetNonLiveOrdersSection(Helpers helpers) : base(helpers)
		{
			howToSection = new HowToSection(helpers);

			getTicketSection = new GetTicketSection(helpers);

			getTicketSection.RegenerateUiRequired += HandleRegenerateUiRequired;
			getTicketSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

			getSelectedNonLiveOrdersButton.Pressed += GetSelectedNonLiveOrdersButton_Pressed;

			showSelectedNonLiveOrdersButton = new CollapseButton(selectedNonLiveOrdersTextBox.Yield(), true) { CollapseText = "Hide Selected Non-Live Orders", ExpandText = "Show Selected Non-Live Orders" };

			GenerateUi();
		}

		public IEnumerable<NonLiveOrder> SelectedNonLiveOrders { get; private set; } = new List<NonLiveOrder>();

		public override void RegenerateUi()
		{
			Clear();
			getTicketSection.RegenerateUi();
			GenerateUi();
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			getTicketSection.IsEnabled = IsEnabled;
		}

		private void GetSelectedNonLiveOrdersButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				var nonLiveOrders = getTicketSection.SelectedTickets.Select(t => NonLiveOrderManager.GetNonLiveOrder(t)).ToList();

				if (withoutDeleteTasksCheckBox.IsChecked)
				{
					nonLiveOrders = nonLiveOrders.Where(nlo => !helpers.NonLiveUserTaskManager.GetNonLiveUserTasks(nlo).Any()).ToList();
				}

				SelectedNonLiveOrders = nonLiveOrders;

				selectedNonLiveOrdersTextBox.Text = string.Join("\n", SelectedNonLiveOrders.Select(nlo => nlo.OrderDescription));
			}
		}

		private void GenerateUi()
		{
			int row = -1;

			row++;

			AddSection(getTicketSection, new SectionLayout(row, 0));

			AddSection(howToSection, new SectionLayout(row, getTicketSection.ColumnCount + 1));

			row += new[] { getTicketSection.RowCount, howToSection.RowCount }.Max();

			AddWidget(additionalFilteringLabel, ++row, 0, 1, 5);

			AddWidget(withoutDeleteTasksCheckBox, ++row, 0);

			AddWidget(getSelectedNonLiveOrdersButton, ++row, 0);
			AddWidget(showSelectedNonLiveOrdersButton, row, 1);
			AddWidget(selectedNonLiveOrdersTextBox, ++row, 1);
		}
	}
}
