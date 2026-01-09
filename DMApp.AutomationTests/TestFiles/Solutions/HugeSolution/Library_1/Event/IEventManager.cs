namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	public interface IEventManager
	{
		Event GetEvent(Guid id);

		Event GetEvent(string projectNumber);

		Event GetEventByName(string name);

		bool AddOrUpdateEvent(Event eventToAddOrUpdate, Event existingEvent = null);

		bool AddOrUpdateOrderToEvent(Guid eventId, Guid orderId, bool orderEventReferenceUpdateRequired = false);

		bool AddOrUpdateOrderToEvent(Guid eventId, LiteOrder order, bool orderEventReferenceUpdateRequired = false);

		bool AddOrUpdateOrderToEvent(Event @event, LiteOrder order, bool orderEventReferenceUpdateRequired = false);

		bool UpdateEventStatus(Guid eventId, Status status);

		bool DeleteEvent(Guid id);

		bool DeleteOrderFromEvent(Guid eventId, Guid orderId);

		bool DeleteOrderFromEvent(Event @event, Guid orderId);

		IEnumerable<Event> GetAllEvents();
	}
}
