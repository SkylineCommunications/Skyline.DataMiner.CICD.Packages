using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ConstructMergeEventsDialogTask : Task
	{
		private readonly IEnumerable<Event> events;
		private readonly IEnumerable<LiteOrder> orders;

		public ConstructMergeEventsDialogTask(Helpers helpers, IEnumerable<Event> events, IEnumerable<LiteOrder> orders) : base(helpers)
		{
			this.events = events ?? throw new ArgumentNullException(nameof(events));
			this.orders = orders ?? throw new ArgumentNullException(nameof(orders));
		}

		public MergeEventsDialog MergeEventsDialog { get; private set; }

		
		public override Task CreateRollbackTask()
		{
			return null;
		}

		public override string Description => "Building UI";

		protected override void InternalExecute()
		{
			MergeEventsDialog = new MergeEventsDialog(helpers, events.ToList(), orders.ToList(), helpers.LockManager);
		}
	}
}


