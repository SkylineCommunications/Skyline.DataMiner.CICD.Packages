namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IAutomationConfiguration
    {
        /// <summary>
        /// Virtual platform of the service that has functions to be automatically configured.
        /// </summary>
        VirtualPlatform VirtualPlatform { get; }

        /// <summary>
        /// IDs of the functions that required automatic configuration.
        /// </summary>
        IReadOnlyList<Guid> AutomatedFunctions { get; }

        /// <summary>
        /// Descriptions of the user tasks that should trigger the automatic configuration.
        /// </summary>
        IReadOnlyList<string> UserTaskTriggers { get; }
    }

    public class SatellliteRxAutomationConfiguration : IAutomationConfiguration
    {
        public VirtualPlatform VirtualPlatform => VirtualPlatform.ReceptionSatellite;

        public IReadOnlyList<Guid> AutomatedFunctions => new List<Guid>
        {
            FunctionGuids.Demodulating, 
            FunctionGuids.Decoding
        };

        public IReadOnlyList<string> UserTaskTriggers => new List<string>
        {
            Descriptions.SatelliteReception.ConfigureIrd
        };
    }
}
