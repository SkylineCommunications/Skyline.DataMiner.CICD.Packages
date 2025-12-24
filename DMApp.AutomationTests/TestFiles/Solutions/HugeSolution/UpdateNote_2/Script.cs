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

namespace UpdateNote_2
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Notes;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Eurovision;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	internal class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController app;
		private AddOrUpdateNoteDialog dialog;

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

					engine.Timeout = TimeSpan.FromHours(10);
					helpers = new Helpers(engine, Scripts.UpdateNote);

					this.app = new InteractiveController(engine);

					string scriptInputParameter = engine.GetScriptParam("FullTicketId").Value;

					var ticketId = TicketingManager.ParseTicketIdsFromScriptInput(scriptInputParameter)[0];
					
					if (!helpers.NoteManager.TryGetNote(ticketId.DataMinerID, ticketId.TID, out Note note))
					{
						ShowMessageDialog(engine, "Unable to get Note", "Error");
						return;
					}

					if (!String.IsNullOrEmpty(note.EurovisionId))
					{
						var synopsis = RetrieveSynopsisWithId(note.EurovisionId);
						if (String.IsNullOrEmpty(synopsis))
						{
							var errorDialog = new MessageDialog(engine, "Unable to retrieve the synopsis for Note: " + scriptInputParameter) { Title = "Unable to retrieve synopsis" };
							errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Synopsis is not available for this note");
							app.Run(errorDialog);
							return;
						}

						EbuDetailsDialog detailsDialog = new EbuDetailsDialog(engine, note.EurovisionId, synopsis);
						detailsDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(String.Empty);
						app.Run(detailsDialog);
					}
					else
					{
						dialog = new AddOrUpdateNoteDialog(engine, note.Page, note);
						dialog.ConfirmButton.Pressed += Dialog_ConfirmButton_Pressed;
						dialog.AcknowledgeButton.Pressed += AcknowledgeButton_Pressed;

						app.Run(dialog);
					}
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
				finally
				{
					helpers?.Dispose();
				}
			});

			timeOutResult = Task.WaitAny(new Task[] { scriptTask }, new TimeSpan(9, 59, 40));
		}

		private string RetrieveSynopsisWithId(string id)
		{
			try
			{
				OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);
				var ebuManager = new EurovisionElement(orderManagerElement.EbuElement);
				return ebuManager.GetSynopsisText(id);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), "RetrieveSynopsisWithId","Something went wrong retrieving the synopsis: " + e);
			}

			return null;
		}

		private void AcknowledgeButton_Pressed(object sender, EventArgs e)
		{
			var updatedNote = dialog.Note;
			updatedNote.Status = Status.AcknowledgedAlarm;

			if (!TryCheckValidityAndUpdateNote(updatedNote)) return;

			ShowMessageDialog((Engine)helpers.Engine, "Successfully acknowledged alarm", "Acknowledge Alarm");
		}

		private void Dialog_ConfirmButton_Pressed(object sender, EventArgs e)
		{
			var updatedNote = dialog.Note;
			if (!TryCheckValidityAndUpdateNote(updatedNote)) return;

			ShowMessageDialog((Engine) helpers.Engine, "Successfully updated the note", "Edit Note");
		}

		private bool TryCheckValidityAndUpdateNote(Note updatedNote)
		{
			if (!dialog.IsValid()) return false;

			helpers.NoteManager.AddOrUpdateNote(updatedNote, out string fullTicketId);

			return true;
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog exceptionDialog = new ExceptionDialog(engine, exception);
			exceptionDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong during editing the Note.");

			if (app.IsRunning) app.ShowDialog(exceptionDialog);
			else app.Run(exceptionDialog);
		}

		private void ShowMessageDialog(Engine engine, string message, string title)
		{
			MessageDialog messageDialog = new MessageDialog(engine, message) { Title = title };
			messageDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(messageDialog);
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