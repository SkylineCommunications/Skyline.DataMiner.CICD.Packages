namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Text;
	using System.Threading.Tasks;

	[Serializable]
	public class ServiceDefinitionNotCreatedInTimeException : Exception
	{
		public ServiceDefinitionNotCreatedInTimeException()
		{
		}

		public ServiceDefinitionNotCreatedInTimeException(string message) : base(message)
		{
		}

		public ServiceDefinitionNotCreatedInTimeException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ServiceDefinitionNotCreatedInTimeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
