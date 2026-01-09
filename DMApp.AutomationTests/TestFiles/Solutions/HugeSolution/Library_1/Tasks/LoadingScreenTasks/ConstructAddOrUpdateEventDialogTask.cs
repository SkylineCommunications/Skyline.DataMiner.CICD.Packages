using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ConstructAddOrUpdateEventDialogTask : Task
	{
		private readonly UserInfo userInfo;
		private readonly Event.Event @event;
		private readonly LockInfo lockInfo;

		public ConstructAddOrUpdateEventDialogTask(Helpers helpers, UserInfo userInfo, Event.Event @event = null, LockInfo lockInfo = null) : base(helpers)
		{
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.@event = @event;
			this.lockInfo = lockInfo;
		}

		public AddOrUpdateEventDialog AddOrUpdateEventDialog { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			AddOrUpdateEventDialog = new AddOrUpdateEventDialog(helpers, userInfo, @event, lockInfo);
		}

		public override string Description => "Building UI";
	}
}
