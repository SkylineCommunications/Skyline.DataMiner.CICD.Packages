namespace LiveOrderForm_6.Dialogs
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class UnableToMergeOrdersDialog : Dialog
	{
		public enum Causes
		{
			[Description("It is not possible to merge multiple orders created by an integration. \nBelow you can find which orders are created by which integration.")]
			MultipleIntegrationOrders,
			[Description("It is not possible to merge rejected or running orders. \nBelow you can find which orders have which status.")]
			RejectedOrRunningOrders,
			[Description("It is not possible to merge locked orders. \nBelow you can find which orders are locked and by whom.")]
			LockedOrders
		}

		private readonly List<Order> orders;
		private readonly Causes cause;

		private readonly Label explanationLabel;
		private readonly Label ordersTitleLabel = new Label("Selected Orders") { Style = TextStyle.Bold };
		private readonly Label orderNameColumnHeaderLabel = new Label("NAME");
		private readonly Label orderIntegrationTypeColumnHeaderLabel = new Label("INTEGRATION TYPE");
		private readonly Label orderStatusColumnHeaderLabel = new Label("STATUS");
		private readonly Label orderLockingStatusColumnHeaderLabel = new Label("LOCKING STATUS");
		private readonly Label orderLockedByColumnHeaderLabel = new Label("LOCKED BY");
		private readonly List<Label> orderNameLabels = new List<Label>();
		private readonly List<Label> orderIntegrationTypeLabels = new List<Label>();
		private readonly List<Label> orderStatusLabels = new List<Label>();
		private readonly List<Label> orderLockingStatusLabels = new List<Label>();
		private readonly List<Label> orderLockedByLabels = new List<Label>();

		public UnableToMergeOrdersDialog(Engine engine, IEnumerable<Order> orders, Causes cause, IEnumerable<LockInfo> lockInfos = null) : base(engine)
		{
			this.Title = "Unable to Merge Orders";
			this.orders = orders.ToList();
			this.cause = cause;

			explanationLabel = new Label(EnumExtensions.GetDescriptionFromEnumValue(cause));

			foreach (Order order in orders.OrderBy(x => x.Name))
			{
				orderNameLabels.Add(new Label(order.Name));
				orderIntegrationTypeLabels.Add(new Label(EnumExtensions.GetDescriptionFromEnumValue(order.IntegrationType)));
				orderStatusLabels.Add(new Label(EnumExtensions.GetDescriptionFromEnumValue(order.Status)));

				if (lockInfos != null)
				{
					LockInfo lockInfo = lockInfos.FirstOrDefault(x => x.ObjectId == order.Id.ToString());
					orderLockingStatusLabels.Add(new Label(lockInfo != null ? (lockInfo.IsLockGranted ? "Unlocked" : "Locked") : "Unknown"));
					orderLockedByLabels.Add(new Label(lockInfo != null ? lockInfo.LockUsername : "Unknown"));
				}
			}

			GenerateUI();
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(explanationLabel, new WidgetLayout(row++, 0, 2, 3));

			AddWidget(ordersTitleLabel, new WidgetLayout(++row, 0, 1, 3));

			AddWidget(orderNameColumnHeaderLabel, new WidgetLayout(++row, 0));
			switch (cause)
			{
				case Causes.MultipleIntegrationOrders:
					AddWidget(orderIntegrationTypeColumnHeaderLabel, new WidgetLayout(row, 1));
					break;
				case Causes.RejectedOrRunningOrders:
					AddWidget(orderStatusColumnHeaderLabel, new WidgetLayout(row, 1));
					break;
				case Causes.LockedOrders:
					AddWidget(orderLockingStatusColumnHeaderLabel, new WidgetLayout(row, 1));
					AddWidget(orderLockedByColumnHeaderLabel, new WidgetLayout(row, 2));
					break;
				default:
					break;
			}

			for (int i = 0; i < orders.Count; i++)
			{
				AddWidget(orderNameLabels[i], new WidgetLayout(++row, 0));
				switch (cause)
				{
					case Causes.MultipleIntegrationOrders:
						AddWidget(orderIntegrationTypeLabels[i], new WidgetLayout(row, 1));
						break;
					case Causes.RejectedOrRunningOrders:
						AddWidget(orderStatusLabels[i], new WidgetLayout(row, 1));
						break;
					case Causes.LockedOrders:
						AddWidget(orderLockingStatusLabels[i], new WidgetLayout(row, 1));
						AddWidget(orderLockedByLabels[i], new WidgetLayout(row, 2));
						break;
					default:
						break;
				}
			}
		}
	}
}