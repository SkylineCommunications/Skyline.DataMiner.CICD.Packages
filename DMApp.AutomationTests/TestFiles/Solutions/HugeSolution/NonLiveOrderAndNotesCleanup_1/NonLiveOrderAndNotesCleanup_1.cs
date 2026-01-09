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

namespace NonLiveOrderAndNotesCleanup_1
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Note = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes.Note;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private Helpers helpers;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			helpers = new Helpers(engine, Scripts.NonLiveOrderAndNotesCleanup);

			var allOrders = helpers.NonLiveOrderManager.AllNonLiveOrdersOlderThan(TimeSpan.FromDays(2 * 365));
			var allNotes = helpers.NoteManager.AllNotesOlderThan(TimeSpan.FromDays(2 * 365));

			try
			{
				DeleteOrders(allOrders);
				DeleteNotes(allNotes);
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(Script), nameof(Run), ex.ToString());
			}
			finally
			{
				helpers.Dispose();
			}
		}

		private void DeleteOrders(List<NonLiveOrder> orders)
		{
			List<string> deletedOrders = new List<string>();

			foreach (var order in orders)
			{
				if (ShouldBeDeleted(order))
				{
					helpers.NonLiveOrderManager.DeleteNonLiveOrder((int)order.DataMinerId, (int)order.TicketId);
					deletedOrders.Add(order.ShortDescription + " [" + order.TicketId.ToString() + "]");
				}
			}

			helpers.Log(nameof(Script), nameof(DeleteOrders), "Deleted orders: " + String.Join(", ", deletedOrders));
		}

		private static bool ShouldBeDeleted(NonLiveOrder order)
		{
			bool isIngest = order is Ingest ingestOrder && ingestOrder.BackupDeletionDate < DateTime.Now.AddYears(-1);
			bool isProject = order is Project projectOrder && projectOrder.BackupDeletionDate < DateTime.Now.AddYears(-1);
			bool isFolderCreation = order is FolderCreation folderCreationOrder && folderCreationOrder.EarliestDeletionDate < DateTime.Now.AddYears(-1);
			bool isExport = order is Export || order is Transfer;

			return isIngest || isProject || isFolderCreation || isExport;
		}

		private void DeleteNotes(List<Note> notes)
		{
			List<string> deletedNotes = new List<string>();

			foreach (var note in notes)
			{
				helpers.NoteManager.DeleteNote(note);
				deletedNotes.Add(note.Title + " [" + note.TicketId.ToString() + "]");
			}

			helpers.Log(nameof(Script), nameof(DeleteNotes), "Deleted notes: " + String.Join(", ", deletedNotes));
		}
	}
}