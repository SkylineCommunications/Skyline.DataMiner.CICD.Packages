namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Helpers
{
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.FixedResourceHandlers;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class FixedEbuSatRxResourceHandler : FixedResourceHandler<FixedEbuDemodulatingResourceConfiguration>
	{
		public FixedEbuSatRxResourceHandler(Helpers helpers) : base(helpers)
		{

		}

		protected override List<FixedEbuDemodulatingResourceConfiguration> ResourceConfigurations => new List<FixedEbuDemodulatingResourceConfiguration>
		{
			new FixedEbuDemodulatingResourceConfiguration("ETL Main Output 35.NS3 03") // NS3 03.Demodulating
			{
				SatelliteName = "Eutelsat 10A (E10A)",
				DownlinkFrequency = 11220.83,
				Polarity = "X (Horizontal)",
				ModulationStandard = "NS4",
				Modulation = "16APSK",
				SymbolRate = 35.294118
			},
			new FixedEbuDemodulatingResourceConfiguration("ETL Main Output 66.NS3 08") // NS3 08.Demodulating
			{
				SatelliteName = "Eutelsat 10A (E10A)",
				DownlinkFrequency = 11262.5,
				Polarity = "Y (Vertical)",
				ModulationStandard = "NS4",
				Modulation = "16APSK",
				SymbolRate = 35.294118
			}
		};

		protected override Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform VirtualPlatform => ServiceDefinition.VirtualPlatform.ReceptionSatellite;
	}
}