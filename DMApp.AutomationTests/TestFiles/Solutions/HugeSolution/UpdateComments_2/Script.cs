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

15/02/2021	1.0.0.1		TRE, Skyline	Initial version
****************************************************************************
*/

namespace UpdateComments_2
{
	using System;
	using System.Threading.Tasks;
	using System.Timers;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Comments;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		// Do not remove this!
		// Engine.ShowUI();

		private Helpers helpers;
		private InteractiveController interactiveController;

		private readonly Timer extendLocksTimer = new Timer();

		private int timeOutResult;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			var scriptTask = Task.Factory.StartNew(() =>
			{
				try
				{
					Initialize(engine);

					RunSafe();
				}
				catch (ScriptAbortException)
				{
					helpers.LockManager?.ReleaseLocks();
					Dispose();
				}
				catch (InteractiveUserDetachedException)
				{
					helpers.LockManager?.ReleaseLocks();
					Dispose();
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						helpers.LockManager?.ReleaseLocks();
						helpers.Log(nameof(Script), nameof(Run),"Something went wrong: " + e);
						Dispose();
						ShowExceptionDialog(engine, e);
					}
				}
			});

			timeOutResult = Task.WaitAny(new [] { scriptTask }, new TimeSpan(9, 59, 40));
			if (timeOutResult == -1)
			{
				helpers.LockManager?.ReleaseLocks();
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);

			helpers = new Helpers(engine, Scripts.UpdateComments);

			interactiveController = new InteractiveController(engine);
		}

		private void RunSafe()
		{
			string scriptInput = helpers.Engine.GetScriptParam("ID").Value;

			bool isLiveVideoOrder = Guid.TryParse(scriptInput, out var orderGuid);

			if (isLiveVideoOrder)
			{
				LiveOrderUpdateCommentHandler.Run(helpers, interactiveController, extendLocksTimer);
			}
			else
			{
				NonLiveOrderUpdateCommentHandler.Run(helpers, interactiveController);
			}
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			var dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(String.Empty);

			if (interactiveController.IsRunning) interactiveController.ShowDialog(dialog);
			else interactiveController.Run(dialog);
		}


		#region IDisposable Support
		private bool disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					extendLocksTimer.Stop();
					extendLocksTimer.Dispose();
				}

				disposedValue = true;
			}
		}

		~Script()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}