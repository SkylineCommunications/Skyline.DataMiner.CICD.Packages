namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	public class OrderDetailsSection : Section
	{
		private readonly CheckBox checkBox = new CheckBox { Width = 18 };
		private readonly Label eventNameLabel = new Label();
		private readonly Label orderNameLabel = new Label();
		private readonly Label orderStartTimeLabel = new Label();
		private readonly Label orderEndTimeLabel = new Label();

		public OrderDetailsSection(LiteOrder order)
		{
			this.Order = order ?? throw new ArgumentNullException(nameof(order));

			eventNameLabel.Text = order.Event.Name;
			orderNameLabel.Text = order.Name;
			orderStartTimeLabel.Text = order.Start.ToLocalTime().ToString();
			orderEndTimeLabel.Text = order.End.ToLocalTime().ToString();

			AddWidget(checkBox, 0, 0, HorizontalAlignment.Center);
			AddWidget(eventNameLabel, 0, 1);
			AddWidget(orderNameLabel, 0, 2);
			AddWidget(orderStartTimeLabel, 0, 3);
			AddWidget(orderEndTimeLabel, 0, 4);
		}

		public LiteOrder Order { get; }

		public bool IsSelected
		{
			get => checkBox.IsChecked || !checkBox.IsVisible;

			set => checkBox.IsChecked = value;
		}

		public bool IsCheckBoxEnabled
		{
			get => checkBox.IsEnabled;

			set => checkBox.IsEnabled = value;
		}
	}

}

