using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	public class EventNotFoundException : Exception
	{
		public EventNotFoundException(string name)
			: base($"Unable to find Event with name {name}")
		{
		}

		public EventNotFoundException(Guid ID)
			: base($"Unable to find Event with ID '{ID}'")
		{
		}

		public EventNotFoundException(string name, Guid ID)
			: base($"Unable to find Event with ID {ID} and name {name}")
		{
		}

		public EventNotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
