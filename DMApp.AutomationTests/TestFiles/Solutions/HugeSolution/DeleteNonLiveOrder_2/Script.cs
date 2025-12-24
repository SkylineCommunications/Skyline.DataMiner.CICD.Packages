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

namespace DeleteNonLiveOrder_2
{
	using System;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController app;

		private int ticketId;
		private int dataminerId;

		private Task scriptTask;
		private int timeOutResult;
		private bool disposedValue;

		public void Run(Engine engine)
		{
			scriptTask = Task.Factory.StartNew(() =>
			{
				try
				{
					//engine.ShowUI();
					helpers = new Helpers(engine, Scripts.DeleteNonLiveOrder);

					string[] dmaAndTicketId = engine.GetScriptParam("ticketId").Value.Split(new[] { '/' });

					if (dmaAndTicketId.Length == 2)
					{
						ticketId = Int32.Parse(dmaAndTicketId[1]);
						dataminerId = Int32.Parse(dmaAndTicketId[0]);
					}
					else
					{
						ticketId = -1;
						dataminerId = -1;
					}

					RunSafe(engine);
				}
				catch (ScriptAbortException)
				{
					throw;
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						engine.Log("Run|Something went wrong: " + e);
						ShowExceptionDialog(engine, e);
					}
				}
			});

			timeOutResult = Task.WaitAny(new Task[] { scriptTask }, new TimeSpan(9, 59, 40));
		}

		private void RunSafe(Engine engine)
		{
			app = new InteractiveController(engine);

			NonLiveOrder nonLiveOrder = helpers.NonLiveOrderManager.GetNonLiveOrder(dataminerId, ticketId);

			Dialog dialog;
			if (nonLiveOrder == null)
			{
				MessageDialog messageDialog = new MessageDialog(engine, String.Format("Unable to retrieve Non-Live Order with ID {0}/{1}", dataminerId, ticketId)) { Title = "Unable to retrieve Order" };
				messageDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Unable to remove Non-Live Order");
				dialog = messageDialog;
			}
			else
			{
				DeleteDialog deleteDialog = new DeleteDialog(engine, nonLiveOrder);
				deleteDialog.YesButton.Pressed += DeleteDialog_YesButton_Pressed;
				deleteDialog.NoButton.Pressed += (sender, args) => engine.ExitSuccess("Non-Live Order was not removed");
				dialog = deleteDialog;
			}

			app.Run(dialog);
		}

		private void DeleteDialog_YesButton_Pressed(object sender, EventArgs e)
		{
			if (!helpers.NonLiveOrderManager.DeleteNonLiveOrder(dataminerId, ticketId))
			{
				MessageDialog messageDialog = new MessageDialog((Engine)helpers.Engine, "Unable to remove Non-Live Order.") { Title = "Removal failed" };
				messageDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("Unable to remove Non-Live Order");
				app.ShowDialog(messageDialog);
			}
			else
			{
				helpers.Engine.ExitSuccess("The Non-Live Order was successfully removed.");
			}
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong during the execution of the script");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
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
}