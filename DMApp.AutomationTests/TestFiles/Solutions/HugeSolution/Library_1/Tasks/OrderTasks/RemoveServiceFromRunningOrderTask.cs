namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Library.Solutions.SRM.Helpers;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class RemoveServiceFromRunningOrderTask : Task
	{
		private readonly Order order;
		private readonly Service serviceToRemove;
		private readonly List<Service> allServicesToRemove;

		public RemoveServiceFromRunningOrderTask(Helpers helpers, Order order, Service serviceToRemove, List<Service> allServicesToRemove = null) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.serviceToRemove = serviceToRemove ?? throw new ArgumentNullException(nameof(serviceToRemove));
			this.allServicesToRemove = allServicesToRemove ?? new List<Service>();
			IsBlocking = true;
		}

		public override string Description => $"Removing service {serviceToRemove.Name} from running order {order.Name}";

		public override Task CreateRollbackTask()
		{
			// TODO: implement rollback task
			return null;
		}

		protected override void InternalExecute()
		{
			ServiceReservationInstance orderReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, order.Id) as ServiceReservationInstance;
			var existingServiceDefinition = orderReservationInstance.GetServiceDefinition();

			// Find ID of node to which the resource is asssigned
			// This step is required because the node ids can change when a new compatible service definition is assigned to the order when making multiple of these calls
			var resourceUsageDefinition = orderReservationInstance.GetServiceResourceUsageDefinitions().FirstOrDefault(x => x.GUID == serviceToRemove.ContributingResource?.ID);
			if (resourceUsageDefinition == null)
			{
				Log(nameof(RemoveServiceFromRunningOrderTask), $"Unable to remove node, contributing resource with ID {serviceToRemove.ContributingResource?.ID} is not assigned to the order reservation");
				return;
			}

			// Remove resources and nodes from Service Definition
			orderReservationInstance = RemoveNode(orderReservationInstance, resourceUsageDefinition);

			// Update order service definition
			UpdateOrderServiceDefinition(orderReservationInstance);

			// Remove existing Service Definition - if applicable
			RemoveServiceDefinition(existingServiceDefinition);

			// Release service resources
			ReleaseResources();

			// Update node ids of other services
			UpdateNodeIds(orderReservationInstance);
		}

		private ServiceReservationInstance RemoveNode(ServiceReservationInstance orderReservationInstance, ServiceResourceUsageDefinition resourceUsageDefinition)
		{
			var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(OrderManager.OrderBookingManagerElementName));

			Log(nameof(RemoveNode), $"Removing service {serviceToRemove.Name} with contributing resource id {serviceToRemove.ContributingResource?.ID} assigned to node id {resourceUsageDefinition.ServiceDefinitionNodeID} from service definition {orderReservationInstance.ServiceDefinitionID}");

			try
			{
				var updateOrderReservationInstance = bookingManager.RemoveResourceAndNode((Engine)helpers.Engine, orderReservationInstance, serviceToRemove.ContributingResource.ID, resourceUsageDefinition.ServiceDefinitionNodeID) as ServiceReservationInstance;
				Log(nameof(RemoveNode), $"Succesfully removed service and node from running order");
				return updateOrderReservationInstance;
			}
			catch (ReservationUpdateException e)
			{
				// This exception gets thrown when a compatible service definition was found, assigned to the order and had its resources assigned to it, but it contains a node with the same id as the one that got removed.
				Log(nameof(RemoveNode), $"Failed to remove service and node from running order: {e}");
				
				// When above exception got thrown, the updated order reservation instance including the new assigned service definition isn't returned. To have the actual data the reservation need to be retrieved to make sure the contributing resource assignment succeeds (Correct node match).
				orderReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, order.Id) as ServiceReservationInstance;

				return orderReservationInstance;
			}
		}

		private void UpdateOrderServiceDefinition(ServiceReservationInstance orderReservationInstance)
		{
			Log(nameof(UpdateOrderServiceDefinition), $"Retrieving updated Service Definition: {orderReservationInstance.ServiceDefinitionID}");
			order.Definition = new ServiceDefinition 
			{
				Id = orderReservationInstance.ServiceDefinitionID, 
				BookingManagerElementName = SrmConfiguration.OrderBookingManagerElementName, 
				Diagram = DataMinerInterface.ServiceManager.GetServiceDefinition(helpers, orderReservationInstance.ServiceDefinitionID).Diagram 
			};

			Log(nameof(RemoveServiceFromRunningOrderTask), $"Retrieving updated Service Definition: {orderReservationInstance.ServiceDefinitionID}: {order.Definition.Diagram.DiagramToString()}");
		}

		private void RemoveServiceDefinition(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
		{
			if (serviceDefinition.IsTemplate) return; // Intermediate service definitions are never Templates

			ResourceManagerHelperExtended resourceManagerHelper = new ResourceManagerHelperExtended();
			resourceManagerHelper.RequestResponseEvent += (s, e) => e.responseMessage = Engine.SLNet.SendSingleResponseMessage(e.requestMessage);
			if (resourceManagerHelper.GetReservationInstancesByServiceDefinition(serviceDefinition).Any()) return;

			Log(nameof(RemoveServiceFromRunningOrderTask), $"Removing service definition {serviceDefinition.Name} with ID {serviceDefinition.ID}");
			helpers.ServiceDefinitionManager.DeleteServiceDefinition(serviceDefinition.ID);
		}

		private void ReleaseResources()
		{
			if (!serviceToRemove.IsSharedSource)
			{
				foreach (var function in serviceToRemove.Functions) function.Resource = null;
				Log(nameof(RemoveServiceFromRunningOrderTask), $"Releasing resources from service {serviceToRemove.Name}");
				if (helpers.ServiceManager.TryReleaseResources(serviceToRemove, serviceToRemove.Functions))
				{
					Log(nameof(RemoveServiceFromRunningOrderTask), nameof(ReleaseResources), $"Releasing resources from service {serviceToRemove.Name} was succesful");
				}
				else
				{
					Log(nameof(RemoveServiceFromRunningOrderTask), nameof(ReleaseResources), $"Releasing resources from service {serviceToRemove.Name} failed");
				}
			}
		}

		private void UpdateNodeIds(ServiceReservationInstance orderReservationInstance)
		{
			foreach (var service in order.AllServices.Concat(allServicesToRemove).Except(new[] { serviceToRemove }))
			{
				var resourceUsageDefinition = orderReservationInstance.GetServiceResourceUsageDefinitions().FirstOrDefault(x => x.GUID == service.ContributingResource?.ID);
				if (resourceUsageDefinition == null)
				{
					Log(nameof(UpdateNodeIds), $"Unable to find resource assigned to the order for service {service.Name} ({service.Id})");
					continue;
				}

				Log(nameof(RemoveServiceFromRunningOrderTask), nameof(UpdateNodeIds), $"Updating node Id of service {service.Name} from {service.NodeId} to {resourceUsageDefinition.ServiceDefinitionNodeID}");
				service.NodeId = resourceUsageDefinition.ServiceDefinitionNodeID;
			}
		}
	}
}
