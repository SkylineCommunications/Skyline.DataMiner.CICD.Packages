/*
****************************************************************************
*  Copyright (c) 2021,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2021	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace MatrixOutputLbandResourceCapabilityUpdates_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script
	{
		private static readonly Guid ResourceInputConnectionsLBandCapabilityProfileId = Guid.Parse("5c8ea48d-fafb-4fae-9f43-8ce2e9347d90");

		private const string MatrixOutputLBandResourcePoolName = "Reception.Satellite.Matrix Output LBAND";
		private const string AntennaResourcePoolName = "Reception.Satellite.Antenna";

		public void Run(Engine engine)
		{
			var resourceManagerHelper = new ResourceManagerHelper();
			resourceManagerHelper.RequestResponseEvent += (s, e) => e.responseMessage = engine.SendSLNetSingleResponseMessage(e.requestMessage);

			var antennaResources = GetAntennaResources(resourceManagerHelper);
			if (!antennaResources.Any()) return;

			UpdatedMatrixOutputLBandResources(resourceManagerHelper, antennaResources);
		}

		private HashSet<string> GetAntennaResources(ResourceManagerHelper resourceManagerHelper)
		{
			var antennaResourcePool = resourceManagerHelper.GetResourcePools(new ResourcePool { Name = AntennaResourcePoolName }).FirstOrDefault();
			if (antennaResourcePool == null) return new HashSet<string>();

			var antennaResources = resourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(antennaResourcePool.ID));
			if (antennaResources == null) return new HashSet<string>();

			return new HashSet<string>(antennaResources.Select(a => a.Name));
		}

		private void UpdatedMatrixOutputLBandResources(ResourceManagerHelper resourceManagerHelper, HashSet<string> antennaResources)
		{
			var treAntennaResources = antennaResources.Where(r => r.Contains("TRE")).ToList();
			var otherAntennaResources = antennaResources.Except(treAntennaResources).ToList();

			var matrixOutputLBandResourcePool = resourceManagerHelper.GetResourcePools(new ResourcePool { Name = MatrixOutputLBandResourcePoolName }).FirstOrDefault() ?? throw new InvalidOperationException($"Unable to find resource pool with name {MatrixOutputLBandResourcePoolName}");

			var matrixOutputLBandResourcesToUpdate = new List<Resource>();
			var matrixOutputLBandResources = resourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(matrixOutputLBandResourcePool.ID));
			foreach (var matrixOutputLBandResource in matrixOutputLBandResources)
			{
				// DiSEqC resources are skipped
				if (!matrixOutputLBandResource.Name.Contains("ETL")) continue;

				var resourceInputConnectionsLBandCapability = matrixOutputLBandResource.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == ResourceInputConnectionsLBandCapabilityProfileId);
				if (resourceInputConnectionsLBandCapability?.Value == null) continue;

				var expectedCapabilityDiscreets = matrixOutputLBandResource.Name.Contains("TRE") ? treAntennaResources : otherAntennaResources;

				bool updateRequired = false;
				foreach (var expectedCapabilityDiscreet in expectedCapabilityDiscreets)
				{
					if (!resourceInputConnectionsLBandCapability.Value.Discreets.Contains(expectedCapabilityDiscreet))
					{
						resourceInputConnectionsLBandCapability.Value.Discreets.Add(expectedCapabilityDiscreet);

						updateRequired = true;
					}
				}

				if (updateRequired) matrixOutputLBandResourcesToUpdate.Add(matrixOutputLBandResource);
			}

			if (matrixOutputLBandResourcesToUpdate.Any()) resourceManagerHelper.AddOrUpdateResources(true, matrixOutputLBandResourcesToUpdate.ToArray());
		}
	}
}