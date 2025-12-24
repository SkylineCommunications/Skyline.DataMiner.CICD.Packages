namespace UpdateService_4
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ConstructUpdateSharedSourceDialogTask : Task
	{
		private readonly Guid sharedSourceServiceId;
		private readonly IEnumerable<Order> ordersUsingSharedSource;
		private readonly IEnumerable<LockInfo> lockInfos;
		private readonly UserInfo userInfo;

		public ConstructUpdateSharedSourceDialogTask(Helpers helpers, Guid sharedSourceServiceId, IEnumerable<Order> ordersUsingSharedSource, IEnumerable<LockInfo> lockInfos, UserInfo userInfo) : base(helpers)
		{
			this.sharedSourceServiceId = sharedSourceServiceId.Equals(Guid.Empty) ? throw new ArgumentException(nameof(sharedSourceServiceId)) : sharedSourceServiceId;
			this.ordersUsingSharedSource = ordersUsingSharedSource ?? throw new ArgumentNullException(nameof(ordersUsingSharedSource));
			this.lockInfos = lockInfos ?? throw new ArgumentNullException(nameof(lockInfos));
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));

			IsBlocking = true;
		}

		public EditSharedSourceDialog EditSharedSourceDialog { get; private set; }

		public override string Description => "Building UI";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			EditSharedSourceDialog = new EditSharedSourceDialog(helpers, sharedSourceServiceId, ordersUsingSharedSource, lockInfos, userInfo);
		}
	}
}