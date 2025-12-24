using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class DeleteEventTask : Task
	{
		private readonly Event oldEvent;

		private readonly Guid eventId;

		public DeleteEventTask(Helpers helpers, Guid eventId)
			: base(helpers)
		{
			this.eventId = eventId;
			oldEvent = base.helpers.EventManager.GetEvent(eventId);
			IsBlocking = true;
		}

		public override string Description => "Deleting Event " + oldEvent.Name;

		public override Task CreateRollbackTask()
		{
			// TODO: does it make sense to add the Event again even though the user wants it removed?
			// return new AddOrUpdateEventTask(engine, progressReporter, oldEvent);
			return null;
		}

		protected override void InternalExecute()
		{
			helpers.EventManager.DeleteEvent(eventId);
		}
	}
}