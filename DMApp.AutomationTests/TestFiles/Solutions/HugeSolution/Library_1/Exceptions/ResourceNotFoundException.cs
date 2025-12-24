namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ResourceNotFoundException : Exception
	{
		public ResourceNotFoundException()
		{
		}

		public ResourceNotFoundException(string name)
			: base($"Unable to find Resource with name {name}")
		{
		}

		public ResourceNotFoundException(Guid ID)
			: base($"Unable to find Resource with ID {ID}")
		{
		}

		public ResourceNotFoundException(string name, Guid ID)
			: base($"Unable to find Resource with ID {ID} and name {name}")
		{
		}

		public ResourceNotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}