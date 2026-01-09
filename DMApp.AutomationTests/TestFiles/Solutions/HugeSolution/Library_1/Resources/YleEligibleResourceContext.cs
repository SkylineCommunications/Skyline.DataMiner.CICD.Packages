namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class YleEligibleResourceContext : EligibleResourceContext
	{
		public YleEligibleResourceContext(string functionDefinitionLabel, Guid resourcePoolId)
		{
			FunctionDefinitionLabel = functionDefinitionLabel ?? throw new ArgumentNullException(nameof(functionDefinitionLabel));
			ResourcePoolId = resourcePoolId;
		}

		public string FunctionDefinitionLabel { get; }

		public Guid ResourcePoolId { get; }

		public EligibleResourceContext GetContextForGetEligibleResourceCall()
		{
			return new EligibleResourceContext
			{
				ContextId = ContextId,
				TimeRange = TimeRange,
				ResourceFilter = ResourceFilter ?? ResourceExposers.PoolGUIDs.Contains(ResourcePoolId),
				RequiredCapabilities = RequiredCapabilities,
				ReservationIdToIgnore = ReservationIdToIgnore,
				NodeIdToIgnore = NodeIdToIgnore,
			};
		}

		public bool CanBeUsedInsteadOf(YleEligibleResourceContext other)
		{
			if (other is null) return false;

			bool functionDefinitionLabelIsEqual = FunctionDefinitionLabel == other.FunctionDefinitionLabel;

			bool timeRangesAreNull = TimeRange == null && other.TimeRange == null;
			bool timeRangesAreEqual = TimeRange != null && TimeRange.Equals(other.TimeRange);
			bool timeRangeIsEqual = timeRangesAreNull || timeRangesAreEqual;

			bool capabilitiesAreEqual = (RequiredCapabilities == null && other.RequiredCapabilities == null) || (RequiredCapabilities != null && RequiredCapabilities.ScrambledEquals(other.RequiredCapabilities));

			bool serviceIdIsEqual = (ReservationIdToIgnore == null && other.ReservationIdToIgnore == null) || (ReservationIdToIgnore != null && ReservationIdToIgnore.Equals(other.ReservationIdToIgnore));

			bool nodeIdIsEqual = NodeIdToIgnore == other.NodeIdToIgnore;

			bool resourcePoolIdIsEqual = ResourcePoolId == other.ResourcePoolId;

			return functionDefinitionLabelIsEqual && timeRangeIsEqual && capabilitiesAreEqual && serviceIdIsEqual && nodeIdIsEqual && resourcePoolIdIsEqual;
		}

		public override string ToString()
		{
			return $"Context for function definition label {FunctionDefinitionLabel},\nResource pool ID {ResourcePoolId},\nfrom {TimeRange.Start} ({TimeRange.Start.Kind}) until {TimeRange.Stop} ({TimeRange.Start.Kind}),\nignoring service '{ReservationIdToIgnore?.Id}' node ID '{NodeIdToIgnore}'\nfor capabilities '{string.Join(";", RequiredCapabilities.Select(c => $"{c.CapabilityProfileID}={c.RequiredString}"))}'";
		}
	}
}
