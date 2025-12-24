namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;

	public enum LiveOrderFormAction
	{
		None,
		[Description("Add new")]
		Add,
		[Description("Edit")]
		Edit,
		[Description("Delete")]
		Delete,
		[Description("Duplicate")]
		Duplicate,
		[Description("Merge")]
		Merge,
		[Description("From template")]
		FromTemplate,
		[Description("View")]
		View,
		[Description("Add Destination")]
		AddDestination,
		[Description("Add Transmission")]
		AddTransmission,
		[Description("Service Edit")]
		EditService,
		[Description("ResourceChange")]
		ResourceChange,
		[Description("Service View")]
		ViewService,
		[Description("ResourceChange_FromRecordingApp")]
		ResourceChange_FromRecordingApp,
		[Description("UpdateTiming")]
		UpdateTiming,
	}

	public enum EditOrderFlows
	{
		None,
		[Description("Add")]
		AddOrder,
		[Description("Edit")]
		EditOrder,
		[Description("Delete")]
		DeleteOrder,
		[Description("Duplicate")]
		DuplicateOrder,
		[Description("Merge")]
		MergeOrders,
		[Description("From template")]
		AddOrderFromTemplate,
		[Description("View")]
		ViewOrder,
		[Description("Add Destination")]
		AddDestinationToOrder,
		[Description("Add Transmission")]
		AddTransmissionToOrder,
		[Description("Service Edit")]
		EditService,
		[Description("ResourceChange")]
		ChangeResourcesForService,
		[Description("Service View")]
		ViewService,
		[Description("ResourceChange_FromRecordingApp")]
		ChangeResourcesForService_FromRecordingApp,
		[Description("UpdateTiming")]
		EditTimingForService_FromRecordingApp,
		[Description("DeleteService")]
		DeleteService,
		[Description("Use Shared Source")]
		UseSharedSource
	}

	public static class FlowMapper
	{
		public static readonly IReadOnlyDictionary<LiveOrderFormAction, EditOrderFlows> Mapping = new Dictionary<LiveOrderFormAction, EditOrderFlows>
		{
			{LiveOrderFormAction.None, EditOrderFlows.AddOrder },
			{LiveOrderFormAction.Add, EditOrderFlows.AddOrder },
			{LiveOrderFormAction.Edit, EditOrderFlows.EditOrder },
			{LiveOrderFormAction.View, EditOrderFlows.ViewOrder },
			{LiveOrderFormAction.Merge, EditOrderFlows.MergeOrders },
			{LiveOrderFormAction.Delete, EditOrderFlows.DeleteOrder },
			{LiveOrderFormAction.Duplicate, EditOrderFlows.DuplicateOrder},
			{LiveOrderFormAction.FromTemplate, EditOrderFlows.AddOrderFromTemplate},
			{LiveOrderFormAction.AddDestination, EditOrderFlows.AddDestinationToOrder},
			{LiveOrderFormAction.AddTransmission, EditOrderFlows.AddTransmissionToOrder},

			{LiveOrderFormAction.EditService, EditOrderFlows.EditService},
			{LiveOrderFormAction.ResourceChange, EditOrderFlows.ChangeResourcesForService},
			{LiveOrderFormAction.ViewService, EditOrderFlows.ViewService},
			{LiveOrderFormAction.ResourceChange_FromRecordingApp, EditOrderFlows.ChangeResourcesForService_FromRecordingApp},
			{LiveOrderFormAction.UpdateTiming, EditOrderFlows.EditTimingForService_FromRecordingApp},

		};
	}
}