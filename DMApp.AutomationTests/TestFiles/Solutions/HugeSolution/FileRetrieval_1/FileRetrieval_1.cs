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
*/

namespace FileRetrieval_1
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript.Components;

	public class Script
	{
		private const string FileRetrieval_ScriptName = "FileRetrieval";

		private const string Path_ScriptInputParam = "path";
		private const string Subscript_ScriptInputParam = "isSubscript";
		private const string Download_ScriptInputParam = "download";

		private const string FileContent_ScriptOutputParam = "fileInfo";

		private const string LocalTempFolderPath = @"C:\Skyline DataMiner\Documents\.Temp";
		private const string RemoteTempFolderPath = @"\Documents\.Temp";

		private InteractiveController app;
		private string filePath;
		private bool isSubscript;
		private bool download;

		/// <summary>
		/// The Script entry point.
		/// Engine.ShowUI();
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				engine.SetFlag(RunTimeFlags.NoKeyCaching);
				engine.Timeout = TimeSpan.FromHours(10);
				GetScriptParams(engine);

				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception e)
			{
				engine.Log("Run|Something went wrong: " + e);
				ShowExceptionDialog(engine, e);
			}
		}

		private void GetScriptParams(IEngine engine)
		{
			filePath = engine.GetScriptParam(Path_ScriptInputParam).Value;
			isSubscript = Convert.ToBoolean(engine.GetScriptParam(Subscript_ScriptInputParam).Value);
			download = Convert.ToBoolean(engine.GetScriptParam(Download_ScriptInputParam).Value);
		}

		private void RunSafe(IEngine engine)
		{
			CleanLocalTempFolder(engine);

			List<RetrievedFile> retrievedFiles = new List<RetrievedFile> { GetLocalFile() };
			if (isSubscript)
			{
				OutputRetrievedFiles(engine, retrievedFiles);
				return;
			}
			else
			{
				retrievedFiles.AddRange(GetRemoteFiles(engine));
			}

			if (download)
			{
				DownloadRetrievedFiles(engine, retrievedFiles);
			}
			else
			{
				OutputRetrievedFiles(engine, retrievedFiles);
			}
		}

		private void DownloadRetrievedFiles(IEngine engine, IEnumerable<RetrievedFile> retrievedFiles)
		{
			// Write contents to temporary folder
			if (!Directory.Exists(LocalTempFolderPath)) Directory.CreateDirectory(LocalTempFolderPath);

			string tempDirToZip = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			if (!Directory.Exists(tempDirToZip)) Directory.CreateDirectory(tempDirToZip);

			List<string> tempFilePaths = new List<string>();
			foreach (var file in retrievedFiles)
			{
				if (String.IsNullOrWhiteSpace(file.Content)) continue;

				string tempFilePath = Path.Combine(tempDirToZip, $"{file.Agent}_{Path.GetFileName(filePath)}");
				File.WriteAllText(tempFilePath, file.Content);
				tempFilePaths.Add(tempFilePath);
			}

			if (!tempFilePaths.Any())
			{
				engine.Log($"Unable to retrieve {filePath} from any agent");
				return;
			}

			// Zip retrieved file to Skyline DataMiner\Documents (= accessible through IIS)
			string zipArchiveName = $"{Path.GetFileNameWithoutExtension(filePath)}.zip";
			string localZipArchivePath = Path.Combine(LocalTempFolderPath, zipArchiveName);
			if (File.Exists(localZipArchivePath)) File.Delete(localZipArchivePath);
			ZipFile.CreateFromDirectory(tempDirToZip, localZipArchivePath);

			// Remove temp files
			try
			{
				foreach (var tempFilePath in tempFilePaths)
				{
					File.Delete(tempFilePath);
				}
			}
			catch (Exception e)
			{
				engine.Log($"Unable to remove temporary files due to: {e}");
			}

			// Download zipped file
			app = new InteractiveController(engine);
			Dialog dialog = new DownloadDialog(engine, Path.Combine(RemoteTempFolderPath, zipArchiveName));
			app.Run(dialog);
		}

		private static void OutputRetrievedFiles(IEngine engine, IEnumerable<RetrievedFile> retrievedFiles)
		{
			var serializedOutput = JsonConvert.SerializeObject(retrievedFiles);
			engine.AddOrUpdateScriptOutput(FileContent_ScriptOutputParam, serializedOutput);
			engine.ExitSuccess("Files retrieved");
		}

		private void ShowExceptionDialog(IEngine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong during the creation of the new event.");
			if (app.IsRunning) app.ShowDialog(dialog); else app.Run(dialog);
		}

		private RetrievedFile GetLocalFile()
		{
			var file = new RetrievedFile
			{
				Agent = Engine.SLNetRaw.ServerDetails.AgentName
			};

			if (!File.Exists(filePath)) return file;

			file.Content = File.ReadAllText(filePath);
			return file;
		}

		private List<RetrievedFile> GetRemoteFiles(IEngine engine)
		{
			List<RetrievedFile> files = new List<RetrievedFile>();

			var thisAgentId = Engine.SLNetRaw.ServerDetails.AgentID;
			var dms = engine.GetDms();

			var otherAgents = dms.GetAgents().Where(x => x.Id != thisAgentId);
			foreach (var otherAgent in otherAgents)
			{
				var executeScriptMessage = new ExecuteScriptMessage
				{
					ScriptName = FileRetrieval_ScriptName,
					DataMinerID = otherAgent.Id,
					HostingDataMinerID = otherAgent.Id,
					Options = new SA(new[]
					{
						"DEFER:FALSE",
						$"PARAMETERBYNAME:{Path_ScriptInputParam}:{filePath}",
						$"PARAMETERBYNAME:{Subscript_ScriptInputParam}:true",
						$"PARAMETERBYNAME:{Download_ScriptInputParam}:false",
					})
				};

				try
				{
					var response = (ExecuteScriptResponseMessage)engine.SendSLNetSingleResponseMessage(executeScriptMessage);
					if (response.ScriptOutput.TryGetValue(FileContent_ScriptOutputParam, out string output) && !String.IsNullOrWhiteSpace(output))
					{
						files.AddRange(JsonConvert.DeserializeObject<List<RetrievedFile>>(output));
					}
				}
				catch (Exception e)
				{
					engine.Log($"Unable to retrieve file {filePath} from agent {otherAgent.Name} due to {e}");
				}
			}

			return files;
		}

		private static void CleanLocalTempFolder(IEngine engine)
		{
			try
			{
				DateTime now = DateTime.Now;
				var filesToRemove = Directory.EnumerateFiles(LocalTempFolderPath).Where(x => now.Subtract(File.GetLastWriteTime(x)) > TimeSpan.FromMinutes(5)).ToList();
				foreach (var file in filesToRemove) File.Delete(file);
			}
			catch (Exception e)
			{
				engine.Log($"Unable to clean local temp folder due to: {e}");
			}
		}
	}

	public class DownloadDialog : Dialog
	{
		private readonly DownloadButton downloadButton = new DownloadButton();

		public DownloadDialog(IEngine engine, string remoteFilePath) : base(engine)
		{
			downloadButton.RemoteFilePath = remoteFilePath;
			downloadButton.DownloadedFileName = Path.GetFileName(remoteFilePath);
			downloadButton.StartDownloadImmediately = true;
			downloadButton.DownloadStarted += (s, e) => engine.ExitSuccess("Files downloaded");

			AddWidget(new Label("Download will start immediately"), 0, 0);
			AddWidget(downloadButton, 1, 0);
		}
	}

	public class RetrievedFile
	{
		public string Agent { get; set; }

		public string Content { get; set; }
	}
}