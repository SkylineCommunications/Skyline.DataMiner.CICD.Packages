using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class GetNonLiveOrderTask : Task
	{
		private readonly int dataminerId;
		private readonly int ticketId;

		public GetNonLiveOrderTask(Helpers helpers, int dataminerId, int ticketId) : base(helpers)
		{
			this.dataminerId = dataminerId;
			this.ticketId = ticketId;
		}

		public override Task CreateRollbackTask()
		{
			return null;
		}

		public NonLiveOrder NonLiveOrder { get; private set; }

		protected override void InternalExecute()
		{
			NonLiveOrder = helpers.NonLiveOrderManager.GetNonLiveOrder(dataminerId, ticketId);
		}

		public override string Description => "Getting Order " + NonLiveOrder?.OrderDescription;
	}
}
