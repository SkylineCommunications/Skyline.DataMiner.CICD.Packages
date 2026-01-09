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
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

/// <summary>
/// The public generic Script class.
/// </summary>
public class Script : IDisposable
{
	private Helpers helpers;
	private bool disposedValue;

	/// <summary>
	/// The main Run method used to initialize the execution.
	/// </summary>
	/// <param name="engine">The engine reference.</param>
	public void Run(Engine engine)
	{
		try
		{
			helpers = new Helpers(engine, Scripts.Ericsson_RX8200_Decoding);

			ProfileLoadHandler.ApplyConfiguration(helpers);

			Dispose();
		}
		catch (Exception e)
		{
			helpers?.Log(nameof(Script), nameof(Run), "Something went wrong when configuring the ProfileLoadScript: " + e);
			Dispose();
			throw; // So ApplyProfile would fail inside script which is executing this PLS to update the service status
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				helpers?.Dispose();
			}

			disposedValue = true;
		}
	}

	~Script()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}