namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
	using System.Collections.Generic;
	using Skyline.DataMiner.MediationSnippets;

	public class EncryptionType_EricssonRx8200 : IMediator
	{
		private readonly Dictionary<string, int> profileParameterValueToProtocolParameterValue = new Dictionary<string, int>
		{
			{ "BISS-1",  0},
			{ "BISS-E",  3},
			{ "BISS-E LA LIGA",  4},
		};

		private readonly Dictionary<int, string> protocolParameterValueToProfileParameterValue = new Dictionary<int, string>
		{
			{ 0, "BISS-1"},
			{ 1, "Mode E Fixed"},
			{ 2, "Mode E TTV"},
			{ 3, "BISS-E"},
			{ 4, "BISS-E LA LIGA"},
			{ -1, "N/A" },
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
