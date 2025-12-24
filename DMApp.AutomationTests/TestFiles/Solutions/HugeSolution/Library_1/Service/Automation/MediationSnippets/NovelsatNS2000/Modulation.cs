namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.MediationSnippets.NovelsatNS2000
{
	using System.Collections.Generic;
	using Skyline.DataMiner.MediationSnippets;

	internal class ModulationNovelsetNs2000 : IMediator
	{
		private readonly Dictionary<string, int> profileParameterValueToProtocolParameterValue = new Dictionary<string, int>
		{
			{ "QPSK", 1},
			{ "8PSK", 2},
			{ "16APSK", 4},
			{ "32APSK", 5},
		};

		private readonly Dictionary<int, string> protocolParameterValueToProfileParameterValue = new Dictionary<int, string>
		{
			{ 0, "BPSK"},
			{ 1, "QPSK"},
			{ 2, "8PSK"},
			{ 3, "16QAM"},
			{ 4, "16APSK"},
			{ 5, "32APSK"},
			{ 6, "64APSK"},
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
