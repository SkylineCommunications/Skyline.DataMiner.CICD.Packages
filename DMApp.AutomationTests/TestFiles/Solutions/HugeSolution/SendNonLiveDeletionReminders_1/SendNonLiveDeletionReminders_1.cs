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
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script : IDisposable
{
	private bool disposedValue;

	private Helpers helpers;

	private string[] inputTicketIds;

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		try
		{
			Initialize(engine);

			foreach (var fullTicketId in inputTicketIds)
			{
				if (helpers.NonLiveUserTaskManager.TryGetNonLiveUserTask(fullTicketId, out var userTask))
				{
					userTask.SendReminderMail();
				}
			}
		}
		catch (Exception e)
		{
			helpers.Log(nameof(Script), nameof(Run), $"Exception occurred: {e}");
		}
		finally
		{
			Dispose();
		}
	}

	private void Initialize(IEngine engine)
	{
		engine.Timeout = TimeSpan.FromHours(1);
		helpers = new Helpers(engine, Scripts.SendNonLiveDeletionReminders);

		var rawScriptInputParameter = helpers.Engine.GetScriptParam("ticketId").Value;

		helpers.Log(nameof(Script), nameof(Initialize), $"Ticket id input parameter value: {rawScriptInputParameter}");

		try
		{
			inputTicketIds = JsonConvert.DeserializeObject<string[]>(rawScriptInputParameter);
		}
		catch (Exception e)
		{
			helpers.Log(nameof(Script), nameof(Initialize), $"Exception while parsing ticket id input property value {rawScriptInputParameter}: {e}");
		}
	}

	#region IDisposable Support
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