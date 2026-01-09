namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Utilities;

	public static partial class DataMinerInterface
	{
		public static class ServiceManager
		{
			[WrappedMethod("ServiceManager", "GetServiceDefinition")]
			public static Net.ServiceManager.Objects.ServiceDefinition GetServiceDefinition(Helpers helpers, Guid id)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var serviceDefinition = SrmManagers.ServiceManager.GetServiceDefinition(id);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return serviceDefinition;
			}

			[WrappedMethod("ServiceManager", "GetServiceDefinitions")]
			public static IEnumerable<Net.ServiceManager.Objects.ServiceDefinition> GetServiceDefinitions(Helpers helpers, FilterElement<Net.ServiceManager.Objects.ServiceDefinition> filter)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var serviceDefinitions = SrmManagers.ServiceManager.GetServiceDefinitions(filter);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return serviceDefinitions;
			}

			[WrappedMethod("ServiceManager", "AddOrUpdateServiceDefinition")]
			public static Net.ServiceManager.Objects.ServiceDefinition AddOrUpdateServiceDefinition(Helpers helpers, Net.ServiceManager.Objects.ServiceDefinition serviceDefinition, bool force = false)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ServiceManager.AddOrUpdateServiceDefinition(serviceDefinition, force);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ServiceManager", "RemoveServiceDefinitions")]
			public static bool RemoveServiceDefinitions(Helpers helpers, out string error, params Net.ServiceManager.Objects.ServiceDefinition[] serviceDefinitions)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ServiceManager.RemoveServiceDefinitions(out error, serviceDefinitions);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}
		}
	}
}
