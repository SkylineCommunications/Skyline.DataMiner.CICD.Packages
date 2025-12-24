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

20/10/2021	1.0.0.1		TRE, Skyline	Initial version
****************************************************************************
*/

namespace PollCeitonResources_1
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Ceiton;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Ceiton;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public class Script
	{
		private InteractiveController app;
		private CeitonElement ceiton;
		private Helpers helpers;

		/// <summary>
		/// The Script entry point.
		/// Engine.ShowUI();
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				engine.SetFlag(RunTimeFlags.NoKeyCaching);
				engine.Timeout = TimeSpan.FromHours(10);
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception e)
			{
				engine.Log("Run|Something went wrong: " + e);
				ShowExceptionDialog(engine, e);
			}
		}

		private void RunSafe(Engine engine)
		{
			helpers = new Helpers(engine, Scripts.PollCeitonResources);
			app = new InteractiveController(engine);
			var orderManagerElement = new OrderManagerElement(helpers);
			ceiton = new CeitonElement(orderManagerElement.CeitonElement);

			CeitonResourcePollingDialog dialog = new CeitonResourcePollingDialog(engine);
			dialog.SendRequestButton.Pressed += (sender, args) =>
			{
				if (!dialog.IsValid()) return;
				switch (dialog.SelectedResourceType)
				{
					case ResourceType.Project:
						var projectRequest = ceiton.PollProject(dialog.ResourceId);
						dialog.AddRequestSection(projectRequest);
						return;
					case ResourceType.Product:
						var productRequest = ceiton.PollProduct(dialog.ResourceId);
						dialog.AddRequestSection(productRequest);
						return;
					default:
						// Unsupported Resource Type
						return;
				}
			};

			app.Run(dialog);
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong during the creation of the new event.");
			if (app.IsRunning) app.ShowDialog(dialog); else app.Run(dialog);
		}
	}
}