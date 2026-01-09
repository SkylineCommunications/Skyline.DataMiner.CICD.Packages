namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class FunctionDefinitionNotFoundException : MediaServicesException
	{
		public FunctionDefinitionNotFoundException()
		{
		}

		public FunctionDefinitionNotFoundException(string name)
			: base($"Unable to find Function Definition with name {name}")
		{
		}

		public FunctionDefinitionNotFoundException(Guid ID)
			: base($"Unable to find Function Definition with ID {ID}")
		{
		}

		public FunctionDefinitionNotFoundException(string name, Guid ID)
			: base($"Unable to find Function Definition with name {name} and ID {ID}")
		{
		}

		public FunctionDefinitionNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}