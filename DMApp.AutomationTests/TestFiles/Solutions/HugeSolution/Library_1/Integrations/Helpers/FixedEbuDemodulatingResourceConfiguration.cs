namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Helpers
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.FixedResourceHandlers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;
	using System;

	public class FixedEbuDemodulatingResourceConfiguration : IFixedResourceConfiguration
	{
		public FixedEbuDemodulatingResourceConfiguration(string resourceName)
		{
			if (String.IsNullOrWhiteSpace(resourceName)) throw new ArgumentNullException(nameof(resourceName));
			ResourceName = resourceName;
		}

		[MatchingFunctionResource(FunctionGuids.SatelliteString)]
		public string SatelliteName { get; set; }

		[MatchingProfileParameter(ProfileParameterGuids.Strings.DownlinkFrequencyString)]
		public double DownlinkFrequency { get; set; }

		[MatchingProfileParameter(ProfileParameterGuids.Strings.PolarizationString)]
		public string Polarity { get; set; }

		[MatchingProfileParameter(ProfileParameterGuids.Strings.ModulationStandardString)]
		public string ModulationStandard { get; set; }

		[MatchingProfileParameter(ProfileParameterGuids.Strings.SymbolRateString)]
		public double SymbolRate { get; set; }

		[MatchingProfileParameter(ProfileParameterGuids.Strings.ModulationString)]
		public string Modulation { get; set; }

		public Guid FunctionId { get; private set; } = FunctionGuids.MatrixOutputLband;

		public string ResourceName { get; private set; }
	}
}