namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Plasma;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class OrderAddOrUpdateSubHandler : OrderUpdateHandler
    {
        public OrderAddOrUpdateSubHandler(Helpers helpers, Order order, OrderUpdateHandlerInput input, Order existingOrder) : base(helpers, order, input)
        {
            this.existingOrder = existingOrder;
        }

        protected override void CollectActionsToExecute()
        {
            actionsToExecute.Add(CheckMessiChangeHighlightingRequired);
            actionsToExecute.Add(CollectServicesToRemoveComparedToExistingOrder);
            actionsToExecute.Add(AddOrUpdateOrderServiceDefinition);
            actionsToExecute.Add(CheckToAdjustStartTimes);
            actionsToExecute.Add(AddOrUpdateEvent);
            actionsToExecute.Add(AddOrUpdateOrderReservation);
            actionsToExecute.Add(UpdateSatelliteRxSynopsisAttachments);
            actionsToExecute.Add(LinkOrderToEvent);
            actionsToExecute.Add(ReleaseLocks);
            actionsToExecute.Add(UpdateOrderManagerAndContractManagerElement);
        }

		private void CheckMessiChangeHighlightingRequired()
		{
			SetLateChangeProperty();

			foreach (var service in order.AllServices.Where(s => s.LateChange))
			{
				service.TryUpdateCustomProperties(Helpers, new Dictionary<string, object> { { ServicePropertyNames.LateChange, true.ToString() } });
			}
		}

		private void SetLateChangeProperty()
		{
			if (order.ChangeTrackingStarted)
			{
				bool oldSourceGotRemovedAndNewSourceWasAdded = order.Change is OrderChange orderChange && (orderChange.CollectionChanges.SingleOrDefault(cc => cc.CollectionName == nameof(order.AllServices))?.Changes?.Count(change => change.ItemIdentifier.Contains("Reception")) ?? 0) >= 2;

				Log(nameof(CheckMessiChangeHighlightingRequired), $"Order source was {(oldSourceGotRemovedAndNewSourceWasAdded ? string.Empty : "not ")}changed");

				if (oldSourceGotRemovedAndNewSourceWasAdded)
				{
					foreach (var recordingService in order.AllServices.Where(s => s.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.Recording))
					{
						bool lessThanTwelveHoursUntilServiceStart = recordingService.Start < DateTime.Now + TimeSpan.FromHours(12);
						if (!lessThanTwelveHoursUntilServiceStart) continue;

						recordingService.LateChange = true;
					}
				}
			}
			else
			{
				Log(nameof(CheckMessiChangeHighlightingRequired), $"Order cahnge tracking was not started");
			}
		}
	}
}
