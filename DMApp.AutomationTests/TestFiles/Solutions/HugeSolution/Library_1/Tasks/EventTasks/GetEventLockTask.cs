namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class GetEventLockTask : Task
	{
		private readonly Event.Event @event;

		public GetEventLockTask(Helpers helpers, Event.Event @event) : base(helpers)
		{
			this.@event = @event ?? throw new ArgumentNullException(nameof(@event));
		}

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			LockInfo = helpers.LockManager.RequestEventLock(@event.Id);
		}

		public override string Description => $"Getting lock for Event {@event.Name}";

		public LockInfo LockInfo { get; private set; }
	}
}
