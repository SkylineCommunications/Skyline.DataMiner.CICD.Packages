namespace Debug_2.Debug.Resources
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library.UI.Filters;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class GetResourcesSection : Section
	{
		private readonly Label header = new Label("Get Resources with filters") { Style = TextStyle.Heading };

		private readonly FilterSection<FunctionResource> functionGuidFilterSection = new GuidFilterSection<FunctionResource>("Function Guid", x => FunctionResourceExposers.FunctionGUID.Equal((Guid)x).CAST<Resource, FunctionResource>());

		private readonly FilterSection<FunctionResource> resourcePoolFilterSection = new ResourcePoolFilterSection("Resource Pool", x => ResourceExposers.PoolGUIDs.Contains((Guid)x).CAST<Resource, FunctionResource>(), SrmManagers.ResourceManager.GetResourcePools());

		private readonly FilterSection<FunctionResource> resourceNameEqualsFilterSection = new StringFilterSection<FunctionResource>("Resource Name Equals", x => ResourceExposers.Name.Equal((string)x).CAST<Resource, FunctionResource>());

		private readonly FilterSection<FunctionResource> resourceNameContainsFilterSection = new StringFilterSection<FunctionResource>("Resource Name Contains", x => ResourceExposers.Name.Contains((string)x).CAST<Resource, FunctionResource>());

		private readonly FilterSection<FunctionResource> resourceIdFilterSection = new GuidFilterSection<FunctionResource>("Resource ID", x => ResourceExposers.ID.Equal((Guid)x).CAST<Resource, FunctionResource>());

		private readonly FilterSection<FunctionResource> dmaIdFilterSection = new IntegerFilterSection<FunctionResource>("DMA ID", x => ResourceExposers.DmaID.Equal(Convert.ToInt32(x)).CAST<Resource, FunctionResource>());

		private readonly FilterSection<FunctionResource> elementIdFilterSection = new IntegerFilterSection<FunctionResource>("Element ID", x => ResourceExposers.ElementID.Equal(Convert.ToInt32(x)).CAST<Resource, FunctionResource>());

		private readonly Button getResourcesBasedOnFiltersButton = new Button("Get Resources Based on Filters") { Style = ButtonStyle.CallToAction };
		private List<FunctionResource> resourcesBasedOnFilters = new List<FunctionResource>();

		private readonly CollapseButton showIndividualResourceSelectionCollapseButton;
		private CheckBoxList selectResourcesCheckBoxList = new CheckBoxList();
		private readonly Button selectAllButton = new Button("Select All");
		private readonly Button unselectAllButton = new Button("Unselect All");
		private readonly Button selectResourcesButton = new Button("Get Individually Selected Resources") { Style = ButtonStyle.CallToAction };

		private readonly CollapseButton showSelectedResourcesButton; 
		private readonly TextBox selectedResourcesTextBox = new TextBox() { IsMultiline = true };

		public GetResourcesSection()
		{
			getResourcesBasedOnFiltersButton.Pressed += (o, e) => SelectedResources = GetResourcesBasedOnFilters();

			showIndividualResourceSelectionCollapseButton = new CollapseButton(new Widget[] {selectResourcesCheckBoxList, selectResourcesButton, selectAllButton, unselectAllButton}, true) { CollapseText = "Hide Individual Resource Selection", ExpandText = "Show Individual Resource Selection" };

			selectAllButton.Pressed += (o, e) => selectResourcesCheckBoxList.CheckAll();
			unselectAllButton.Pressed += (o, e) => selectResourcesCheckBoxList.UncheckAll();

			selectResourcesButton.Pressed += (o, e) => SelectedResources = GetIndividuallySelectedResources();

			showSelectedResourcesButton = new CollapseButton(selectedResourcesTextBox.Yield(), true) { CollapseText = "Hide Selected Resources", ExpandText = "Show Selected Resources"};

			GenerateUi();
		}

		public IEnumerable<FunctionResource> SelectedResources { get; private set; } = new List<FunctionResource>();

		public event EventHandler RegenerateUi;

		private IEnumerable<FunctionResource> GetResourcesBasedOnFilters()
		{
			selectedResourcesTextBox.Text = String.Empty;
			if (!this.ActiveFiltersAreValid<FunctionResource>()) return new List<FunctionResource>();

			resourcesBasedOnFilters = SrmManagers.ResourceManager.GetResources(this.GetCombinedFilterElement<FunctionResource>().CAST<FunctionResource, Resource>()).Cast<FunctionResource>().ToList();

			int previousAmountOfOptions = selectResourcesCheckBoxList.Options.Count();

			selectResourcesCheckBoxList.SetOptions(resourcesBasedOnFilters.Select(r => r.Name).OrderBy(name => name).ToList());
			selectResourcesCheckBoxList.CheckAll();

			var selectedResources = GetIndividuallySelectedResources();

			selectedResourcesTextBox.Text = String.Join("\n", selectedResources.Select(r => r.Name).OrderBy(name => name));

			if (selectResourcesCheckBoxList.Options.Count() != previousAmountOfOptions) RegenerateUi?.Invoke(this, EventArgs.Empty);

			return selectedResources;
		}

		private IEnumerable<FunctionResource> GetIndividuallySelectedResources()
		{
			var selectedResourceNames = selectResourcesCheckBoxList.Checked;

			var selectedResources = resourcesBasedOnFilters.Where(r => selectedResourceNames.Contains(r.Name)).ToList();

			selectedResourcesTextBox.Text = String.Join("\n", selectedResources.Select(r => r.Name).OrderBy(name => name));

			return selectedResources;
		}

		public void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(header, ++row, 0, 1, 2);

			AddSection(functionGuidFilterSection, new SectionLayout(++row, 0));

			AddSection(resourcePoolFilterSection, new SectionLayout(++row, 0));

			AddSection(resourceNameEqualsFilterSection, new SectionLayout(++row, 0));

			AddSection(resourceNameContainsFilterSection, new SectionLayout(++row, 0));

			AddSection(resourceIdFilterSection, new SectionLayout(++row, 0));

			AddSection(dmaIdFilterSection, new SectionLayout(++row, 0));

			AddSection(elementIdFilterSection, new SectionLayout(++row, 0));

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(getResourcesBasedOnFiltersButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(showIndividualResourceSelectionCollapseButton, ++row, 0);
			AddWidget(selectResourcesCheckBoxList, 0, 2, selectResourcesCheckBoxList.Options.Count() != 0 ? selectResourcesCheckBoxList.Options.Count() : 1, 1, verticalAlignment: VerticalAlignment.Top);
			AddWidget(selectAllButton, 0, 3, verticalAlignment: VerticalAlignment.Top);
			AddWidget(unselectAllButton, 0, 4, verticalAlignment: VerticalAlignment.Top);
			AddWidget(selectResourcesButton, ++row, 0, verticalAlignment: VerticalAlignment.Top);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(showSelectedResourcesButton, ++row, 0, verticalAlignment:VerticalAlignment.Top);
			AddWidget(selectedResourcesTextBox, row, 1);
		}
	}
}
