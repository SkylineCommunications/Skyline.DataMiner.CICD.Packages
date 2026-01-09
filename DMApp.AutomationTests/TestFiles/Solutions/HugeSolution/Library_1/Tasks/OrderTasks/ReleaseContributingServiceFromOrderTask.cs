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

	public class ReleaseContributingServiceFromOrderTask : Task
	{
		private readonly Order order;
		private readonly Dictionary<string, Guid> nodeLabelsWithResourceToRelease;
		private ServiceReservationInstance orderReservationInstance;

		public ReleaseContributingServiceFromOrderTask(Helpers helpers, Order order, IEnumerable<Service> servicesToRelease, ServiceReservationInstance orderReservationInstance = null) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			if (servicesToRelease == null) throw new ArgumentException(nameof(servicesToRelease));
			this.orderReservationInstance = orderReservationInstance;

			nodeLabelsWithResourceToRelease = new Dictionary<string, Guid>();
			foreach (var service in servicesToRelease) nodeLabelsWithResourceToRelease.Add(service.NodeLabel, service.Id);

			IsBlocking = true;
		}

		public ReleaseContributingServiceFromOrderTask(Helpers helpers, Order order, Dictionary<string, Guid> nodeLabelsWithResourceToRelease, ServiceReservationInstance orderReservationInstance = null) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.nodeLabelsWithResourceToRelease = nodeLabelsWithResourceToRelease ?? throw new ArgumentException(nameof(nodeLabelsWithResourceToRelease));
			this.orderReservationInstance = orderReservationInstance;

			IsBlocking = true;
		}

		public override string Description => $"Releasing contributing services from order {order?.Name}";

		public override Task CreateRollbackTask()
		{
			return new AssignContributingServicesToOrderTask(helpers, order, nodeLabelsWithResourceToRelease);
		}

		protected override void InternalExecute()
		{
			orderReservationInstance = orderReservationInstance ?? DataMinerInterface.ResourceManager.GetReservationInstance(helpers, order.Id) as ServiceReservationInstance;
			var requests = new List<AssignResourceRequest>();
			foreach (string nodeLabel in nodeLabelsWithResourceToRelease.Keys)
			{
				requests.Add(new AssignResourceRequest
				{
					TargetNodeLabel = nodeLabel,
					NewResourceId = Guid.Empty
				});
			}

			helpers.Log(nameof(ReleaseContributingServiceFromOrderTask), nameof(InternalExecute), $"AssignResourceRequests: {String.Join(", ", requests.Select(x => x.TargetNodeLabel + " - " + x.NewResourceId))}");
			orderReservationInstance = DataMinerInterface.ReservationInstance.AssignResources(helpers, orderReservationInstance, requests.ToArray());
			order.Reservation = orderReservationInstance;
		}
	}
}
