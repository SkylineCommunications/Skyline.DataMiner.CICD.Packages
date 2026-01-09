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

using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Type = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script : IDisposable
{
	private bool disposedValue;

	private Helpers helpers;

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		engine.Timeout = TimeSpan.FromHours(1);
		helpers = new Helpers(engine, Scripts.HandleNonLiveFolderDeletion);

		try
		{
			var now = DateTime.Now.ToUniversalTime().Date;
			var maxLookupTime = now.AddDays(15).Date; // All user tasks which will be tagged as deleted in a 14 days timespan. (15 days to be sure all of them are in there)
			if (!helpers.NonLiveUserTaskManager.TryGetAllFutureUserTasks(now, maxLookupTime, out var allFutureUserTasks))
			{
				helpers.Log(nameof(Script), nameof(Run), $"Failed to retrieve all pending user tasks in the system.");
			}
			else
			{
				foreach (var futureUserTask in allFutureUserTasks)
				{
					UpdateNonLiveUserTask(futureUserTask);
				}
			}
		}
		catch (ScriptAbortException)
		{
			// do nothing
		}
		catch (Exception e)
		{
			helpers.Log(nameof(Script), nameof(Run), "Something went wrong: " + e);
		}
		finally
		{
			Dispose();
		}
	}

	private void UpdateNonLiveUserTask(NonLiveUserTask nonLiveUserTask)
	{
		try
		{
			var now = DateTime.Now;
			var timeUntilDeletionDate = nonLiveUserTask.DeleteDate.Date - now.Date;
			
			nonLiveUserTask.Status = nonLiveUserTask.LinkedOrderType == Type.IplayFolderCreation ? UserTaskStatus.DeleteDateNear : UserTaskStatus.BackupDeleteDateNear;

			nonLiveUserTask.AddOrUpdate(helpers);
			bool reminderMailShouldBeSend = timeUntilDeletionDate == TimeSpan.FromDays(1) || timeUntilDeletionDate == TimeSpan.FromDays(14);
			if (reminderMailShouldBeSend)
			{
				nonLiveUserTask.SendReminderMail();
			}
		}
		catch (Exception e)
		{
			helpers.Log(nameof(Script), nameof(UpdateNonLiveUserTask), $"Something went wrong during user task add or update with ticket id {nonLiveUserTask.ID}: {e}");
		}
	}

	#region IDisposable Support
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue && disposing)
		{
			helpers.Dispose();	
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