namespace DeleteIntegrationOrders_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class IntegrationOrdersDialog : Dialog
	{
		public IntegrationOrdersDialog(Engine engine) : base(engine)
		{
			Title = "Delete Integration Orders";

			PlasmaOrdersRadioButtonList.Changed += (sender, args) =>
			{
				PlasmaOrdersDropdown.IsEnabled = (args.SelectedValue == "Single");
				PlasmaMultipleOrdersTextBox.IsEnabled = (args.SelectedValue == "Multiple");
			};

			GenerateUI();
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(new Label("Plasma") { Style = TextStyle.Heading }, ++row, 0, 1, 3);

			AddWidget(new Label("Orders"), ++row, 0);
			AddWidget(PlasmaOrdersRadioButtonList, row, 1);

			AddWidget(PlasmaOrdersDropdown, ++row, 1);
			AddWidget(DeleteSelectedPlasmaOrderButton, row, 2);

			AddWidget(PlasmaMultipleOrdersTextBox, ++row, 1);

			AddWidget(new Label("Events"), ++row, 0);
			AddWidget(PlasmaEventsDropdown, row, 1);
			AddWidget(DeleteSelectedPlasmaEventButton, row, 2);

			AddWidget(new Label("Feenix") { Style = TextStyle.Heading }, ++row, 0, 1, 3);

			AddWidget(new Label("Orders"), ++row, 0);
			AddWidget(FeenixOrdersDropdown, row, 1);
			AddWidget(DeleteSelectedFeenixOrderButton, row, 2);

			AddWidget(new Label("Events"), ++row, 0);
			AddWidget(FeenixEventsDropdown, row, 1);
			AddWidget(DeleteSelectedFeenixEventButton, row, 2);

			AddWidget(new Label("Eurovision") { Style = TextStyle.Heading }, ++row, 0, 1, 3);

			AddWidget(new Label("Orders"), ++row, 0);
			AddWidget(EbuOrdersDropdown, row, 1);
			AddWidget(DeleteSelectedEbuOrderButton, row, 2);

			AddWidget(new Label("Events"), ++row, 0);
			AddWidget(EbuEventsDropdown, row, 1);
			AddWidget(DeleteSelectedEbuEventButton, row, 2);

			AddWidget(new Label("Ceiton") { Style = TextStyle.Heading }, ++row, 0, 1, 3);

			AddWidget(new Label("Events"), ++row, 0);
			AddWidget(CeitonEventsDropdown, row, 1);
			AddWidget(DeleteSelectedCeitonEventButton, row, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(DeleteAllIntegrationsButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(ExitButton, row + 1, 0);
		}

		public void InitPlasmaOrders(List<ISrmObject> orders)
		{
			PlasmaOrders = orders;
			PlasmaOrdersDropdown.Options = orders.Select(x => x.Name);
		}

		public void InitPlasmaEvents(List<ISrmObject> events)
		{
			PlasmaEvents = events;
			PlasmaEventsDropdown.Options = events.Select(x => x.Name);
		}

		public void InitFeenixOrders(List<ISrmObject> orders)
		{
			FeenixOrders = orders;
			FeenixOrdersDropdown.Options = orders.Select(x => x.Name);
		}

		public void InitFeenixEvents(List<ISrmObject> events)
		{
			FeenixEvents = events;
			FeenixEventsDropdown.Options = events.Select(x => x.Name);
		}

		public void InitEbuOrders(List<ISrmObject> orders)
		{
			EbuOrders = orders;
			EbuOrdersDropdown.Options = orders.Select(x => x.Name);
		}

		public void InitEbuEvents(List<ISrmObject> events)
		{
			EbuEvents = events;
			EbuEventsDropdown.Options = events.Select(x => x.Name);
		}

		public void InitCeitonEvents(List<ISrmObject> events)
		{
			CeitonEvents = events;
			CeitonEventsDropdown.Options = events.Select(x => x.Name);
		}

		public List<ISrmObject> PlasmaOrders { get; private set; } = new List<ISrmObject>();

		public List<ISrmObject> PlasmaEvents { get; private set; } = new List<ISrmObject>();

		public List<ISrmObject> FeenixOrders { get; private set; } = new List<ISrmObject>();

		public List<ISrmObject> FeenixEvents { get; private set; } = new List<ISrmObject>();

		public List<ISrmObject> EbuOrders { get; private set; } = new List<ISrmObject>();

		public List<ISrmObject> EbuEvents { get; private set; } = new List<ISrmObject>();

		public List<ISrmObject> CeitonEvents { get; private set; } = new List<ISrmObject>();

		public ISrmObject[] SelectedPlasmaOrders
		{
			get
			{
				if (PlasmaOrdersRadioButtonList.Selected == "Single")
				{
					return new ISrmObject[] { PlasmaOrders.FirstOrDefault(x => x.Name.Equals(PlasmaOrdersDropdown.Selected)) };
				}
				else
				{
					string[] splitPlasmaIds = PlasmaMultipleOrdersTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
					List<ISrmObject> selectedOrders = new List<ISrmObject>();
					foreach (string plasmaId in splitPlasmaIds)
					{
						selectedOrders.Add(PlasmaOrders.FirstOrDefault(x => x.Name.Contains(plasmaId)));
					}

					return selectedOrders.ToArray();
				}
			}
		}

		public ISrmObject SelectedPlasmaEvent
		{
			get
			{
				return PlasmaEvents.FirstOrDefault(x => x.Name.Equals(PlasmaEventsDropdown.Selected));
			}
		}

		public ISrmObject SelectedFeenixOrder
		{
			get
			{
				return FeenixOrders.FirstOrDefault(x => x.Name.Equals(FeenixOrdersDropdown.Selected));
			}
		}

		public ISrmObject SelectedFeenixEvent
		{
			get
			{
				return FeenixEvents.FirstOrDefault(x => x.Name.Equals(FeenixEventsDropdown.Selected));
			}
		}

		public ISrmObject SelectedEbuOrder
		{
			get
			{
				return EbuOrders.FirstOrDefault(x => x.Name.Equals(EbuOrdersDropdown.Selected));
			}
		}

		public ISrmObject SelectedEbuEvent
		{
			get
			{
				return EbuEvents.FirstOrDefault(x => x.Name.Equals(EbuEventsDropdown.Selected));
			}
		}

		public ISrmObject SelectedCeitonEvent
		{
			get
			{
				return CeitonEvents.FirstOrDefault(x => x.Name.Equals(CeitonEventsDropdown.Selected));
			}
		}

		public RadioButtonList PlasmaOrdersRadioButtonList { get; private set; } = new RadioButtonList(new string[] { "Single", "Multiple" }, "Single");

		public DropDown PlasmaOrdersDropdown { get; private set; } = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		public DropDown PlasmaEventsDropdown { get; private set; } = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		public TextBox PlasmaMultipleOrdersTextBox { get; private set; } = new TextBox { IsMultiline = true, Height = 300, IsEnabled = false };

		public DropDown FeenixOrdersDropdown { get; private set; } = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		public DropDown FeenixEventsDropdown { get; private set; } = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		public DropDown EbuOrdersDropdown { get; private set; } = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		public DropDown EbuEventsDropdown { get; private set; } = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		public DropDown CeitonEventsDropdown { get; private set; } = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		public Button DeleteAllIntegrationsButton { get; private set; } = new Button("Delete All");

		public Button DeleteSelectedPlasmaOrderButton { get; private set; } = new Button("Delete Selected Plasma Order(s)");

		public Button DeleteSelectedPlasmaEventButton { get; private set; } = new Button("Delete Selected Plasma Event");

		public Button DeleteAllPlasmaOrdersButton { get; private set; } = new Button("Delete All Plasma Orders");

		public Button DeleteSelectedFeenixOrderButton { get; private set; } = new Button("Delete Selected Feenix Order");

		public Button DeleteSelectedFeenixEventButton { get; private set; } = new Button("Delete Selected Feenix Event");

		public Button DeleteAllFeenixOrdersButton { get; private set; } = new Button("Delete All Feenix Orders");

		public Button DeleteSelectedEbuOrderButton { get; private set; } = new Button("Delete Selected EBU Order");

		public Button DeleteSelectedEbuEventButton { get; private set; } = new Button("Delete Selected EBU Event");

		public Button DeleteAllEbuOrdersButton { get; private set; } = new Button("Delete All EBU Orders");

		public Button DeleteSelectedCeitonEventButton { get; private set; } = new Button("Delete Selected Ceiton Event");

		public Button DeleteAllCeitonEventsButton { get; private set; } = new Button("Delete All Ceiton Events");

		public Button ExitButton { get; private set; } = new Button("Exit");
	}
}