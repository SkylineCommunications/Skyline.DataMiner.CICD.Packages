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

dd/mm/2020	1.0.0.1		XXX, Skyline	Initial version
22/03/2021	1.0.0.2		GVH, Skyline	Linking between user tasks and non live orders
****************************************************************************
*/



//---------------------------------
// UpdateTicketStatus_4.cs
//---------------------------------

namespace UpdateTicketStatus_4
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;
	using UserGroup = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;

		public enum Type
		{
			[Description("Order")]
			Order = 0,
			[Description("Service")]
			Service = 1,
			[Description("User Task")]
			UserTask = 2
		}

		private static Dictionary<string, List<UserGroup>> userGroupByPage = new Dictionary<string, List<UserGroup>>
		{
			{ "mcr-operator", new List<UserGroup>{ UserGroup.McrOperator, UserGroup.FiberSpecialist, UserGroup.MwSpecialist } },
			{ "mcr-operator-task-list", new List<UserGroup>{ UserGroup.McrOperator, UserGroup.FiberSpecialist, UserGroup.MwSpecialist } },
			{ "mcr-specialist-task-list", new List<UserGroup>{ UserGroup.McrOperator, UserGroup.FiberSpecialist, UserGroup.MwSpecialist } },
			{ "media-operator", new List<UserGroup> { UserGroup.MediaOperator } },
			{ "media-operator-task-list", new List<UserGroup> { UserGroup.MediaOperator } },
			{ "media-operator-messi-news", new List<UserGroup> { UserGroup.MediaOperator } },
			{ "media-operator-messi-live", new List<UserGroup> { UserGroup.MediaOperator } },
			{ "tom-ut", new List<UserGroup>()}, // user from this page should not be able to update user task statuses (DCP196576)
		};

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				helpers = new Helpers(engine, Scripts.UpdateTicketStatus);

				string rawFullTicketId = engine.GetScriptParam("FullTicketId").Value;
				var fullTicketId = rawFullTicketId.Trim('"', '[', ']');

				string[] splitTicketId = fullTicketId.Split('/');
				int[] convertedSplitTicketId = splitTicketId.Length > 1 ? Array.ConvertAll(splitTicketId, Convert.ToInt32) : new int[0];

				Guid.TryParse(fullTicketId, out var parsedId);

				string type = engine.GetScriptParam("Type").Value;
				if (!Enum.TryParse(type.Replace(" ", string.Empty), ignoreCase: true, result: out Type convertedType))
				{
					helpers.Log(nameof(Script), nameof(Run), $"Invalid script param 'type': '{type}'");
					return;
				}

				string page = engine.GetScriptParam("Page").Value;

				if (!userGroupByPage.TryGetValue(page, out var userGroups))
				{
					userGroups = Enum.GetValues(typeof(UserGroup)).Cast<UserGroup>().ToList(); 
				}

				HandleTicketUpdate(convertedType, convertedSplitTicketId, parsedId, fullTicketId, userGroups);
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

		private void HandleTicketUpdate(Type convertedType, int[] convertedSplitTicketId, Guid parsedId, string fullTicketId, List<UserGroup> userGroups)
		{
			switch (convertedType)
			{
				case Type.Order:
					if (UserTaskHandler.TryUpdateAllOrderUserTasks(helpers, parsedId, userGroups))
					{
						helpers.Log(nameof(Script), nameof(HandleTicketUpdate), $"Order with id {fullTicketId} succeeds to update all of his user tasks");
					}
					return;

				case Type.Service:
					if (UserTaskHandler.TryUpdateAllServiceUserTasks(helpers, parsedId, userGroups))
					{
						helpers.Log(nameof(Script), nameof(HandleTicketUpdate), $"Updating service {fullTicketId} user tasks succeeds");
					}
					return;

				case Type.UserTask:
					HandleTicketsOfTypeUserTask(convertedSplitTicketId, parsedId, fullTicketId);
					return;

				default:
					helpers.Log(nameof(Script), nameof(HandleTicketUpdate), $"Invalid type '{convertedType}' was retrieved by Update Ticket Status script");

					return;
			}
		}

		private void HandleTicketsOfTypeUserTask(int[] convertedSplitTicketId, Guid reservationId, string fullTicketId)
		{
			if (UserTaskHandler.TryUpdateUserTask(helpers, fullTicketId, reservationId))
			{
				helpers.Log(nameof(Script), nameof(HandleTicketsOfTypeUserTask), $"Updating user task with id: {fullTicketId} succeeded");
				return;
			}

			if (NoteHandler.TryUpdateNote(helpers, convertedSplitTicketId, reservationId))
			{
				helpers.Log(nameof(Script), nameof(HandleTicketsOfTypeUserTask), $"Updating note with id: {fullTicketId} succeeded");
				return;
			}

			if (NonLiveOrderHandler.TryUpdateNonLiveOrder(helpers, convertedSplitTicketId, reservationId))
			{
				helpers.Log(nameof(Script), nameof(HandleTicketsOfTypeUserTask), $"Updating non live order with id: {fullTicketId} succeeded");
			}
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