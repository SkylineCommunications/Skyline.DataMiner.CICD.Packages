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

namespace Debug_2
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System.Collections.Generic;
	using System.IO;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private const string LocalTempFolderPath = @"C:\Skyline DataMiner\Documents\.Temp";
		private const string RemoteTempFolderPath = @"\Documents\.Temp";

		private Helpers helpers;
		private InteractiveController app;
		private OverviewDialog overviewDialog;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			try
			{
				Initialize(engine);

				app.Run(overviewDialog);
			}
			catch (ScriptAbortException)
			{
				// Nothing to log
			}
			catch (InteractiveUserDetachedException)
			{
				// Nothing to log
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Something went wrong: {e}");

				ExceptionDialog exceptionDialog = new ExceptionDialog(engine, e);
				exceptionDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("OK");
				if (app.IsRunning) app.ShowDialog(exceptionDialog);
				else app.Run(exceptionDialog);
			}
			finally
			{
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			//engine.ShowUI();
			engine.ShowProgress("Loading...");
			engine.Timeout = TimeSpan.FromHours(10);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoCheckingSets);
			engine.SetFlag(RunTimeFlags.NoInformationEvents);

			helpers = new Helpers(engine, Scripts.Debug);

			app = new InteractiveController(engine);

			overviewDialog = new OverviewDialog(helpers);

			overviewDialog.FindReservationsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindReservationsDialog);
			overviewDialog.FindReservationOrderLoggingButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindReservationLoggingDialog);
			overviewDialog.GetServiceConfigurationsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.ServiceConfigurationsDialog);
			overviewDialog.RetriggerIntegrationUpdatesButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.RetriggerIntegrationUpdatesDialog);
			overviewDialog.ReservationsWithoutJobsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindReservationsWithoutJobDialog);
			overviewDialog.MetricsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.MetricsDialog);
			overviewDialog.SendIntegrationNotificationButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog);
			overviewDialog.UpdateOrderUiPropertiesButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.UpdateOrderUiPropertiesDialog);
			overviewDialog.LogCollectorButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.LogCollectorDialog);
			overviewDialog.EligibleResourcesButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.GetEligibleResourcesDialog);
			overviewDialog.ResourceOccupancyButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.ResourceOccupancyDialog);
			overviewDialog.MoveToQuarantinedStateButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.MoveToQuarantinedStateDialog);
			overviewDialog.FindQuarantinedOrdersButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindQuarantinedOrdersDialog);
			overviewDialog.FindProfileParameterButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindProfileParameterDialog);
			overviewDialog.FindTicketsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindTicketsDialog);
			overviewDialog.EditReservationPropertiesButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.EditReservationPropertiesDialog);
			overviewDialog.OrderHistoryButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.OrderHistoryDialog);
			overviewDialog.StopOrderNowButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.StopOrderNowDialog);
			overviewDialog.AddOrUpdateServiceConfigurationsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.AddOrUpdateServiceConfigurationsDialog);
			overviewDialog.FixServiceConfigurationsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FixServiceConfigurationsDialog);
			overviewDialog.DeleteOrdersButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.DeleteOrdersDialog);
			overviewDialog.FixResourceConfigurationButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FixResourceConfigurationDialog);
			overviewDialog.DeleteTemplatesButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.DeleteTemplatesDialog);
			overviewDialog.EditOrderTemplateButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.EditOrderTemplatesDialog);
			overviewDialog.EditEventTemplateButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.EditEventTemplatesDialog);
			overviewDialog.AnalyzePlasmaRecordingsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.AnalyzePlasmaRecordingsDialog);
			overviewDialog.VizremButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.VizremDialog);
			overviewDialog.ActiveFunctionsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.ActiveFunctionsDialog);
			overviewDialog.GetServiceDefinitionButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.GetServiceDefinitionDialog);
			overviewDialog.UpdateServiceDefinitionsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.UpdateServiceDefinitionsDialog);
			overviewDialog.FindResourcesWithFiltersButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindResourcesWithFiltersDialog);
			overviewDialog.FindReservationsWithFiltersButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindReservationsWithFiltersDialog);
			overviewDialog.RemoveDuplicatePropertiesButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.RemoveDuplicatePropertiesDialog);
			overviewDialog.FindTicketsWithFiltersButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FindTicketsWithFiltersDialog);
			overviewDialog.UpdateNonLiveOrderUserTasksButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.UpdateNonLiveOrderUserTasksDialog);
			overviewDialog.TestDataMinerInterfaceButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.TestDataMinerInterfaceDialog);
			overviewDialog.FixMissingServiceDefinitionsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.FixMissingServiceDefinitionsDialog);
			overviewDialog.AddOrUpdateResourcesButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.AddOrUpdateResourcesDialog);
			overviewDialog.ReassignJobButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.ReassignJobDialog);
			overviewDialog.ManageSectionDefinitionsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.ManageSectionDefinitionsDialog);
			overviewDialog.ProfileLoadLoggingButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.ProfileLoadLoggingDialog);
			overviewDialog.AutoUpdateOrderPropertiesButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.AutoUpdateOrderPropertiesDialog);
			overviewDialog.NonLiveOrdersButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.NonLiveOrdersDialog);
			overviewDialog.DownloadLoggingButton.Pressed += (sender, args) => DownloadLogging();

			overviewDialog.SendIntegrationNotificationDialog.FeenixButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog.FeenixNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.CeitonButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog.CeitonNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.PlasmaButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog.PlasmaNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.PlasmaOldButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog.PlasmaNotificationOldDialog);
			overviewDialog.SendIntegrationNotificationDialog.EurovisionButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog.EurovisionNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.PebbleBeachButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog.PbsNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.EvsButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog.EvsNotificationDialog);

			overviewDialog.FindReservationsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FindReservationLoggingDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.ServiceConfigurationsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.RetriggerIntegrationUpdatesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FindReservationsWithoutJobDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.MetricsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.SendIntegrationNotificationDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.UpdateOrderUiPropertiesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.LogCollectorDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.GetEligibleResourcesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.ResourceOccupancyDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.MoveToQuarantinedStateDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FindQuarantinedOrdersDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FindProfileParameterDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FindTicketsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.EditReservationPropertiesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.OrderHistoryDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.StopOrderNowDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.AddOrUpdateServiceConfigurationsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FixServiceConfigurationsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.DeleteOrdersDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FixResourceConfigurationDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.DeleteTemplatesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.EditOrderTemplatesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.EditEventTemplatesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.AnalyzePlasmaRecordingsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.VizremDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.VizremDialog.TriggerBookFlowButton.Pressed += (sender, args) => overviewDialog.VizremDialog.CreateVizremOrder(app);
			overviewDialog.ActiveFunctionsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.GetServiceDefinitionDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.UpdateServiceDefinitionsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FindResourcesWithFiltersDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FindReservationsWithFiltersDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.RemoveDuplicatePropertiesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FindTicketsWithFiltersDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.UpdateNonLiveOrderUserTasksDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.TestDataMinerInterfaceDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.FixMissingServiceDefinitionsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.AddOrUpdateResourcesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.ReassignJobDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.ManageSectionDefinitionsDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.ProfileLoadLoggingDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.AutoUpdateOrderPropertiesDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);
			overviewDialog.NonLiveOrdersDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog);

			overviewDialog.SendIntegrationNotificationDialog.FeenixNotificationDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.PlasmaNotificationDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.PlasmaNotificationOldDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.CeitonNotificationDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.EurovisionNotificationDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog);
			overviewDialog.SendIntegrationNotificationDialog.PbsNotificationDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog);

			overviewDialog.GenerateFieldsFileButton.Pressed += (sender, args) => GenerateFieldsFile(engine);

			SubscribeOnUpdateOrderUiPropertiesDialog();
			SubscribeOnAnalyzePlasmaRecordingsDialog();
			SubscribeOnFixServiceConfigurationsDialog();

			if (overviewDialog.SendIntegrationNotificationDialog.EvsNotificationDialog != null)
			{
				overviewDialog.SendIntegrationNotificationDialog.EvsNotificationDialog.BackButton.Pressed += (sender, args) => app.ShowDialog(overviewDialog.SendIntegrationNotificationDialog);
			}
		}

		private void GenerateFieldsFile(Engine engine)
		{
			ReflectionHandler.GetFields(engine, helpers, "YLE.UI");
			ReflectionHandler.WriteFile(helpers);
			engine.Log(nameof(Script), nameof(GenerateFieldsFile), $"UiFields Count: {ReflectionHandler.UiToolTips.Count}");
			engine.Log(nameof(Script), nameof(GenerateFieldsFile), $"UiFields: {JsonConvert.SerializeObject(ReflectionHandler.UiToolTips)}");
		}

		private void SubscribeOnUpdateOrderUiPropertiesDialog()
		{
			overviewDialog.UpdateOrderUiPropertiesDialog.ShowConfirmationDialog += (sender, args) =>
			{
				args.NoButton.Pressed += (s, a) => app.ShowDialog(overviewDialog.UpdateOrderUiPropertiesDialog);
				app.ShowDialog(args);
			};

			overviewDialog.UpdateOrderUiPropertiesDialog.ShowProgressDialog += (sender, args) => app.ShowDialog(args);

			overviewDialog.UpdateOrderUiPropertiesDialog.OrdersUpdated += (sender, args) => app.ShowDialog(overviewDialog.UpdateOrderUiPropertiesDialog);
		}

		private void SubscribeOnAnalyzePlasmaRecordingsDialog()
		{
			overviewDialog.AnalyzePlasmaRecordingsDialog.ShowConfirmationDialog += (sender, args) =>
			{
				args.NoButton.Pressed += (s, a) => app.ShowDialog(overviewDialog.AnalyzePlasmaRecordingsDialog);
				app.ShowDialog(args);
			};

			overviewDialog.AnalyzePlasmaRecordingsDialog.ShowProgressDialog += (sender, args) => app.ShowDialog(args);

			overviewDialog.AnalyzePlasmaRecordingsDialog.OrdersUpdated += (sender, args) => app.ShowDialog(overviewDialog.AnalyzePlasmaRecordingsDialog);
		}

		private void SubscribeOnFixServiceConfigurationsDialog()
		{
			overviewDialog.FixServiceConfigurationsDialog.ShowConfirmationDialog += (sender, args) =>
			{
				args.NoButton.Pressed += (s, a) => app.ShowDialog(overviewDialog.FixServiceConfigurationsDialog);
				app.ShowDialog(args);
			};

			overviewDialog.FixServiceConfigurationsDialog.ShowProgressDialog += (sender, args) => app.ShowDialog(args);

			overviewDialog.FixServiceConfigurationsDialog.OrdersUpdated += (sender, args) => app.ShowDialog(overviewDialog.FixServiceConfigurationsDialog);
		}

		private void DownloadLogging()
		{
			try
			{
				var subsScript = helpers.Engine.PrepareSubScript("FileRetrieval");
				subsScript.SelectScriptParam("path", $@"C:\Skyline_Data\OrderLogging\{overviewDialog.CurrentIdTextBox.Text}.txt");
				subsScript.SelectScriptParam("isSubscript", "false");
				subsScript.SelectScriptParam("download", "false");
				subsScript.Synchronous = true;
				subsScript.StartScript();

				var result = subsScript.GetScriptResult();

				var retrievedFiles = JsonConvert.DeserializeObject<List<RetrievedFile>>(result["fileInfo"]);
				string tempDirToZip = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				if (!Directory.Exists(tempDirToZip)) Directory.CreateDirectory(tempDirToZip);

				foreach (var file in retrievedFiles)
				{
					if (String.IsNullOrWhiteSpace(file.Content)) continue;

					string tempFilePath = Path.Combine(tempDirToZip, $"{file.Agent}_{overviewDialog.CurrentIdTextBox.Text}.txt");
					File.WriteAllText(tempFilePath, file.Content);
				}

				string zipArchiveName = $"{overviewDialog.CurrentIdTextBox.Text}.zip";
				string localZipArchivePath = Path.Combine(LocalTempFolderPath, zipArchiveName);
				if (File.Exists(localZipArchivePath)) File.Delete(localZipArchivePath);
				System.IO.Compression.ZipFile.CreateFromDirectory(tempDirToZip, localZipArchivePath);

				DownloadDialog dialog = new DownloadDialog(helpers.Engine, Path.Combine(RemoteTempFolderPath, zipArchiveName));
				dialog.DownloadButton.DownloadStarted += (s, e) => app.ShowDialog(overviewDialog);
				app.ShowDialog(dialog);
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(OverviewDialog), nameof(DownloadLogging), $"Unable to retrieve log files due to: {ex}");
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	public class RetrievedFile
	{
		public string Agent { get; set; }

		public string Content { get; set; }
	}
}