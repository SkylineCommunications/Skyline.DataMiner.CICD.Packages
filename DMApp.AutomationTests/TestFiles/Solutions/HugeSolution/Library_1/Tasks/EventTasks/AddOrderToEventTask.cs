using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class AddOrderToEventTask : Task
	{
		private readonly Guid eventId;
		private readonly LiteOrder order;
		private readonly bool wasOrderLinkedToEvent;
		private readonly Event _event;

		public AddOrderToEventTask(Helpers helpers, Guid eventId, Guid orderId)
			: base(helpers)
		{
			this.eventId = eventId;
			this.order = base.helpers.OrderManager.GetLiteOrder(orderId);
			IsBlocking = false;

			_event = base.helpers.EventManager.GetEvent(eventId);
			wasOrderLinkedToEvent = _event.HasOrder(orderId);
		}

		public AddOrderToEventTask(Helpers helpers, Guid eventId, LiteOrder order) : base(helpers)
		{
			this.eventId = eventId;
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			IsBlocking = false;

			_event = base.helpers.EventManager.GetEvent(eventId);
			wasOrderLinkedToEvent = _event.HasOrder(order.Id);
		}

		public AddOrderToEventTask(Helpers helpers, Order order) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			IsBlocking = false;

			_event = order.Event ?? throw new ArgumentException("Order Event property is null", nameof(order));
			wasOrderLinkedToEvent = _event.HasOrder(order.Id);
		}

		public override string Description => "Adding Order to Event";

		public override Task CreateRollbackTask()
		{
			// If the order was already linked to the event, then this task did not change anything and should thus not remove the linking between the order and the event
			if (wasOrderLinkedToEvent) return null;

			return new DeleteOrderFromEventTask(helpers, eventId, order.Id);
		}

		protected override void InternalExecute()
		{
			helpers.EventManager.AddOrUpdateOrderToEvent(_event, order);
		}
	}
}