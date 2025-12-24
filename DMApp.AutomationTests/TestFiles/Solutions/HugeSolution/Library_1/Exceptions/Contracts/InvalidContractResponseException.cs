namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Contracts
{
	using System;

	public class InvalidContractResponseException : MediaServicesException
	{
		public InvalidContractResponseException() : base("The Contract Manager did not provide a valid response.")
		{
		}

		public InvalidContractResponseException(string message, Exception inner) : base("The Contract Manager did not provide a valid response.", inner)
		{
		}
	}
}
