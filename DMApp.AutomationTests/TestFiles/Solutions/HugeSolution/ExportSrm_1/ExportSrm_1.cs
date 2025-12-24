/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2023	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
This script will run the Full Export for all active Booking Manager elements present on the DMA.
Run this script on the same agent as the one that hosts the Booking Managers to be able to create a package.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Common;
using Skyline.DataMiner.Library.Automation;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	private const string FullExportScriptName = "SRM_ExportFullConfiguration";
	private const string FullExportScript_InputData_Key = "Input Data";

	private const string ResourceExportScriptName = "SRM_DiscoverResources";
	private const string ResourceExportScript_Operation_Key = "Operation";
	private const string ResourceExportScript_FilePath_Key = "File Path";
	private const string ResourceExportScript_InputData_Key = "Input Data";

	private const string TempOutputFolder = @"C:\.SrmExportTemp";
	private const string OutputFolder = @"C:\Skyline_Data\SRMBackup";

	private readonly Logger logger = new Logger();
	private readonly List<Dictionary<string, string>> exportBookingManagerFailureResults = new List<Dictionary<string, string>>();

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		engine.Timeout = TimeSpan.FromHours(3);

		try
		{
			string outputFolder = PrepareBackup();

			List<string> outputFiles = new List<string>();
			outputFiles.AddRange(ExportBookingManagers(engine, outputFolder));
			outputFiles.Add(ExportResources(engine, outputFolder));

			BuildPackage(outputFiles);

			RemoveTempFolder();

			if (exportBookingManagerFailureResults.Any())
			{
				SendEmail(engine);
			}
		}
		catch (Exception e)
		{
			logger.Log(nameof(Run), $"Something went wrong: {e}");
			SendEmail(engine);
		}
	}

	private string PrepareBackup()
	{
		string backupFolder = Path.Combine(TempOutputFolder, $"SrmExport_{DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss")}");
		if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);
		return backupFolder;
	}

	private IEnumerable<string> ExportBookingManagers(Engine engine, string outputFolder)
	{
		IDms dms = Engine.SLNetRaw.GetDms();
		List<string> outputFileLocations = new List<string>();

		var bookingManagers = dms.GetElements().Where(x => x.State == ElementState.Active && x.Protocol.Name.Equals("Skyline Booking Manager")).ToList();

		foreach (var bookingManager in bookingManagers)
		{
			logger.Log(nameof(ExportBookingManagers), $"Exporting {bookingManager.Name}...");

			Skyline.DataMiner.Library.Solutions.SRM.Model.ExportFullConfiguration.OutputData output;

			output = ExportBookingManager(engine, bookingManager.Name);

			var localDmaInfo = engine.GetLocalDataMinerInfo();
			if (!localDmaInfo.AgentName.Equals(output.AgentName))
			{
				// Unable to copy the export files as they are not accessible from this agent
				logger.Log(nameof(ExportBookingManagers), $"Unable to copy export files as this script is run on a different agent than the one hosting Booking Manager {bookingManager.Name}");
				continue;
			}

			logger.Log(nameof(ExportBookingManagers), $"Export succeeded, file {output.ExportedFilePath} on Agent {output.AgentName}");

			logger.Log(nameof(ExportBookingManagers), $"Moving export for {bookingManager.Name}...");

			string fileName = Path.GetFileName(output.ExportedFilePath).Replace("Booking Manager", String.Empty).Split('_')[0].Trim() + Path.GetExtension(output.ExportedFilePath);
			string newFilePath = Path.Combine(outputFolder, fileName);
			File.Move(output.ExportedFilePath, newFilePath);

			outputFileLocations.Add(newFilePath);

			logger.Log(nameof(ExportBookingManagers), $"Moving export succeeded");
		}

		return outputFileLocations;
	}

	private Skyline.DataMiner.Library.Solutions.SRM.Model.ExportFullConfiguration.OutputData ExportBookingManager(IEngine engine, string bookingManagerName)
	{
		var inputData = new Skyline.DataMiner.Library.Solutions.SRM.Model.ExportFullConfiguration.InputData { BookingManagerName = bookingManagerName, IsInteractive = false };

		var fullExportScript = engine.PrepareSubScript(FullExportScriptName);
		fullExportScript.Synchronous = true;
		fullExportScript.SelectScriptParam(FullExportScript_InputData_Key, JsonConvert.SerializeObject(inputData));
		fullExportScript.StartScript();

		var scriptResults = fullExportScript.GetScriptResult();
		bool succeeded = scriptResults.TryGetValue("Result", out var result);
		if (!succeeded)
		{
			exportBookingManagerFailureResults.Add(scriptResults);

			foreach (var exceptionResult in scriptResults)
			{
				logger.Log(nameof(ExportBookingManagers), $"SRM full export configuration script returned error message: {exceptionResult.Key}/{exceptionResult.Value}");
			}

			return new Skyline.DataMiner.Library.Solutions.SRM.Model.ExportFullConfiguration.OutputData();
		}

		return JsonConvert.DeserializeObject<Skyline.DataMiner.Library.Solutions.SRM.Model.ExportFullConfiguration.OutputData>(result);
	}

	private string ExportResources(IEngine engine, string outputFolder)
	{
		logger.Log(nameof(ExportBookingManagers), $"Exporting Resources...");

		var inputData = new Skyline.DataMiner.Library.Solutions.SRM.Model.DiscoverResources.InputData { ForceUpdate = false, IsSilent = true };
		string outputFile = Path.Combine(outputFolder, "resources.xlsx");

		var fullExportScript = engine.PrepareSubScript(ResourceExportScriptName);
		fullExportScript.Synchronous = true;
		fullExportScript.SelectScriptParam(ResourceExportScript_Operation_Key, "Export");
		fullExportScript.SelectScriptParam(ResourceExportScript_FilePath_Key, outputFile);
		fullExportScript.SelectScriptParam(ResourceExportScript_InputData_Key, JsonConvert.SerializeObject(inputData));
		fullExportScript.StartScript();

		logger.Log(nameof(ExportBookingManagers), $"Exporting Resources succeeded");

		return outputFile;
	}

	private void BuildPackage(IEnumerable<string> files)
	{
		logger.Log(nameof(ExportBookingManagers), $"Building package...");

		var zipFiles = files.Where(x => !String.IsNullOrWhiteSpace(x) && Path.GetExtension(x).Equals(".zip", StringComparison.InvariantCultureIgnoreCase)).ToList();
		var otherFiles = files.Where(x => !zipFiles.Contains(x)).ToList();

		string fileName = $"SrmExport_{DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss")}.zip";
		string tempFilePath = Path.Combine(TempOutputFolder, fileName);
		using (var archive = ZipFile.Open(tempFilePath, ZipArchiveMode.Create))
		{
			// Merge zip files
			foreach (var zipFile in zipFiles)
			{
				var zipArchive = ZipFile.OpenRead(zipFile);
				
				foreach (var zipArchiveEntry in zipArchive.Entries)
				{
					var fileEntry = archive.CreateEntry($"{Path.GetFileNameWithoutExtension(zipFile)}/" + zipArchiveEntry.FullName);
					using (var streamWriter = new StreamWriter(fileEntry.Open()))
					{
						using (StreamReader reader = new StreamReader(zipArchiveEntry.Open()))
						{
							streamWriter.Write(reader.ReadToEnd());
						}
					}
				}

				zipArchive.Dispose();
			}

			// Add additional files
			foreach (var otherFile in otherFiles)
			{
				archive.CreateEntryFromFile(otherFile, Path.GetFileName(otherFile));
			}
		}

		// Move zipped archive to backup folder
		string backupFilePath = Path.Combine(OutputFolder, fileName);
		if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
		File.Move(tempFilePath, backupFilePath);

		logger.Log(nameof(ExportBookingManagers), $"Building package succeeded");
	}

	private void RemoveTempFolder()
	{
		logger.Log("Removing temporary folder...");
		Directory.Delete(TempOutputFolder, true);
		logger.Log("Removing temporary folder succeeded");
	}

	private void SendEmail(Engine engine)
	{
		var localDmaInfo = engine.GetLocalDataMinerInfo();
		engine.SendEmail(new EmailOptions
		{
			Title = $"Scheduled SRM Backup from {localDmaInfo.Name} failed",
			TO = "squad.deploy-the.pioneers@skyline.be",
			Message = logger.Logging
		});
	}
}

public class Logger
{
	private readonly StringBuilder logBuilder = new StringBuilder();

	public string Logging => logBuilder.ToString();

	public void Log(string message)
	{
		logBuilder.AppendLine($"{DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss")}|{message}");
	}

	public void Log(string method, string message)
	{
		logBuilder.AppendLine($"{DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss")}|{method}|{message}");
	}

	public void Log(string className, string method, string message)
	{
		logBuilder.AppendLine($"{DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss")}|{className}|{method}|{message}");
	}
}