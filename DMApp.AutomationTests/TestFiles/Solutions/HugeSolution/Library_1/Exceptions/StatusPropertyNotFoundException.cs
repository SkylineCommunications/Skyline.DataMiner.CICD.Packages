namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	[Serializable]
	public class StatusPropertyNotFoundNotFoundException : MediaServicesException
	{
		public StatusPropertyNotFoundNotFoundException()
		{
		}

		public StatusPropertyNotFoundNotFoundException(Guid ID)
			: base($"Unable to find status property for reservation with ID {ID}")
		{
		}

		public StatusPropertyNotFoundNotFoundException(string name, Guid ID)
			: base($"Unable to find status property for reservation with name {name} and ID {ID}")
		{
		}
	}
}