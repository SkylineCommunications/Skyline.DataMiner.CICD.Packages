namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class FunctionInfo
	{
		public FunctionInfo(string serviceName, Function function)
		{
			ServiceName = serviceName;
			FunctionLabel = function.Definition.Label;
			Resource = function.Resource;
			ProfileParameterValues = function.Parameters.Concat(function.InterfaceParameters).ToDictionary(p => p.Id, p => p.Value);
		}

		public string ServiceName { get; }

		public string FunctionLabel { get; }

		public FunctionResource Resource { get; }

		public Dictionary<Guid, object> ProfileParameterValues { get; }

		public override string ToString()
		{
			return $"Service {ServiceName} Function {FunctionLabel} Resource '{Resource?.Name}', Profile Parameters {string.Join(", ", ProfileParameterValues.Select(pair => $"{pair.Key}='{pair.Value}'"))}";
		}
	}
}
