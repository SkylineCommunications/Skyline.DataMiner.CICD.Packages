namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.MediationSnippets.EricssonRX8200
{
	using System.Collections.Generic;
	using Skyline.DataMiner.MediationSnippets;

	public class ModulationStandardMappedToAuto : IMediator
	{
		private readonly Dictionary<string, int> profileParameterValueToProtocolParameterValue = new Dictionary<string, int>
		{
			{ "DVB-S",  3},
			{ "DVB-S2",  3},
			{ "DVB-S2X",  3},
			//{ "NS3",  N/A},
			//{ "NS4",  N/A},
		};

		private readonly Dictionary<int, string> protocolParameterValueToProfileParameterValue = new Dictionary<int, string>
		{
			{ 0, "DVB-S"},
			{ 2, "DVB-S2"},
			{ 3, "Auto"},
			{ 4, "DVB-S2X"},
		};

		public ParameterValue ConvertDeviceToProfile(IMediation mediation, ParameterSet setValue)
		{
			string stringValue = protocolParameterValueToProfileParameterValue[(int)setValue.Value.GetDoubleValue()];

			return new StringParameterValue(stringValue);
		}

		public ProfileToDeviceResult ConvertProfileToDevice(IMediation mediation, ParameterValue value)
		{
			var doubleValue = new DoubleParameterValue(profileParameterValueToProtocolParameterValue[value.GetStringValue()]);

			return new ProfileToDeviceResult(doubleValue);
		}
	}
}
