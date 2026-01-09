/*
****************************************************************************
*  Copyright (c) 2019,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2019	1.0.0.1		XXX, Skyline	Initial Version
****************************************************************************
*/



//---------------------------------
// DeleteEvent_2.cs
//---------------------------------

// Added so DM recognizes this script as an IAS.
// engine.ShowUI();

namespace DeleteEvent_2
{
	using System;
	using System.Threading.Tasks;
	using System.Timers;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script : IDisposable
	{
		private Helpers helpers;
		private Engine engine;
		private InteractiveController app;
		private string jobId;
		private LockInfo lockInfo;
		private Event eventInfo;

		private int attemptsExtendLocking = 1;
		private readonly Timer extendLocksTimer = new Timer();

		private int timeOutResult;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			Task scriptTask = Task.Factory.StartNew(() =>
			{
				try
				{
					engine.SetFlag(RunTimeFlags.NoKeyCaching);
					engine.Timeout = TimeSpan.FromHours(10);

					app = new InteractiveController(engine);
					helpers = new Helpers(engine, Scripts.DeleteEvent);
					this.engine = engine;

					jobId = engine.GetScriptParam("jobId").Value;

					lockInfo = helpers.LockManager.RequestEventLock(jobId);

					eventInfo = GetEvent();

					RunSafe(engine);
				}
				catch (ScriptAbortException)
				{
					Dispose();
				}
				catch (InteractiveUserDetachedException)
				{
					Dispose();
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						ShowExceptionDialog(engine, e);
						Dispose();
					}
				}
			});

			timeOutResult = Task.WaitAny(new [] { scriptTask }, new TimeSpan(9, 59, 40));
			if (timeOutResult == -1)
			{
				Dispose();
			}
		}

		private void RunSafe(Engine engine)
		{
			app = new InteractiveController(engine);

			if (lockInfo != null && lockInfo.IsLockGranted)
			{
				extendLocksTimer.Elapsed += ExtendLocksTimer_Elapsed;
				extendLocksTimer.Interval = lockInfo.ReleaseLocksAfter.TotalMilliseconds;
				extendLocksTimer.AutoReset = true;
				extendLocksTimer.Enabled = true;
			}

			if (eventInfo.IntegrationType == IntegrationType.None)
			{
				DeleteEventDialog deleteEventDialog = new DeleteEventDialog(engine, lockInfo);
				deleteEventDialog.YesButton.Pressed += DeleteEventDialog_YesButton_Pressed;
				deleteEventDialog.NoButton.Pressed += (sender, args) => engine.ExitSuccess("The event was not deleted.");
				deleteEventDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("The event was not deleted.");
				app.Run(deleteEventDialog);
			}
			else
			{
				ShowMessageDialog(engine, "Unable to delete this event as it is created/updated by an integration", "Unable to Delete");
			}
		}

		private void DeleteEventDialog_YesButton_Pressed(object sender, EventArgs e)
		{
			if (eventInfo == null)
			{
				var dialog = new MessageDialog(engine, String.Format("Event ({0}) could not be found", jobId)) { Title = "Unable to retrieve Event" };
				dialog.OkButton.Pressed += (send, args) => engine.ExitSuccess("Unable to retrieve the event.");
				app.ShowDialog(dialog);

				return;
			}

			if (helpers.EventManager.HasOrders(eventInfo.Id))
			{
				var dialog = new MessageDialog(engine, String.Format("Event {0} can't be deleted as it still contains orders", eventInfo.Name)) { Title = "Event not deleted" };
				dialog.OkButton.Pressed += (send, args) => engine.ExitSuccess("Event not deleted.");
				app.ShowDialog(dialog);

				return;
			}

			if (!helpers.EventManager.DeleteEvent(eventInfo.Id))
			{
				var dialog = new MessageDialog(engine, String.Format("Unable to delete event {0}", eventInfo.Name)) { Title = "Unable to delete Event" };
				dialog.OkButton.Pressed += (send, args) => engine.ExitSuccess("Unable to delete the event.");
				app.ShowDialog(dialog);

				return;
			}

			engine.ExitSuccess("The event was successfully deleted.");
		}

		private void ExtendLocksTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (attemptsExtendLocking >= 10)
			{
				extendLocksTimer.Stop();
				return;
			}

			if (eventInfo != null)
			{
				var eventLockInfo = helpers.LockManager.RequestEventLock(eventInfo.Id, extendLock: true);
				if (eventLockInfo.LockUsername.Contains("error"))
				{
					ShowExceptionDialog(engine, new Exception("Extend lock request didn't succeed"));
				}
			}

			attemptsExtendLocking++;
		}

		private Event GetEvent()
		{
			try
			{
				Guid jobGuid;
				if (!Guid.TryParse(jobId, out jobGuid))
				{
					return null;
				}

				return helpers.EventManager.GetEvent(jobGuid);
			}
			catch (Exception e)
			{
				engine.Log("GetEvent|Retrieving Event failed: " + e);
			}

			return null;
		}


		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong during the creation of the new Event.");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private void ShowMessageDialog(Engine engine, string message, string title)
		{
			MessageDialog dialog = new MessageDialog(engine, message) { Title = title };
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

					helpers.LockManager.ReleaseLocks();

					extendLocksTimer.Stop();
					extendLocksTimer.Dispose();
				}

				disposedValue = true;
			}
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

//---------------------------------
// DeleteEventDialog.cs
//---------------------------------