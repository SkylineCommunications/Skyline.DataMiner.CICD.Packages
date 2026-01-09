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

namespace ViewTicket_1
{
	using System;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController app;
		private LoadNonLiveOrderFormDialog loadNonLiveOrderFormDialog;
		private MainDialog nonLiveOrderForm;

		private int timeOutResult;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			var scriptTask = Task.Factory.StartNew(() =>
			{
				try
				{
					Initialize(engine);

					loadNonLiveOrderFormDialog = new LoadNonLiveOrderFormDialog(helpers, LoadNonLiveOrderFormDialog.ScriptAction.View);

					if (loadNonLiveOrderFormDialog.Execute()) ShowNonLiveOrderForm();
					else app.Run(loadNonLiveOrderFormDialog);
				}
				catch (InteractiveUserDetachedException)
				{
					Dispose();
				}
				catch (ScriptAbortException)
				{
					Dispose();
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						engine.Log("Run|Something went wrong: " + e);
						ShowExceptionDialog(e);
					}

					Dispose();
				}
				finally
				{
					Dispose();
				}
			});

			timeOutResult = Task.WaitAny(new[] { scriptTask }, new TimeSpan(9, 59, 40));
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);
			//engine.ShowUI();

			helpers = new Helpers(engine, Scripts.ViewTicket);
			app = new InteractiveController(engine);
		}

		private void ShowNonLiveOrderForm()
		{
			nonLiveOrderForm = loadNonLiveOrderFormDialog.NonLiveOrderFormDialog;
			/*
			nonLiveOrderForm.CancelButton.Pressed += Dialog_CancelButton_Pressed;
			nonLiveOrderForm.BookButton.Pressed += Dialog_SaveOrBookButton_Pressed;
			nonLiveOrderForm.SaveButton.Pressed += Dialog_SaveOrBookButton_Pressed;
			nonLiveOrderForm.RejectOrderButton.Pressed += Dialog_RejectOrderButton_Pressed;
			*/

			if (app.IsRunning) app.ShowDialog(nonLiveOrderForm);
			else app.Run(nonLiveOrderForm);
		}

		private void ShowExceptionDialog(Exception exception)
		{
			ExceptionDialog exceptionDialog = new ExceptionDialog((Engine)helpers.Engine, exception);
			exceptionDialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("Something went wrong during the creation of the new Order.");
			if (app.IsRunning) app.ShowDialog(exceptionDialog); else app.Run(exceptionDialog);
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

		~Script()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
