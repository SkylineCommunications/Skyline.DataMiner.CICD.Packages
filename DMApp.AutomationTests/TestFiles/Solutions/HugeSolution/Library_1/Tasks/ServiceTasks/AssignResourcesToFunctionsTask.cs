namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.Net.Time;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using Function = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
    using Skyline.DataMiner.Net.ResourceManager.Objects;

    public class AssignResourcesToFunctionsTask : Task
	{
		private readonly Service service;
		private readonly Order order;
		private readonly Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges;
		private readonly Dictionary<string, FunctionResource> originalFunctionResources;

		public AssignResourcesToFunctionsTask(Helpers helpers, Service service, Order order, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null) : base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.overwrittenFunctionTimeRanges = overwrittenFunctionTimeRanges;

			originalFunctionResources = service.Functions.ToDictionary(f => f.Definition.Label, f => f.Resource);

			Log(nameof(AssignResourcesToFunctionsTask), $"Original Function Resources: {String.Join(", ", originalFunctionResources.Select(x => $"{x.Key} - {x.Value?.Name}"))}");

			IsBlocking = true;
		}

		public override string Description => $"Assigning resources to service {service.Name}";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			var functionAssignments = service.AssignResourcesToFunctions(helpers, order, overwrittenFunctionTimeRanges);

			Log(nameof(InternalExecute), $"Function assignment report: {String.Join(", ", functionAssignments.Select(fa => $"Function: {fa.Key} -> resource assigned: {fa.Value}"))}");

			bool timeRangeWasOverwritten = overwrittenFunctionTimeRanges != null;
			if (functionAssignments.Values.Any(x => !x) && timeRangeWasOverwritten)
			{
				// Reset function resources to functions, for which resource assignment failed, to original values before triggering resource assignment again
				foreach (var function in service.Functions)
                {
					if (!functionAssignments.ContainsKey(function.Definition.Label) || functionAssignments[function.Definition.Label]) continue;
					function.Resource = originalFunctionResources[function.Definition.Label];
                }

				Log(nameof(InternalExecute), $"Retrying resource assignment without overwritten function time ranges");

				service.AssignResourcesToFunctions(helpers, order);
			}
		}
	}
}
