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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ExtensionMethods;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Messages;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		engine.Timeout = TimeSpan.FromMinutes(15);
		engine.SetFlag(RunTimeFlags.AllowUndef);
		engine.SetFlag(RunTimeFlags.NoInformationEvents);

		foreach (var alarm in engine.GetActiveAlarms().Where(alarm => alarm.Status == "Clearable"))
		{
			try
			{
				if (alarm.TimeOfArrival < DateTime.Now.AddMinutes(-15))
				{
					engine.ClearAlarm(alarm);
				}
			}
			catch (Exception e)
			{
				engine.GenerateInformation(string.Format("Failed to clear alarm {0} due to Exception: {1}", alarm.ToString(), e.Message));
			}
		}
	}

}

namespace ExtensionMethods
{
	public static class Extensions
	{
		/// <summary>
		/// Clears a DataMiner Alarm
		/// </summary>
		/// <param name="engine">Skyline.DataMiner.Automation.Engine instance</param>
		/// <param name="alarm">AlarmEventMessage object with information of the alarm to clear</param>
		public static void ClearAlarm(this Engine engine, AlarmEventMessage alarm)
		{
			engine.SendSLNetMessage(new SetAlarmStateMessage(alarm.DataMinerID, alarm.AlarmID, 7, string.Empty));
			//engine.GenerateInformation("Cleared Alarm: " + alarm.ToString());
		}

		/// <summary>
		/// Gets the active alarms
		/// </summary>
		/// <param name="engine">Skyline.DataMiner.Automation.Engine instance</param>
		/// <returns>An AlarmEventMessage array with the active alarms</returns>
		public static AlarmEventMessage[] GetActiveAlarms(this Engine engine)
		{
			return (engine.SendSLNetMessage(new GetActiveAlarmsMessage())[0] as ActiveAlarmsResponseMessage).ActiveAlarms;
		}
	}
}
