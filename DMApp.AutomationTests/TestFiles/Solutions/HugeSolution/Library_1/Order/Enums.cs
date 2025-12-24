using System;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
	using System.ComponentModel;

	public enum BackupType
	{
		[Description("None")]
		None = 0,

		[Description("Cold Backup")]
		Cold = 1,

		[Description("Standby Backup")]
		StandBy = 2,

		[Description("Active Backup")]
		Active = 3
	}

	public enum OrderType
	{
		[Description("Video")]
		Video = 0,

		Radio = 1
	}

	public enum OrderSubType
	{
		[Description("Normal")]
		Normal = 0,

		[Description("Vizrem")]
		Vizrem = 1
	}

	public enum OrderAction
	{
		Book = 0,

		Save = 1
	}

	public enum Status
	{
		/// <summary>
		/// Order which may or may not happen.
		/// Could be that only generic order details are filled in.
		/// Mandatory fields in service configurations are at this point not mandatory yet.
		/// </summary>
		[Description("Preliminary")]
		Preliminary = 0,

		/// <summary>
		/// Preliminary order is completed.
		/// This means the order has at least one source and one destination.
		/// </summary>
		[Description("Planned")]
		Planned = 1,

		/// <summary>
		/// Order which is rejected by an operator.
		/// This will be the case if the order is not possible to be produced or is wrongly configured.
		/// </summary>
		[Description("Rejected")]
		Rejected = 2,

		/// <summary>
		/// Order which is confirmed by operator manually or is automatically generated with proper information.
		/// </summary>
		[Description("Confirmed")]
		Confirmed = 3,

		/// <summary>
		/// Order which was previously confirmed by operator but was changed after that by customer.
		/// Order which was created automatically by integrations and set as confirmed and was changed withing 24h of the start of the order.
		/// </summary>
		[Description("Change Requested")]
		ChangeRequested = 4,

		/// <summary>
		/// Order where at least one service is in running state.
		/// Only routing and recording services can be added at this point.
		/// </summary>
		[Description("Running")]
		Running = 5,

		/// <summary>
		/// Order that contains running recording services and all live services are completed.
		/// </summary>
		[Description("File Processing")]
		FileProcessing = 6,

		/// <summary>
		/// All services are in post roll or successfully completed in the order.
		/// </summary>
		[Description("Completed")]
		Completed = 7,

		/// <summary>
		/// One or more services are completed with errors in the order.
		/// </summary>
		[Description("Completed With Errors")]
		CompletedWithErrors = 8,

		/// <summary>
		/// Order was cancelled.
		/// </summary>
		[Description("Cancelled")]
		Cancelled = 9,

		/// <summary>
		/// EBU Order was manually booked and a request for one or more services has been sent to EBU.
		/// When an order has this state it is saved until booked by the HandleIntegrationUpdate script with the synopsis information received from EBU.
		/// </summary>
		[Description("Waiting for EBU")]
		WaitingOnEbu = 10,

        /// <summary>
		/// When an order has this state it is saved until the mcr operator fills in the missing details and confirms. Then the order will be booked if the timespan is within 1 week.
		/// </summary>
		[Description("Planned Unknown Source")]
        PlannedUnknownSource = 11
	}

	[Flags]
	public enum ResourceAndProfileParameterUpdateOptions
	{
		None = 0,
		Resources = 1,
		ProfileParameters = 2,
        SkipResourceIsAvailableCheck = 4,
	}
}