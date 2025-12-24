using System;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks
{
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class GetEventTask : Task
	{
		private readonly Guid eventGuid;

		public GetEventTask(Helpers helpers, string eventGuid) : base(helpers)
		{
			if (!Guid.TryParse(eventGuid, out this.eventGuid)) throw new ArgumentException("Parameter does not have a Guid format", nameof(eventGuid));

			IsBlocking = true;
		}

		public Event.Event Event { get; private set; }

		public override string Description => "Getting Event " + Event?.Name;

		protected override void InternalExecute()
		{
			Event = helpers.EventManager.GetEvent(eventGuid) ?? throw new EventNotFoundException(eventGuid);
		}

		public override Task CreateRollbackTask()
		{
			return null;
		}
	}
}
