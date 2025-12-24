namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class SatelliteTxUserTaskCreator : LiveUserTaskCreator
	{
		public SatelliteTxUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask>
			                            {
				                            { Descriptions.SatelliteTransmission.EquipmentConfiguration, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteTransmission.EquipmentConfiguration, UserGroup.McrOperator) }, 
				                            ////{ Descriptions.SatelliteTransmission.StartUplink, new UserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteTransmission.StartUplink, UserGroup.McrOperator) }, 
				                            ////{ Descriptions.SatelliteTransmission.StopUplink, new UserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteTransmission.StopUplink, UserGroup.McrOperator) }, 
				                            { Descriptions.SatelliteTransmission.SpaceNeeded, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteTransmission.SpaceNeeded, UserGroup.BookingOffice) },
				                            { Descriptions.SatelliteTransmission.VerifySatellite, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteTransmission.VerifySatellite, UserGroup.McrOperator) }
			                            };

			userTaskConditions = new Dictionary<string, bool>
			                          {
				                          { Descriptions.SatelliteTransmission.EquipmentConfiguration, true },
										  ////{ Descriptions.SatelliteTransmission.StartUplink, true },
										  ////{ Descriptions.SatelliteTransmission.StopUplink, RequiresSatTxStopUplinkUserTask() },
										  { Descriptions.SatelliteTransmission.SpaceNeeded, true },
				                          { Descriptions.SatelliteTransmission.VerifySatellite, ServiceHasSatelliteOther() && ServiceHasOtherSatelliteName() }
			                          };
		}

		private bool ServiceHasSatelliteOther()
		{
			Function.Function satelliteFunction = service.Functions.FirstOrDefault(f => f.Id == FunctionGuids.Satellite);
			if (satelliteFunction == null) throw new FunctionNotFoundException(FunctionGuids.Satellite);

			return satelliteFunction.Resource != null && satelliteFunction.Resource.Name == "Other";
		}

		private bool ServiceHasOtherSatelliteName()
		{
			var otherSatelliteNameParameter = parameters.FirstOrDefault(param => param.Name.Equals("Other Satellite Name", StringComparison.InvariantCultureIgnoreCase));
			return otherSatelliteNameParameter != null && !String.IsNullOrWhiteSpace(Convert.ToString(otherSatelliteNameParameter.Value));
		}

        //private bool RequiresSatTxStopUplinkUserTask()
        //{
        //    if (service == null) throw new ServiceNotFoundException(virtualPlatform: ServiceDefinition.VirtualPlatform.TransmissionSatellite);

        //    bool isSatTxStopUplinkUserTaskRequired = service.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.TransmissionSatellite && service.Start <= DateTime.Now;

        //    return isSatTxStopUplinkUserTaskRequired;
        //}
	}
}