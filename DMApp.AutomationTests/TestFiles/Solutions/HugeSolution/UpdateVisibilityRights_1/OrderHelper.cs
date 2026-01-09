namespace UpdateVisibilityRights_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class OrderHelper
	{
		private readonly Helpers helpers;
		private readonly JobManagerHelper jobManager;

		public OrderHelper(Helpers helpers)
		{
			this.helpers = helpers;
			jobManager = new JobManagerHelper(m => Engine.SLNetRaw.HandleMessages(m));
		}

		public BookedOrder GetOrder(string name)
		{
			ReservationInstance orderReservation = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, 
				new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.Name.Contains(name))
					.AND<ReservationInstance>(ReservationInstanceExposers.Properties.DictField("Type").Equal("Video"))
			).FirstOrDefault();

			if (orderReservation == null) return null;
			return RetrieveJobAndServices(orderReservation);
		}

		public BookedOrder GetOrder(Guid guid)
		{
			ReservationInstance orderReservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, guid);
			if (orderReservation == null) return null;
			return RetrieveJobAndServices(orderReservation);
		}

		public void UpdateVisibilityRights(BookedOrder order, HashSet<int> orderSecurityViewIds, HashSet<int> eventSecurityViewIds, ProgressDialog progressDialog)
		{
			progressDialog.AddProgressLine($"Updating Visibility Rights on Order {order.Reservation.Name}...");
			bool success = TryUpdateSecurityViewIds(order.Reservation, orderSecurityViewIds);
			progressDialog.AddProgressLine($"Updating Visibility Rights on Order {order.Reservation.Name} {(success ? "was successful" : "failed")}");

			progressDialog.AddProgressLine($"Updating Service Configuration for Order {order.Reservation.Name}...");
			success = UpdateServiceConfigurations(order.Reservation, order.ServiceConfigurations, orderSecurityViewIds);
			progressDialog.AddProgressLine($"Updating Service Configuration for Order {order.Reservation.Name} {(success ? "was successful" : "failed")}");

			foreach (var service in order.Services)
			{
				progressDialog.AddProgressLine($"Updating Visibility Rights on Service {service.Reservation.Name}...");
				success = TryUpdateSecurityViewIds(service.Reservation, orderSecurityViewIds);
				progressDialog.AddProgressLine($"Updating Visibility Rights on Service {service.Reservation.Name} {(success ? "was successful" : "failed")}");
			}

			progressDialog.AddProgressLine($"Updating Visibility Rights on Event {order.Job.ID}...");
			success = TryUpdateSecurityViewIds(order.Job, eventSecurityViewIds);
			progressDialog.AddProgressLine($"Updating Visibility Rights on Event {order.Job.ID} {(success ? "was successful" : "failed")}");
		}

		private bool TryUpdateSecurityViewIds(ReservationInstance reservation, HashSet<int> viewIds)
		{
			try
			{
				reservation.SecurityViewIDs = viewIds.ToList();
				DataMinerInterface.ResourceManager.AddOrUpdateReservationInstances(helpers, reservation);
				return true;
			}
			catch(Exception e)
			{
				helpers.Log(nameof(OrderHelper), nameof(TryUpdateSecurityViewIds), $"Exception occurred for reservation {reservation.Name}: {e}");
				return false;
			}
		}

		private bool TryUpdateSecurityViewIds(Job job, HashSet<int> viewIds)
		{
			try
			{
				job.SecurityViewIDs = viewIds.ToList();
				jobManager.Jobs.Update(job);
				return true;
			}
			catch(Exception e)
			{
				helpers.Log(nameof(OrderHelper), nameof(TryUpdateSecurityViewIds), $"Exception occurred for Job {job.ID}: {e}");
				return false;
			}
		}

		private BookedOrder RetrieveJobAndServices(ReservationInstance reservation)
		{
			Dictionary<int, ServiceConfiguration> serviceConfigs = GetServiceConfigurations(reservation.ID);

			BookedOrder order = new BookedOrder(reservation, serviceConfigs);
			foreach (var resource in reservation.ResourcesInReservationInstance)
			{
				ReservationInstance serviceReservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, resource.GUID);
				if (serviceReservation != null) order.Services.Add(new BookedService(serviceReservation));
			}

			order.Job = jobManager.Jobs.Read(JobExposers.ID.Equal(order.JobId)).FirstOrDefault();

			return order;
		}

		private Dictionary<int, ServiceConfiguration> GetServiceConfigurations(Guid orderId)
		{
			Dictionary<int, ServiceConfiguration> serviceConfigs;
			if (!helpers.OrderManagerElement.TryGetServiceConfigurations(orderId, out serviceConfigs))
			{
				helpers.Log(nameof(OrderHelper), nameof(GetServiceConfigurations), $"Failed to retrieve service configurations for order {orderId}");
			}

			return serviceConfigs;
		}

		private bool UpdateServiceConfigurations(ReservationInstance orderReservation, Dictionary<int, ServiceConfiguration> configurations, HashSet<int> orderSecurityViewIds)
		{
			foreach (var kvp in configurations) kvp.Value.SecurityViewIds = orderSecurityViewIds;
			string serializedConfigurations = JsonConvert.SerializeObject(configurations, Formatting.None);
			DateTime endTime = orderReservation.End + orderReservation.GetPostRoll();
			return helpers.OrderManagerElement.AddOrUpdateServiceConfigurations(orderReservation.ID, endTime, serializedConfigurations);
		}
	}
}