namespace LiveOrderForm_6.Sections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	public class ExtendedServiceWithOccupiedResourcesSection : Section
	{
		private readonly List<ExtendedServiceWithOccupiedResource> extendedServiceWithOccupiedResources;

		private readonly CollapseButton collapseButton = new CollapseButton(false) { CollapseText = "-", ExpandText = "+", Width = 44 };
		private readonly Label extendedServiceNameLabel;
		private readonly Label functionNameColumnHeaderLabel = new Label("FUNCTION");
		private readonly Label resourceNameColumnHeaderLabel = new Label("RESOURCE");
		private readonly Label usedByServiceColumnHeaderLabel = new Label("USED BY SERVICE");
		private readonly Label usedByOrderColumnHeaderLabel = new Label("USED BY ORDER");

		public ExtendedServiceWithOccupiedResourcesSection(Engine engine, IEnumerable<ExtendedServiceWithOccupiedResource> extendedServiceWithOccupiedResources, Order order)
		{
			if (extendedServiceWithOccupiedResources == null || !extendedServiceWithOccupiedResources.Any()) throw new InvalidOperationException("Empty List");

			this.extendedServiceWithOccupiedResources = extendedServiceWithOccupiedResources.ToList();

			collapseButton.Pressed += CollapseButton_Pressed;
			extendedServiceNameLabel = new Label(extendedServiceWithOccupiedResources.First().ExtendedService.GetShortDescription(order)) { Style = TextStyle.Bold };

			GenerateUI();
		}

		private void CollapseButton_Pressed(object sender, EventArgs e)
		{
			foreach (var widget in Widgets)
			{
				if (widget != collapseButton && widget != extendedServiceNameLabel)
				{
					widget.IsVisible = !collapseButton.IsCollapsed;
				}
			}
		}

		private void GenerateUI()
		{
			Clear();

			int row = -1;

			AddWidget(collapseButton, new WidgetLayout(++row, 0));
			AddWidget(extendedServiceNameLabel, new WidgetLayout(row, 1, 1, 3));

			AddWidget(functionNameColumnHeaderLabel, new WidgetLayout(++row, 1));
			AddWidget(resourceNameColumnHeaderLabel, new WidgetLayout(row, 2));
			AddWidget(usedByServiceColumnHeaderLabel, new WidgetLayout(row, 3));
			AddWidget(usedByOrderColumnHeaderLabel, new WidgetLayout(row, 4));

			foreach (var occupiedResource in extendedServiceWithOccupiedResources)
			{
				AddWidget(new Label(occupiedResource.Function.Name), new WidgetLayout(++row, 1));
				AddWidget(new Label(occupiedResource.Resource.Name), new WidgetLayout(row, 2));
				AddWidget(new Label($"{occupiedResource.OccupyingService.Name} ({occupiedResource.OccupyingService.StartWithPreRoll} - {occupiedResource.OccupyingService.EndWithPostRoll})"), new WidgetLayout(row, 3));
				AddWidget(new Label(occupiedResource.OccupyingOrderName), new WidgetLayout(row, 4));
			}
		}
	}
}