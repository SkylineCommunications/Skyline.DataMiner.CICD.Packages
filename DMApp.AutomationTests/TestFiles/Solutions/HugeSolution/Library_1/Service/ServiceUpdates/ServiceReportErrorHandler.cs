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

	public class ServiceReportErrorHandler : ServiceUpdateHandler
	{
		public ServiceReportErrorHandler(Helpers helpers, Order orderContainingService, Service service)
			: base (helpers, orderContainingService, service)
		{

		}

		protected override void CollectTasks()
		{
			Log(nameof(CollectTasks), "Update Specific Service Custom Properties Task");

			var updateSpecificServiceCustomPropertiesTask = new UpdateReportedIssueCustomPropertyTask(Helpers, service);
			tasks.Add(updateSpecificServiceCustomPropertiesTask);
		}
	}
}
