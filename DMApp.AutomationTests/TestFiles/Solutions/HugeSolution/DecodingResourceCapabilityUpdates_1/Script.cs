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

namespace DecodingResourceCapabilityUpdates_1
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
		private static readonly Guid ResourceOutputConnectionsASICapabilityProfileId = Guid.Parse("3c08c3a7-d1fb-46f2-877b-8927345cf931");
		private static readonly Guid ResourceInputConnectionsASICapabilityProfileId = Guid.Parse("58a7e9e4-6a39-42c9-b275-92f9dc7f9003");
		private static readonly Guid ModulationStandardCapabilityProfileId = Guid.Parse("ed8446e9-e373-419d-9b9f-8a1c94ec8ae7");

		private const string MatrixOutputASIResourcePoolName = "Reception.Satellite.Matrix Output ASI";
		private const string DecodingResourcePoolName = "Reception.Satellite.Decoding";
		private const string DemodulatingResourcePoolName = "Reception.Satellite.Demodulating";

		private const string ns3ModulationStandard = "NS3";
		private const string ns4ModulationStandard = "NS4";

		public void Run(Engine engine)
		{
			var resourceManagerHelper = new ResourceManagerHelper();
			resourceManagerHelper.RequestResponseEvent += (s, e) => e.responseMessage = engine.SendSLNetSingleResponseMessage(e.requestMessage);

			var resourcesConnectedToMatrixOutputASI = GetResourcesConnectedToMatrixOutputAsiResource(resourceManagerHelper);
			if (!resourcesConnectedToMatrixOutputASI.Any()) return;

			var demodulatingResources = GetNs3DemodulatingResources(resourceManagerHelper);
			demodulatingResources.AddRange(GetTREDemodulatingResources(resourceManagerHelper));
			if (!demodulatingResources.Any()) return;

			UpdateDecodingResourcesWithCorrectLinksToDemodulatingResources(resourceManagerHelper, resourcesConnectedToMatrixOutputASI, demodulatingResources);
		}

		private HashSet<string> GetResourcesConnectedToMatrixOutputAsiResource(ResourceManagerHelper resourceManagerHelper)
		{
			var resourcesConnectedToMatrixOutputASI = new HashSet<string>();

			var matrixOutputASIResourcePool = resourceManagerHelper.GetResourcePools(new ResourcePool { Name = MatrixOutputASIResourcePoolName }).FirstOrDefault();
			if (matrixOutputASIResourcePool == null) return resourcesConnectedToMatrixOutputASI;

			var matrixOutputASIResources = resourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(matrixOutputASIResourcePool.ID));
			if (matrixOutputASIResources == null) return resourcesConnectedToMatrixOutputASI;

			foreach (var matrixOutputASIResource in matrixOutputASIResources)
			{
				var resourceOutputConnectionsASICapability = matrixOutputASIResource.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == ResourceOutputConnectionsASICapabilityProfileId);
				if (resourceOutputConnectionsASICapability?.Value?.Discreets == null) continue;

				foreach (var discreetValue in resourceOutputConnectionsASICapability.Value.Discreets)
				{
					if (String.IsNullOrEmpty(discreetValue)) continue;

					resourcesConnectedToMatrixOutputASI.Add(discreetValue);
				}
			}

			return resourcesConnectedToMatrixOutputASI;
		}

		private List<string> GetNs3DemodulatingResources(ResourceManagerHelper resourceManagerHelper)
		{
			var ns3DemodulatingResources = new HashSet<string>();

			var demodulatingResourcePool = resourceManagerHelper.GetResourcePools(new ResourcePool { Name = DemodulatingResourcePoolName }).FirstOrDefault();
			if (demodulatingResourcePool == null) return ns3DemodulatingResources.ToList();

			var demodulatingResources = resourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(demodulatingResourcePool.ID));
			if (demodulatingResources == null) return ns3DemodulatingResources.ToList();

			foreach (var demodulatingResource in demodulatingResources)
			{
				var modulatingStandardCapability = demodulatingResource.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == ModulationStandardCapabilityProfileId);
				if (modulatingStandardCapability?.Value?.Discreets == null) continue;

				foreach (var discreetValue in modulatingStandardCapability.Value.Discreets)
				{
					if (discreetValue == ns3ModulationStandard || discreetValue == ns4ModulationStandard) ns3DemodulatingResources.Add(demodulatingResource.Name);
				}
			}

			return ns3DemodulatingResources.ToList();
		}

		private List<string> GetTREDemodulatingResources(ResourceManagerHelper resourceManagerHelper)
		{
			var treDemodulatingResources = new HashSet<string>();

			var demodulatingResourcePool = resourceManagerHelper.GetResourcePools(new ResourcePool { Name = DemodulatingResourcePoolName }).FirstOrDefault();
			if (demodulatingResourcePool == null) return treDemodulatingResources.ToList();

			var demodulatingResources = resourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(demodulatingResourcePool.ID));
			if (demodulatingResources == null) return treDemodulatingResources.ToList();

			foreach (var demodulatingResource in demodulatingResources)
			{
				if (demodulatingResource.Name.Contains("TRE")) treDemodulatingResources.Add(demodulatingResource.Name);
			}

			return treDemodulatingResources.ToList();
		}

		private void UpdateDecodingResourcesWithCorrectLinksToDemodulatingResources(ResourceManagerHelper resourceManagerHelper, HashSet<string> resourcesConnectedToMatrixOutputASI, List<string> demodulatingResources)
		{
			var decodingResourcePool = resourceManagerHelper.GetResourcePools(new ResourcePool { Name = DecodingResourcePoolName }).FirstOrDefault();
			if (decodingResourcePool == null) return;

			var decodingResourcesToUpdate = new List<Resource>();
			var decodingResources = resourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(decodingResourcePool.ID));
			foreach (var decodingResource in decodingResources)
			{
				if (!resourcesConnectedToMatrixOutputASI.Contains(decodingResource.Name)) continue;

				var resourceInputConnectionsASICapability = decodingResource.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == ResourceInputConnectionsASICapabilityProfileId);
				if (resourceInputConnectionsASICapability?.Value == null) continue;

				bool updateRequired = false;
				foreach (var ns3DemodulatingResource in demodulatingResources)
				{
					if (!resourceInputConnectionsASICapability.Value.Discreets.Contains(ns3DemodulatingResource))
					{
						resourceInputConnectionsASICapability.Value.Discreets.Add(ns3DemodulatingResource);

						updateRequired = true;
					}
				}

				if (updateRequired) decodingResourcesToUpdate.Add(decodingResource);
			}

			if (decodingResourcesToUpdate.Any()) resourceManagerHelper.AddOrUpdateResources(true, decodingResourcesToUpdate.ToArray());
		}
	}
}