namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class ProfileParametersAndResourcesNotUpdatedException : Exception
	{
		public ProfileParametersAndResourcesNotUpdatedException(List<Function.Function> functions) : base($"Unable to update profile parameters and resource for functions {string.Join(";", functions.Select(f => f.Name))}")
		{

		}
	}
}
