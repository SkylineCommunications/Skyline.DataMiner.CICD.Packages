namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class SectionNotFoundException : MediaServicesException
	{
		public SectionNotFoundException(string message) : base(message)
		{
		}

		public SectionNotFoundException(Guid serviceID)
			: base($"Unable to find Section for Service with ID {serviceID}")
		{
		}

		public SectionNotFoundException(Guid serviceID, Exception inner)
			: base($"Unable to find Section for Service with ID {serviceID}", inner)
		{
		}
	}
}