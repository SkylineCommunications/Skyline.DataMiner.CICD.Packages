namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class FieldDescriptorNotFoundException : MediaServicesException
	{
		public FieldDescriptorNotFoundException()
		{
		}

		public FieldDescriptorNotFoundException(string message) : base(message)
		{
		}

		public FieldDescriptorNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}
