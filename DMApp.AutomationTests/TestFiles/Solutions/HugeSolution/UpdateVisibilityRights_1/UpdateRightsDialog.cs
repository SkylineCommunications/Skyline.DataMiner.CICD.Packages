namespace UpdateVisibilityRights_1
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UpdateRightsDialog : Dialog
	{
		private readonly Label reservationNameLabel = new Label("Name");
		private readonly Label reservationIdLabel = new Label("GUID");
		private readonly Label orderRightsLabel = new Label("Order Rights");
		private readonly Label eventRightsLabel = new Label("Event Rights");
		private readonly Dictionary<string, int> visibilityRights;

		private BookedOrder order;

		public UpdateRightsDialog(IEngine engine, Dictionary<string, int> visibilityRights) : base(engine)
		{
			Title = "Update Visibility Rights";

			this.visibilityRights = visibilityRights;

			OrderNameTextBox = new TextBox { PlaceHolder = "Name", Width = 400 };
			OrderGuidTextBox = new TextBox { PlaceHolder = "GUID", ValidationText = "Invalid GUID", Width = 400 };

			FindOrderByNameButton = new Button("Find By Name") { Width = 150 };
			FindOrderByGuidButton = new Button("Find By GUID") { Width = 150 };

			OrderSecurityViewIdsCheckBoxList = new CheckBoxList(visibilityRights.Keys);
			EventSecurityViewIdsCheckBoxList = new CheckBoxList(visibilityRights.Keys);
			UpdateSecurityViewIdsButton = new Button("Update Rights") { Width = 150, IsEnabled = false };

			GenerateUI();
		}

		public void GenerateUI()
		{
			Clear();

			int row = -1;

			AddWidget(reservationNameLabel, ++row, 0, 1, 2);
			AddWidget(OrderNameTextBox, row, 2);
			AddWidget(FindOrderByNameButton, row, 3);

			AddWidget(reservationIdLabel, ++row, 0, 1, 2);
			AddWidget(OrderGuidTextBox, row, 2);
			AddWidget(FindOrderByGuidButton, row, 3);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(orderRightsLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(OrderSecurityViewIdsCheckBoxList, row, 1, 1, 2);

			AddWidget(eventRightsLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(EventSecurityViewIdsCheckBoxList, row, 1, 1, 2);

			AddWidget(UpdateSecurityViewIdsButton, ++row, 1, 1, 2);
		}

		public void UpdateOrderVisibilityRights(HashSet<int> orderRights)
		{
			foreach (var kvp in visibilityRights)
			{
				if (orderRights.Contains(kvp.Value))
				{
					OrderSecurityViewIdsCheckBoxList.Check(kvp.Key);
				}
				else
				{
					OrderSecurityViewIdsCheckBoxList.Uncheck(kvp.Key);
				}
			}
		}

		public void UpdateEventVisibilityRights(IEnumerable<int> eventRights)
		{
			foreach (var kvp in visibilityRights)
			{
				if (eventRights.Contains(kvp.Value))
				{
					EventSecurityViewIdsCheckBoxList.Check(kvp.Key);
				}
				else
				{
					EventSecurityViewIdsCheckBoxList.Uncheck(kvp.Key);
				}
			}
		}

		public BookedOrder Order
		{
			get { return order; }
			set
			{
				order = value;
				if (order == null)
				{
					OrderSecurityViewIdsCheckBoxList.SetOptions(new string[0]);
					EventSecurityViewIdsCheckBoxList.SetOptions(new string[0]);
					UpdateSecurityViewIdsButton.IsEnabled = false;
				}
				else
				{
					UpdateOrderVisibilityRights(order.SecurityViewIds);
					UpdateEventVisibilityRights(order.Job.SecurityViewIDs);
					UpdateSecurityViewIdsButton.IsEnabled = true;
				}
			}
		}

		public HashSet<int> SelectedOrderSecurityViewIds
		{
			get
			{
				HashSet<int> viewIds = new HashSet<int>();
				foreach (string checkedOption in OrderSecurityViewIdsCheckBoxList.Checked)
				{
					viewIds.Add(visibilityRights[checkedOption]);
				}

				return viewIds;
			}
		}

		public HashSet<int> SelectedEventSecurityViewIds
		{
			get
			{
				HashSet<int> viewIds = new HashSet<int>();
				foreach (string checkedOption in EventSecurityViewIdsCheckBoxList.Checked)
				{
					viewIds.Add(visibilityRights[checkedOption]);
				}

				return viewIds;
			}
		}

		public TextBox OrderNameTextBox { get; private set; }

		public TextBox OrderGuidTextBox { get; private set; }

		public Button FindOrderByNameButton { get; private set; }

		public Button FindOrderByGuidButton { get; private set; }

		public CheckBoxList OrderSecurityViewIdsCheckBoxList { get; private set; }

		public CheckBoxList EventSecurityViewIdsCheckBoxList { get; private set; }

		public Button UpdateSecurityViewIdsButton { get; private set; }
	}
}