
using System;
using System.Collections.Generic;
using System.Text;
using Skyline.DataMiner.MediationSnippets;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
	using Skyline.DataMiner.MediationSnippets;
	internal class FecNovelsatNs2000 : IMediator
	{
		private readonly Dictionary<string, int> profileParameterValueToProtocolParameterValue = new Dictionary<string, int>
		{
			{ "1/2",  9},
			{ "1/3",  3},
			{ "1/4",  1},
			{ "1/5",  0},
			{ "2/3",  17},
			{ "2/5",  4},
			{ "3/4",  20},
			{ "3/5",  14},
			{ "4/5",  23},
			{ "5/6",  26},
			{ "8/9",  29},
			{ "9/10",  30},
			{ "19/30",  16},
			{ "32/45",  25},
		};

		private readonly Dictionary<int, string> protocolParameterValueToProfileParameterValue = new Dictionary<int, string>
		{
			{ 0, "1/5"},
			{ 1 , "1/4"},
			{ 2 , "1/4 Short"},
			{ 3 , "1/3"},
			{ 4 , "2/5"},
			{ 5 , "13/30"},
			{ 6 , "4/9"},
			{ 7 , "7/15"},
			{ 8 , "22/45"},
			{ 9 , "1/2"},
			{ 10 , "1/2 Short"},
			{ 11 , "8/15"},
			{ 12 , "5/9"},
			{ 13 , "17/30"},
			{ 14 , "3/5"},
			{ 15 , "28/45"},
			{ 16 , "19/30"},
			{ 17 , "2/3"},
			{ 18 , "32/45"},
			{ 19 , "11/15"},
			{ 20 , "3/4"},
			{ 21 , "3/4 Short"},
			{ 22 , "7/9"},
			{ 23 , "4/5"},
			{ 24 , "4/5 Short"},
			{ 25 , "32/45"},
			{ 26 , "5/6"},
			{ 27 , "5/6 Short"},
			{ 28 , "7/8"},
			{ 29 , "8/9"},
			{ 30 , "9/10"},
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
