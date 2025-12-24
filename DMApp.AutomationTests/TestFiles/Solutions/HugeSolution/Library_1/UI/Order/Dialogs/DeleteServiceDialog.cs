namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public sealed class DeleteServiceDialog : YleDialog
	{
		private readonly Label titleLable = new Label { Style = TextStyle.Bold };

		private readonly Label serviceNameLabel = new Label();

		public DeleteServiceDialog(Helpers helpers, Order order, UserInfo userInfo, Service serviceToDelete) : base(helpers)
		{
			ServiceToDelete = serviceToDelete ?? throw new ArgumentNullException(nameof(serviceToDelete));
			Order = order ?? throw new ArgumentNullException(nameof(order));
			UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			Title = "Delete Service";
			titleLable.Text = $"Are you sure you want to delete service {ServiceToDelete.GetShortDescription(order)} ?";
			serviceNameLabel.Text = $"Start time: {ServiceToDelete.Start} | End time: {ServiceToDelete.End}";

			GenerateUi();
		}

		public Order Order { get; }

		public UserInfo UserInfo { get; }

		public Service ServiceToDelete { get; }

		public YleButton NoButton { get; set; } = new YleButton("No") { Width = 120 };

		public YleButton YesButton { get; set; } = new YleButton("Yes") { Width = 120, Style = ButtonStyle.CallToAction };

		public void GenerateUi()
		{
			Clear();
			MinWidth = 800;

			int row = -1;

			AddWidget(titleLable, ++row, 0, 1, 4);
			AddWidget(serviceNameLabel, new WidgetLayout(++row, 0, 1, 6));
			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(NoButton, new WidgetLayout(++row, 0, 1, 2));
			AddWidget(YesButton, new WidgetLayout(row, 3, 1, 2));

			//SetColumnWidth(0, 40);
			//SetColumnWidth(1, 100);
			//SetColumnWidth(3, 250);

			foreach (var yleWidget in Widgets.OfType<IYleWidget>())
			{
				// To enable logging of user input
				yleWidget.Helpers = helpers;
			}
		}

		protected override void HandleEnabledUpdate()
		{
			// do nothing
		}
	}
}
