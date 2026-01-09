/*
****************************************************************************
*  Copyright (c) 2022,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2022	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Linq;
using MigrateResources_1;
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Library.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Common;
using YleServiceConfiguration = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.ServiceConfiguration;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	public static void Run(Engine engine)
	{
		engine.Timeout = TimeSpan.FromHours(10);

		Helpers helpers = null;
		try
		{
			helpers = new Helpers(engine, Scripts.MigrateResourcesYLE);

			var oldElementId = engine.GetScriptParam("oldElementId").Value;
			var newElementId = engine.GetScriptParam("newElementId").Value;
			helpers.Log("Script", "Run", $"Migrating resources from element {oldElementId} to {newElementId}");

			var migrationResult = MigrateElementResources(helpers, oldElementId, newElementId);
			if (migrationResult.IsSuccessful)
			{
				helpers.Log("Script", "Run", $"Element resource migration succeeded (removing old element)");
				RemoveElement(helpers, oldElementId);
			}
			else
			{
				helpers.Log("Script", "Run", $"Element resource migration failed (partially): {migrationResult.ErrorMessage}");
			}

			UpdateServiceConfigurations(helpers, migrationResult);

			engine.AddOrUpdateScriptOutput("ElementResourceMigrationResult", JsonConvert.SerializeObject(migrationResult, Formatting.None));
		}
		catch (Exception e)
		{
			helpers?.Log("Script", "Run", $"Exception: {e}");
		}
		finally
		{
			helpers?.Dispose();
		}
	}

	public static ElementResourceMigrationResult MigrateElementResources(Helpers helpers, string oldElementId, string newElementId)
	{
		helpers.LogMethodStart("Script", "MigrateElementResources", out var stopwatch);

		var migrateResourcesSubScript = helpers.Engine.PrepareSubScript("MigrateResources");
		migrateResourcesSubScript.Synchronous = true;
		migrateResourcesSubScript.SelectScriptParam("oldElementId", oldElementId);
		migrateResourcesSubScript.SelectScriptParam("newElementId", newElementId);
		migrateResourcesSubScript.StartScript();

		var result = migrateResourcesSubScript.GetScriptResult()["ElementResourceMigrationResult"];
		helpers.Engine.ClearScriptOutput("ElementResourceMigrationResult");

		helpers.Log("Script", "MigrateElementResources", $"ElementResourceMigrationResult: {result}");

		helpers.LogMethodCompleted("Script", "MigrateElementResources", null, stopwatch);

		return JsonConvert.DeserializeObject<ElementResourceMigrationResult>(result);
	}

	public static void UpdateServiceConfigurations(Helpers helpers, ElementResourceMigrationResult result)
	{
		helpers.LogMethodStart("Script", "UpdateServiceConfigurations", out var stopwatch);

		try
		{
			var serviceConfigurationsToUpdate = GetServiceConfigurationsToUpdate(helpers, result);
			helpers.Log("Script", "UpdateServiceConfigurations", $"{serviceConfigurationsToUpdate.Count} service configurations require an update");

			foreach (var updatedServiceConfiguration in serviceConfigurationsToUpdate)
			{
				helpers.Log("Script", "UpdateServiceConfigurations", $"Updating service configuration for order {updatedServiceConfiguration.Key}");

				var serializedServiceConfigurations = JsonConvert.SerializeObject(updatedServiceConfiguration.Value, Formatting.None);
				var succeeded = helpers.OrderManagerElement.AddOrUpdateServiceConfigurations(updatedServiceConfiguration.Key, DateTime.MaxValue, serializedServiceConfigurations);
				helpers.Log("Script", "UpdateServiceConfigurations", $"Service configuration update {(succeeded ? "succeeded" : "failed")}");
			}
		}
		catch (Exception e)
		{
			helpers.Log("Script", "UpdateServiceConfigurations", $"Exception updating service configurations: {e}");
		}
		finally
		{
			helpers.LogMethodCompleted("Script", "UpdateServiceConfigurations", null, stopwatch);
		}
	}

	public static void RemoveElement(Helpers helpers, string elementId)
	{
		try
		{
			var dms = Engine.SLNetRaw.GetDms();
			var element = dms.GetElement(new DmsElementId(elementId));
			element.Delete();
		}
		catch (Exception e)
		{
			helpers.Log("Script", "RemoveElement", $"Exception removing element {elementId}: {e}");
		}
	}

	private static Dictionary<Guid, Dictionary<int, YleServiceConfiguration>> GetServiceConfigurationsToUpdate(Helpers helpers, ElementResourceMigrationResult result)
	{
		var serviceConfigurationsPerOrderId = new Dictionary<Guid, Dictionary<int, YleServiceConfiguration>>();

		foreach (var resourceMigrationResult in result.ResourceMigrationResults.Where(r => r.IsSuccessful))
		{
			UpdateServiceConfigurationForResourceMigration(helpers, resourceMigrationResult, serviceConfigurationsPerOrderId);
		}

		return serviceConfigurationsPerOrderId;
	}

	private static void UpdateServiceConfigurationForResourceMigration(Helpers helpers, ResourceMigrationResult result, Dictionary<Guid, Dictionary<int, YleServiceConfiguration>> serviceConfigurationsPerOrderId)
	{
		foreach (var reservationInstanceResourceAssignmentResult in result.ReservationInstanceResourceAssignmentResults.Where(r => r.IsSuccessful))
		{
			var reservationInstance = helpers.ResourceManager.GetReservationInstance(reservationInstanceResourceAssignmentResult.Id);
			if (!reservationInstance.Properties.Dictionary.TryGetValue(ServicePropertyNames.OrderIdsPropertyName, out var orderIdPropertyValue)) continue;

			foreach (var orderIdString in Convert.ToString(orderIdPropertyValue).Split(';'))
			{
				if (!Guid.TryParse(orderIdString, out var orderId)) continue;

				var updatedServiceConfigurations = LoadServiceConfigurationForOrder(helpers, orderId, serviceConfigurationsPerOrderId);
				if (updatedServiceConfigurations == null) continue;

				UpdateServiceConfiguration(result, updatedServiceConfigurations);
			}
		}
	}

	private static Dictionary<int, YleServiceConfiguration> LoadServiceConfigurationForOrder(Helpers helpers, Guid orderId, Dictionary<Guid, Dictionary<int, YleServiceConfiguration>> serviceConfigurationsPerOrderId)
	{
		if (!serviceConfigurationsPerOrderId.TryGetValue(orderId, out var updatedServiceConfigurations))
		{
			if (!helpers.OrderManagerElement.TryGetServiceConfigurations(orderId, out var serviceConfigurations)) return null;

			updatedServiceConfigurations = serviceConfigurations;
			serviceConfigurationsPerOrderId[orderId] = updatedServiceConfigurations;
		}

		return updatedServiceConfigurations;
	}

	private static void UpdateServiceConfiguration(ResourceMigrationResult result, Dictionary<int, YleServiceConfiguration> serviceConfigurations)
	{
		foreach (var serviceConfiguration in serviceConfigurations.Values)
		{
			var functionConfiguration = serviceConfiguration.Functions.FirstOrDefault(f => f.Value.ResourceId == result.OldId).Value;
			if (functionConfiguration == null) continue;

			functionConfiguration.ResourceId = result.NewId.Value;
		}
	}
}