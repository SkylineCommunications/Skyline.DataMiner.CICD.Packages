namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class FunctionParameterNotFoundException : MediaServicesException
	{
		public FunctionParameterNotFoundException()
		{
		}

		public FunctionParameterNotFoundException(string name)
			: base($"Unable to find Function Parameter with name {name}")
		{
		}

		public FunctionParameterNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}