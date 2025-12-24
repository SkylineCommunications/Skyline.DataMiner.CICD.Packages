namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public sealed class UpdateServiceDialog : EditOrderDialog
	{
		private readonly Label orderNameLabel = new Label { Style = TextStyle.Bold };

		public UpdateServiceDialog(Helpers helpers, Order order, Event @event, UserInfo userInfo, Service serviceBeingEdited, LockInfo lockInfo, EditOrderFlows flow) : base(helpers, order, @event, userInfo, Configuration.Scripts.UpdateService, flow, lockInfo)
		{
			ServiceBeingEdited = serviceBeingEdited as DisplayedService ?? throw new ArgumentNullException(nameof(serviceBeingEdited));

			ServiceBeingEdited.IsDisplayed = true;

			orderNameLabel.Text = $"Order {Order.Name}";

			MultiThreadedInitialize();
			Initialize();

			GenerateUi();
			HandleVisibilityAndEnabledUpdate();
		}

		public DisplayedService ServiceBeingEdited { get; private set; }

		public YleButton ConfirmButton { get; set; } = new YleButton("Confirm") { Width = 120, Style = ButtonStyle.CallToAction };

		public YleButton ReportIssueButton { get; set; } = new YleButton("Report Issue") { Width = 120 };

		public YleButton ExitButton { get; set; } = new YleButton("Exit") { Width = 120, Style = ButtonStyle.CallToAction };

		protected override void AdditionalScriptActionsAsFinalInitialization()
		{
			// no additional actions required
		}

		protected override void GenerateUi()
		{
			Clear();
			MinWidth = 800;

			int row = -1;

			AddWidget(lockInfoLabel, ++row, 0, 1, 4);

			AddWidget(orderNameLabel, new WidgetLayout(++row, 0, 1, 6));

			var serviceSection = orderSection.GetServiceSections(ServiceBeingEdited).First();
			AddSection(serviceSection, new SectionLayout(++row, 0));
			row += serviceSection.RowCount;

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));
			if (isReadOnly)
			{
				AddWidget(ExitButton, new WidgetLayout(++row, 0, 1, 6));
			}
			else
			{
				AddWidget(ConfirmButton, new WidgetLayout(++row, 0, 1, 6));
				AddWidget(ReportIssueButton, new WidgetLayout(++row, 0, 1, 6));
			}

			AddWidget(validationLabel, new WidgetLayout(row + 1, 0, 1, 6));

			SetColumnWidth(0, 40);
			SetColumnWidth(1, 100);
			SetColumnWidth(3, 250);
			SetColumnWidth(7, 300); // longer resource dropdowns

			foreach (var yleWidget in Widgets.OfType<IYleWidget>())
			{
				// To enable logging of user input
				yleWidget.Helpers = helpers;
			}
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			LogMethodStart(nameof(HandleVisibilityAndEnabledUpdate), out var stopwatch);

			lockInfoLabel.IsVisible = lockInfo != null && !lockInfo.IsLockGranted;

			orderSection.IsEnabled = IsEnabled;

			ExitButton.IsEnabled = IsEnabled;
			ConfirmButton.IsEnabled = IsEnabled;

			ReportIssueButton.IsVisible = (ServiceBeingEdited.Status == YLE.Service.Status.ServiceRunning || ServiceBeingEdited.Status == YLE.Service.Status.PostRoll || ServiceBeingEdited.Definition.VirtualPlatform == VirtualPlatform.Recording && ServiceBeingEdited.Status == YLE.Service.Status.FileProcessing) && userInfo.IsMcrUser;
			ReportIssueButton.IsEnabled = IsEnabled;

			LogMethodCompleted(nameof(HandleVisibilityAndEnabledUpdate), stopwatch);
		}

		protected override string GetTitle()
		{
			return "Service Edit";
		}

		protected override void SubscribeToOrderControllerAndOrderSection()
		{
			orderController.ServiceReplaced += OrderController_ServiceReplaced;
			orderController.ValidationRequired += (s, e) => ConfirmButton.IsEnabled = IsValid(false, false, false);
		}

		private void OrderController_ServiceReplaced(object sender, EventArguments.ServiceReplacedEventArgs e)
		{
			ServiceBeingEdited.IsDisplayed = false;

			ServiceBeingEdited = e.ReplacingService as DisplayedService;
			ServiceBeingEdited.IsDisplayed = true;
		}
	}
}
