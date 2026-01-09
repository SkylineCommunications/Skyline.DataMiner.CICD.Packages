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
namespace StopOrder
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using StopOrder.Dialogs;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using TaskStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		#region Fields
		private readonly Timer extendLocksTimer = new Timer();

		private Helpers helpers;
		private InteractiveController app;

		private LoadStopOrderDialog loadStopOrderDialog;
		private AddOrUpdateReportDialog reportDialog;
		private ConfirmStopDialog confirmStopDialog;

		private Order receivedOrder;
		private UserInfo userInfo;
		#endregion

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
					loadStopOrderDialog = new LoadStopOrderDialog(helpers, extendLocksTimer);
					if (loadStopOrderDialog.Execute()) ShowConfirmStopDialog();
					else app.Run(confirmStopDialog);
				}
				catch (ScriptAbortException)
				{
					helpers.Log(nameof(Script), "STOP SCRIPT", "STOP ORDER");
					Dispose();
				}
				catch (InteractiveUserDetachedException)
				{
					helpers.Log(nameof(Script), "STOP SCRIPT", "STOP ORDER");
					Dispose();
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						helpers.Log(nameof(Script), nameof(Run), $"Something went wrong: {e.ToString()}");
						engine.Log("Run|Something went wrong: " + e);

						ShowExceptionDialog(engine, e);
					}
				}
				finally
				{
					Dispose();
				}
			});

			timeOutResult = System.Threading.Tasks.Task.WaitAny(new[] { scriptTask }, new TimeSpan(9, 59, 40));
			if (timeOutResult == -1) Dispose();
		}

		private void ShowConfirmStopDialog()
		{
			confirmStopDialog = loadStopOrderDialog.ConfirmStopOrderDialog;
			receivedOrder = loadStopOrderDialog.Orders.Single();

			if (receivedOrder.Status == Status.Running)
			{
				confirmStopDialog.NoButton.Pressed += (o, args) => helpers.Engine.ExitSuccess("No Button Pressed");
				confirmStopDialog.YesButton.Pressed += ConfirmStopDialog_YesButton_Pressed;

				userInfo = loadStopOrderDialog.UserInfo;

				if (app.IsRunning) app.ShowDialog(confirmStopDialog);
				else app.Run(confirmStopDialog);
			}
			else
			{
				helpers.Log(nameof(Script), nameof(ShowConfirmStopDialog), $"Order {receivedOrder.Name} with status {receivedOrder.Status} is not running.");
				ShowMessageDialog(helpers.Engine, $"Order { receivedOrder.Name} with status { receivedOrder.Status}. \n Only orders with RUNNING status can be stopped.", "No Changes made");
			}
		}

		private void ConfirmStopDialog_YesButton_Pressed(object sender, EventArgs e)
		{
			StopOrder();
		}

		private void StopOrder()
		{
			reportDialog = new AddOrUpdateReportDialog(helpers, userInfo.IsMcrUser) { Title = "Stop Order" };
			reportDialog.RollBackButton.IsVisible = false;
			reportDialog.OkButton.Pressed += (o, args) =>
            {
				helpers.LockManager.ReleaseLocks();
				helpers.Engine.ExitSuccess("Order and linked services are stopped.");
			};

			app.ShowDialog(reportDialog);

			helpers.ReportProgress("Start executing stop functionality ...");

			if (receivedOrder != null)
			{
				receivedOrder.StopNow = true;

				var result = receivedOrder.StopOrderAndLinkedServices(helpers);

				if (result.Tasks.Any())
				{
					reportDialog.Finish(result);

					// release the locks if all tasks were successful
					if (result.Tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

					app.ShowDialog(reportDialog);
				}
				else ShowMessageDialog(helpers.Engine, "No changes made", "No Changes made");
			}
			else
			{
				helpers.Engine.Log(nameof(StopOrder) + "received order doesn't exist.");
			}
		}

		private void Initialize(Engine engine)
		{
			engine.SetFlag(RunTimeFlags.NoInformationEvents);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoCheckingSets);
			//engine.ShowUI();

			helpers = new Helpers(engine, Scripts.StopOrder);
			app = new InteractiveController(engine);
		}

		private void ShowExceptionDialog(IEngine engine, Exception exception)
		{
			var dialog = new ExceptionDialog((Engine)engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong.");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private void ShowMessageDialog(IEngine engine, string message, string title)
		{
			var dialog = new MessageDialog((Engine)engine, message) { Title = title };
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers.Dispose();
				}
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