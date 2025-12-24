using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceUpdates
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Service = Service;

    public class ServiceStopHandler : ServiceUpdateHandler
    {
        public ServiceStopHandler(Helpers helpers, Order orderContainingService, Service service)
            : base(helpers, orderContainingService, service)
        {

        }

        protected override void CollectTasks()
        {
            Log(nameof(CollectTasks), "Stop service Task");

            var stopServiceTask = new StopServiceTask(Helpers, service, orderContainingService);
            tasks.Add(stopServiceTask);
        }
    }
}
