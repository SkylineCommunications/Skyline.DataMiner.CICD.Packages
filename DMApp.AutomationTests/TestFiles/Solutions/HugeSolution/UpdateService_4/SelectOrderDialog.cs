namespace UpdateService_4
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class SelectOrderDialog : Dialog
	{
		private readonly RadioButtonList ordersRadioButtonList = new RadioButtonList();
		private readonly Dictionary<string, Order> orderOptionMapping = new Dictionary<string, Order>();

		public SelectOrderDialog(IEngine engine, IEnumerable<Order> orders) : base(engine)
		{
			Title = "Select Order to Update";

			Initialize(orders);
			GenerateUi();
		}

		private void Initialize(IEnumerable<Order> orders)
		{
			foreach (var order in orders)
			{
				string option = $"{order.DisplayName} ({order.Start} - {order.End})";
				orderOptionMapping.Add(option, order);
			}

			ordersRadioButtonList.Options = orderOptionMapping.Select(x => x.Key).OrderBy(x => x);
		}

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(new Label("Select the order for which you want to swap the source"), ++row, 0);
			AddWidget(ordersRadioButtonList, ++row, 0);
			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(ContinueButton, row + 1, 0);
		}

		public Order SelectedOrder => orderOptionMapping[ordersRadioButtonList.Selected];

		public Button ContinueButton { get; private set; } = new Button("Continue");
	}
}
