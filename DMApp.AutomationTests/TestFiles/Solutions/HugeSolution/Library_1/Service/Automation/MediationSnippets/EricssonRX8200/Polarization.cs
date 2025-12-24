using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
	using Skyline.DataMiner.MediationSnippets;

	public class Polarization_EricssonRx8200 : IMediator
	{
		private readonly Dictionary<string, int> profileParameterValueToProtocolParameterValue = new Dictionary<string, int>
		{
			{ "X (Horizontal)",  1},
			{ "Y (Vertical)",  0},
		};

		private readonly Dictionary<int, string> protocolParameterValueToProfileParameterValue = new Dictionary<int, string>
		{
			{ 1, "X (Horizontal)"},
			{ 0, "Y (Vertical)"},
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
