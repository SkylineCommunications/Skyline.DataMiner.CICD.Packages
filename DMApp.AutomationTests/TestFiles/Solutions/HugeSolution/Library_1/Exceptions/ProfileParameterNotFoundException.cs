using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ProfileParameterNotFoundException : MediaServicesException
	{
		public ProfileParameterNotFoundException()
		{
		}

		public ProfileParameterNotFoundException(string name, string functionName = null, IEnumerable<ProfileParameter> collectionThatShouldContainProfileParameter = null)
			: base($"Unable to find Profile Parameter with name {name}{(!string.IsNullOrWhiteSpace(functionName) ? $" in function {functionName}" : string.Empty)}{(collectionThatShouldContainProfileParameter != null ? $" between {string.Join(",", collectionThatShouldContainProfileParameter.Select(p => p.Name))}" : string.Empty)}")
		{
		}

		public ProfileParameterNotFoundException(Guid ID, string functionName = null, IEnumerable<Guid> options = null)
			: base($"Unable to find Profile Parameter with ID {ID}{(!string.IsNullOrWhiteSpace(functionName) ? $" in function {functionName}" : string.Empty)}{(options != null ? $" between {string.Join(",", options)}" : string.Empty)}")
		{
		}

		public ProfileParameterNotFoundException(string name, Guid ID)
			: base($"Unable to find Profile Parameter with name {name} and ID {ID}")
		{
		}

		public ProfileParameterNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}