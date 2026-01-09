namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class AssignOccupiedResourcesToOrderHandler : OrderUpdateHandler
	{
		private List<FunctionResource> occupiedResources = new List<FunctionResource>();
		private List<ReservationInstance> occupyingOrderReservations = new List<ReservationInstance>();
		private readonly List<Order> occupyingOrders = new List<Order>();

		public AssignOccupiedResourcesToOrderHandler(Helpers helpers, Order order)
			: base(helpers, order, new OrderUpdateHandlerInput { IsHighPriority = true })
		{
		}

		protected override void CollectActionsToExecute()
		{
			actionsToExecute.Add(CollectOccupyingOrders);
			actionsToExecute.Add(DoubleCheckOccupyingOrderLocks);
			actionsToExecute.Add(ClearResourcesFromOccupyingOrders);
			actionsToExecute.Add(BookServicesForPrimaryOrder);
			actionsToExecute.Add(AssignResourcesToOccupyingOrders);
			actionsToExecute.Add(BookServicesForOccupyingOrders);
		}

		private void CollectOccupyingOrders()
		{
			Report("Checking occupied resources...");

			occupiedResources = order.AllServices
				.SelectMany(s => s.Functions)
				.Select(f => f.Resource)
				.OfType<OccupiedResource>()
				.Cast<FunctionResource>()
				.ToList();

			Log(nameof(CollectOccupyingOrders), $"Occupied resources: '{string.Join(", ", occupiedResources.Select(r => r.Name))}'");
			if (!occupiedResources.Any()) throw new InvalidOperationException($"No occupied resource objects found");

			occupyingOrderReservations = occupiedResources
				.Cast<OccupiedResource>()
				.SelectMany(r => r.OccupyingServices)
				.SelectMany(os => os.Orders)
				.Distinct()
				.ToList();

			Log(nameof(CollectOccupyingOrders), $"Occupying order reservations: '{string.Join(", ", occupyingOrderReservations.Select(r => r.Name))}'");
			if (!occupyingOrderReservations.Any()) throw new InvalidOperationException($"No occupying order reservations found");

			ConsiderConnectedRoutingOutputResourcesAsOccupied();

			Report("Checking occupied resources succeeded");
		}

		private void ConsiderConnectedRoutingOutputResourcesAsOccupied()
		{
			foreach (var occupyingOrderReservation in occupyingOrderReservations)
			{
				var occupyingOrder = Helpers.OrderManager.GetOrder(occupyingOrderReservation);
				occupyingOrders.Add(occupyingOrder);

				foreach (var service in occupyingOrder.AllServices)
				{
					ConsiderConnectedRoutingOutputResourcesAsOccupied(order, service);
				}
			}
		}

		private void ConsiderConnectedRoutingOutputResourcesAsOccupied(Order occupyingOrder, Service.Service service)
		{
			var occupyingFunctions = service.Functions.Where(f => occupiedResources.Contains(f.Resource)).ToList();

			foreach (var occupyingFunction in occupyingFunctions)
			{
				var parentService = occupyingOrder.AllServices.SingleOrDefault(s => s.Children.Contains(service));

				bool occupyingFunctionIsConnectedToMatrixOutputSdi = service.Definition.FunctionIsFirst(occupyingFunction) && parentService != null && parentService.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.Routing;
				if (!occupyingFunctionIsConnectedToMatrixOutputSdi) continue;

				var lastResourceRequiringFunctionOfParentService = parentService.GetLastResourceRequiringFunction(Helpers);
				if (lastResourceRequiringFunctionOfParentService.Resource == null) continue;

				occupiedResources.Add(lastResourceRequiringFunctionOfParentService.Resource);

				Log(nameof(ConsiderConnectedRoutingOutputResourcesAsOccupied), $"Parent service of occupying function {occupyingFunction.Definition.Label} of service {service.Name} is routing {parentService.Name}, considering its resource {lastResourceRequiringFunctionOfParentService.ResourceName} as occupied.");
			}
		}

		private void DoubleCheckOccupyingOrderLocks()
		{
			foreach (var occupyingOrder in occupyingOrderReservations)
			{
				var lockInfo = Helpers.LockManager.RequestOrderLock(occupyingOrder.ID);

				if (!lockInfo.IsLockGranted) throw new InvalidOperationException($"Lock for occupying order {occupyingOrder.Name} was not granted");
			}
		}

		private void ClearResourcesFromOccupyingOrders()
		{
			foreach (var occupyingOrder in occupyingOrders)
			{
				Report($"Removing occupied resources from order {occupyingOrder.Name}...");

				foreach (var service in occupyingOrder.AllServices)
				{
					var occupyingFunctions = service.Functions.Where(f => occupiedResources.Contains(f.Resource)).ToList();

					bool isOccupyingService = occupyingFunctions.Any();
					if (!isOccupyingService) continue;

					foreach (var function in occupyingFunctions)
					{
						function.EnforceSelectedResource = service.Definition.VirtualPlatformServiceType == ServiceDefinition.VirtualPlatformType.Reception; // allow free reselection of resource later for non-receptions
						function.Resource = null;
					}

					var updateResourcesTask = new UpdateResourcesTask(Helpers, service, occupyingFunctions);
					tasks.Add(updateResourcesTask);
					if (!updateResourcesTask.Execute())
					{
						Log(nameof(ClearResourcesFromOccupyingOrders), "Update resources task failed");
						isSuccessful = false;
						return;
					}

					var udpateCustomPropertiesTask = new UpdateCustomPropertiesTask(Helpers, service, service, occupyingOrder);
					tasks.Add(udpateCustomPropertiesTask);
					if (!udpateCustomPropertiesTask.Execute())
					{
						Log(nameof(ClearResourcesFromOccupyingOrders), "Update custom properties task failed");
						isSuccessful = false;
						return;
					}
				}

				Report($"Removing occupied resources from order {occupyingOrder.Name} succeeded");
			}
		}

		private void BookServicesForPrimaryOrder()
		{
			Report($"Updating services for order {order.Name}...");

			tasks.AddRange(order.BookServices(Helpers).Tasks);

			Report($"Updating services for order {order.Name} succeeded");
		}

		private void AssignResourcesToOccupyingOrders()
		{
			foreach (var occupyingOrder in occupyingOrders)
			{
				Report($"Replacing occupied resources in order {occupyingOrder.Name}...");

				foreach (var service in occupyingOrder.AllServices)
				{
					var assignResourcesToFunctionsTask = new AssignResourcesToFunctionsTask(Helpers, service, occupyingOrder, null);
					tasks.Add(assignResourcesToFunctionsTask);
					assignResourcesToFunctionsTask.Execute();
				}

				Report($"Replacing occupied resources in order {occupyingOrder.Name} succeeded");
			}
		}

		private void BookServicesForOccupyingOrders()
		{
			foreach (var occupyingOrder in occupyingOrders)
			{
				Report($"Updating services for order {occupyingOrder.Name}...");

				tasks.AddRange(occupyingOrder.BookServices(Helpers).Tasks);

				Report($"Updating services for order {occupyingOrder.Name} succeeded");
			}
		}
	}
}
