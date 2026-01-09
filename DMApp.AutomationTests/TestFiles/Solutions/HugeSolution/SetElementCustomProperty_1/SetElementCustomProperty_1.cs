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

namespace SetElementCustomProperty_1
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script : IDisposable
	{
		private bool disposedValue;

		private Helpers helpers;
		private Element element;

		private string propertyNameInputParameter;

		private CommentsDialog commentsDialog;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{			
			try
			{
				Initialize(engine);
				var controller = new InteractiveController(engine);
				controller.Run(commentsDialog);
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

		private void Initialize(Engine engine)
		{
			//// engine.ShowUI();
			engine.Timeout = TimeSpan.FromHours(1);
			helpers = new Helpers(engine, Scripts.SetElementCustomProperty);

			var elementNameInputParameter = helpers.Engine.GetScriptParam("Element Name").Value;
			propertyNameInputParameter = helpers.Engine.GetScriptParam("Property Name").Value;

			if (!string.IsNullOrWhiteSpace(elementNameInputParameter)) element = helpers.Engine.FindElement(elementNameInputParameter);
			var existingPropertyValue = GetExistingPropertyValue();

			commentsDialog = new CommentsDialog(engine, existingPropertyValue);
			commentsDialog.OkButton.Pressed += (s, e) => SetPropertyValue(engine, commentsDialog.MessageTextBox.Text);
			commentsDialog.CancelButton.Pressed += (s, e) => engine.ExitSuccess("Set element custom property set got canceled");
		}

		private void SetPropertyValue(Engine engine, string propertyValue)
		{
			if (!string.IsNullOrWhiteSpace(propertyNameInputParameter) && !string.IsNullOrWhiteSpace(propertyValue))
			{
				if (element != null)
				{
					element.SetPropertyValue(propertyNameInputParameter, propertyValue);
					Retry(() => { return element.GetPropertyValue(propertyNameInputParameter) == propertyValue; }, TimeSpan.FromSeconds(5)); // Checking if property is eventually updated
				}
			}

			engine.ExitSuccess("Success");
		}

		private string GetExistingPropertyValue()
		{
			if (element != null && !string.IsNullOrWhiteSpace(propertyNameInputParameter))
			{
				return element.GetPropertyValue(propertyNameInputParameter);
			}

			return string.Empty;
		}

		/// <summary>
		/// Retry until success or until timeout. 
		/// </summary>
		/// <param name="func">Operation to retry.</param>
		/// <param name="timeout">Max TimeSpan during which the operation specified in <paramref name="func"/> can be retried.</param>
		/// <returns><c>true</c> if one of the retries succeeded within the specified <paramref name="timeout"/>. Otherwise <c>false</c>.</returns>
		private static bool Retry(Func<bool> func, TimeSpan timeout)
		{
			bool success = false;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			do
			{
				success = func();
				if (!success)
				{
					System.Threading.Thread.Sleep(100);
				}
			}
			while (!success && sw.Elapsed <= timeout);

			return success;
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
}