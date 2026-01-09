namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class OrderBookServicesHandler : OrderUpdateHandler
	{
		public OrderBookServicesHandler(Helpers helpers, Order order) : base(helpers, order, new OrderUpdateHandlerInput())
		{
		}

		protected override void CollectActionsToExecute()
		{
			actionsToExecute.Add(GetExistingOrderAndEvent);
			actionsToExecute.Add(ReleaseRoutingResourcesInBackground);
			actionsToExecute.Add(GenerateProcessingServices);
			actionsToExecute.Add(CancelServicesToRemove);
			actionsToExecute.Add(CheckMessiChangeHighlightConditionsOnRecordings);
			actionsToExecute.Add(AddOrUpdateNonRoutingNonConverterServices);
            actionsToExecute.Add(GenerateVizremConverterServices);
			actionsToExecute.Add(AddOrUpdateVizremConverterServices);
			actionsToExecute.Add(GenerateRoutingServices);
			actionsToExecute.Add(CheckMessiChangeHighlightConditionsOnRoutings);
			actionsToExecute.Add(AddOrUpdateRoutingServices);
			actionsToExecute.Add(CollectServicesToRemoveComparedToExistingOrder);
			actionsToExecute.Add(AddOrUpdateOrderServiceDefinition);
			actionsToExecute.Add(AddOrUpdateOrderReservation);
			actionsToExecute.Add(PromoteReceptionToSharedSource);
			actionsToExecute.Add(AddOrUpdateUserTasks);
			actionsToExecute.Add(DeleteServices);
			actionsToExecute.Add(UpdateServiceUiProperties);
			actionsToExecute.Add(UpdateOrderUiProperties);
			actionsToExecute.Add(TryStartOrderNowIfApplicable);
			actionsToExecute.Add(TryStopPreviousRunningOrderNowIfApplicable);
		}
	}
}