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

namespace AddService_1
{


	using System;
	using System.Linq;
	using System.Timers;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using TaskStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	internal class Script : IDisposable
	{
		private readonly Timer extendLocksTimer = new Timer();

		private Helpers helpers;
		private InteractiveController app;

		private ReportDialog rollbackReportDialog;
		private LoadAddServiceDialog loadEditOrderDialog;
		private LiveOrderFormDialog editOrderDialog;
		private ReportDialog reportDialog;

		private LiveOrderFormAction scriptAction;
		private readonly string selectedOrderIdForExiting = Guid.Empty.ToString();
		private UserInfo userInfo;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			int timeOutResult = 0;
			var scriptTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				try
				{
					Initialize(engine);

					loadEditOrderDialog = new LoadAddServiceDialog(helpers, extendLocksTimer);

					if (loadEditOrderDialog.Execute()) ShowDialog();
					else app.Run(loadEditOrderDialog);
				}
				catch (InteractiveUserDetachedException)
				{
					Dispose();
				}
				catch (ScriptAbortException)
				{
					Dispose();
					throw;
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						helpers.Log(nameof(Script), nameof(Run), $"Something went wrong: {e}");

						Dispose();

						ShowExceptionDialog(engine, e);
					}
				}
			});

			timeOutResult = System.Threading.Tasks.Task.WaitAny(new System.Threading.Tasks.Task[] { scriptTask }, new TimeSpan(9, 59, 40));
			if (timeOutResult == -1) Dispose();
		}

		private void ShowDialog()
		{
			scriptAction = loadEditOrderDialog.ScriptAction;
			userInfo = loadEditOrderDialog.UserInfo;

			editOrderDialog = loadEditOrderDialog.EditOrderDialog;

			editOrderDialog.BookOrderButton.Pressed += OrderForm_BookButton_Pressed;
			editOrderDialog.ExitButton.Pressed += (s, e) => helpers.Engine.ExitSuccess("no changes");

			app.Run(editOrderDialog);
		}

		private void Initialize(Engine engine)
		{
			engine.SetFlag(RunTimeFlags.NoInformationEvents);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoCheckingSets);
			engine.Timeout = TimeSpan.FromHours(10);
			//engine.ShowUI();

			helpers = new Helpers(engine, Scripts.AddService);

			app = new InteractiveController(engine);
		}

		private void OrderForm_BookButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			helpers.LogMethodStart(nameof(Script), nameof(OrderForm_BookButton_Pressed), out var stopwatch);

			try
			{
				if (!editOrderDialog.IsValid(saveOrder: false, confirmOrder: false, requestEventLock: true))
				{
					helpers.LogMethodCompleted(nameof(Script), nameof(OrderForm_BookButton_Pressed), null, stopwatch);
					return;
				}

				AddOrUpdateOrder(OrderAction.Book);
			}
			catch (Exception exception)
			{
				ShowExceptionDialog(helpers.Engine, exception);
			}

			helpers.LogMethodCompleted(nameof(Script), nameof(OrderForm_BookButton_Pressed), null, stopwatch);
		}

		private void AddOrUpdateOrder(OrderAction orderAction)
		{
			reportDialog = new AddOrUpdateReportDialog(helpers, userInfo.IsMcrUser) { Title = "Updating Order" };

			reportDialog.OkButton.Pressed += ReportDialog_OkButton_Pressed;
			reportDialog.RollBackButton.Pressed += ReportDialog_RollbackButton_Pressed;
			app.ShowDialog(reportDialog);

			var tasks = editOrderDialog.Finish(orderAction).Tasks;

			if (tasks.Any())
			{
				reportDialog.Finish(tasks);

				// release the locks if all tasks were successful
				if (tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

				app.ShowDialog(reportDialog);
			}
			else ShowMessageDialog(helpers.Engine, "No changes made", "No Changes made");

			helpers.Log(nameof(Script), nameof(AddOrUpdateOrder), $"Order Updated");
		}

		private void ReportDialog_RollbackButton_Pressed(object sender, EventArgs e)
		{
			RollBackTasks();

			helpers.LockManager.ReleaseLocks();
		}

		private void ReportDialog_OkButton_Pressed(object sender, EventArgs e)
		{
			helpers.LockManager.ReleaseLocks();

			if (reportDialog.TasksWereSuccessful)
			{
				helpers.Engine.ExitSuccess(selectedOrderIdForExiting);
			}
			else if (reportDialog.ShouldRollback)
			{
				RollBackTasks();
			}
			else
			{
				// Exit script when the user chooses to ignore the failed tasks
				helpers.Engine.ExitSuccess(scriptAction == LiveOrderFormAction.Add ? "Unable to create the new Order." : "Unable to update the existing Order.");
			}
		}

		private void RollBackTasks()
		{
			// Roll back tasks - Only roll back tasks that were successful
			rollbackReportDialog = new RollBackReportDialog(helpers) { Title = scriptAction == LiveOrderFormAction.Edit ? "Roll back Order update" : "Roll back Order creation" };
			rollbackReportDialog.OkButton.Pressed += (s, e) => helpers.Engine.ExitSuccess(rollbackReportDialog.TasksWereSuccessful ? "Successfully rolled back " : "Failed to roll back");


			app.ShowDialog(rollbackReportDialog);

			// Only roll back tasks that were successful
			var rollbackTasks = reportDialog.UpdateResults.SelectMany(ur => ur.Tasks).Where(t => t.Status == TaskStatus.Ok).Select(t => t.CreateRollbackTask()).Where(t => t != null).Reverse().ToList();

			foreach (var rollbackTask in rollbackTasks)
			{
				if (!rollbackTask.Execute())
				{
					helpers.Log(nameof(Script), nameof(RollBackTasks), "Rolling back " + rollbackTask.Description + "failed: " + rollbackTask.Exception);
					break;
				}
			}

			rollbackReportDialog.Finish(rollbackTasks);
			app.ShowDialog(rollbackReportDialog);
		}


		private void ShowMessageDialog(IEngine engine, string message, string title)
		{
			var dialog = new MessageDialog((Engine)engine, message) { Title = title };
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private void ShowExceptionDialog(IEngine engine, Exception exception)
		{
			ExceptionDialog exceptionDialog = new ExceptionDialog(engine, exception);
			exceptionDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong during adding a new service.");

			if (app.IsRunning) app.ShowDialog(exceptionDialog);
			else app.Run(exceptionDialog);
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue && disposing)
			{
				helpers.Dispose();

				helpers.LockManager.ReleaseLocks();

				extendLocksTimer.Stop();
				extendLocksTimer.Dispose();	
			}

			disposedValue = true;
		}

		~Script()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}
		#endregion
	}
}