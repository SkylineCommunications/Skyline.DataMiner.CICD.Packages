namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Orders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Debug_2.Debug.Reservations;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Ticketing;

	public class DeleteOrdersDialog : DebugDialog
	{
		private readonly Label deleteOrdersLabel = new Label("Delete Orders") { Style = TextStyle.Heading };

		private readonly GetReservationsSection getOrderReservationsSection;

		private readonly Button deleteOrdersButton = new Button("Delete Orders") { Width = 200 };

		public DeleteOrdersDialog(Helpers helpers) : base(helpers)
		{
			Title = "Delete Orders";

			getOrderReservationsSection = new GetReservationsSection(helpers);

			Initialize();
			GenerateUi();

			getOrderReservationsSection.AddDefaultPropertyFilter("Type", "Video");
		}

		private void Initialize()
		{
			getOrderReservationsSection.RegenerateUiRequired += GetOrderReservationsSection_RegenerateUi;

			deleteOrdersButton.Pressed += DeleteOrdersButton_Pressed;
		}

		private void GetOrderReservationsSection_RegenerateUi(object sender, EventArgs e)
		{
			getOrderReservationsSection.RegenerateUi();
			GenerateUi();
		}

		private void DeleteOrdersButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				var orderReservations = getOrderReservationsSection.SelectedReservations.Where(r => r.Properties.Dictionary.TryGetValue("Type", out var typeValue) && typeValue.Equals("Video"));

				var sb = new StringBuilder();

				var ticketingHelper = new TicketingGatewayHelper { HandleEventsAsync = false };
				ticketingHelper.RequestResponseEvent += (s, args) => args.responseMessage = Automation.Engine.SLNet.SendSingleResponseMessage(args.requestMessage);

				var resourceManagerHelper = new ResourceManagerHelper();
				resourceManagerHelper.RequestResponseEvent += (s, ea) => ea.responseMessage = Automation.Engine.SLNet.SendSingleResponseMessage(ea.requestMessage);

				var serviceHelper = new ServiceManagerHelper();
				serviceHelper.RequestResponseEvent += (s, args) => args.responseMessage = Automation.Engine.SLNet.SendSingleResponseMessage(args.requestMessage);

				try
				{
					var serviceReservationsToRemove = new HashSet<ReservationInstance>();
					var servicesToRemove = new List<ServiceID>();
					var ticketsToRemove = new List<Ticket>();

					CollectThingsToDelete(orderReservations, sb, ticketingHelper, serviceReservationsToRemove, servicesToRemove, ticketsToRemove);

					var removedOrderReservations = RemoveReservationInstances(orderReservations);
					var removedServiceReservations = RemoveReservationInstances(serviceReservationsToRemove);

					RemoveOrdersFromEvent(removedOrderReservations);
					RemoveOrdersFromOrderManager(removedOrderReservations);
					RemoveServices(servicesToRemove);
					RemoveTickets(ticketingHelper, ticketsToRemove);

					sb.AppendLine($"Removed reservations:");
					foreach (var removedReservation in removedOrderReservations.Union(removedServiceReservations))
					{
						sb.AppendLine($"{removedReservation.Name}({removedReservation.ID})");
					}

					ShowRequestResult($"Successfully deleted orders {DateTime.Now}", sb.ToString());
				}
				catch (Exception ex)
				{
					sb.AppendLine(ex.ToString());
					ShowRequestResult("Exception while deleting", sb.ToString());
				}
			}
		}

		private void CollectThingsToDelete(IEnumerable<ReservationInstance> orderReservations, StringBuilder sb, TicketingGatewayHelper ticketingHelper, HashSet<ReservationInstance> serviceReservationsToRemove, List<ServiceID> servicesToRemove, List<Ticket> ticketsToRemove)
		{
			foreach (var orderReservationInstance in orderReservations.Cast<ServiceReservationInstance>())
			{
				var orderId = orderReservationInstance.ID;

				try
				{
					if (orderReservationInstance.ServiceID != null) servicesToRemove.Add(orderReservationInstance.ServiceID);

					foreach (var resourceUsage in orderReservationInstance.ResourcesInReservationInstance)
					{
						var serviceReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, resourceUsage.GUID) as ServiceReservationInstance ?? throw new ServiceNotFoundException(resourceUsage.GUID);
						serviceReservationsToRemove.Add(serviceReservationInstance);

						if (serviceReservationInstance.ServiceID != null) servicesToRemove.Add(serviceReservationInstance.ServiceID);

						var userTaskTickets = ticketingHelper.GetTickets(filter: TicketingExposers.CustomTicketFields.DictStringField("Service ID").Equal(resourceUsage.GUID.ToString()));
						ticketsToRemove.AddRange(userTaskTickets);
					}
				}
				catch (Exception ex1)
				{
					sb.AppendLine($"Exception while finding order ID {orderId} and its contributing resources: {ex1}");
				}

				try
				{
					var reservationsLinkedToOrder = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, ReservationInstanceExposers.Properties.DictStringField(ServicePropertyNames.OrderIdsPropertyName).Equal(orderId.ToString()));

					foreach (var serviceReservation in reservationsLinkedToOrder)
					{
						serviceReservationsToRemove.Add(serviceReservation);
					}
				}
				catch (Exception ex2)
				{
					sb.AppendLine($"Exception while finding reservations with OrderIds property equal to {orderId}: {ex2}");
				}
			}
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private List<ReservationInstance> RemoveReservationInstances(IEnumerable<ReservationInstance> reservationInstances)
		{
			var removedReservations = new List<ReservationInstance>();
			try
			{
				DataMinerInterface.ResourceManager.RemoveReservationInstances(helpers, reservationInstances.ToArray());
				removedReservations.AddRange(reservationInstances);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveReservationInstances), $"Exception removing reservations: {e}");
			}

			return removedReservations;
		}

		private void RemoveServices(List<ServiceID> services)
		{
			foreach (var serviceId in services)
			{
				try
				{
					helpers.Engine.SendSLNetMessage(new SetDataMinerInfoMessage
					{
						Uia1 = new UIA(new[] { (uint)serviceId.DataMinerID, (uint)serviceId.SID }),
						What = 74
					});
				}
				catch (Exception e)
				{
					helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveServices), $"Exception removing service {serviceId}: {e}");
				}
			}
		}

		private void RemoveTickets(TicketingGatewayHelper ticketingHelper, List<Ticket> tickets)
		{
			if (tickets.Any())
			{
				try
				{
					if (!ticketingHelper.RemoveTickets(out var error, tickets.ToArray()))
					{
						helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveTickets), String.Format("Error removing tickets: {0}", error));
					}
				}
				catch (Exception e)
				{
					helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveTickets), String.Format("Exception removing tickets: {0}", e));
				}
			}
		}

		private void RemoveOrdersFromEvent(List<ReservationInstance> orderReservationInstances)
		{
			var helper = new JobManagerHelper(m => helpers.Engine.SendSLNetMessages(m));

			foreach (var orderReservationInstance in orderReservationInstances)
			{
				var eventIdPropertyValue = orderReservationInstance.Properties.FirstOrDefault(p => p.Key == "EventId").Value;
				if (eventIdPropertyValue == null)
				{
					helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveOrdersFromEvent), "Order EventId property is null");
					continue;
				}

				if (!Guid.TryParse(Convert.ToString(eventIdPropertyValue), out var eventId))
				{
					helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveOrdersFromEvent), String.Format("Order EventId property is not a valid Guid: {0}", eventIdPropertyValue));
					continue;
				}

				if (!TryGetJob(helper, eventId, out var job))
				{
					continue;
				}		

				TryRemoveOrderSection(helper, orderReservationInstance, eventId, job);
			}
		}

		private bool TryGetJob(JobManagerHelper helper, Guid eventId, out Job job)
		{
			try
			{
				job = helper.Jobs.Read(JobExposers.ID.Equal(eventId)).FirstOrDefault();
				if (job == null)
				{
					helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveOrdersFromEvent), String.Format("Job {0} not found", eventId));
					return false;
				}

				return true;
			}
			catch (Exception e)
			{
				job = null;
				helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveOrdersFromEvent), String.Format("Exception retrieving job '{0}': {1}", eventId, e));
				return false;
			}
		}

		private void TryRemoveOrderSection(JobManagerHelper helper, ReservationInstance orderReservationInstance, Guid eventId, Job job)
		{
			try
			{
				var sectionsToRemove = new List<Skyline.DataMiner.Net.Sections.Section>();
				foreach (var section in job.Sections)
				{
					foreach (var fieldValue in section.FieldValues)
					{
						bool sectionContainsOrderId = fieldValue.Value.Type == typeof(Guid);
						if (sectionContainsOrderId && (Guid)fieldValue.Value.Value == orderReservationInstance.ID)
						{
							sectionsToRemove.Add(section);

						}
					}
				}

				foreach (var sectionToRemove in sectionsToRemove)
				{
					job.Sections.Remove(sectionToRemove);
				}

				helper.Jobs.Update(job);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveOrdersFromEvent), String.Format("Exception removing order '{0}' from job '{1}': {2}", orderReservationInstance.ID, eventId, e));
			}
		}

		private void RemoveOrdersFromOrderManager(List<ReservationInstance> orderReservationInstances)
		{
			var orderManagerElement = helpers.Engine.FindElementsByProtocol("Finnish Broadcasting Company Order Manager").FirstOrDefault();

			foreach (var orderReservationInstance in orderReservationInstances)
			{
				try
				{
					orderManagerElement.SetParameterByPrimaryKey(2006, orderReservationInstance.ID.ToString(), "Delete");
				}
				catch (Exception e)
				{
					helpers.Log(nameof(DeleteOrdersDialog), nameof(RemoveOrdersFromOrderManager), String.Format("Exception removing order '{0}' from order manager element: {1}", orderReservationInstance.ID, e));
				}
			}

		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(deleteOrdersLabel, ++row, 0, 1, 5);

			AddSection(getOrderReservationsSection, ++row, 0);
			row += getOrderReservationsSection.RowCount;

			AddWidget(deleteOrdersButton, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			row++;
			foreach (var responseSection in responseSections)
			{
				responseSection.Collapse();
				AddSection(responseSection, row, 0);
				row += responseSection.RowCount;
			}
		}
	}
}
