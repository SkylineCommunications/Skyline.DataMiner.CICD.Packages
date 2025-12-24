namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
    using Skyline.DataMiner.Utils.YLE.Integrations;
    using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class UnsupportedIntegrationException : Exception
	{
		public UnsupportedIntegrationException()
		{
		}

		public UnsupportedIntegrationException(IntegrationType type)
			: base($"Unsupported integration: {type}")
		{
		}

		public UnsupportedIntegrationException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected UnsupportedIntegrationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
