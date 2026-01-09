namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public static partial class DataMinerInterface
	{
		public static class ProfileHelper
		{
			[WrappedMethod("ResourceManager", "ProfileDefinitions.ReadAll")]
			public static List<Net.Profiles.ProfileDefinition> ReadAllProfileDefinitions(Helpers helpers)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ProfileHelper.ProfileDefinitions.ReadAll();

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "ProfileDefinitions.Read")]
			public static List<Net.Profiles.ProfileDefinition> ReadProfileDefinitions(Helpers helpers, FilterElement<Net.Profiles.ProfileDefinition> filter)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ProfileHelper.ProfileDefinitions.Read(filter);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "ProfileParameters.ReadAll")]
			public static List<Net.Profiles.Parameter> ReadAllProfileParameters(Helpers helpers)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ProfileHelper.ProfileParameters.ReadAll();

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}
		}
	}
}
