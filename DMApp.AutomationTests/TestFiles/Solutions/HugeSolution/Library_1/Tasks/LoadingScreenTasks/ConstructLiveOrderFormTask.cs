namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ConstructLiveOrderFormTask : Task
	{
		private readonly Order order;
		private readonly Event @event;
		private readonly LockInfo lockInfo;
		private readonly UserInfo userInfo;
		private readonly LiveOrderFormAction scriptAction;

		public ConstructLiveOrderFormTask(Helpers helpers, Order order, Event @event, LockInfo lockInfo, UserInfo userInfo, LiveOrderFormAction scriptAction) : base(helpers)
		{
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.order = order;
			this.@event = @event;
			this.lockInfo = lockInfo;
			this.scriptAction = scriptAction;
	
			IsBlocking = true;
		}

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			EditOrderDialog = new LiveOrderFormDialog(helpers, order, @event, userInfo, Configuration.Scripts.LiveOrderForm, FlowMapper.Mapping[scriptAction], lockInfo);
		}

		public override string Description => "Building UI";

		public LiveOrderFormDialog EditOrderDialog { get; private set; }
	}
}