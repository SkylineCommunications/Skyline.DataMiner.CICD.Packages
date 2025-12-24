namespace UpdateService_4
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ConstructDeleteServiceDialogTask : Task
	{
		private readonly Guid serviceToDeleteId;
		private readonly Order order;
		private readonly UserInfo userInfo;

		public ConstructDeleteServiceDialogTask(Helpers helpers, Guid serviceToDeleteId, Order order, UserInfo userInfo) : base(helpers)
		{
			this.serviceToDeleteId = serviceToDeleteId;
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			IsBlocking = true;
		}

		public DeleteServiceDialog DeleteServiceDialog { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			order.AcceptChanges();
			DeleteServiceDialog = new DeleteServiceDialog(helpers, order, userInfo, order.AllServices.Single(s => s.Id == serviceToDeleteId));
		}

		public override string Description => "Building UI";
	}
}