namespace LiveOrderForm_6.Dialogs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class UseOrderTemplateDialog : Dialog
	{
		private readonly Label TemplatesTitle = new Label("Templates") { Style = TextStyle.Bold };
		private readonly CheckBox ShowEventOrderTemplatesCheckBox = new CheckBox("Show Event Order Templates") { IsChecked = false };
		private readonly RadioButtonList TemplatesRadioButtonList = new RadioButtonList();
		private readonly Label OrderDetailsTitle = new Label("Order Details") { Style = TextStyle.Bold };
		private readonly Label OrderNameLabel = new Label("Order Name");
		private readonly Label StartTimeLabel = new Label("Start Time");
		private readonly Label EndTimeLabel = new Label("End Time");
		private readonly YleTextBox OrderNameTextBox = new YleTextBox();
		private readonly DateTimePicker StartTimeDateTimePicker = new DateTimePicker();
		private readonly DateTimePicker EndTimeDateTimePicker = new DateTimePicker { IsEnabled = false };
		private readonly CollapseButton ServicesCollapseButton = new CollapseButton { Width = 44, CollapseText = "-", ExpandText = "+", IsCollapsed = true };
		private readonly Label ServicesTitle = new Label("Services") { Style = TextStyle.Bold };
		private readonly Helpers helpers;
		private readonly List<OrderTemplate> templates;
		private readonly LockInfo lockInfo;

		public UseOrderTemplateDialog(Helpers helpers, List<OrderTemplate> orderTemplates, LockInfo lockInfo) : base(helpers.Engine)
		{
			this.helpers = helpers;
			this.templates = orderTemplates ?? throw new ArgumentNullException(nameof(orderTemplates));
			this.lockInfo = lockInfo ?? throw new ArgumentNullException(nameof(lockInfo));

			Title = "Order From Template";
			StartTimeDateTimePicker.DateTime = DateTime.Now.RoundToMinutes().AddHours(1);

			ShowEventOrderTemplatesCheckBox.Changed += (s, a) =>
			{
				UpdateTemplateList();
				UpdateEndTime();
				GenerateUI();
			};

			TemplatesRadioButtonList.Changed += (s, a) =>
			{
				UpdateEndTime();
				GenerateUI();
			};

			StartTimeDateTimePicker.Changed += (s, a) =>
			{
				UpdateEndTime();
				GenerateUI();
			};

			UpdateTemplateList();
			UpdateEndTime();
			GenerateUI();
			SetEnabledStatus();
		}

		public OrderTemplate SelectedTemplate
		{
			get
			{
				if (String.IsNullOrEmpty(TemplatesRadioButtonList.Selected)) return null;
				return templates.FirstOrDefault(x => x.Name == TemplatesRadioButtonList.Selected);
			}
		}

		public string OrderName => OrderNameTextBox.Text;

		public DateTime StartTime => StartTimeDateTimePicker.DateTime;

		public bool HasValidTemplates => templates.Any();

		public Button ContinueButton { get; private set; } = new Button("Continue") { Width = 150, Style = ButtonStyle.CallToAction };

		public bool IsValid
		{
			get
			{
				bool templateSelected = SelectedTemplate != null;

				bool orderNameSpecified = !String.IsNullOrWhiteSpace(OrderName);
				if (!orderNameSpecified)
				{
					OrderNameTextBox.ValidationText = "Specify an order name";
					OrderNameTextBox.ValidationState = UIValidationState.Invalid;
					return false;
				}

				bool doesOrderNameContainIllegalCharacters = OrderName.Any(character => LiteOrder.OrderNameDisallowedCharacters.Contains(character));
				if (doesOrderNameContainIllegalCharacters)
				{
					OrderNameTextBox.ValidationText = "Characters " + String.Join(" , ", LiteOrder.OrderNameDisallowedCharacters) + " are not allowed";
					OrderNameTextBox.ValidationState = UIValidationState.Invalid;
					return false;
				}

				bool isOrderNameUnique = IsOrderNameUnique(OrderName);
				if(!isOrderNameUnique)
				{
					OrderNameTextBox.ValidationText = "This name is already in use by another order";
					OrderNameTextBox.ValidationState = UIValidationState.Invalid;
					return false;
				}

				OrderNameTextBox.ValidationState = UIValidationState.Valid;

				return templateSelected ;
			}
		}

		private int ServiceDepth
		{
			get
			{
				if (SelectedTemplate == null) return 0;
				return GetDepth(SelectedTemplate.Sources, 5);
			}
		}

		private bool IsOrderNameUnique(string name)
		{
			var order = helpers.ReservationManager.GetReservation(name);
			return order == null;
		}

		private bool IsReadonly => !lockInfo.IsLockGranted;

		private void UpdateTemplateList()
		{
			IEnumerable<string> options;
			if (ShowEventOrderTemplatesCheckBox.IsChecked)
			{
				options = templates.Select(x => x.Name);
			}
			else
			{
				options = templates.Where(x => !x.IsPartOfEventTemplate).Select(x => x.Name);
			}

			TemplatesRadioButtonList.Options = options.OrderBy(x => x);
		}

		private void UpdateEndTime()
		{
			if (SelectedTemplate == null)
			{
				EndTimeDateTimePicker.DateTime = StartTime;
			}
			else
			{
				EndTimeDateTimePicker.DateTime = StartTime.Add(SelectedTemplate.Duration);
			}
		}

		private void GenerateUI()
		{
			Clear();

			int row = -1;
			int depth = ServiceDepth;
			int valueColumn = 2 + depth;

			if (!lockInfo.IsLockGranted) AddWidget(new Label(String.Format("Unable to add an Order to this Event as it is currently locked by {0}", lockInfo.LockUsername)), ++row, 0, 1, valueColumn + 1);

			AddWidget(TemplatesTitle, ++row, 0, 1, valueColumn);
			AddWidget(ShowEventOrderTemplatesCheckBox, row, valueColumn);

			AddWidget(TemplatesRadioButtonList, ++row, 0, 1, valueColumn);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(OrderDetailsTitle, ++row, 0, 1, valueColumn + 1);

			AddWidget(OrderNameLabel, ++row, 0, 1, valueColumn);
			AddWidget(OrderNameTextBox, row, valueColumn);

			AddWidget(StartTimeLabel, ++row, 0, 1, valueColumn);
			AddWidget(StartTimeDateTimePicker, row, valueColumn);

			AddWidget(EndTimeLabel, ++row, 0, 1, valueColumn);
			AddWidget(EndTimeDateTimePicker, row, valueColumn);

			if (SelectedTemplate != null && SelectedTemplate.Sources != null && SelectedTemplate.Sources.Any())
			{
				AddWidget(ServicesCollapseButton, ++row, 0);
				AddWidget(ServicesTitle, row, 1, 1, depth + 1);

				ServicesCollapseButton.LinkedWidgets.Clear();
				ServicesCollapseButton.LinkedWidgets.AddRange(AddServiceWidgets(SelectedTemplate.Sources, 1, valueColumn, ref row));
			}

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(ContinueButton, row + 1, 0, 1, valueColumn + 1, HorizontalAlignment.Right, VerticalAlignment.Center);

			SetColumnWidth(0, 40);
			for (int i = 1; i <= depth; i++) SetColumnWidth(0, 25);
		}

		private void SetEnabledStatus()
		{
			foreach (var widget in Widgets)
			{
				if (widget is InteractiveWidget interactiveWidget && !(interactiveWidget is CollapseButton) && widget != EndTimeDateTimePicker)
				{
					interactiveWidget.IsEnabled = !IsReadonly;
				}
			}
		}

		private List<Widget> AddServiceWidgets(List<ServiceTemplate> templates, int columnIdx, int valueColumn, ref int row)
		{
			List<Widget> widgets = new List<Widget>();
			if (templates == null || !templates.Any()) return widgets;

			foreach (ServiceTemplate template in templates)
			{
				TimeSpan serviceStartOffset = SelectedTemplate.ServiceOffsets[template.Id];
				DateTime start = StartTimeDateTimePicker.DateTime.Add(serviceStartOffset);
				DateTime end = start.Add(template.Duration);

				Label serviceNameLabel = new Label(template.ServiceDefinitionName) { IsVisible = !ServicesCollapseButton.IsCollapsed };
				Label serviceTimingLabel = new Label($"{start.ToString("g")} - {end.ToString("g")}") { IsVisible = !ServicesCollapseButton.IsCollapsed };

				widgets.Add(serviceNameLabel);
				widgets.Add(serviceTimingLabel);

				AddWidget(serviceNameLabel, ++row, columnIdx, 1, valueColumn - columnIdx);
				AddWidget(serviceTimingLabel, row, valueColumn, HorizontalAlignment.Right);

				widgets.AddRange(AddServiceWidgets(template.Children, columnIdx + 1, valueColumn, ref row));
			}

			return widgets;
		}

		private static int GetDepth(IEnumerable<ServiceTemplate> templates, int currentDepth) 
		{
			if (templates == null || !templates.Any()) return currentDepth;
			int maxDepth = currentDepth;
			foreach (var template in templates) 
			{
				int newDepth = GetDepth(template.Children, currentDepth++);
				if (newDepth > maxDepth) maxDepth = newDepth;
			}

			return maxDepth;
		}
	}
}