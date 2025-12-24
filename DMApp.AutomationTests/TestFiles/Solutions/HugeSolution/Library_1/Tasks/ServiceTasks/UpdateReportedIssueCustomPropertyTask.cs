using System;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;


namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using System.Collections.Generic;
	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class UpdateReportedIssueCustomPropertyTask : Task
	{
		private readonly Service service;

		public UpdateReportedIssueCustomPropertyTask(Helpers helpers, Service service) : base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			IsBlocking = false;
		}

		public override string Description => "Update Service " + service.Name;

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			if (!service.TryUpdateCustomProperties(helpers,new Dictionary<string, object>() { { ServicePropertyNames.ReportedIssuePropertyName, service.HasAnIssueBeenreportedManually.ToString() } }))
			{
				throw new CustomPropertyUpdateFailedException(service.Name);
			}
		}
	}
}
