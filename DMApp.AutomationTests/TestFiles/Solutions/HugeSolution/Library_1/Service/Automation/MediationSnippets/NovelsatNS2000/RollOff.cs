
using System;
using System.Collections.Generic;
using System.Text;
using Skyline.DataMiner.MediationSnippets;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
	using Skyline.DataMiner.MediationSnippets;
	internal class RollOffNovelsatNs2000 : IMediator
	{
		private readonly Dictionary<string, int> profileParameterValueToProtocolParameterValue = new Dictionary<string, int>
		{
			{ "5%",  0},
			{ "10%",  1},
			{ "15%",  2},
			{ "20%",  3},
			{ "25%",  4},
            //{ "30%",  0},
            { "35%",  5},
		};

		private readonly Dictionary<int, string> protocolParameterValueToProfileParameterValue = new Dictionary<int, string>
		{
			{ 0, "5%"},
			{ 1, "10%"},
			{ 2 , "15%"},
			{ 3 , "20%"},
			{ 4 , "25%"},
            //{ 0 , "30%"},
            { 5 , "35%"},
            { 6 , "2%"},
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
