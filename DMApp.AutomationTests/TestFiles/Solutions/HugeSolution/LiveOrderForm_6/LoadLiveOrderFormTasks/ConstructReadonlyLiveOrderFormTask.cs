namespace LiveOrderForm_6.LoadLiveOrderFormTasks
{
	using System;
	using Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ConstructReadonlyLiveOrderFormTask : Task
	{
		private readonly Order order;
		private readonly Event @event;
		private readonly UserInfo userInfo;

		public ConstructReadonlyLiveOrderFormTask(Helpers helpers, Order order, Event @event, UserInfo userInfo) : base(helpers)
		{
			this.order = order;
			this.@event = @event;
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
	
			IsBlocking = true;
		}

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			EditOrderDialog = new LiveOrderFormDialog(helpers, order, @event, userInfo, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.Scripts.LiveOrderForm, EditOrderFlows.ViewOrder);
		}

		public override string Description => "Building UI ";

		public LiveOrderFormDialog EditOrderDialog { get; private set; }
	}
}