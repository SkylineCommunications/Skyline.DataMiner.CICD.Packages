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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Library.Solutions.SRM;
using Skyline.DataMiner.Net.Profiles;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
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

	private static void Import(string filePath)
	{
		var json = File.ReadAllText(filePath);

		var profileImportExport = ProfileImportExport.Deserialize(json);

		var profileParameters = profileImportExport.ProfileParameterXmls.Select(x => Parameter.FromXml(x)).ToArray();
		SrmManagers.ProfileHelper.ProfileParameters.AddOrUpdateBulk(profileParameters);

		var profileDefinitions = profileImportExport.ProfileDefinitionXmls.Select(x => ProfileDefinition.FromXml(x)).ToArray();
		SrmManagers.ProfileHelper.ProfileDefinitions.AddOrUpdateBulk(profileDefinitions);
	}

	private static void Export(string filePath)
	{
		var allProfileParameters = SrmManagers.ProfileHelper.ProfileParameters.ReadAll().Select(p => p.ToXml()).ToList();
		var allProfileDefinitions = SrmManagers.ProfileHelper.ProfileDefinitions.ReadAll().Select(p => p.ToXml()).ToList();

		var profileImportExport = new ProfileImportExport
		{
			ProfileParameterXmls = allProfileParameters,
			ProfileDefinitionXmls = allProfileDefinitions
		};

		File.WriteAllText(filePath, profileImportExport.Serialize());
	}

	private static void Delete()
	{
		var allProfileDefinitions = SrmManagers.ProfileHelper.ProfileDefinitions.ReadAll().ToArray();
		SrmManagers.ProfileHelper.ProfileDefinitions.RemoveBulk(allProfileDefinitions);

		var allProfileParameters = SrmManagers.ProfileHelper.ProfileParameters.ReadAll().ToArray();
		SrmManagers.ProfileHelper.ProfileParameters.RemoveBulk(allProfileParameters);
	}

	private sealed class ProfileImportExport
	{
		public List<string> ProfileParameterXmls { get; set; }

		public List<string> ProfileDefinitionXmls { get; set; }

		public static ProfileImportExport Deserialize(string json)
		{
			return JsonConvert.DeserializeObject<ProfileImportExport>(json);
		}

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}