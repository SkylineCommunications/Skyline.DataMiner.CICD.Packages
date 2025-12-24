namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class AddServicesToRunningOrderSubHandler : OrderUpdateHandler
    {
        public AddServicesToRunningOrderSubHandler(Helpers helpers, Order order, OrderUpdateHandlerInput input, Order existingOrder) : base(helpers, order, input)
        {          
            this.existingOrder = existingOrder ?? throw new ArgumentNullException(nameof(existingOrder));
        }

        protected override void CollectActionsToExecute()
        {
            actionsToExecute.Add(ReInitOrderToBeCreatedAsNewIncludingAddedServices);
            actionsToExecute.Add(UpdateExistingRunningOrder);
            actionsToExecute.Add(AddOrUpdateNewOrder);
            actionsToExecute.Add(UpdateSatelliteRxSynopsisAttachments);
            actionsToExecute.Add(ClearIntegrationReferencesOnExistingRunningOrder);
        }

        private void ReInitOrderToBeCreatedAsNewIncludingAddedServices()
        {
            Report("Prepare order to be created as new ...");
            var now = DateTime.Now.RoundToMinutes();

            order.Id = Guid.Empty; // Setting actual order as new.
            order.StartNow = true;
            order.ConvertedFromRunningToStartNow = true;
            order.PreviousRunningOrderId = existingOrder.Id;

            Log(nameof(ReInitOrderToBeCreatedAsNewIncludingAddedServices), $"Added services: {string.Join(", ", order.GetAddedServices().Select(s => s.Name))}");

            foreach (var service in order.GetAddedServices())
            {
                if (!service.IsSharedSource && !service.IsBooked)
                {
                    bool serviceStartIsWithinStartNowFrame = service.StartWithPreRoll <= now.Add(TimeSpan.FromMinutes(Order.StartNowDelayInMinutesWhenAddingServicesToRunningOrder));

                    service.Id = Guid.NewGuid();
                    service.NodeId = 0;
                    service.ShouldStartDirectly = serviceStartIsWithinStartNowFrame;
                    continue;
                }
                
                service.OrderReferences.Remove(existingOrder.Id);
                service.UpdateOrderReferencesProperty(Helpers, service.ReservationInstance);
            }

            Report("New order will be created");
        }

        private void UpdateExistingRunningOrder()
        {        
            var releaseContributingServiceFromOrderTask = new ReleaseContributingServiceFromOrderTask(Helpers, existingOrder, existingOrder.AllServices);
            tasks.Add(releaseContributingServiceFromOrderTask);

            if (!releaseContributingServiceFromOrderTask.Execute())
            {
                Log(nameof(UpdateExistingRunningOrder), $"Releasing service contributing resources on order {existingOrder.Name} ({existingOrder.Id}) failed");
                isSuccessful = false;
                return;
            }

            existingOrder.ManualName += Constants.ReplacedKeyWord + $" [{Guid.NewGuid()}]";

            var updateExistingOrderNameTask = new ChangeOrderNameTask(Helpers, existingOrder, order.Name);
            tasks.Add(updateExistingOrderNameTask);
            if (!updateExistingOrderNameTask.Execute())
            {
                Log(nameof(UpdateExistingRunningOrder), $"Setting order {existingOrder.Id} name to {existingOrder.Name} failed");
                isSuccessful = false;
                return;
            }

            var servicesToRemove = new List<Service>();

			foreach (var existingService in existingOrder.AllServices)
			{
                bool serviceIsPartOfNewOrder = order.AllServices.Exists(s => s.Name == existingService.Name);
                if (serviceIsPartOfNewOrder) continue;

                servicesToRemove.Add(existingService);
			}

            Log(nameof(UpdateExistingRunningOrder), $"Services to remove from existing order: {string.Join(", ", servicesToRemove.Select(s => $"{s.Name} ({s.Id})"))}");

            Helpers.OrderManagerElement.TriggerDeleteServices(existingOrder, servicesToRemove.Select(s => s.Id).ToList()); 
        }

        private void AddOrUpdateNewOrder()
        {
            var orderUpdateHandler = new OrderAddOrUpdateSubHandler(Helpers, order, new OrderUpdateHandlerInput { IsHighPriority = isHighPriority, Options = options, ProcessChronologically = processChronologically }, existingOrder);

            var updateResult = orderUpdateHandler.Execute();

            tasks.AddRange(updateResult.Tasks);
            Exceptions.AddRange(updateResult.Exceptions);
        }
        
        private void ClearIntegrationReferencesOnExistingRunningOrder()
        {
            existingOrder.ClearIntegrationReferences(Helpers, clearAllowed: true); // Integration references need to be removed from the previous running order, as each update after an add running edit need to happen on the most active order.
        }
    }
}
