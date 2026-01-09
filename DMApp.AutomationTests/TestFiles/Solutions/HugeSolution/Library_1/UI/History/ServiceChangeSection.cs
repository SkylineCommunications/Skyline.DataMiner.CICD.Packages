namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.History
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

	public class ServiceChangeSection : Section
	{
		private bool isVisible = false;

		private CollapseButton collapseButton;
		private Label header;

		ClassChangeSection classChangeSection;

		Dictionary<string, List<PropertyChangeSection>> functionChangeSections = new Dictionary<string, List<PropertyChangeSection>>();

		public ServiceChangeSection(ServiceChange serviceChange)
		{
			Initialize(serviceChange);
			GenerateUi();
			UpdateWidgetVisibility();
		}

		public new bool IsVisible
		{
			get => isVisible;
			set
			{
				isVisible = value;
				UpdateWidgetVisibility();
			}
		}

		private void UpdateWidgetVisibility()
		{
			collapseButton.IsVisible = isVisible;
			header.IsVisible = isVisible;

			foreach (var widget in Widgets.Except(new Widget[] { collapseButton, header }))
			{
				widget.IsVisible = isVisible && !collapseButton.IsCollapsed;
			}
		}

		private void Initialize(ServiceChange serviceChange)
		{
			header = new Label($"Service {serviceChange.ServiceDisplayName ?? serviceChange.ServiceName}") { Style = TextStyle.Bold };

			collapseButton = new YleCollapseButton(true);
			collapseButton.Pressed += (s, a) => UpdateWidgetVisibility();

			classChangeSection = new ClassChangeSection(serviceChange, new ClassChangeSectionConfiguration());

			foreach (var functionChange in serviceChange.FunctionChanges)
			{
				var functionChangePropertySections = new List<PropertyChangeSection>();

				functionChangePropertySections.AddRange(functionChange.ProfileParameterChanges.Select(ppc => new PropertyChangeSection(ppc.ProfileParameterName, ppc.Change)));

				if (functionChange.ResourceChange != null) functionChangePropertySections.Add(new PropertyChangeSection("Resource", functionChange.ResourceChange));

				functionChangeSections.Add(functionChange.FunctionLabel, functionChangePropertySections);
			}
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(collapseButton, ++row, 0);
			AddWidget(header, row, 1, 1, 5);

			AddSection(classChangeSection, new SectionLayout(++row, 1));
			row += classChangeSection.RowCount;

			foreach (var functionChange in functionChangeSections)
			{
				AddWidget(new Label(functionChange.Key) { Style = TextStyle.Heading }, ++row, 1, 1, 5);
				foreach (var functionPropertyChange in functionChange.Value)
				{
					AddSection(functionPropertyChange, new SectionLayout(++row, 1));
					row += functionPropertyChange.RowCount;
				}
			}
		}
	}
}
