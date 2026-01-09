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

using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Library;
using Skyline.DataMiner.Library.Solutions.SRM;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.SLDataGateway;
using Skyline.DataMiner.Net.ServiceManager.Objects;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public static void Run(Engine engine)
	{
		var filePath = engine.GetScriptParam("FilePath").Value;
		var directory = Path.GetDirectoryName(filePath);
		if (!Directory.Exists(directory))
		{
			engine.Log($"Directory {directory} does not exist!");
			return;
		}

		var action = engine.GetScriptParam("Action").Value;
		switch (action)
		{
			case "Clear":
				Delete();
				break;
			case "Import":
				Import(engine, filePath);
				break;
			case "ClearAndImport":
				Delete();
				Import(engine, filePath);
				break;
			case "Export":
				Export(filePath);
				break;
			default:
				engine.Log("Unsupported action: " + action);
				return;
		}
	}

	private static void Delete()
	{
		var filter = ServiceDefinitionExposers.IsTemplate.Equal(true);
		var templateServiceDefinitions = SrmManagers.ServiceManager.GetServiceDefinitions(filter);

		SrmManagers.ServiceManager.RemoveServiceDefinitions(out var error, templateServiceDefinitions.ToArray());
	}

	private static void Import(Engine engine, string filePath)
	{
		var json = File.ReadAllText(filePath);

		var serviceDefinitionImportExport = ServiceDefinitionImportExport.Deserialize(json);

		var serviceDefinitions = serviceDefinitionImportExport.ServiceDefinitionJsons.Select(s => JsonConvert.DeserializeObject<ServiceDefinition>(s)).ToArray();
		SrmManagers.ServiceManager.AddOrUpdateServiceDefinitions(serviceDefinitions, true);

		RegisterServiceDefinitionsProperties(engine, serviceDefinitions);
	}

	private static void Export(string filePath)
	{
		var filter = ServiceDefinitionExposers.IsTemplate.Equal(true);
		var templateServiceDefinitions = SrmManagers.ServiceManager.GetServiceDefinitions(filter);

		var serviceDefinitions = templateServiceDefinitions.Where(s => s.Name.StartsWith("_"));
		var serviceDefinitionImportExport = new ServiceDefinitionImportExport
		{
			ServiceDefinitionJsons = serviceDefinitions.Select(s => JsonConvert.SerializeObject(s)).ToArray()
		};

		File.WriteAllText(filePath, serviceDefinitionImportExport.Serialize());
	}

	/// <summary>
	/// Registers all Service Definition's Properties.
	/// This includes Service Definition, Node and Interface Properties.
	/// </summary>
	private static void RegisterServiceDefinitionsProperties(Engine engine, ServiceDefinition[] serviceDefinitions)
	{
		var serviceDefinitionProperties = serviceDefinitions
			.SelectMany(x => x.Properties)
			.Select(x => new PropertyInfo { Name = x.Name, Value = "Service" })
			.ToList();

		var serviceDefinitionNodeProperties = serviceDefinitions
			.SelectMany(x => x.Diagram.Nodes.SelectMany(y => y.Properties))
			.Select(x => new PropertyInfo { Name = x.Name, Value = "ServiceDefinitionNode" });

		var serviceDefinitionInterfaceProperties = serviceDefinitions
			.SelectMany(x => x.Diagram.Nodes.SelectMany(y => y.InterfaceConfigurations.SelectMany(z => z.Properties)))
			.Select(x => new PropertyInfo { Name = x.Name, Value = "ServiceDefinitionInterface" });

		serviceDefinitionProperties.AddRange(serviceDefinitionNodeProperties);
		serviceDefinitionProperties.AddRange(serviceDefinitionInterfaceProperties);

		var propertiesToAdd = serviceDefinitionProperties.DistinctBy(x => new { x.Name, x.Value });

		var configuredProperties = DataMinerSystemInfo.GetRegisteredProperties().ToArray();

		foreach (var property in propertiesToAdd)
		{
			if (configuredProperties.Any(y => y.Name == property.Name && y.Type == property.Value))
			{
				continue;
			}

			if (!DataMinerSystemInfo.RegisterPropertyConfig(property, property.Value))
			{
				engine.GenerateInformation($"Property with name {property.Name} of type {property.Value} not added.");
			}
		}
	}

	private sealed class ServiceDefinitionImportExport
	{
		public string[] ServiceDefinitionJsons { get; set; }

		public static ServiceDefinitionImportExport Deserialize(string json)
		{
			return JsonConvert.DeserializeObject<ServiceDefinitionImportExport>(json);
		}

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}