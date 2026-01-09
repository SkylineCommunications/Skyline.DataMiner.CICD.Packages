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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Messages.SLDataGateway;
using Skyline.DataMiner.Net.Ticketing.Helpers;
using Skyline.DataMiner.Net.Ticketing.Helpers.Visualization;
using Skyline.DataMiner.Net.Tickets;

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
		//Define Colors
		Color Green = Color.FromArgb(0, 90, 204, 101);
		Color Grey = Color.FromArgb(0, 184, 189, 183);
		Color Yellow = Color.FromArgb(0, 229, 232, 63);
		Color Orange = Color.FromArgb(0, 240, 189, 79);
		Color White = Color.FromArgb(0, 255, 255, 255);
		Color Red = Color.FromArgb(0, 242, 145, 145);
		Color Blue = Color.FromArgb(0, 0, 128, 255);


		// Create helper
		var ticketingHelper = new TicketingHelper(engine.SendSLNetMessages);

		// Retrieve the existing resolver////////////////////////////////////////////////////////User Tasks
		var filter = TicketFieldResolverExposers.Name.Equal("User Tasks");
		var resolver = ticketingHelper.TicketFieldResolvers.Read(filter).FirstOrDefault();
		if (resolver == null)
		{
			engine.GenerateInformation("No resolver found with that name");
			return;
		}

		// Change the resolver
		resolver.VisualizationHints = new TicketingVisualizationHints
		{
			VisualFieldEnums = new List<VisualFieldEnum>
				{
					new VisualFieldEnum("State", 1, Grey.ToArgb()),//Incomplete
                    new VisualFieldEnum("State", 2, Green.ToArgb()),//Complete
                    new VisualFieldEnum("State", 3, Green.ToArgb())//Closed
				}
		};
		// Update the resolver on server
		ticketingHelper.TicketFieldResolvers.Update(resolver);

		// Retrieve the existing resolver////////////////////////////////////////////////////////Non-Live Ingest/Export
		filter = TicketFieldResolverExposers.Name.Equal("Ingest/Export");
		resolver = ticketingHelper.TicketFieldResolvers.Read(filter).FirstOrDefault();
		if (resolver == null)
		{
			engine.GenerateInformation("No resolver found with that name");
			return;
		}
		// Change the resolver
		resolver.VisualizationHints = new TicketingVisualizationHints
		{
			VisualFieldEnums = new List<VisualFieldEnum>
				{
					new VisualFieldEnum("State", 0, Grey.ToArgb()),//Preliminary
					new VisualFieldEnum("State", 1, Yellow.ToArgb()),//Submitted
                    new VisualFieldEnum("State", 2, Green.ToArgb()),//Work In Progress
                    new VisualFieldEnum("State", 3, Orange.ToArgb()),//Change Requested
					new VisualFieldEnum("State", 4, White.ToArgb()),//Completed
					new VisualFieldEnum("State", 5, Red.ToArgb())//Cancelled By Customer
				}
		};
		// Update the resolver on server
		ticketingHelper.TicketFieldResolvers.Update(resolver);

		// Retrieve the existing resolver////////////////////////////////////////////////////////Notes
		filter = TicketFieldResolverExposers.Name.Equal("Notes");
		resolver = ticketingHelper.TicketFieldResolvers.Read(filter).FirstOrDefault();
		if (resolver == null)
		{
			engine.GenerateInformation("No resolver found with that name");
			return;
		}
		// Change the resolver
		resolver.VisualizationHints = new TicketingVisualizationHints
		{
			VisualFieldEnums = new List<VisualFieldEnum>
				{
					new VisualFieldEnum("State", 1, Grey.ToArgb()),  //Open
					new VisualFieldEnum("State", 2, White.ToArgb()), //Closed
					new VisualFieldEnum("State", 3, Red.ToArgb()),  //Alarm
					new VisualFieldEnum("State", 4, Blue.ToArgb()) //Alarm Acknowledged
				}
		};

		// Update the resolver on server
		ticketingHelper.TicketFieldResolvers.Update(resolver);
	}
}