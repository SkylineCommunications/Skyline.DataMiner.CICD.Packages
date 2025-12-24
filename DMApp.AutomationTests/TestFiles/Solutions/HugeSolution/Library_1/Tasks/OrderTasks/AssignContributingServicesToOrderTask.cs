namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.AssignProfilesAndResources;
	using System.Linq;
	using Newtonsoft.Json;

	public class AssignContributingServicesToOrderTask : Task
	{
		private readonly Order order;
		private readonly Dictionary<string, Guid> nodeLabelsWithResourceToAssign = new Dictionary<string, Guid>();
		private ServiceReservationInstance orderReservationInstance;

		public AssignContributingServicesToOrderTask(Helpers helpers, Order order, ServiceReservationInstance orderReservationInstance = null) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.orderReservationInstance = orderReservationInstance;

			foreach(var service in order.AllServices) nodeLabelsWithResourceToAssign.Add(service.NodeLabel, service.ContributingResource?.ID ?? Guid.Empty);

			IsBlocking = true;
		}

		public AssignContributingServicesToOrderTask(Helpers helpers, Order order, Dictionary<string, Guid> nodeLabelsWithResourceToAssign, ServiceReservationInstance orderReservationInstance = null) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.nodeLabelsWithResourceToAssign = nodeLabelsWithResourceToAssign ?? throw new ArgumentNullException(nameof(nodeLabelsWithResourceToAssign));
			this.orderReservationInstance = orderReservationInstance;

			IsBlocking = true;
		}

		public override string Description => $"Assigning contributing services to order {order?.Name}";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			try
			{
				orderReservationInstance = orderReservationInstance ?? DataMinerInterface.ResourceManager.GetReservationInstance(helpers, order.Id) as ServiceReservationInstance;
				var requests = new List<AssignResourceRequest>();
				foreach (var kvp in nodeLabelsWithResourceToAssign)
				{
					requests.Add(new AssignResourceRequest
					{
						TargetNodeLabel = kvp.Key,
						NewResourceId = kvp.Value
					});
				}

				helpers.Log(nameof(AssignContributingServicesToOrderTask), nameof(InternalExecute), $"AssignResourceRequests: {String.Join(", ", requests.Select(x => x.TargetNodeLabel + " - " + x.NewResourceId))}");
				orderReservationInstance = DataMinerInterface.ReservationInstance.AssignResources(helpers, orderReservationInstance, requests.ToArray());
			}
			catch(Exception)
			{
				helpers.Log(nameof(AssignContributingServicesToOrderTask), nameof(InternalExecute), $"Order reservation used as argument for AssignResources: {JsonConvert.SerializeObject(orderReservationInstance)}");

				var realtimeOrderReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, order.Id) as ServiceReservationInstance;

				helpers.Log(nameof(AssignContributingServicesToOrderTask), nameof(InternalExecute), $"Realtime order reservation: {JsonConvert.SerializeObject(realtimeOrderReservationInstance)}");

				throw;
			}
		}
	}
}
