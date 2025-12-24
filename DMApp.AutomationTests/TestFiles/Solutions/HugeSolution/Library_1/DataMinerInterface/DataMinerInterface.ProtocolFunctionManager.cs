namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System.Collections.Generic;
	using System.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;

	public static partial class DataMinerInterface
	{
		public static class ProtocolFunctionManager
		{
			[WrappedMethod("ProtocolFunctionManager", "GetAllProtocolFunctions")]
			public static List<ProtocolFunction> GetAllProtocolFunctions(Helpers helpers)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ProtocolFunctionManager.GetAllProtocolFunctions();

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ProtocolFunctionManager", "GetFunctionDefinition")]
			public static SystemFunctionDefinition GetFunctionDefinition(Helpers helpers, Net.FunctionDefinitionID id)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ProtocolFunctionManager.GetFunctionDefinition(id);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}
		}
	}
}
