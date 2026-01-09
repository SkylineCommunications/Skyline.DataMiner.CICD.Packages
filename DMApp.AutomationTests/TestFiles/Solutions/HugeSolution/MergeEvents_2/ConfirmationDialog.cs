namespace MergeEvents_2
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	public class ConfirmationDialog : Dialog
	{
		private readonly Label warningLabel = new Label("Are you sure you want to merge the events? This will cause the following events and/or orders to be removed.");
		private readonly Label eventsTitle = new Label("Events") { Style = TextStyle.Bold };
		private readonly List<Label> eventNameLabels = new List<Label>();
		private readonly Label ordersTitle = new Label("Orders") { Style = TextStyle.Bold };
		private readonly List<Label> orderNameLabels = new List<Label>();

		public ConfirmationDialog(Engine engine, List<Event> eventsToRemove, List<LiteOrder> ordersToRemove) : base(engine)
		{
			foreach (Event eventToRemove in eventsToRemove) eventNameLabels.Add(new Label(eventToRemove.Name));
			foreach (LiteOrder orderToRemove in ordersToRemove) orderNameLabels.Add(new Label(orderToRemove.Name));

			Initialize();
			GenerateUi();
		}

		private void GenerateUi()
		{
			int row = -1;
			AddWidget(warningLabel, ++row, 0, 1, 2);

			if (eventNameLabels.Any())
			{
				AddWidget(eventsTitle, ++row, 0, 1, 2);
				foreach (Label eventNameLabel in eventNameLabels) AddWidget(eventNameLabel, ++row, 0, 1, 2);
			}

			if (orderNameLabels.Any())
			{
				AddWidget(ordersTitle, ++row, 0, 1, 2);
				foreach (Label orderNameLabel in orderNameLabels) AddWidget(orderNameLabel, ++row, 0, 1, 2);
			}

			AddWidget(new WhiteSpace(), ++row, 0, 1, 2);

			AddWidget(BackButton, ++row, 0);
			AddWidget(ContinueButton, ++row, 0);
		}

		private void Initialize()
		{
			Title = "Merge Events";

			BackButton = new Button("Back") { Width = 200 };
			ContinueButton = new Button("Continue") { Width = 200, Style = ButtonStyle.CallToAction };
		}

		public Button BackButton
		{
			get; private set;
		}

		public Button ContinueButton
		{
			get; private set;
		}
	}
}