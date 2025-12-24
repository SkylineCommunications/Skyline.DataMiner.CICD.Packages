namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Contracts
{
	using System;

	public class NoContractsException : MediaServicesException
	{
		public NoContractsException(string userNameOrCompanyName) : base("No Contracts found for " + userNameOrCompanyName)
		{
		}

		public NoContractsException(string userNameOrCompanyName, Exception inner) : base("No Contracts found for " + userNameOrCompanyName, inner)
		{
		}
	}
}
