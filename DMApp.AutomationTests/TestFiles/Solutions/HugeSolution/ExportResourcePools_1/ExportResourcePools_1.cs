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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Library.Solutions.SRM;
using Skyline.DataMiner.Net.Messages;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
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
				Import(filePath);
				break;
			case "ClearAndImport":
				Delete();
				Import(filePath);
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
		var resourcePools = SrmManagers.ResourceManager.GetResourcePools();
		SrmManagers.ResourceManager.RemoveResourcePools(resourcePools);
	}

	private static void Import(string filePath)
	{
		var json = File.ReadAllText(filePath);

		var resourcePoolImportExport = ResourcePoolImportExport.Deserialize(json);

		var resourcePools = resourcePoolImportExport.ResourcePools.Select(p => new ResourcePool(p.Key) { Name = p.Value }).ToArray();
		SrmManagers.ResourceManager.AddOrUpdateResourcePools(resourcePools);
	}

	private static void Export(string filePath)
	{
		var resourcePools = SrmManagers.ResourceManager.GetResourcePools();

		var resourcePoolImportExport = new ResourcePoolImportExport
		{
			ResourcePools = resourcePools.ToDictionary(p => p.ID, p => p.Name)
		};

		File.WriteAllText(filePath, resourcePoolImportExport.Serialize());
	}

	private sealed class ResourcePoolImportExport
	{
		public Dictionary<Guid, string> ResourcePools { get; set; }

		public static ResourcePoolImportExport Deserialize(string json)
		{
			return JsonConvert.DeserializeObject<ResourcePoolImportExport>(json);
		}

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}