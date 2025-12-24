using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

    using Service = Service.Service;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

    public class StopServiceTask : Task
    {
        private readonly Service service;

        private readonly Order order;

        public StopServiceTask(Helpers helpers, Service service, Order order)
            : base(helpers)
        {
            this.service = service;
            this.order = order;
            IsBlocking = true;
        }

        public override string Description => "Stopping service " + service.Name;

        public override Task CreateRollbackTask()
        {
            return null;
        }

        protected override void InternalExecute()
        {
            // to make sure that the service got removed from the order inside TryChangeResources().
            service.ContributingResource = null;

            order.TryChangeResources(helpers, new List<Service>() { service });
            service.TryStopServiceNow(helpers);
        }
    }
}
