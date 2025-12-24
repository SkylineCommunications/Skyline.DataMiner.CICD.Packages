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

03/08/2020	1.0.0.1		TIMGE, Skyline	Initial version
****************************************************************************
*/

using Skyline.DataMiner.Utils.InteractiveAutomationScript;

namespace ShowPebbleBeachDetails_2
{

	public class UIDetailValueLabel : Label
	{
		public UIDetailValueLabel(string text) : base(text)
		{
			if (text == null || text.Equals("-1") || text.Equals("12/29/1899 12:00:00 AM"))
			{
				Text = "Not found";
			}
			else if (text.Length > 200)
			{
				Text = text.Substring(0, 75) + "...";
			}
			else
			{
				Text = text;
			}
		}
	}
}