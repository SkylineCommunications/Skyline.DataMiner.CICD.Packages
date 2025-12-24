namespace LiveOrderForm_6.Dialogs
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	public class OrderDeletionDialog : Dialog
	{
		private readonly Label informationLabel = new Label();

		public OrderDeletionDialog(IEngine engine, Order order, LockInfo lockInfo) : base(engine)
		{
			if (engine == null) throw new ArgumentNullException("engine");
			if (order == null) throw new ArgumentNullException("order");
			if (lockInfo == null) throw new ArgumentNullException("lockInfo");

			Title = "Delete Order";

			YesButton = new Button("Yes") { MinWidth = 100, Style = ButtonStyle.CallToAction };
			NoButton = new Button("No") { MinWidth = 100 };
			OkButton = new Button("Ok") { MinWidth = 100, Style = ButtonStyle.CallToAction };

			if (!lockInfo.IsLockGranted)
			{
				informationLabel.Text = String.Format("Unable to delete Order as it is currently locked by user {0}", lockInfo.LockUsername);
				AddWidget(informationLabel, 0, 0, 1, 2);
				AddWidget(OkButton, 1, 0);
			}
			else
			{
				informationLabel.Text = String.Format("Are you sure you want to delete the following Order?\n{0}", order.Name);

				AddWidget(informationLabel, 0, 0, 1, 2);
				AddWidget(YesButton, 1, 0);
				AddWidget(NoButton, 1, 1, HorizontalAlignment.Right);
			}
		}

		public Button YesButton { get; private set; }

		public Button NoButton { get; private set; }

		public Button OkButton { get; private set; }
	}
}