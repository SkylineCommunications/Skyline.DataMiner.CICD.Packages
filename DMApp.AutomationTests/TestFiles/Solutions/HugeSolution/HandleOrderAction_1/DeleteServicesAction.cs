namespace HandleOrderAction_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class DeleteServicesAction : OrderAction
	{
		private readonly List<string> semiColonSeparatedServiceIds;
		private readonly bool removeAllServices;

		public DeleteServicesAction(Helpers helpers, Guid orderId, List<string> semiColonSeparatedServiceIds, bool removeAllServices) : base(helpers, orderId)
		{
			this.semiColonSeparatedServiceIds = semiColonSeparatedServiceIds ?? throw new ArgumentNullException(nameof(semiColonSeparatedServiceIds));
			this.removeAllServices = removeAllServices;
		}

		public override void Execute()
		{
			if (TryDeleteServices(out var errorMessage))
			{
				Log(nameof(Execute), "Deleting services succeeded");
			}
			else
			{
				Log(nameof(Execute), $"Deleting services couldn't proceed: {errorMessage}");
			}
		}

		public override void HandleException(string errorMessage)
		{
			// nothing to do
		}

		private bool TryDeleteServices(out string errorMessage)
		{
			errorMessage = string.Empty;

			if (!TryGetOrder(out var order, out errorMessage)) return false;
			var servicesToRemove = GetServicesToRemove(order);

			try
			{
				Log(nameof(TryDeleteServices), $"Removing the following services: {string.Join(";", servicesToRemove.Select(x => x.Id))}");

				if (!servicesToRemove.Any())
				{
					errorMessage = "No services to remove";
					Log(nameof(TryDeleteServices), errorMessage);

					return false;
				}

				if (order.RemoveBookedServices(helpers, servicesToRemove))
				{
					return true;
				}
				else
				{
					errorMessage = "Removing booked services failed";
					return false;
				}
				
			}
			catch (Exception e)
			{
				errorMessage = $"Exception deleting services from order: {e}";
				return false;
			}
		}

		private List<Service> GetServicesToRemove(Order order)
		{
			Log(nameof(GetServicesToRemove), $"ServiceIds script param value: '{string.Join(";", semiColonSeparatedServiceIds)}', Remove all services: {removeAllServices}");

			if (removeAllServices) return order.AllServices.Where(x => x.IsBooked).ToList();

			var servicesToRemove = new List<Service>();
			foreach (string serviceIdValue in semiColonSeparatedServiceIds)
			{
				Log(nameof(GetServicesToRemove), $"Service id value: {serviceIdValue}");

				if (!Guid.TryParse(serviceIdValue, out var serviceId))
				{
					Log(nameof(GetServicesToRemove), $"Parsing service guid {serviceIdValue} failed");
					continue;
				}

				// Services need to be retrieved, services to be removed are already detached from the order.
				if (!helpers.ServiceManager.TryGetService(serviceId, out var service))
				{
					Log(nameof(GetServicesToRemove), $"Retrieving service with id {serviceId} failed");
				}
				else
				{
					servicesToRemove.Add(service);
				}
			}

			return servicesToRemove;
		}
	}
}