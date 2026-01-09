/*
****************************************************************************
*  Copyright (c) 2020,  Skyline Communications NV  All Rights Reserved.    *
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

03/02/2020	1.0.0.1		JVT, Skyline	Initial version
****************************************************************************
*/

namespace UpdateService_4
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using TaskStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;

	internal class Script : YleInteractiveScript
	{
		private readonly Timer extendLocksTimer = new Timer();

		private LoadUpdateServiceDialog loadUpdateServiceDialog;
		private UpdateServiceDialog updateServiceDialog;
		private EditSharedSourceDialog editSharedSourceDialog;
		private DeleteServiceDialog deleteServiceDialog;
		private UseSharedSourceDialog useSharedSourceDialog;
		private ReportIssueDialog reportIssueDialog;
		private ReportDialog reportDialog;

		protected override Scripts ScriptName => Scripts.UpdateService;

		protected override TimeSpan? TimeOut => new TimeSpan(9, 59, 40);

		protected override void InternalRun()
		{
			loadUpdateServiceDialog = new LoadUpdateServiceDialog(helpers, extendLocksTimer);

			if (loadUpdateServiceDialog.Execute()) ShowDialog();
			else app.Run(loadUpdateServiceDialog);
		}

		private void ShowDialog()
		{
			if (loadUpdateServiceDialog.IsSharedSource && (loadUpdateServiceDialog.ScriptAction == ScriptAction.Edit || loadUpdateServiceDialog.ScriptAction == ScriptAction.ResourceChange))
			{
				ShowUpdateSharedSourceDialog();
			}
			else if (loadUpdateServiceDialog.ScriptAction == ScriptAction.Delete)
			{
				ShowDeleteServiceDialog();
			}
			else if (loadUpdateServiceDialog.ScriptAction == ScriptAction.UseSharedSource)
			{
				if (loadUpdateServiceDialog.OrderSelectionRequired)
				{
					ShowSelectOrderDialog();
				}
				else
				{
					ShowUseSharedSourceDialog();
				}
			}
			else
			{
				ShowUpdateServiceDialog();
			}
		}

		private void ShowUpdateServiceDialog()
		{
			updateServiceDialog = loadUpdateServiceDialog.UpdateServiceDialog;
			updateServiceDialog.ConfirmButton.Pressed += ServiceDialog_ConfirmButton_Pressed;
			updateServiceDialog.ExitButton.Pressed += (o, e) => helpers.Engine.ExitSuccess("exit");
			updateServiceDialog.ReportIssueButton.Pressed += ReportIssueButton_Pressed;

			if (app.IsRunning) app.ShowDialog(updateServiceDialog);
			else app.Run(updateServiceDialog);
		}

		private void ShowDeleteServiceDialog()
		{
			deleteServiceDialog = loadUpdateServiceDialog.DeleteServiceDialog;
			deleteServiceDialog.NoButton.Pressed += (o, e) => helpers.Engine.ExitSuccess("exit");
			deleteServiceDialog.YesButton.Pressed += ServiceDialog_DeleteButton_Pressed;

			if (app.IsRunning) app.ShowDialog(deleteServiceDialog);
			else app.Run(deleteServiceDialog);
		}

		private void ShowSelectOrderDialog()
		{
			SelectOrderDialog selectOrderDialog = loadUpdateServiceDialog.SelectOrderDialog;
			selectOrderDialog.ContinueButton.Pressed += (s, e) => ShowUseSharedSourceDialog();

			if (app.IsRunning) app.ShowDialog(selectOrderDialog);
			else app.Run(selectOrderDialog);
		}

		private void ShowUseSharedSourceDialog()
		{
			useSharedSourceDialog = loadUpdateServiceDialog.UseSharedSourceDialog;
			useSharedSourceDialog.ConfirmButton.Pressed += UseSharedServiceDialog_ConfirmButton_Pressed;
			useSharedSourceDialog.ExitButton.Pressed += (o, e) => helpers.Engine.ExitSuccess("exit");

			if (app.IsRunning) app.ShowDialog(useSharedSourceDialog);
			else app.Run(useSharedSourceDialog);
		}

		private void ServiceDialog_DeleteButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			bool stopInsteadOfRemove = deleteServiceDialog.ServiceToDelete.IsOrShouldBeRunning;
			var parentService = deleteServiceDialog.Order.AllServices.SingleOrDefault(x => x.Children.Contains(deleteServiceDialog.ServiceToDelete)) ?? throw new NotFoundException("No parent service found.");

			if (stopInsteadOfRemove)
			{
				deleteServiceDialog.ServiceToDelete.TryStopServiceNow(helpers);
				Log($"Stopped service {deleteServiceDialog.ServiceToDelete.Name}");
			}
			else
			{
				parentService.Children.Remove(deleteServiceDialog.ServiceToDelete);
				Log($"Removed service {deleteServiceDialog.ServiceToDelete.Name} from the children of service {parentService.Name}");
			}

			var serviceToBeChecked = parentService;
			while (!serviceToBeChecked.Children.Any())
			{
				parentService = deleteServiceDialog.Order.AllServices.SingleOrDefault(x => x.Children.Contains(serviceToBeChecked));
				if (parentService is null) break;

				if (stopInsteadOfRemove)
				{
					serviceToBeChecked.TryStopServiceNow(helpers);
					Log($"Stopped service {serviceToBeChecked.Name}");
				}
				else
				{
					parentService.Children.Remove(serviceToBeChecked);
					Log($"Removed service {serviceToBeChecked.Name} from the children of service {parentService.Name}");
				}

				serviceToBeChecked = parentService;
			}

			var tasks = new List<Task>();
			TimeSpan duration;

			if (!deleteServiceDialog.Order.AllServices.Any(x => x.Definition.IsEndPointService))
			{
				var cancelOrderTask = new CancelOrderTask(helpers, deleteServiceDialog.Order);
				tasks.Add(cancelOrderTask);
				cancelOrderTask.Execute();
				duration = cancelOrderTask.Duration;
			}
			else
			{
				var deleteResult = deleteServiceDialog.Order.AddOrUpdate(helpers, loadUpdateServiceDialog.UserInfo.IsMcrUser);
				tasks.AddRange(deleteResult.Tasks);
				duration = deleteResult.Duration;
			}

			reportDialog = new AddOrUpdateReportDialog(helpers, loadUpdateServiceDialog.UserInfo.IsMcrUser);
			reportDialog.OkButton.Pressed += (o, arg) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Successfully deleted the service"); };
			reportDialog.RollBackButton.Pressed += (o, arg) => RollBackTasks();

			if (tasks.Any())
			{
				reportDialog.Finish(tasks, duration);

				if (tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

				app.ShowDialog(reportDialog);
			}
			else ShowMessageDialog("No changes made", "No Changes made");
		}

		private void ShowUpdateSharedSourceDialog()
		{
			editSharedSourceDialog = loadUpdateServiceDialog.EditSharedSourceDialog;
			editSharedSourceDialog.ConfirmButton.Pressed += EditSharedSourceDialog_ConfirmButton_Pressed;

			if (app.IsRunning) app.ShowDialog(editSharedSourceDialog);
			else app.Run(editSharedSourceDialog);
		}

		private void ServiceDialog_ConfirmButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			if (!updateServiceDialog.IsValid(false, false, false))
			{
				return;
			}

			var updateResult = updateServiceDialog.Finish(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderAction.Book);
			var tasks = updateResult.Tasks.ToList();

			reportDialog = new AddOrUpdateReportDialog(helpers, loadUpdateServiceDialog.UserInfo.IsMcrUser);
			reportDialog.OkButton.Pressed += (o, args) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Successfully updated the service"); };
			reportDialog.RollBackButton.Pressed += (o, args) => RollBackTasks();

			if (tasks.Any())
			{
				reportDialog.Finish(tasks, updateResult.Duration);

				// release the locks if all tasks were successful
				if (tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

				app.ShowDialog(reportDialog);
			}
			else ShowMessageDialog("No changes made", "No Changes made");
		}

		private void EditSharedSourceDialog_ConfirmButton_Pressed(object sender, EventArgs e)
		{
			if (!editSharedSourceDialog.IsValid)
			{
				return;
			}

			reportDialog = new AddOrUpdateReportDialog(helpers, loadUpdateServiceDialog.UserInfo.IsMcrUser);
			reportDialog.OkButton.Pressed += (o, args) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Successfully updated the Shared Source"); };
			reportDialog.RollBackButton.Pressed += (o, args) => RollBackTasks();
			app.ShowDialog(reportDialog);

			var tasks = editSharedSourceDialog.Finish().Tasks.ToList();

			if (tasks.Any())
			{
				reportDialog.Finish(tasks);

				// release the locks if all tasks were successful
				if (tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

				app.ShowDialog(reportDialog);
			}
			else ShowMessageDialog("No changes made", "No Changes made");
		}

		private void UseSharedServiceDialog_ConfirmButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			if (!useSharedSourceDialog.IsValid(false, false, false))
			{
				return;
			}

			var updateResult = useSharedSourceDialog.Finish(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderAction.Book);
			var tasks = updateResult.Tasks.ToList();

			reportDialog = new AddOrUpdateReportDialog(helpers, loadUpdateServiceDialog.UserInfo.IsMcrUser);
			reportDialog.OkButton.Pressed += (o, args) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Successfully updated the service"); };
			reportDialog.RollBackButton.Pressed += (o, args) => RollBackTasks();

			if (tasks.Any())
			{
				reportDialog.Finish(tasks, updateResult.Duration);

				// release the locks if all tasks were successful
				if (tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

				app.ShowDialog(reportDialog);
			}
			else ShowMessageDialog("No changes made", "No Changes made");
		}

		private void ReportIssueButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			reportIssueDialog = new ReportIssueDialog(helpers.Engine);
			reportIssueDialog.ConfirmButton.Pressed += ReportIssueDialog_ConfirmButton_Pressed;
			reportIssueDialog.CancelButton.Pressed += (o, args) => app.ShowDialog(updateServiceDialog);

			app.ShowDialog(reportIssueDialog);
		}

		private void ReportIssueDialog_ConfirmButton_Pressed(object sender, EventArgs e)
		{
			if (!reportIssueDialog.IsValid()) return;

			updateServiceDialog.Order.ErrorDescription = reportIssueDialog.ReportIssueTextBox.Text;
			updateServiceDialog.ServiceBeingEdited.HasAnIssueBeenreportedManually = true;

			reportDialog = new AddOrUpdateReportDialog(helpers, loadUpdateServiceDialog.UserInfo.IsMcrUser);
			reportDialog.RollBackButton.IsVisible = false;
			reportDialog.OkButton.Pressed += (o, args) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Successfully updated the order and service"); };
			app.ShowDialog(reportDialog);

			var tasks = updateServiceDialog.Order.GetUpdateCustomPropertiesWhenIssueReportedManuallyTasks(helpers, updateServiceDialog.ServiceBeingEdited);

			if (tasks.Any())
			{
				reportDialog.Finish(tasks);

				// release the locks if all tasks were successful
				if (tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

				app.ShowDialog(reportDialog);
			}
			else ShowMessageDialog("No changes made", "No Changes made");
		}

		private void RollBackTasks()
		{
			// Roll back tasks - Only roll back tasks that were successful
			RollBackReportDialog rollbackReportDialog = new RollBackReportDialog(helpers) { Title = "Roll back service update" };
			rollbackReportDialog.OkButton.Pressed += (o, e) => helpers.Engine.ExitSuccess("Successfully rolled back the service update");

			app.ShowDialog(rollbackReportDialog);

			// Only roll back tasks that were successful
			List<Task> rollbackTasks = reportDialog.UpdateResults.SelectMany(ur => ur.Tasks).Where(t => t.Status == TaskStatus.Ok).Select(t => t.CreateRollbackTask()).Where(t => t != null).Reverse().ToList();

			foreach (Task rollbackTask in rollbackTasks)
			{
				if (!rollbackTask.Execute())
				{
					helpers.Log(nameof(UpdateService_4.Script), nameof(RollBackTasks), "Rolling back " + rollbackTask.Description + "failed: " + rollbackTask.Exception);
					break;
				}
			}

			rollbackReportDialog.Finish(rollbackTasks);

			helpers.LockManager.ReleaseLocks();

			app.ShowDialog(rollbackReportDialog);
		}

		protected override void EngineShowUiInComments()
		{
			//engine.ShowUI();
		}

		protected override void HandleException(Exception e)
		{
			ShowExceptionDialog(e);
		}

		#region IDisposable Support
		private bool disposedValue;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!disposedValue && disposing)
			{
				helpers.Dispose();
				helpers.LockManager.ReleaseLocks();

				extendLocksTimer.Stop();
				extendLocksTimer.Dispose();
			}

			disposedValue = true;
		}
		#endregion
	}
}