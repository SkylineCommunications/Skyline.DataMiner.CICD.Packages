namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence
{
	using Newtonsoft.Json;

	public enum Action
	{
		Add,
		Delete,
		Update
	}

	public class ExternalRecurringOrderRequest
	{
		/// <summary>
		/// The action to be executed by the order manager.
		/// </summary>
		public Action Action { get; set; }

		/// <summary>
		/// The info about the recurring order.
		/// </summary>
		public RecurringSequenceInfo RecurringSequenceInfo { get; set; }

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}
	}
}
