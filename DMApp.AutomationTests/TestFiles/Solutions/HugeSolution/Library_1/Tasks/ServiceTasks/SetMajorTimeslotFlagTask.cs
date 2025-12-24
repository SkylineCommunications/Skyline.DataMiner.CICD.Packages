using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

    using Service = Service.Service;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
    using System.Collections.Generic;

    public class SetMajorTimeslotFlagTask : Task
    {
        private readonly Service service;

        public SetMajorTimeslotFlagTask(Helpers helpers, Service service)
            : base(helpers)
        {
            this.service = service;
            IsBlocking = false;
        }

        public override string Description => "Major timeslot change flag set on " + service.Name;

        public override Task CreateRollbackTask()
        {
            return new ClearMajorTimeslotFlagTask(helpers, service);
        }

        protected override void InternalExecute()
        {
            service.MajorTimeslotChange = true;
        }
    }
}