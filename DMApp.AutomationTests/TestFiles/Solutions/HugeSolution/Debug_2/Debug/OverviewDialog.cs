namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug
{
	using System;
	using Debug_2.Debug.NonLive;
	using Debug_2.Debug.NonLiveUserTasks;
	using Debug_2.Debug.Orders;
	using Debug_2.Debug.Reservations;
	using Debug_2.Debug.Resources;
	using Debug_2.Debug.ServiceDefinitions;
	using Debug_2.Debug.Tickets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Debug.VIZREM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.BookingManagers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Debug.Services;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Functions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Jobs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.LogCollector;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Orders;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.ProfileParameters;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Reservations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.ServiceConfigurations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Templates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Tickets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class OverviewDialog : Dialog
	{
		private readonly Helpers helpers;

		public TextBox CurrentIdTextBox { get; private set; } = new TextBox();

		private readonly Label localTimeZoneLabel = new Label(TimeZoneInfo.Local.DisplayName);
		private readonly Label reservationsLabel = new Label("Reservations") { Style = TextStyle.Heading };
		private readonly Label ordersLabel = new Label("Orders") { Style = TextStyle.Heading };
		private readonly Label servicesLabel = new Label("Services") { Style = TextStyle.Heading };
		private readonly Label nonLiveUserTasksLabel = new Label("Non-Live Order User Tasks") { Style = TextStyle.Heading };
		private readonly Label nonLiveOrdersLabel = new Label("Non-Live Orders") { Style = TextStyle.Heading };
		private readonly Label serviceConfigurationsLabel = new Label("Service Configurations") { Style = TextStyle.Heading };
		private readonly Label integrationsLabel = new Label("Integrations") { Style = TextStyle.Heading };
		private readonly Label resourcesLabel = new Label("Resources") { Style = TextStyle.Heading };
		private readonly Label ticketsLabel = new Label("Tickets") { Style = TextStyle.Heading };
		private readonly Label loggingLabel = new Label("Logging") { Style = TextStyle.Heading };
		private readonly Label templatesLabel = new Label("Templates") { Style = TextStyle.Heading };
		private readonly Label functionsLabel = new Label("Functions") { Style = TextStyle.Heading };
		private readonly Label serviceDefinitionsLabel = new Label("Service Definitions") { Style = TextStyle.Heading };
		private readonly Label bookingManagersLabel = new Label("Booking Managers") { Style = TextStyle.Heading };
		private readonly Label eventsLabel = new Label("Events") { Style = TextStyle.Heading };

		private readonly Button printLoggingButton = new Button("Print Logging") { Width = 300 };

		public OverviewDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;

			Title = "Overview";

			CurrentIdTextBox.Text = Engine.GetScriptParam(1).Value;

			FindReservationsDialog = new FindReservationsDialog(helpers);
			FindReservationLoggingDialog = new FindReservationLoggingDialog(helpers);
			EditReservationPropertiesDialog = new EditReservationPropertiesDialog(helpers);
			ServiceConfigurationsDialog = new GetServiceConfigurationsDialog(helpers);
			RetriggerIntegrationUpdatesDialog = new RetriggerIntegrationUpdatesDialog(helpers);
			ResourceOccupancyDialog = new ResourceOccupancyDialog(helpers);
			FindReservationsWithoutJobDialog = new FindReservationsWithoutJobDialog(helpers);
			MetricsDialog = new MetricsDialog(helpers);
			SendIntegrationNotificationDialog = new SendIntegrationNotificationDialog(helpers);
			UpdateOrderUiPropertiesDialog = new UpdateOrderUiPropertiesDialog(helpers);
			LogCollectorDialog = new LogCollectorDialog(helpers);
			GetEligibleResourcesDialog = new GetEligibleResourcesDialog(helpers);
			MoveToQuarantinedStateDialog = new MoveToQuarantinedStateDialog(helpers);
			FindQuarantinedOrdersDialog = new FindQuarantinedOrdersDialog(helpers);
			FindProfileParameterDialog = new FindProfileParameterDialog(helpers);
			FindTicketsDialog = new FindTicketsDialog(helpers);
			StopOrderNowDialog = new StopOrderNowDialog(helpers);
			OrderHistoryDialog = new OrderHistoryDialog(helpers);
			AddOrUpdateServiceConfigurationsDialog = new AddOrUpdateServiceConfigurationsDialog(helpers);
			FixServiceConfigurationsDialog = new FixServiceConfigurationsDialog(helpers);
			DeleteOrdersDialog = new DeleteOrdersDialog(helpers);
			FixResourceConfigurationDialog = new FixResourceConfigurationDialog(helpers);
			DeleteTemplatesDialog = new DeleteTemplatesDialog(helpers);
			EditOrderTemplatesDialog = new EditOrderTemplatesDialog(helpers);
			EditEventTemplatesDialog = new EditEventTemplatesDialog(helpers);
			AnalyzePlasmaRecordingsDialog = new AnalyzePlasmaRecordingsDialog(helpers);
			VizremDialog = new VizremDialog(helpers);
			ActiveFunctionsDialog = new ActiveFunctionsDialog(helpers);
			GetServiceDefinitionDialog = new GetServiceDefinitionDialog(helpers);
			UpdateServiceDefinitionsDialog = new UpdateServiceDefinitionsDialog(helpers);
			FindResourcesWithFiltersDialog = new FindResourcesWithFiltersDialog(helpers);
			FindReservationsWithFiltersDialog = new FindReservationsWithFiltersDialog(helpers);
			RemoveDuplicatePropertiesDialog = new RemoveDuplicatePropertiesDialog(helpers);
			FindTicketsWithFiltersDialog = new FindTicketsWithFiltersDialog(helpers);
			UpdateNonLiveOrderUserTasksDialog = new UpdateNonLiveOrderUserTasksDialog(helpers);
			TestDataMinerInterfaceDialog = new TestDataMinerInterfaceDialog(helpers);
			FixMissingServiceDefinitionsDialog = new FixMissingServiceDefinitionsDialog(helpers);
			AddOrUpdateResourcesDialog = new AddOrUpdateResourcesDialog(helpers);
			ReassignJobDialog = new ReassignJobDialog(helpers);
			ManageSectionDefinitionsDialog = new ManageSectionDefinitionsDialog(helpers);
			ProfileLoadLoggingDialog = new ProfileLoadLoggingDialog(helpers);
			AutoUpdateOrderPropertiesDialog = new AutoUpdateOrderPropertiesDialog(helpers);
			NonLiveOrdersDialog = new NonLiveOrdersDialog(helpers);

			printLoggingButton.Pressed += (sender, args) => helpers.Dispose();

			GenerateUi();
		}

		public Button DownloadLoggingButton { get; private set; } = new Button("Download Logging") { Width = 150 };

		public Button FindReservationsButton { get; } = new Button("Find Reservations...") { Width = 300 };

		public FindReservationsDialog FindReservationsDialog { get; }

		public Button FindReservationOrderLoggingButton { get; } = new Button("Find Reservation Order Logging...") { Width = 300 };

		public FindReservationLoggingDialog FindReservationLoggingDialog { get; }

		public Button EditReservationPropertiesButton { get; } = new Button("Edit Reservation Properties...") { Width = 300 };

		public EditReservationPropertiesDialog EditReservationPropertiesDialog { get; }

		public Button GetServiceConfigurationsButton { get; } = new Button("Get Service Configurations...") { Width = 300 };

		public GetServiceConfigurationsDialog ServiceConfigurationsDialog { get; }

		public Button RetriggerIntegrationUpdatesButton { get; } = new Button("Retrigger Integrations...") { Width = 300 };

		public RetriggerIntegrationUpdatesDialog RetriggerIntegrationUpdatesDialog { get; }

		public Button ResourceOccupancyButton { get; } = new Button("Resource Occupancy...") { Width = 300 };

		public ResourceOccupancyDialog ResourceOccupancyDialog { get; }

		public Button ReservationsWithoutJobsButton { get; } = new Button("Reservations Without Events...") { Width = 300 };

		public MoveToQuarantinedStateDialog MoveToQuarantinedStateDialog { get; }

		public Button MoveToQuarantinedStateButton { get; } = new Button("Move to Quarantined State...") { Width = 300 };

		public FindQuarantinedOrdersDialog FindQuarantinedOrdersDialog { get; }

		public Button FindQuarantinedOrdersButton { get; } = new Button("Find Quarantined Orders...") { Width = 300 };

		public FindReservationsWithoutJobDialog FindReservationsWithoutJobDialog { get; }

		public Button MetricsButton { get; } = new Button("Metrics...") { Width = 300 };

		public MetricsDialog MetricsDialog { get; }

		public SendIntegrationNotificationDialog SendIntegrationNotificationDialog { get; }

		public Button SendIntegrationNotificationButton { get; } = new Button("Send Integration Notification...") { Width = 300 };

		public Button UpdateOrderUiPropertiesButton { get; } = new Button("Update Order UI Properties...") { Width = 300 };

		public UpdateOrderUiPropertiesDialog UpdateOrderUiPropertiesDialog { get; }

		public Button LogCollectorButton { get; } = new Button("Log Collector...") { Width = 300 };

		public LogCollectorDialog LogCollectorDialog { get; }

		public Button EligibleResourcesButton { get; } = new Button("Eligible Resources...") { Width = 300 };

		public GetEligibleResourcesDialog GetEligibleResourcesDialog { get; }

		public Button FindProfileParameterButton { get; } = new Button("Find Profile Parameter...") { Width = 300 };

		public FindProfileParameterDialog FindProfileParameterDialog { get; }

		public FindTicketsDialog FindTicketsDialog { get; }

		public Button FindTicketsButton { get; } = new Button("Find Tickets...") { Width = 300 };

		public OrderHistoryDialog OrderHistoryDialog { get; }

		public Button OrderHistoryButton { get; } = new Button("Order History...") { Width = 300 };

		public StopOrderNowDialog StopOrderNowDialog { get; }

		public Button StopOrderNowButton { get; } = new Button("Stop Order Now...") { Width = 300 };

		public AddOrUpdateServiceConfigurationsDialog AddOrUpdateServiceConfigurationsDialog { get; }

		public Button AddOrUpdateServiceConfigurationsButton { get; } = new Button("Add or Update Service Configurations...") { Width = 300 };

		public FixServiceConfigurationsDialog FixServiceConfigurationsDialog { get; }

		public Button FixServiceConfigurationsButton = new Button("Fix Service Configurations...") { Width = 300 };

		public DeleteOrdersDialog DeleteOrdersDialog { get; }

		public Button DeleteOrdersButton { get; } = new Button("Delete Orders...") { Width = 300 };

		public FixResourceConfigurationDialog FixResourceConfigurationDialog { get; }

		public Button FixResourceConfigurationButton { get; } = new Button("Fix Resource Configuration...") { Width = 300 };

		public DeleteTemplatesDialog DeleteTemplatesDialog { get; }

		public Button DeleteTemplatesButton { get; } = new Button("Delete Templates...") { Width = 300 };

		public EditOrderTemplatesDialog EditOrderTemplatesDialog { get; }

		public Button EditOrderTemplateButton { get; } = new Button("Edit Order Templates...") { Width = 300 };

		public EditEventTemplatesDialog EditEventTemplatesDialog { get; }

		public Button EditEventTemplateButton { get; } = new Button("Edit Event Templates...") { Width = 300 };

		public AnalyzePlasmaRecordingsDialog AnalyzePlasmaRecordingsDialog { get; }

		public Button AnalyzePlasmaRecordingsButton { get; } = new Button("Analyze Plasma Recordings...") { Width = 300 };

		public Button GenerateFieldsFileButton { get; } = new Button("Generate Tooltip file") { Width = 300 };

		public VizremDialog VizremDialog { get; }

		public Button VizremButton { get; } = new Button("VIZREM...") { Width = 300, IsVisible = false };

		public ActiveFunctionsDialog ActiveFunctionsDialog { get; }

		public Button ActiveFunctionsButton { get; } = new Button("View Active Functions...") { Width = 300 };

		public GetServiceDefinitionDialog GetServiceDefinitionDialog { get; }

		public Button GetServiceDefinitionButton { get; } = new Button("Get Service Definitions...") { Width = 300 };

		public UpdateServiceDefinitionsDialog UpdateServiceDefinitionsDialog { get; }

		public Button UpdateServiceDefinitionsButton { get; } = new Button("Update Service Definition...") { Width = 300 };

		public FindResourcesWithFiltersDialog FindResourcesWithFiltersDialog { get; }

		public Button FindResourcesWithFiltersButton { get; } = new Button("Find Resources With Filters...") { Width = 300 };

		public FindReservationsWithFiltersDialog FindReservationsWithFiltersDialog { get; }

		public Button FindReservationsWithFiltersButton { get; } = new Button("Find Reservations with Filters...") { Width = 300 };

		public RemoveDuplicatePropertiesDialog RemoveDuplicatePropertiesDialog { get; }

		public Button RemoveDuplicatePropertiesButton { get; } = new Button("Remove Duplicate Booking Manager Properties...") { Width = 300 };

		public FindTicketsWithFiltersDialog FindTicketsWithFiltersDialog { get; }

		public Button FindTicketsWithFiltersButton { get; } = new Button("Find Tickets With Filters...") { Width = 300 };

		public UpdateNonLiveOrderUserTasksDialog UpdateNonLiveOrderUserTasksDialog { get; set; }

		public Button UpdateNonLiveOrderUserTasksButton { get; } = new Button("Update Non-Live Order User Tasks...") { Width = 300 };

		public TestDataMinerInterfaceDialog TestDataMinerInterfaceDialog { get; }

		public Button TestDataMinerInterfaceButton { get; } = new Button("Test DataMiner Interface...") { Width = 300 };

		public FixMissingServiceDefinitionsDialog FixMissingServiceDefinitionsDialog { get; }

		public Button FixMissingServiceDefinitionsButton { get; } = new Button("Fix Missing Service Definitions...") { Width = 300 };

		public AddOrUpdateResourcesDialog AddOrUpdateResourcesDialog { get; }

		public Button AddOrUpdateResourcesButton { get; } = new Button("Add or Update Resources...") { Width = 300 };

		public ReassignJobDialog ReassignJobDialog { get; }

		public Button ReassignJobButton { get; } = new Button("Reassign Job...") { Width = 300 };

		public ManageSectionDefinitionsDialog ManageSectionDefinitionsDialog { get; }

		public Button ManageSectionDefinitionsButton { get; } = new Button("Manage Section Definitions...") { Width = 300 };

		public ProfileLoadLoggingDialog ProfileLoadLoggingDialog { get; set; }

		public Button ProfileLoadLoggingButton { get; } = new Button("Profile Load Logging...") { Width = 300 };

		public AutoUpdateOrderPropertiesDialog AutoUpdateOrderPropertiesDialog { get; set; }

		public Button AutoUpdateOrderPropertiesButton { get; } = new Button("Automatically Update Order Properties...") { Width = 300 };

		public NonLiveOrdersDialog NonLiveOrdersDialog { get; set; }

		public Button NonLiveOrdersButton { get; } = new Button("Non-Live Orders...") { Width = 300 };

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(localTimeZoneLabel, ++row, 0, 1, 5);
			AddWidget(CurrentIdTextBox, ++row, 0, 1, 5);
			AddWidget(DownloadLoggingButton, row, 5);

			AddWidget(reservationsLabel, ++row, 0);
			AddWidget(FindReservationsButton, ++row, 0);
			AddWidget(FindReservationsWithFiltersButton, ++row, 0);
			AddWidget(TestDataMinerInterfaceButton, ++row, 0);

			AddWidget(ordersLabel, ++row, 0, 1, 5);
			AddWidget(UpdateOrderUiPropertiesButton, ++row, 0);
			AddWidget(AutoUpdateOrderPropertiesButton, ++row, 0);
			AddWidget(MoveToQuarantinedStateButton, ++row, 0);
			AddWidget(FindQuarantinedOrdersButton, ++row, 0);
			AddWidget(FindProfileParameterButton, ++row, 0);
			AddWidget(EditReservationPropertiesButton, ++row, 0);
			AddWidget(OrderHistoryButton, ++row, 0);
			AddWidget(StopOrderNowButton, ++row, 0);
			AddWidget(DeleteOrdersButton, ++row, 0);

			AddWidget(servicesLabel, ++row, 0, 1, 5);
			AddWidget(ProfileLoadLoggingButton, ++row, 0);

			AddWidget(eventsLabel, ++row, 0, 1, 5);
			AddWidget(ReservationsWithoutJobsButton, ++row, 0);
			AddWidget(ReassignJobButton, ++row, 0);
			AddWidget(ManageSectionDefinitionsButton, ++row, 0);

			AddWidget(serviceDefinitionsLabel, ++row, 0, 1, 5);
			AddWidget(GetServiceDefinitionButton, ++row, 0);
			AddWidget(UpdateServiceDefinitionsButton, ++row, 0);
			AddWidget(FixMissingServiceDefinitionsButton, ++row, 0);

			AddWidget(functionsLabel, ++row, 0);
			AddWidget(ActiveFunctionsButton, ++row, 0);

			AddWidget(serviceConfigurationsLabel, ++row, 0, 1, 5);
			AddWidget(GetServiceConfigurationsButton, ++row, 0);
			AddWidget(AddOrUpdateServiceConfigurationsButton, ++row, 0);
			AddWidget(FixServiceConfigurationsButton, ++row, 0);

			AddWidget(integrationsLabel, ++row, 0, 1, 5);
			AddWidget(RetriggerIntegrationUpdatesButton, ++row, 0);
			AddWidget(SendIntegrationNotificationButton, ++row, 0);
			AddWidget(AnalyzePlasmaRecordingsButton, ++row, 0);
			AddWidget(VizremButton, ++row, 0);

			AddWidget(resourcesLabel, ++row, 0, 1, 5);
			AddWidget(FindResourcesWithFiltersButton, ++row, 0);
			AddWidget(EligibleResourcesButton, ++row, 0);
			AddWidget(ResourceOccupancyButton, ++row, 0);
			AddWidget(FixResourceConfigurationButton, ++row, 0);
			AddWidget(AddOrUpdateResourcesButton, ++row, 0);

			AddWidget(ticketsLabel, ++row, 0, 1, 5);
			AddWidget(FindTicketsButton, ++row, 0);
			AddWidget(FindTicketsWithFiltersButton, ++row, 0);

			AddWidget(nonLiveOrdersLabel, ++row, 0, 1, 5);
			AddWidget(NonLiveOrdersButton, ++row, 0);

			AddWidget(nonLiveUserTasksLabel, ++row, 0, 1, 5);
			AddWidget(UpdateNonLiveOrderUserTasksButton, ++row, 0);

			AddWidget(templatesLabel, ++row, 0, 1, 5);
			AddWidget(EditOrderTemplateButton, ++row, 0);
			AddWidget(EditEventTemplateButton, ++row, 0);
			AddWidget(DeleteTemplatesButton, ++row, 0);

			AddWidget(loggingLabel, ++row, 0, 1, 5);
			AddWidget(FindReservationOrderLoggingButton, ++row, 0);
			AddWidget(MetricsButton, ++row, 0);
			AddWidget(LogCollectorButton, ++row, 0);

			AddWidget(bookingManagersLabel, ++row, 0, 1, 5);
			AddWidget(RemoveDuplicatePropertiesButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(GenerateFieldsFileButton, ++row, 0);

			AddWidget(printLoggingButton, ++row, 0);
		}
	}
}
