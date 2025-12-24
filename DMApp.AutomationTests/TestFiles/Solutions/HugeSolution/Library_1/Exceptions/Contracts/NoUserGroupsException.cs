namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Contracts
{
	using System;

	public class NoUserGroupsException : MediaServicesException
	{
		public NoUserGroupsException(string userNameOrCompanyName) : base("No User Groups found for " + userNameOrCompanyName)
		{
		}

		public NoUserGroupsException(string userNameOrCompanyName, Exception inner) : base("No User Groups found for " + userNameOrCompanyName, inner)
		{
		}
	}
}
