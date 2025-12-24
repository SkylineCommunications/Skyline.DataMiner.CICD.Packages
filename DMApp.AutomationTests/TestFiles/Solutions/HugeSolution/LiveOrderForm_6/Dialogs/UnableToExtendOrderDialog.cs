namespace LiveOrderForm_6.Dialogs
{
	using System.Collections.Generic;
	using System.Linq;
	using Sections;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	public class UnableToExtendOrderDialog : Dialog
	{
		private readonly IEnumerable<ExtendedServiceWithOccupiedResourcesSection> resourceUsageSections;

		private readonly Label explanationLabel = new Label("Some resources are not available for the extended time slot.\nExtension of the Services is only possible after freeing up these resources. \nBelow you can find more information about what Services are already using them.");
		private readonly Label occupiedResourcesTitleLabel = new Label("Occupied Resources") { Style = TextStyle.Bold };

		public UnableToExtendOrderDialog(Engine engine, IEnumerable<ExtendedServiceWithOccupiedResource> resourceUsages, Order order) : base(engine)
		{
			resourceUsageSections = resourceUsages.GroupBy(x => x.ExtendedService).Select(x => new ExtendedServiceWithOccupiedResourcesSection(engine, x, order));

			OkButton = new Button("OK") { Width = 150, Style = ButtonStyle.CallToAction };

			GenerateUI();
		}

		public Button OkButton { get; set; }

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(explanationLabel, new WidgetLayout(++row, 0, 3, 4));
			row += 2;

			AddWidget(occupiedResourcesTitleLabel, new WidgetLayout(++row, 0, 1, 4));

			foreach (var resourceUsageSection in resourceUsageSections)
			{
				AddSection(resourceUsageSection, new SectionLayout(++row, 0));
				row += resourceUsageSection.RowCount;
			}

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));
			AddWidget(OkButton, new WidgetLayout(++row, 0, 1, 4));

			SetColumnWidth(1, 150);
			SetColumnWidth(2, 300);
			SetColumnWidth(3, 700);
		}
	}
}