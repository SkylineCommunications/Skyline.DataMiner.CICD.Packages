using Skyline.DataMiner.Net.Messages;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile
{
	using System;
	using System.Collections.Generic;

	public interface IProfileManager
	{
		void PrepareCache();

		ProfileDefinition GetProfileDefinition(Guid id);

		Dictionary<Guid, ProfileDefinition> GetInterfaceProfileDefinitions(FunctionDefinition functionDefinition);

		ProfileParameter GetProfileParameter(Guid guid);

		ProfileParameter GetProfileParameter(string profileParameterName);
	}
}
