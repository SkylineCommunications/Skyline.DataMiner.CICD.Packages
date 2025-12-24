namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Linq; 

	public class ProfileParametersNotUpdatedException : Exception
	{
		public ProfileParametersNotUpdatedException(List<Function.Function> functions) : base($"Unable to update profile parameters for functions {string.Join(";", functions.Select(f => f.Name))}")
		{

		}
	}
}
