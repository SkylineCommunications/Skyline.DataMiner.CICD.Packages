using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class AddOrUpdateEventTask : Task
	{
		private readonly Event eventToAddOrUpdate;
		private readonly Event previousEvent;
        private readonly LockInfo lockEventInfo;

        public AddOrUpdateEventTask(Helpers helpers, Event eventToAddOrUpdate, Event existingEvent = null, Locking.LockInfo lockEventInfo = null)
			: base(helpers)
		{
			this.eventToAddOrUpdate = eventToAddOrUpdate;
			previousEvent = existingEvent ?? (eventToAddOrUpdate.Id == Guid.Empty ? null : base.helpers.EventManager.GetEvent(eventToAddOrUpdate.Id));
            this.lockEventInfo = lockEventInfo;
            IsBlocking = true;
		}

		public override string Description => "Add Or Update Event " + eventToAddOrUpdate.Name;

		public override Task CreateRollbackTask()
		{
			if (previousEvent == null)
			{
				// Delete Event
				return new DeleteEventTask(helpers, eventToAddOrUpdate.Id);
			}
			else
			{
				// Set back old values
				return new AddOrUpdateEventTask(helpers, previousEvent);
			}
		}

		protected override void InternalExecute()
		{
			if (lockEventInfo != null && !lockEventInfo.IsLockGranted)
            {				
				IsBlocking = false;
				throw new AddOrUpdateEventFailedException("Lock is not granted.");
            }

			bool addOrUpdateSuccessful = helpers.EventManager.AddOrUpdateEvent(eventToAddOrUpdate, previousEvent);

			if (!addOrUpdateSuccessful)
			{
				throw new AddOrUpdateEventFailedException();
			}
		}
	}
}