namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.MediationSnippets.NovelsatNS2000
{
	using System.Collections.Generic;
	using Skyline.DataMiner.MediationSnippets;

	internal class ModulationStandardNovelsetNs2000 : IMediator
	{ 
		private readonly Dictionary<string, int> profileParameterValueToProtocolParameterValue = new Dictionary<string, int>
		{
			{ "DVB-S2", 2},
			{ "NS3",  3},
			{ "NS4",  4},
		};

		private readonly Dictionary<int, string> protocolParameterValueToProfileParameterValue = new Dictionary<int, string>
		{
			{ 2, "DVB-S2"},
			{ 3, "NS3"},
			{ 4, "NS4"},
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
