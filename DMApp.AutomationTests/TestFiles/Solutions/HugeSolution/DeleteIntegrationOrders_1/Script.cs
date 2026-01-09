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

namespace DeleteIntegrationOrders_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers; 
		private InteractiveController app;
		private IntegrationOrdersDialog integrationsDialog;
		private ProgressDialog progressDialog;
		private Engine engine;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// Engine.ShowUI();
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				this.engine = engine;
				engine.SetFlag(RunTimeFlags.NoKeyCaching);
				engine.Timeout = TimeSpan.FromHours(10);

				helpers = new Helpers(engine, Scripts.DeleteIntegrationOrders);

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
			app = new InteractiveController(engine);
			integrationsDialog = new IntegrationOrdersDialog(engine);
			progressDialog = new ProgressDialog(engine);

			progressDialog.OkButton.Pressed += (sender, args) =>
			{
				app.ShowDialog(integrationsDialog);
			};

			// Retrieve Integration Orders & Events
			integrationsDialog.InitPlasmaOrders(GetIntegrationOrders(IntegrationType.Plasma));
			integrationsDialog.InitPlasmaEvents(GetIntegrationEvents(IntegrationType.Plasma));

			integrationsDialog.InitFeenixOrders(GetIntegrationOrders(IntegrationType.Feenix));
			integrationsDialog.InitFeenixEvents(GetIntegrationEvents(IntegrationType.Feenix));

			integrationsDialog.InitEbuOrders(GetIntegrationOrders(IntegrationType.Eurovision));
			integrationsDialog.InitEbuOrders(GetIntegrationEvents(IntegrationType.Eurovision));

			integrationsDialog.InitCeitonEvents(GetIntegrationEvents(IntegrationType.Ceiton));

			integrationsDialog.ExitButton.Pressed += (sender, args) => engine.ExitSuccess("OK");

			integrationsDialog.DeleteSelectedPlasmaOrderButton.Pressed += (sender, args) => DeleteIntegrationOrders(integrationsDialog.SelectedPlasmaOrders);
			integrationsDialog.DeleteSelectedPlasmaEventButton.Pressed += (sender, args) => DeleteIntegrationEvents(integrationsDialog.SelectedPlasmaEvent);
			integrationsDialog.DeleteAllPlasmaOrdersButton.Pressed += (sender, args) => DeleteIntegrationOrders(integrationsDialog.PlasmaOrders.ToArray());

			integrationsDialog.DeleteSelectedFeenixOrderButton.Pressed += (sender, args) => DeleteIntegrationOrders(integrationsDialog.SelectedFeenixOrder);
			integrationsDialog.DeleteSelectedFeenixEventButton.Pressed += (sender, args) => DeleteIntegrationEvents(integrationsDialog.SelectedFeenixEvent);
			integrationsDialog.DeleteAllFeenixOrdersButton.Pressed += (sender, args) => DeleteIntegrationOrders(integrationsDialog.FeenixOrders.ToArray());

			integrationsDialog.DeleteSelectedEbuOrderButton.Pressed += (sender, args) => DeleteIntegrationOrders(integrationsDialog.SelectedEbuOrder);
			integrationsDialog.DeleteSelectedEbuEventButton.Pressed += (sender, args) => DeleteIntegrationEvents(integrationsDialog.SelectedEbuEvent);
			integrationsDialog.DeleteAllEbuOrdersButton.Pressed += (sender, args) => DeleteIntegrationOrders(integrationsDialog.EbuOrders.ToArray());

			integrationsDialog.DeleteSelectedCeitonEventButton.Pressed += (sender, args) => DeleteIntegrationEvents(integrationsDialog.SelectedCeitonEvent);
			integrationsDialog.DeleteAllCeitonEventsButton.Pressed += (sender, args) => DeleteIntegrationEvents(integrationsDialog.CeitonEvents.ToArray());

			integrationsDialog.DeleteAllIntegrationsButton.Pressed += (sender, args) =>
			{
				// Delete Orders
				List<ISrmObject> orders = new List<ISrmObject>(integrationsDialog.PlasmaOrders);
				orders.AddRange(integrationsDialog.FeenixOrders);
				orders.AddRange(integrationsDialog.EbuOrders);
				DeleteIntegrationOrders(orders.ToArray());
			};

			// Define dialogs here
			app.Run(integrationsDialog);
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong");
			if (app.IsRunning) app.ShowDialog(dialog); else app.Run(dialog);
		}

		private List<ISrmObject> GetIntegrationOrders(IntegrationType type)
		{
			List<ISrmObject> orders = new List<ISrmObject>();
			foreach (var reservation in DataMinerInterface.ResourceManager.GetReservationInstancesByProperty(helpers, LiteOrder.PropertyNameIntegration, type.ToString()))
			{
				ServiceReservationInstance serviceInstance = reservation as ServiceReservationInstance;
				if (serviceInstance != null && serviceInstance.ContributingResourceID == Guid.Empty)
				{
					orders.Add(new IntegrationOrder
					{
						Type = type,
						ID = serviceInstance.ID,
						Name = serviceInstance.Name,
						Reservation = serviceInstance
					});
				}
			}

			return orders;
		}

		private List<ISrmObject> GetIntegrationEvents(IntegrationType type)
		{
			List<ISrmObject> events = new List<ISrmObject>();

			JobManagerHelper jobManager = new JobManagerHelper(m => engine.SendSLNetMessages(m));
			var sectionDefinitions = jobManager.SectionDefinitions.Read(SectionDefinitionExposers.Name.NotEqual(String.Empty));

			// search for a section with a field named Status
			var defaultSectionDefinition = sectionDefinitions.FirstOrDefault(x => x.GetAllFieldDescriptors().Any(y => y.Name.Equals("Integration", StringComparison.OrdinalIgnoreCase))) ?? throw new NotFoundException("Unable to find Integration section definition");
			var integrationField = defaultSectionDefinition.GetAllFieldDescriptors().FirstOrDefault(d => d.Name.Contains("Integration")) ?? throw new NotFoundException("Unable to find Integration field descriptor");

			var filter = JobExposers.FieldValues.JobField(integrationField.ID).Equal(type.ToString());
			var jobs = jobManager.Jobs.Read(filter);
			foreach (Job job in jobs)
			{
				jobManager.StitchJob(job);
				events.Add(new IntegrationEvent { ID = job.ID.Id, Name = job.GetJobName(), Type = type, Job = job });
			}

			return events;
		}

		private void DeleteIntegrationOrders(params ISrmObject[] srmObjects)
		{
			app.ShowDialog(progressDialog);
			helpers.ProgressReported += (sender, args) => progressDialog.AddProgressLine(args.Progress);

			foreach (IntegrationOrder order in srmObjects.OfType<IntegrationOrder>())
			{
				if (order == null) continue;

				helpers.ReportProgress("Deleting " + order.Name + "...");

				List<ReservationInstance> reservationInstancesToRemove = new List<ReservationInstance>();
				List<ServiceID> servicesToRemove = new List<ServiceID>();
				foreach (var resourceUsage in order.Reservation.ResourcesInReservationInstance)
				{
					var resourceReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, resourceUsage.GUID) as ServiceReservationInstance;
					if (resourceReservationInstance == null) continue;

					reservationInstancesToRemove.Add(resourceReservationInstance);
					servicesToRemove.Add(resourceReservationInstance.ServiceID);
				}

				reservationInstancesToRemove.Add(order.Reservation);

				try
				{
					DataMinerInterface.ResourceManager.RemoveReservationInstances(helpers, reservationInstancesToRemove.ToArray());
					helpers.ReportProgress("Reservations were removed");
					
					foreach (var serviceId in servicesToRemove)
					{
						engine.SendSLNetMessage(new SetDataMinerInfoMessage
						{
							Uia1 = new UIA(new[] { (uint)serviceId.DataMinerID, (uint)serviceId.SID }),
							What = 74
						});

						helpers.ReportProgress("Service " + serviceId.DataMinerID + "/" + serviceId.SID + " was removed");
					}

					helpers.ReportProgress(order.Name + "was deleted");
				}
				catch (Exception e)
				{
					helpers.ReportProgress("Something went wrong when deleting: " + order.Name + "|" + e);
				}
			}

			progressDialog.Finish();
			app.ShowDialog(progressDialog);
		}

		private void DeleteIntegrationEvents(params ISrmObject[] srmObjects)
		{
			app.ShowDialog(progressDialog);
			helpers.ProgressReported += (sender, args) => progressDialog.AddProgressLine(args.Progress);

			JobManagerHelper jobManager = new JobManagerHelper(m => engine.SendSLNetMessages(m));
			foreach (IntegrationEvent eventInfo in srmObjects.OfType<IntegrationEvent>())
			{
				try
				{
					helpers.ReportProgress("Deleting " + eventInfo.Name + "...");
					jobManager.Jobs.Delete(eventInfo.Job);
					helpers.ReportProgress(eventInfo.Name + "was deleted");
				}
				catch (Exception e)
				{
					helpers.ReportProgress("Something went wrong when deleting: " + eventInfo.Name + "|" + e);
				}
			}
		}

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

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}