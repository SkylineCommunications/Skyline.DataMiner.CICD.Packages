using System;
using System.Runtime.Serialization;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	[Serializable]
	public class OrderNotFoundException : Exception
	{
		public OrderNotFoundException(Guid Id) : base($"Unable to find Order with ID {Id}")
		{
		}

		public OrderNotFoundException()
		{
		}

		public OrderNotFoundException(string message)
			: base(message)
		{
		}

		public OrderNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected OrderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
