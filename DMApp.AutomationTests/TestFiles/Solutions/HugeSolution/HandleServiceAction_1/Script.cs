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
****************************************************************************
*/

namespace HandleServiceAction_1
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;

		public void Run(Engine engine)
		{
			try
			{				
				Initialize(engine);

				var serviceReservationId = Guid.Parse(engine.GetScriptParam("ReservationGuid").Value);

				var service = helpers.ServiceManager.GetService(serviceReservationId) ?? throw new ServiceNotFoundException(serviceReservationId); 

				var orderReservationId = service.OrderReferences.First();

				helpers.AddOrderReferencesForLogging(orderReservationId);

				var rawAction = engine.GetScriptParam("Action").Value.ToLower();
				var rawBookingManagerInfo = engine.GetScriptParam("Booking Manager Info").Value;

				Log(nameof(Run), $"Launching script UpdateServiceStatus asynchronous from Handle Service Action");

				engine.SendSLNetSingleResponseMessage(new ExecuteScriptMessage("UpdateServiceStatus")
				{
					Options = new SA(new[]
					{
						$"PARAMETER:3:{rawAction}",
						$"PARAMETER:4:{rawBookingManagerInfo}",
						$"PARAMETER:5:{serviceReservationId}",
						"OPTIONS:0",
						"CHECKSETS:FALSE",
						"EXTENDED_ERROR_INFO",
						"DEFER:TRUE" // async execution
		            })
				});
			}
			catch (Exception e)
			{
				Log(nameof(Run), $"Exception handling service action: {e}");
			}
			finally
			{
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(1);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);

			this.helpers = new Helpers(engine, Scripts.HandleServiceAction);
		}

		private void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers?.Log(nameof(Script), nameOfMethod, message, nameOfObject);
		}

		#region IDisposable Support
		private bool disposedValue; // To detect redundant calls

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

		// override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
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

			// Uncomment the following line if the finalizer is overridden above.
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}