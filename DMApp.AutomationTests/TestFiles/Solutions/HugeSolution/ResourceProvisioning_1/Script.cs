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

dd/mm/2021	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace ResourceProvisioning_1
{
	using System;
	using Skyline.DataMiner.Automation;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public static void Run(Engine engine)
		{
			if (engine.IsInteractive)
			{
				// Necessary to execute the script in silent mode
				// ShowUI
				// RN 9775
			}

			engine.Timeout = TimeSpan.FromHours(12);

			// execute standard solution script
			var filePath = engine.GetScriptParam("File Path").Value;
			ImportResources(engine, filePath);

			// execute custom scripts that are needed to tweak some resource configurations (not working with standard solution resource excel)
			UpdateDecodingResourceCapabilities(engine);
			UpdateMatrixOutputLBandCapabilities(engine);
		}

		private static void ImportResources(Engine engine, string filePath)
		{
			var script = engine.PrepareSubScript("SRM_DiscoverResources");
			script.Synchronous = true;
			script.PerformChecks = false;
			script.SelectScriptParam("Operation", "Import");
			script.SelectScriptParam("File Path", filePath);
			script.SelectScriptParam("Input Data", @"{""IsSilent"": true}");
			script.StartScript();
		}

		private static void UpdateDecodingResourceCapabilities(Engine engine)
		{
			var script = engine.PrepareSubScript("DecodingResourceCapabilityUpdates");
			script.Synchronous = true;
			script.PerformChecks = false;
			script.StartScript();
		}

		private static void UpdateMatrixOutputLBandCapabilities(Engine engine)
		{
			var script = engine.PrepareSubScript("MatrixOutputLbandResourceCapabilityUpdates");
			script.Synchronous = true;
			script.PerformChecks = false;
			script.StartScript();
		}
	}
}