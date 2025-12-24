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

13/03/2020	1.0.0.1		JVT, Skyline	Initial version

****************************************************************************
*/

namespace HandleEventAction_3
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using EventStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Status;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script
	{
		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			engine.Timeout = TimeSpan.FromMinutes(30);

			var helpers = new Helpers(engine, Scripts.HandleEventAction);

			try
			{
				var eventIds = JsonConvert.DeserializeObject<List<string>>(engine.GetScriptParam("EventIds").Value).Select(x => Guid.Parse(x)).ToList();
				var status = engine.GetScriptParam("Status").Value.GetEnumValue<EventStatus>();

				foreach (var eventId in eventIds)
				{
					if (!helpers.EventManager.UpdateEventStatus(eventId, status))
					{
						helpers.Log(nameof(Script), nameof(Run), $"Event {eventId} status could not be updated to {status}");
					}
				}	
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception: {e}");
			}
			finally
			{
				helpers.Dispose();
			}
		}
	}
}