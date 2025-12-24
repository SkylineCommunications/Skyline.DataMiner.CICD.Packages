namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.MediationSnippets.EricssonRX8200
{
	using System.Collections.Generic;
	using Skyline.DataMiner.MediationSnippets;

	public class InputSource : IMediator
	{
		private readonly Dictionary<int, string> profileParameterValueToProtocolParameterValue = new Dictionary<int, string>
		{
			{ 0,  "ASI (0)"},
			{ 1,  "SAT (1)"},
		};

		private readonly Dictionary<int, int> protocolParameterValueToProfileParameterValue = new Dictionary<int, int>
		{
			{ -1, -1},
			{ 0, 0},
			{ 1, 1},
		};

		public ParameterValue ConvertDeviceToProfile(IMediation mediation, ParameterSet setValue)
		{
			int intValue = protocolParameterValueToProfileParameterValue[(int)setValue.Value.GetDoubleValue()];

			return new DoubleParameterValue(intValue);
		}

		public ProfileToDeviceResult ConvertProfileToDevice(IMediation mediation, ParameterValue value)
		{
			var stringValue = new StringParameterValue(profileParameterValueToProtocolParameterValue[(int)value.GetDoubleValue()]);

			return new ProfileToDeviceResult(stringValue);
		}
	}
}
