namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class UpdateSecurityViewIdsTask : Task
	{
		private readonly Service service;

		public UpdateSecurityViewIdsTask(Helpers helpers, Service service) : base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			IsBlocking = false;
		}

		public override string Description => $"Updating visibility rights on {service?.Name}";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			service.ReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, service.Id) as ServiceReservationInstance ?? throw new ReservationNotFoundException(service.Id);

			service.UpdateSecurityViewIds(helpers, service.SecurityViewIds);
		}
	}
}
