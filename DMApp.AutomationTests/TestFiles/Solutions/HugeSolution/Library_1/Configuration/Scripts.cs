namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	using System.ComponentModel;

	public enum Scripts
	{
		None,
		[Description("Live Order Form")]
		LiveOrderForm,
		[Description("Update Service")]
		UpdateService,
		[Description("Add Service")]
		AddService,
		[Description("Add Event")]
		AddEvent,
		[Description("Add Event From Template")]
		AddEventFromTemplate,
		[Description("Add Note")]
		AddNote,
		[Description("Add or Update Non-Live Order")]
		AddOrUpdateNonLiveOrder,
		[Description("Assign Ticket To Me")]
		AssignTicketToMe,
		[Description("Book Multiple Orders")]
		BookMultipleOrders,
		[Description("Cancel Multiple Orders")]
		CancelMultipleOrders,
		[Description("Confirm Orders")]
		ConfirmOrders,
		[Description("Delete Event")]
		DeleteEvent,
		[Description("Delete Non-Live Order")]
		DeleteNonLiveOrder,
		[Description("Duplicate Non-Live Order")]
		DuplicateNonLiveOrder,
		[Description("Aggregate Metrics")]
		AggregateMetrics,
		[Description("Assign Recording Resources")]
		AssignRecordingResources,
		[Description("Create Invoice Report")]
		CreateInvoiceReport,
		[Description("Debug")]
		Debug,
		[Description("Clear Expired Services")]
		ClearExpiredServices,
		[Description("Configure Contract Manager")]
		ConfigureContractManager,
		[Description("CustomerUI Launcher")]
		CustomerUiLauncher,
		[Description("Delete Integration Orders")]
		DeleteIntegrationOrders,
		[Description("Delete Unused Reservations")]
		DeleteUnusedReservations,
		[Description("Edit Order Template")]
		EditOrderTemplate,
		[Description("Execute Feenix Stop Command")]
		ExecuteFeenixStopCommand,
		[Description("Force Delete Order")]
		ForceDeleteOrder,
		[Description("Handle Event Action")]
		HandleEventAction,
		[Description("Handle Integration Update")]
		HandleIntegrationUpdate,
		[Description("Handle Non-Live Folder Deletion")]
		HandleNonLiveFolderDeletion,
		[Description("Handle Order Action")]
		HandleOrderAction,
		[Description("Handle Recurring Order Action")]
		HandleRecurringOrderAction,
		[Description("Handle Order Start Failure")]
		HandleOrderStartFailure,
		[Description("Handle Service Action")]
		HandleServiceAction,
		[Description("Handle Service Start Failure")]
		HandleServiceStartFailure,
		[Description("Local Backup")]
		LocalBackup,
		[Description("Merge Events")]
		MergeEvents,
		[Description("Migrate Resources In Bulk YLE")]
		MigrateResourcesInBulkYLE,
		[Description("Migrate Resources YLE")]
		MigrateResourcesYLE,
		[Description("Non-Live Local Backup")]
		NonLiveLocalBackup,
		[Description("Non-Live Order And Notes Cleanup")]
		NonLiveOrderAndNotesCleanup,
		[Description("Non-Live User Tasks Bulk Update")]
		NonLiveUserTasksBulkUpdate,
		[Description("Ericsson RX8200 Decoding PLS")]
		Ericsson_RX8200_Decoding,
		[Description("Ericsson RX8200 Demodulating PLS")]
		Ericsson_RX8200_Demodulating,
		[Description("Novelsat NS2000 Demodulating PLS")]
		Novelsat_NS2000_Demodulating,
		[Description("Poll Ceiton Resources")]
		PollCeitonResources,
		[Description("Release Recording Resources")]
		ReleaseRecordingResources,
		[Description("Reprocess Orders")]
		ReprocessOrders,
		[Description("Run Log Collector")]
		RunLogCollector,
		[Description("Send Non-Live Deletion Reminders")]
		SendNonLiveDeletionReminders,
		[Description("Send Order Debug Report")]
		SendOrderDebugReport,
		[Description("Set Element Custom Property")]
		SetElementCustomProperty,
		[Description("Show Ceiton Details")]
		ShowCeitonDetails,
		[Description("Show EBU Details")]
		ShowEBUDetails,
		[Description("Show Feenix Details")]
		ShowFeenixDetails,
		[Description("Show Order History")]
		ShowOrderHistory,
		[Description("Show Pebble Beach Details")]
		ShowPebbleBeachDetails,
		[Description("Show Plasma Details")]
		ShowPlasmaDetails,
		[Description("Stop Order")]
		StopOrder,
		[Description("Unassign Ticket")]
		UnassignTicket,
		[Description("Update Event")]
		UpdateEvent,
		[Description("Update Non-Live User Task")]
		UpdateNonLiveUserTask,
		[Description("Update Note")]
		UpdateNote,
		[Description("Update Order Status")]
		UpdateOrderStatus,
		[Description("Update Orders after UserTask Status Change")]
		UpdateOrdersAfterUserTaskStatusChange,
		[Description("Update Service Status")]
		UpdateServiceStatus,
		[Description("Update Ticket Status")]
		UpdateTicketStatus,
		[Description("Update Visibility Rights")]
		UpdateVisibilityRights,
		[Description("View Ticket")]
		ViewTicket,
		[Description("Trigger Pebble Beach Integration Update")]
		TriggerPebbleBeachIntegrationUpdate,
		[Description("Update Comments")]
		UpdateComments,
	}
}
