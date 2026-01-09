namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Functions;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ConstructAssignReleaseResourceDialogTask : Task
    {
        private readonly Guid serviceToUpdateId;
        private readonly Order order;
        private readonly LockInfo lockInfo;
        private readonly UserInfo userInfo;
        private readonly ResourceScriptAction resourceAction;

        public ConstructAssignReleaseResourceDialogTask(Helpers helpers, ResourceScriptAction resourceAction, Guid serviceToUpdateId, Order order, LockInfo lockInfo, UserInfo userInfo)
            : base(helpers)
        {
            this.serviceToUpdateId = serviceToUpdateId;
            this.order = order ?? throw new ArgumentNullException(nameof(order));
            this.lockInfo = lockInfo ?? throw new ArgumentNullException(nameof(lockInfo));
            this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
            this.resourceAction = resourceAction;

            IsBlocking = true;
        }

        public AssignReleaseResourceDialog AssignReleaseResourceDialog { get; private set; }

        public override Task CreateRollbackTask()
        {
            return null;
        }

        protected override void InternalExecute()
        {
            AssignReleaseResourceDialog = new AssignReleaseResourceDialog(helpers, resourceAction, serviceToUpdateId, order, lockInfo, userInfo);
        }

        public override string Description => "Building UI";
    }
}
