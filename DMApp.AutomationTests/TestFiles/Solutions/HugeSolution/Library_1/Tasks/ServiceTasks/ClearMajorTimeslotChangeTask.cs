using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

    using Service = Service.Service;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

    public class ClearMajorTimeslotFlagTask : Task
    {
        private readonly Service service;

        public ClearMajorTimeslotFlagTask(Helpers helpers, Service service)
            : base(helpers)
        {
            this.service = service;
            IsBlocking = false;
        }

        public override string Description => "Major timeslot change flag clear on " + service.Name;

        public override Task CreateRollbackTask()
        {
            return new SetMajorTimeslotFlagTask(helpers ,service);
        }

        protected override void InternalExecute()
        {
            service.MajorTimeslotChange = false;
        }
    }
}