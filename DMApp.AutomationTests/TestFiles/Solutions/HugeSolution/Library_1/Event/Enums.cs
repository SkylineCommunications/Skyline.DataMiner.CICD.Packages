namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event
{
	using System.ComponentModel;

	public enum Status
	{
		/// <summary>
		/// Event which may or may not happen.
		/// </summary>
		[Description("Preliminary")]
		Preliminary = 0,

		/// <summary>
		/// Event that does not contain any orders.
		/// </summary>
		[Description("Planned")]
		Planned = 1,

		/// <summary>
		/// Event that contains at least one order.
		/// </summary>
		[Description("Confirmed")]
		Confirmed = 2,

		/// <summary>
		/// Event that contains at least one order and where start time is passed and end time is not reached.
		/// </summary>
		[Description("Ongoing")]
		Ongoing = 3,

		/// <summary>
		/// Event that contains at least one order and where end time is passed.
		/// </summary>
		[Description("Completed")]
		Completed = 4,

		/// <summary>
		/// Event that was cancelled.
		/// </summary>
		[Description("Cancelled")]
		Cancelled = 5
	}

	public enum EventSubType
	{
		[Description("Normal")]
		Normal = 0,

		[Description("Vizrem")]
		Vizrem = 1
	}
}