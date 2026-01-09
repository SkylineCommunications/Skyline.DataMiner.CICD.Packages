using System.Diagnostics;

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

	using Service = Service.Service;

	public class SatelliteRxUserTaskCreator : LiveUserTaskCreator
	{
		private const string SatelliteTruck = "Satelliittiauto";

		public SatelliteRxUserTaskCreator(Helpers helpers, Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			userTaskConstructors = new Dictionary<string, UserTask>
			                       {
				                       { Descriptions.SatelliteReception.SelectAntenna, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteReception.SelectAntenna, UserGroup.McrOperator) },
				                       { Descriptions.SatelliteReception.ConfigureIrd, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteReception.ConfigureIrd, UserGroup.McrOperator) },
				                       { Descriptions.SatelliteReception.SpaceNeeded, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteReception.SpaceNeeded, UserGroup.BookingOffice) },
				                       { Descriptions.SatelliteReception.SteerAntenna, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteReception.SteerAntenna, UserGroup.McrOperator) },
				                       { Descriptions.SatelliteReception.ConfigureNs3, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteReception.ConfigureNs3, UserGroup.McrOperator) },
				                       { Descriptions.SatelliteReception.ConfigureNs4, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteReception.ConfigureNs4, UserGroup.McrOperator) },
				                       { Descriptions.SatelliteReception.VerifySatellite, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.SatelliteReception.VerifySatellite, UserGroup.McrOperator) }
			                       };

			userTaskConditions = new Dictionary<string, bool>
			                     {
				                     { Descriptions.SatelliteReception.SelectAntenna, SelectAntennaUserTaskRequired()},
				                     { Descriptions.SatelliteReception.ConfigureIrd, true },
				                     { Descriptions.SatelliteReception.SpaceNeeded, EventHasCeitonResource(SatelliteTruck) },
				                     { Descriptions.SatelliteReception.SteerAntenna, ServiceHasSatelliteOther() || ServiceHasNoSatelliteResource() || ServiceHasSteerableOrNoAntenna() },
				                     { Descriptions.SatelliteReception.ConfigureNs3, ServiceHasModulationStandardNs3() },
				                     { Descriptions.SatelliteReception.ConfigureNs4, ServiceHasModulationStandardNs4() },
				                     { Descriptions.SatelliteReception.VerifySatellite, ServiceHasSatelliteOther() && ServiceHasOtherSatelliteName() }
			                     };

			stopwatch.Stop();
			Log(nameof(SatelliteRxUserTaskCreator),$"Constructor elapsed time = {stopwatch.Elapsed}",service.Name);
		}

		private bool SelectAntennaUserTaskRequired()
		{
			bool userTaskRequired = ServiceHasSatelliteOther() || ServiceHasNoSatelliteResource() || ServiceHasSteerableOrNoAntenna();

			Log(nameof(SelectAntennaUserTaskRequired), $"{Descriptions.SatelliteReception.SelectAntenna} user task {(userTaskRequired ? string.Empty : "not ")} required", service.Name);

			return userTaskRequired;
		}

		private bool ServiceHasNoSatelliteResource()
		{
			var satelliteFunction = service.Functions.FirstOrDefault(f => f.Id == FunctionGuids.Satellite);
			if (satelliteFunction == null) throw new FunctionNotFoundException(FunctionGuids.Satellite);

			bool serviceHasNoSatelliteResource = satelliteFunction.Resource == null;

			Log(nameof(ServiceHasNoSatelliteResource), $"Service has {(serviceHasNoSatelliteResource ? "no" : "a")} satellite resource.");

			return serviceHasNoSatelliteResource;
		}

		private bool ServiceHasSatelliteOther()
		{
			Function.Function satelliteFunction = service.Functions.FirstOrDefault(f => f.Id == FunctionGuids.Satellite);
			if (satelliteFunction == null) throw new FunctionNotFoundException(FunctionGuids.Satellite);

			return satelliteFunction.Resource != null && satelliteFunction.Resource.Name == "Other";
		}

        private bool ServiceHasSteerableOrNoAntenna()
        {
            Function.Function antennaFunction = service.Functions.FirstOrDefault(f => f.Id == FunctionGuids.Antenna);
            if (antennaFunction == null) throw new FunctionNotFoundException(FunctionGuids.Antenna);

            var antennaResource = antennaFunction.Resource;
            if (antennaResource == null) return true;

            var orbitalPositionCapability = antennaResource.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == ProfileParameterGuids._OrbitalPosition);
            if (orbitalPositionCapability == null) throw new FunctionParameterNotFoundException("_Orbital Position");

            return orbitalPositionCapability.IsTimeDynamic;
        }

		private bool ServiceHasOtherSatelliteName()
		{
			var otherSatelliteNameParameter = parameters.FirstOrDefault(param => param.Name.Equals("Other Satellite Name", StringComparison.InvariantCultureIgnoreCase));
			return otherSatelliteNameParameter != null && !String.IsNullOrWhiteSpace(Convert.ToString(otherSatelliteNameParameter.Value));
		}

		private bool ServiceHasModulationStandardNs3()
		{
			var modulationStandardParameter = parameters.FirstOrDefault(param => param.Id == ProfileParameterGuids.ModulationStandard);
			return modulationStandardParameter != null && Convert.ToString(modulationStandardParameter.Value) == "NS3";
		}

		private bool ServiceHasModulationStandardNs4()
		{
			var modulationStandardParameter = parameters.FirstOrDefault(param => param.Id == ProfileParameterGuids.ModulationStandard);
			return modulationStandardParameter != null && Convert.ToString(modulationStandardParameter.Value) == "NS4";
		}
	}
}