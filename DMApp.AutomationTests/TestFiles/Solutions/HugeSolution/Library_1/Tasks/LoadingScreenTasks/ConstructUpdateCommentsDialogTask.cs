using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Comments;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ConstructUpdateCommentsDialogTask : Task
	{
		private readonly UserInfo userInfo;
		private readonly LockInfo lockInfo;
		private readonly Order.Order order;

		public ConstructUpdateCommentsDialogTask(Helpers helpers, UserInfo userInfo, Order.Order order, LockInfo lockInfo) : base(helpers)
		{
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.lockInfo = lockInfo ?? throw new ArgumentNullException(nameof(lockInfo));
			this.order = order ?? throw new ArgumentNullException(nameof(order));

			IsBlocking = true;
		}

		protected override void InternalExecute()
		{
			UpdateCommentsDialog = new UpdateCommentsDialog(helpers.Engine, order, lockInfo, userInfo);
		}
		public override Task CreateRollbackTask()
		{
			return null;
		}


		public UpdateCommentsDialog UpdateCommentsDialog { get; private set; }

		public override string Description => "Building UI";
	}
}
