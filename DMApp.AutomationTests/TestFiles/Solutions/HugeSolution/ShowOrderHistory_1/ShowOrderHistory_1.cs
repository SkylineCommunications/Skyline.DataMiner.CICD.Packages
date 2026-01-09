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
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.History;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script : IDisposable
{
	Helpers helpers;
	InteractiveController app;

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		helpers = new Helpers(engine, Scripts.ShowOrderHistory);

		try
		{
			engine.Timeout = TimeSpan.FromHours(10);
			//engine.ShowUI();

			app = new InteractiveController(engine);

			string orderId = engine.GetScriptParam(1).Value;

			if (!Guid.TryParse(orderId, out Guid orderGuid)) throw new InvalidOperationException($"ID '{orderId}' is not a Guid");

			helpers.AddOrderReferencesForLogging(orderGuid);

			helpers.AddOrderReferencesForLogging(orderGuid);

			Dialog dialogToRun = null;
			if (helpers.OrderManagerElement.TryGetOrderHistory(orderGuid, out var orderHistoryChapters))
			{
				var orderHistoryDialog = new OrderHistoryDialog(helpers, orderHistoryChapters);
				orderHistoryDialog.BackButton.Pressed += (s, a) => engine.ExitSuccess("exit");

				dialogToRun = orderHistoryDialog;
			}
			else
			{
				var dialog = new MessageDialog(helpers.Engine, "No history available for this order") { Title = "Order History" };
				dialog.OkButton.Pressed += (s, a) => engine.ExitSuccess("exit");

				dialogToRun = dialog;
			}
			
			app.Run(dialogToRun);
		}
		catch (InteractiveUserDetachedException)
		{
			// do nothing
		}
		catch (Exception e)
		{
			helpers.Log(nameof(Script),nameof(Run),$"Something went wrong during show order history script execution: {e}");
		}
		finally
		{
			Dispose();
		}	
	}

	

	private static string history = @"[
    {
        'UserName': 'victor scherpereel',
        'ScriptName': 'dummy script',
        'Timestamp': '2022-06-27T11:42:00+02:00',
        'OrderChange': {
            'ServiceChanges': [
                {
                    'ServiceName': 'Dummy [1c8dfb6b-20fd-49e0-89bf-200d8d41bf80]',
                    'PropertyChanges': [
                        {
                            'PropertyName': 'Start',
                            'Change': {
                                'OldValue': '6/07/2022 15:01:00',
                                'NewValue': '6/07/2022 16:01:00'
                            }
                        }
                    ],
                    'FunctionChanges': [
                        {
                            'FunctionLabel': 'function 37353',
                            'FunctionId': '3d0a2430-3237-4393-a037-062a74646636',
                            'ProfileParameterChanges': [
                                {
                                    'ProfileParameterName': 'profile parameter',
                                    'Change': {
                                        'OldValue': 'default value',
                                        'NewValue': 'new value'
                                    }
                                }
                            ],
                            'ResourceChange': {
                                'OldValue': 'dummy resource A',
                                'NewValue': 'None'
                            }
                        },
                        {
                            'FunctionLabel': 'function 41268',
                            'FunctionId': '3d0a2430-3237-4393-a037-062a74646636',
                            'ProfileParameterChanges': [
                                {
                                    'ProfileParameterName': 'profile parameter',
                                    'Change': {
                                        'OldValue': 'default value',
                                        'NewValue': 'new value'
                                    }
                                }
                            ],
                            'ResourceChange': {
                                'OldValue': 'None',
                                'NewValue': 'dummy resource B'
                            }
                        }
                    ],
                    'ClassName': 'Service'
                },
                {
                    'ServiceName': 'Dummy [63e5980f-3765-45fe-891d-9e4ea0424c61]',
                    'FunctionChanges': [
                        {
                            'FunctionLabel': 'function 591343',
                            'FunctionId': 'a8bdeceb-d60e-4fea-a3a5-68f54a55ec09',
                            'ResourceChange': {
                                'OldValue': 'b946e26c-3278-4de2-b239-3a4ce59fbd11',
                                'NewValue': 'None'
                            }
                        }
                    ],
                    'ClassName': 'Service',
                    'PropertyChanges': [
                        {
                            'PropertyName': 'Status',
                            'Change': {
                                'OldValue': 'ServiceCompleted',
                                'NewValue': 'ServiceCompletedWithErrors'
                            }
                        }
                    ]
                }
            ],
            'ClassName': 'Order',
            'PropertyChanges': [
                {
                    'PropertyName': 'Name',
                    'Change': {
                        'OldValue': 'test order name',
                        'NewValue': 'new order name'
                    }
                },
                {
                    'PropertyName': 'ShortDescription',
                    'Change': {
                        'OldValue': 'test order name',
                        'NewValue': 'new order name'
                    }
                },
                {
                    'PropertyName': 'SportsPlanning',
                    'Change': {
                        'OldValue': '',
                        'NewValue': '{\'Sport\':null,\'Description\':null,\'Commentary\':null,\'Commentary2\':null,\'CompetitionTime\':0.0,\'JournalistOne\':null,\'JournalistTwo\':null,\'JournalistThree\':null,\'Location\':null,\'TechnicalResources\':null,\'LiveHighlightsFile\':null,\'RequestedBroadcastTime\':0.0,\'ProductionNumberPlasmaId\':null,\'ProductNumberCeiton\':null,\'CostDepartment\':null,\'AdditionalInformation\':\'new value\'}'
                    }
                }
            ],
            'ClassChanges':[
                {
                    'ClassName': 'BillingInfo',
                    'ClassChanges': [],
                    'PropertyChanges': [
                        {
                            'PropertyName': 'CompetitionTime',
                            'Change': {
                                'OldValue': '1657119689100',
                                'NewValue': '1657126889100'
                            }
                        },
                        {
                            'PropertyName': 'RequestedBroadcastTime',
                            'Change': {
                                'OldValue': '1657119689100',
                                'NewValue': '1657126889100'
                            }
                        }
                    ],
                    'CollectionChanges': []
                }
            ]
        }
    }
]";
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