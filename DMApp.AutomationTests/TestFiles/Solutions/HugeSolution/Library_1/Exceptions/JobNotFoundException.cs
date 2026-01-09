namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	[Serializable]
	public class JobNotFoundException : Exception
	{
		public JobNotFoundException()
		{
		}

		public JobNotFoundException(Guid id)
			: base($"Unable to find Job with ID {id}")
		{
		}

		public JobNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
