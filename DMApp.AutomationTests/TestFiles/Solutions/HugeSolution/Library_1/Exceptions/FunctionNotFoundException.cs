using System.Collections.Generic;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class FunctionNotFoundException : MediaServicesException
	{
		public FunctionNotFoundException() : base("Unable to find Function")
		{
		}

		public FunctionNotFoundException(string name, IEnumerable<string> options, bool labelInsteadOfName = false)
			: base($"Unable to find Function with {(labelInsteadOfName ? "label" : "name")} {name} among options {string.Join(", ", options)}")
		{
		}

		public FunctionNotFoundException(Guid ID)
			: base($"Unable to find Function with ID {ID}")
		{
		}

		public FunctionNotFoundException(int NodeId)
			: base($"Unable to find Function with Node ID {NodeId}")
		{
		}

		public FunctionNotFoundException(string name, Guid ID)
			: base($"Unable to find Function with name {name} and ID {ID}")
		{
		}

		public FunctionNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public FunctionNotFoundException(string message) : base(message)
		{
		}
	}
}