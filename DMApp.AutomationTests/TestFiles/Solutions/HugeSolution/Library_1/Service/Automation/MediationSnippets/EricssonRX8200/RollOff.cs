using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
    using Skyline.DataMiner.MediationSnippets;

    public class RollOffEricssonRx8200 : IMediator
    {
        private readonly Dictionary<string, int> profileParameterValueToProtocolParameterValue = new Dictionary<string, int>
        {
            { "5%",  3},
            { "10%",  4},
            { "15%",  5},
            { "20%",  0},
            { "25%",  2},
            //{ "30%",  0},
            { "35%",  1},
        };

        private readonly Dictionary<int, string> protocolParameterValueToProfileParameterValue = new Dictionary<int, string>
        {
            { 3, "5%"},
            { 4, "10%"},
            { 5 , "15%"},
            { 0 , "20%"},
            { 2 , "25%"},
            //{ 0 , "30%"},
            { 1 , "35%"},
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
