namespace Debug_2.Debug.Reservations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library.UI.Filters;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class GetReservationsSection : YleSection
	{
		private readonly Label header = new Label("Get Reservations with filters") { Style = TextStyle.Heading };

		private readonly FilterSection<ReservationInstance> reservationIdFilterSection = new GuidFilterSection<ReservationInstance>("Reservation ID", x => ReservationInstanceExposers.ID.Equal((Guid)x));

		private readonly FilterSection<ReservationInstance> reservationServiceDefinitionIdFilterSection = new GuidFilterSection<ReservationInstance>("Service Definition ID", x => ServiceReservationInstanceExposers.ServiceDefinitionID.Equal((Guid)x));

		private readonly FilterSection<ReservationInstance> reservationNameEqualsFilterSection = new StringFilterSection<ReservationInstance>("Reservation Name Equals", x => ReservationInstanceExposers.Name.Equal((string)x));

		private readonly FilterSection<ReservationInstance> reservationNameContainsFilterSection = new StringFilterSection<ReservationInstance>("Reservation Name Contains", x => ReservationInstanceExposers.Name.Contains((string)x));

		private readonly FilterSection<ReservationInstance> reservationStartFromFilterSection = new DateTimeFilterSection<ReservationInstance>("Reservation Start From", x => ReservationInstanceExposers.Start.GreaterThanOrEqual((DateTime)x));

		private readonly FilterSection<ReservationInstance> reservationStartUntilFilterSection = new DateTimeFilterSection<ReservationInstance>("Reservation Start Until", x => ReservationInstanceExposers.Start.LessThanOrEqual((DateTime)x));

		private readonly FilterSection<ReservationInstance> reservationEndFromFilterSection = new DateTimeFilterSection<ReservationInstance>("Reservation End From", x => ReservationInstanceExposers.End.GreaterThanOrEqual((DateTime)x));

		private readonly FilterSection<ReservationInstance> reservationEndUntilFilterSection = new DateTimeFilterSection<ReservationInstance>("Reservation End Until", x => ReservationInstanceExposers.End.LessThanOrEqual((DateTime)x));

		private readonly List<FilterSection<ReservationInstance>> resourceFilterSections = new List<FilterSection<ReservationInstance>>();
		private readonly Button addResourceFilterButton = new Button("Add Resource Filter");

		private readonly List<FilterSection<ReservationInstance>> propertyFilterSections = new List<FilterSection<ReservationInstance>>();
		private readonly Button addPropertyFilterButton = new Button("Add Property Filter");

		private readonly Button getReservationsBasedOnFiltersButton = new Button("Get Reservations Based on Filters") { Style = ButtonStyle.CallToAction };
		private List<ReservationInstance> reservationsBasedOnFilters = new List<ReservationInstance>();

		private readonly Label amountOfReservationsLabel = new Label(string.Empty);
		private readonly CheckBoxList selectReservationsCheckBoxList = new CheckBoxList();
		private readonly Button selectAllButton = new Button("Select All") { IsVisible = false };
		private readonly Button unselectAllButton = new Button("Unselect All") { IsVisible = false };

		public GetReservationsSection(Helpers helpers) : base(helpers)
		{
			addResourceFilterButton.Pressed += AddResourceFilterButton_Pressed;

			addPropertyFilterButton.Pressed += AddPropertyFilterButton_Pressed;

			getReservationsBasedOnFiltersButton.Pressed += GetReservationsBasedOnFiltersButton_Pressed;

			selectAllButton.Pressed += (o, e) => selectReservationsCheckBoxList.CheckAll();
			unselectAllButton.Pressed += (o, e) => selectReservationsCheckBoxList.UncheckAll();

			selectReservationsCheckBoxList.Changed += (o, e) => SelectedReservations = GetIndividuallySelectedReservations();

			GenerateUi();
		}

		private void GetReservationsBasedOnFiltersButton_Pressed(object sender, EventArgs e)
		{
			SelectedReservations = GetReservationsBasedOnFilters();
			selectAllButton.IsVisible = SelectedReservations.Any();
			unselectAllButton.IsVisible = SelectedReservations.Any();
			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
		}

		public IEnumerable<ReservationInstance> SelectedReservations { get; private set; } = new List<ReservationInstance>();

		public event EventHandler RegenerateUiRequired;

		public event EventHandler<IEnumerable<ReservationInstance>> ReservationsUpdated;

		public void AddDefaultPropertyFilter(string propertyName, string propertyValue)
		{
			var propertyFilterSection = new StringPropertyFilterSection<ReservationInstance>("Property", (propName, propValue) => ReservationInstanceExposers.Properties.DictStringField((string)propName).Equal((string)propValue));

			propertyFilterSection.SetDefault(propertyName, propertyValue);

			propertyFilterSections.Add(propertyFilterSection);

			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
		}

		public override void RegenerateUi()
		{
			GenerateUi();
		}

		private IEnumerable<ReservationInstance> GetReservationsBasedOnFilters()
		{
			if (!this.ActiveFiltersAreValid<ReservationInstance>()) return new List<ReservationInstance>();

			reservationsBasedOnFilters = SrmManagers.ResourceManager.GetReservationInstances(this.GetCombinedFilterElement<ReservationInstance>()).ToList();

			int previousAmountOfOptions = selectReservationsCheckBoxList.Options.Count();

			amountOfReservationsLabel.Text = $"Found {reservationsBasedOnFilters.Count} matching reservations";

			selectReservationsCheckBoxList.SetOptions(reservationsBasedOnFilters.Select(r => r.Name).OrderBy(name => name).ToList());
			selectReservationsCheckBoxList.CheckAll();

			var selectedResources = GetIndividuallySelectedReservations();

			if (selectReservationsCheckBoxList.Options.Count() != previousAmountOfOptions) RegenerateUiRequired?.Invoke(this, EventArgs.Empty);

			return selectedResources;
		}

		private IEnumerable<ReservationInstance> GetIndividuallySelectedReservations()
		{
			var selectedReservationNames = selectReservationsCheckBoxList.Checked;

			var selectedReservations = reservationsBasedOnFilters.Where(r => selectedReservationNames.Contains(r.Name)).ToList();

			return selectedReservations;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(header, ++row, 0, 1, 2);

			AddSection(reservationNameEqualsFilterSection, new SectionLayout(++row, 0));

			AddSection(reservationNameContainsFilterSection, new SectionLayout(++row, 0));

			AddSection(reservationIdFilterSection, new SectionLayout(++row, 0));

			AddSection(reservationStartFromFilterSection, new SectionLayout(++row, 0));

			AddSection(reservationStartUntilFilterSection, new SectionLayout(++row, 0));

			AddSection(reservationEndFromFilterSection, new SectionLayout(++row, 0));

			AddSection(reservationEndUntilFilterSection, new SectionLayout(++row, 0));

			AddSection(reservationServiceDefinitionIdFilterSection, new SectionLayout(++row, 0));

			foreach (var resourceFilterSection in resourceFilterSections)
			{
				AddSection(resourceFilterSection, new SectionLayout(++row, 0));
			}

			foreach (var propertyFilterSection in propertyFilterSections)
			{
				AddSection(propertyFilterSection, new SectionLayout(++row, 0));
			}

			AddWidget(addResourceFilterButton, ++row, 0);

			AddWidget(addPropertyFilterButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(getReservationsBasedOnFiltersButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(amountOfReservationsLabel, 0, 3);
			AddWidget(selectReservationsCheckBoxList, 1, 3, selectReservationsCheckBoxList.Options.Any() ? selectReservationsCheckBoxList.Options.Count() : 1, 1, verticalAlignment: VerticalAlignment.Top);
			AddWidget(selectAllButton, 1, 4, verticalAlignment: VerticalAlignment.Top);
			AddWidget(unselectAllButton, 1, 5, verticalAlignment: VerticalAlignment.Top);
		}

		private void AddPropertyFilterButton_Pressed(object sender, EventArgs e)
		{
			var propertyFilterSection = new StringPropertyFilterSection<ReservationInstance>("Property", (propertyName, propertyValue) => ReservationInstanceExposers.Properties.DictStringField((string)propertyName).Equal((string)propertyValue));

			propertyFilterSections.Add(propertyFilterSection);

			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
		}

		private void AddResourceFilterButton_Pressed(object sender, EventArgs e)
		{
			var resourceFilterSection = new GuidFilterSection<ReservationInstance>("Uses Resource ID", (resourceId) => ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains((Guid)resourceId));

			resourceFilterSections.Add(resourceFilterSection);

			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			resourceFilterSections.ForEach(f => f.IsEnabled = IsEnabled);

			propertyFilterSections.ForEach(f => f.IsEnabled = IsEnabled);
		}
	}
}
