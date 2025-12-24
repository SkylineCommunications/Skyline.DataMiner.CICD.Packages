namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Text;

	public class ServiceDefinitionsForLiveOrderForm
	{
		public ConcurrentBag<ServiceDefinition> ReceptionServiceDefinitions { get; set; } = new ConcurrentBag<ServiceDefinition>();

		public ConcurrentBag<ServiceDefinition> DestinationServiceDefinitions { get; set; } = new ConcurrentBag<ServiceDefinition>();

		public ConcurrentBag<ServiceDefinition> RecordingServiceDefinitions { get; set; } = new ConcurrentBag<ServiceDefinition>();

		public ConcurrentBag<ServiceDefinition> TransmissionServiceDefinitions { get; set; } = new ConcurrentBag<ServiceDefinition>();

		public ServiceDefinition GraphicsProcessingServiceDefinition { get; set; }

		public ServiceDefinition AudioProcessingServiceDefinition { get; set; }

        public ServiceDefinition VideoProcessingServiceDefinition { get; set; }

		public ServiceDefinition RoutingServiceDefinition { get; set; }

		public ConcurrentBag<ServiceDefinition> VizremStudios { get; set; } = new ConcurrentBag<ServiceDefinition>();

		public ConcurrentBag<ServiceDefinition> VizremFarms { get; set; } = new ConcurrentBag<ServiceDefinition>();

		public ConcurrentBag<ServiceDefinition> VizremNC2Converters { get; set; } = new ConcurrentBag<ServiceDefinition>();

		public ConcurrentBag<ServiceDefinition> VizremNdiRouters { get; set; } = new ConcurrentBag<ServiceDefinition>();

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Service Definitions for LiveOrderForm:");
			foreach (var sd in ReceptionServiceDefinitions) sb.AppendLine($"- {sd.Name}");
			foreach (var sd in DestinationServiceDefinitions) sb.AppendLine($"- {sd.Name}");
			foreach (var sd in RecordingServiceDefinitions) sb.AppendLine($"- {sd.Name}");
			foreach (var sd in TransmissionServiceDefinitions) sb.AppendLine($"- {sd.Name}");
			if (GraphicsProcessingServiceDefinition != null) sb.AppendLine($"- {GraphicsProcessingServiceDefinition.Name}");
			if (AudioProcessingServiceDefinition != null) sb.AppendLine($"- {AudioProcessingServiceDefinition.Name}");
			if (VideoProcessingServiceDefinition != null) sb.AppendLine($"- {VideoProcessingServiceDefinition.Name}");
			foreach (var sd in VizremStudios) sb.AppendLine($"- {sd.Name}");

			return sb.ToString();
		}
	}
}
