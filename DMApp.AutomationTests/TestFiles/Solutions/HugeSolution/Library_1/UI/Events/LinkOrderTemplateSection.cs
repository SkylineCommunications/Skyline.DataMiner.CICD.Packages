namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	public class LinkOrderTemplateSection : Section
	{
		private readonly DropDown orderTemplatesDropdown = new DropDown();
		private readonly CollapseButton servicesCollapseButton = new CollapseButton { Width = 44, CollapseText = "-", ExpandText = "+", IsCollapsed = true };
		private readonly Label servicesTitle = new Label("Services") { Style = TextStyle.Heading };
		private readonly Label orderOffsetLabel = new Label("Order Offset");
		private readonly TimePicker orderOffsetTimePicker = new TimePicker() { DateTimeFormat = DateTimeFormat.LongTime, Time = TimeSpan.FromHours(0), Minimum = TimeSpan.FromHours(0), HasSpinnerButton = true, IsSpinnerButtonEnabled = true }; 

		private readonly ObservableCollection<OrderTemplate> availableOrderTemplates;

		public LinkOrderTemplateSection(ObservableCollection<OrderTemplate> availableOrderTemplates)
		{
			this.availableOrderTemplates = availableOrderTemplates;

			orderTemplatesDropdown.Options = this.availableOrderTemplates.Select(x => x.Name).OrderBy(x => x).ToList();
			SelectedTemplate = this.availableOrderTemplates.FirstOrDefault(x => x.Name.Equals(orderTemplatesDropdown.Selected));
			this.availableOrderTemplates.Remove(SelectedTemplate);

			orderTemplatesDropdown.Changed += (s, a) => OtherTemplateSelected();
			availableOrderTemplates.CollectionChanged += AvailableOrderTemplates_CollectionChanged;
			orderOffsetTimePicker.Changed += (s, a) => { RequiresUiUpdate?.Invoke(this, new EventArgs()); };
		}

		internal void GenerateUI(int depth, int valueColumn)
		{
			Clear();

			int row = -1;

			AddWidget(orderTemplatesDropdown, ++row, 0, 1, valueColumn + 1);
			
			AddWidget(orderOffsetLabel, ++row, 0, 1, valueColumn);
			AddWidget(orderOffsetTimePicker, row, valueColumn);

			if (SelectedTemplate != null && SelectedTemplate.Sources != null && SelectedTemplate.Sources.Any())
			{
				AddWidget(servicesCollapseButton, ++row, 0);
				AddWidget(servicesTitle, row, 1, 1, depth + 1);

				servicesCollapseButton.LinkedWidgets.Clear();
				servicesCollapseButton.LinkedWidgets.AddRange(AddServiceWidgets(SelectedTemplate.Sources, 1, valueColumn, ref row));
			}

			AddWidget(DeleteLinkedOrderTemplateButton, row + 1, 0, 1, valueColumn);
		}

		private List<Widget> AddServiceWidgets(List<ServiceTemplate> templates, int columnIdx, int valueColumn, ref int row)
		{
			List<Widget> widgets = new List<Widget>();
			if (templates == null || !templates.Any()) return widgets;

			foreach (ServiceTemplate template in templates)
			{
				TimeSpan serviceStartOffset = SelectedTemplate.ServiceOffsets[template.Id].Add(Offset);

				Label serviceNameLabel = new Label(template.ServiceDefinitionName) { IsVisible = !servicesCollapseButton.IsCollapsed };
				Label serviceTimingLabel = new Label($"Offset: {serviceStartOffset.ToString("g")} - Duration: {template.Duration.ToString("g")}") { IsVisible = !servicesCollapseButton.IsCollapsed };

				widgets.Add(serviceNameLabel);
				widgets.Add(serviceTimingLabel);

				AddWidget(serviceNameLabel, ++row, columnIdx, 1, valueColumn - columnIdx);
				AddWidget(serviceTimingLabel, row, valueColumn, HorizontalAlignment.Right);

				widgets.AddRange(AddServiceWidgets(template.Children, columnIdx + 1, valueColumn, ref row));
			}

			return widgets;
		}

		private void AvailableOrderTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			// Update possible options
			HashSet<string> options = new HashSet<string>(this.availableOrderTemplates.Select(x => x.Name));
			if (SelectedTemplate != null) options.Add(SelectedTemplate.Name);

			orderTemplatesDropdown.Options = options.OrderBy(x => x);
		}

		private void OtherTemplateSelected()
		{
			availableOrderTemplates.Add(SelectedTemplate);
			SelectedTemplate = availableOrderTemplates.FirstOrDefault(x => x.Name.Equals(orderTemplatesDropdown.Selected));
			availableOrderTemplates.Remove(SelectedTemplate);
			RequiresUiUpdate?.Invoke(this, new EventArgs());
		}

		public OrderTemplate SelectedTemplate { get; private set; }

		public TimeSpan Offset
		{
			get
			{
				return new TimeSpan(orderOffsetTimePicker.Time.Hours, orderOffsetTimePicker.Time.Minutes, 0);
			}
		}

		public Button DeleteLinkedOrderTemplateButton { get; private set; } = new Button("Delete") { Width = 75 };

		internal int ServiceDepth
		{
			get
			{
				if (SelectedTemplate == null) return 0;
				return GetDepth(SelectedTemplate.Sources, 1);
			}
		}

		internal event EventHandler RequiresUiUpdate;

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
