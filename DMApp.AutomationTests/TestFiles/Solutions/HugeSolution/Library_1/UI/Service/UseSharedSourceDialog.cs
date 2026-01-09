namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UseSharedSourceDialog : EditOrderDialog
	{
		public UseSharedSourceDialog(Helpers helpers, Order order, Event @event, UserInfo userInfo, Service serviceBeingEdited, LockInfo lockInfo, EditOrderFlows flow) : base(helpers, order, @event, userInfo, Configuration.Scripts.UpdateService, flow, lockInfo)
		{
			ServiceBeingEdited = serviceBeingEdited as DisplayedService ?? throw new ArgumentNullException(nameof(serviceBeingEdited));

			MultiThreadedInitialize();
			Initialize();

			GenerateUi();
			HandleVisibilityAndEnabledUpdate();
		}

		public DisplayedService ServiceBeingEdited { get; private set; }

		public NormalOrderSection NormalOrderSection { get; private set; }

		public YleButton ConfirmButton { get; set; } = new YleButton("Confirm") { Width = 120, Style = ButtonStyle.CallToAction };

		public YleButton ExitButton { get; set; } = new YleButton("Exit") { Width = 120, Style = ButtonStyle.CallToAction };

		protected override void AdditionalScriptActionsAsFinalInitialization()
		{
			NormalOrderSection = orderSection as NormalOrderSection ?? throw new InvalidOperationException();
			NormalOrderSection.UseSharedSources(true);
		}

		protected override void GenerateUi()
		{
			Clear();
			MinWidth = 800;

			int row = -1;

			AddWidget(lockInfoLabel, ++row, 0, 1, 4);

			AddWidget(new Label(order.DisplayName) { Style = TextStyle.Bold }, ++row, 0, 1, 6);

			AddSection(NormalOrderSection, new SectionLayout(++row, 0));
			row += NormalOrderSection.RowCount;

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0, 1, 6));
			if (isReadOnly)
			{
				AddWidget(ExitButton, new WidgetLayout(++row, 0, 1, 6));
			}
			else
			{
				AddWidget(ConfirmButton, new WidgetLayout(++row, 0, 1, 6));
			}

			AddWidget(validationLabel, new WidgetLayout(row + 1, 0, 1, 6));

			SetColumnWidth(0, 0);
			SetColumnWidth(1, 0);

			foreach (var yleWidget in Widgets.OfType<IYleWidget>())
			{
				// To enable logging of user input
				yleWidget.Helpers = helpers;
			}
		}

		protected override string GetTitle()
		{
			return "Use Shared Source";
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			LogMethodStart(nameof(HandleVisibilityAndEnabledUpdate), out var stopwatch);

			lockInfoLabel.IsVisible = lockInfo != null && !lockInfo.IsLockGranted;

			orderSection.IsEnabled = IsEnabled;

			ExitButton.IsEnabled = IsEnabled;
			ConfirmButton.IsEnabled = IsEnabled;

			LogMethodCompleted(nameof(HandleVisibilityAndEnabledUpdate), stopwatch);
		}

		protected override void SubscribeToOrderControllerAndOrderSection()
		{
			// nothing to do
		}
	}
}
