namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.ConnectorAPI.YLE.OrderManager.Messages;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallBulk;
	using Skyline.DataMiner.Core.InterAppCalls.Common.Shared;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Time;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using ServiceConfiguration = Service.Configuration.ServiceConfiguration;

	public class OrderManagerElement
	{
		private readonly Helpers helpers;
		private readonly Element element;

		private TimeSpan enableResourceElementTimeout;

		private IDmsElement feenixElement;
		private IDmsElement plasmaElement;
		private IDmsElement pebbleBeachElement;
		private IDmsElement ceitonElement;
		private IDmsElement ebuElement;

		// cached table data
		private IDictionary<string, object[]> deviceAutomationConfigurationTable;
		private IDictionary<string, object[]> plasmaNewsInclusionConfigurationTable;
		private IDictionary<string, object[]> plasmaNewsRecordingConfigurationTable;
		private IDictionary<string, object[]> serviceResourceAllocationWindowTable;
		private IDictionary<string, object[]> autoHiddenRecordingOrderConfigurationTable;
		private IDictionary<string, object[]> evsMessiNewsTargetsTable;

		public OrderManagerElement(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.element = helpers.Engine.FindElementsByProtocol(OrderManagerProtocol.Name).SingleOrDefault(e => e.IsActive) ?? throw new ElementByProtocolNotFoundException(OrderManagerProtocol.Name);
		}

		public Element Element => element;

		public IDmsElement FeenixElement
		{
			get
			{
				if (feenixElement != null) return feenixElement;

				string feenixElementId = Convert.ToString(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.FeenixElementIdParameterId));
				feenixElement = Engine.SLNetRaw.GetDms().GetElement(new DmsElementId(feenixElementId));

				return feenixElement;
			}
			set => feenixElement = value;
		}

		public IDmsElement PlasmaElement
		{
			get
			{
				if (plasmaElement != null) return plasmaElement;

				string plasmaElementId = Convert.ToString(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.PlasmaElementIdParameterId));
				plasmaElement = Engine.SLNetRaw.GetDms().GetElement(new DmsElementId(plasmaElementId));

				return plasmaElement;
			}
			set => plasmaElement = value;
		}

		public IDmsElement PebbleBeachElement
		{
			get
			{
				if (pebbleBeachElement != null) return pebbleBeachElement;

				string pbsElementId = Convert.ToString(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.PebbleBeachElementIdParameterId));
				pebbleBeachElement = Engine.SLNetRaw.GetDms().GetElement(new DmsElementId(pbsElementId));

				return pebbleBeachElement;
			}
			set => pebbleBeachElement = value;
		}

		public IDmsElement CeitonElement
		{
			get
			{
				if (ceitonElement != null) return ceitonElement;

				string ceitonElementId = Convert.ToString(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.CeitonElementIdParameterId));
				ceitonElement = Engine.SLNetRaw.GetDms().GetElement(new DmsElementId(ceitonElementId));

				return ceitonElement;
			}
			set => ceitonElement = value;
		}

		public IDmsElement EbuElement
		{
			get
			{
				if (ebuElement != null) return ebuElement;

				string ebuElementId = Convert.ToString(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.EbuElementIdParameterId));
				ebuElement = Engine.SLNetRaw.GetDms().GetElement(new DmsElementId(ebuElementId));

				return ebuElement;
			}
			set => ebuElement = value;
		}

		/// <summary>
		/// Gets the status of the Ceiton integration indicating if it's enabled or not.
		/// </summary>
		public bool IsCeitonIntegrationEnabled => GetStatusFromParameter(OrderManagerProtocol.CeitonStatusParameterId);

		/// <summary>
		/// Gets the status of the Plasma integration indicating if it's enabled or not.
		/// </summary>
		public bool IsPlasmaIntegrationEnabled => GetStatusFromParameter(OrderManagerProtocol.PlasmaStatusParameterId);

		/// <summary>
		/// Gets the status of the Feenix integration indicating if it's enabled or not.
		/// </summary>
		public bool IsFeenixIntegrationEnabled => GetStatusFromParameter(OrderManagerProtocol.FeenixStatusParameterId);

		/// <summary>
		/// Gets the status of the EBU integration indicating if it's enabled or not.
		/// </summary>
		public bool IsEbuIntegrationEnabled => GetStatusFromParameter(OrderManagerProtocol.EbuStatusParameterId);

		/// <summary>
		/// Gets the status of the Pebble Beach integration indicating if it's enabled or not.
		/// </summary>
		public bool IsPebbleBeachIntegrationEnabled => GetStatusFromParameter(OrderManagerProtocol.PebbleBeachStatusParameterId);

		public TimeSpan EnableResourceElementTimeout
		{
			get
			{
				if (enableResourceElementTimeout == default)
				{
					enableResourceElementTimeout = TimeSpan.FromSeconds(Convert.ToDouble(element.GetParameter(OrderManagerProtocol.EnableResourceElementTimeoutPid)));
				}

				return enableResourceElementTimeout;
			}
		}

		public bool TryGetPlasmaNewsInclusion(string programTitle, out bool included)
		{
			included = false;

			try
			{
				plasmaNewsInclusionConfigurationTable = plasmaNewsInclusionConfigurationTable ?? DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.PlasmaNewsInclusionConfigurationTable.TablePid);
			}
			catch (Exception)
			{
				Log(nameof(TryGetPlasmaNewsInclusion), $"Unable to find Plasma News Inclusion Configuration table.");
				return false;
			}

			if (plasmaNewsInclusionConfigurationTable is null || plasmaNewsInclusionConfigurationTable.Values.Any(row => row.Count() < 2))
			{
				Log(nameof(TryGetPlasmaNewsInclusion), $"Plasma News Inclusion Configuration table has unexpected format.");
				return false;
			}

			var matchingRows = plasmaNewsInclusionConfigurationTable.Values.Where(row => Regex.IsMatch(Convert.ToString(row[0]), programTitle, RegexOptions.IgnoreCase));

			if (!matchingRows.Any())
			{
				Log(nameof(TryGetPlasmaNewsInclusion), $"Unable to get inclusion as Plasma News Inclusion Configuration table has no row where regex matches '{programTitle}'.");
				return false;
			}

			var mostMatchingRow = matchingRows.OrderByDescending(row => Convert.ToString(row[0]).Length).First(); // the row with the longest value is the most specific one

			included = Convert.ToBoolean(Convert.ToInt16(mostMatchingRow[1]));

			Log(nameof(TryGetPlasmaNewsInclusion), $"Program title '{programTitle}' is {(included ? "included" : "excluded")} as news program. According to the config for{Convert.ToString(mostMatchingRow[0])}");

			return true;
		}

		public bool TryGetPlasmaNewsRecordingConfiguration(string programTitle, out bool shouldAddPgmNewsRecording)
		{
			shouldAddPgmNewsRecording = false;

			try
			{
				plasmaNewsRecordingConfigurationTable = plasmaNewsRecordingConfigurationTable ?? DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.PlasmaNewsRecordingConfigurationTable.TablePid);
			}
			catch (Exception)
			{
				Log(nameof(TryGetPlasmaNewsRecordingConfiguration), $"Unable to find Plasma News Recording Configuration table.");
				return false;
			}

			if (plasmaNewsRecordingConfigurationTable is null || plasmaNewsRecordingConfigurationTable.Values.Any(row => row.Count() < 2))
			{
				Log(nameof(TryGetPlasmaNewsRecordingConfiguration), $"Plasma News Recording Configuration table has unexpected format.");
				return false;
			}

			var matchingRow = plasmaNewsRecordingConfigurationTable.Values.SingleOrDefault(row => Regex.IsMatch(Convert.ToString(row[0]), programTitle, RegexOptions.IgnoreCase));

			if (matchingRow is null)
			{
				Log(nameof(TryGetPlasmaNewsRecordingConfiguration), $"Unable to get Plasma News Recording requirement as Plasma News Recording Configuration table has no row where regex matches '{programTitle}'.");
				return false;
			}

			shouldAddPgmNewsRecording = Convert.ToBoolean(Convert.ToInt16(matchingRow[1]));

			Log(nameof(TryGetPlasmaNewsRecordingConfiguration), $"Based on Program title '{programTitle}' a PGM news recording should {(shouldAddPgmNewsRecording ? string.Empty : " not")} be added");

			return true;
		}

		public List<Guid> GetFailedOrderIds()
		{
			var ordersTable = DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.OrdersTable.TablePid);

			return ordersTable.Values.Where(row => Convert.ToInt32(row[OrderManagerProtocol.OrdersTable.BookServicesStatusParameterId]) == 4 /*Fail*/).Select(row => Guid.Parse(Convert.ToString(row[0]))).ToList();
		}

		public void AddOrUpdateOrderHistory(Guid orderId, OrderHistoryChapter chapter)
		{
			var commands = InterAppCallFactory.CreateNew();
			commands.ReturnAddress = new ReturnAddress(element.DmaId, element.ElementId, 9000001);

			var message = new AddOrUpdateOrderHistoryMessage
			{
				OrderId = orderId,
				OrderHistoryChapter = chapter.ToString(),
			};

			commands.Messages.Add(message);

			var knownTypes = new List<Type> { typeof(IInterAppCall), typeof(AddOrUpdateOrderHistoryMessage), typeof(GetOrderHistoryRequest), typeof(GetOrderHistoryResponse) };

			commands.Send(Engine.SLNetRaw, element.DmaId, element.ElementId, 9000000, knownTypes);
		}

		public bool TryGetOrderHistory(Guid orderId, out List<OrderHistoryChapter> orderHistoryChapters)
		{
			try
			{
				orderHistoryChapters = GetOrderHistory(orderId);
				return true;
			}
			catch (Exception ex)
			{
				Log(nameof(TryGetOrderHistory), $"Exception while getting order history for ID {orderId}: {ex}");
				orderHistoryChapters = new List<OrderHistoryChapter>();
				return false;
			}
		}

		public List<OrderHistoryChapter> GetOrderHistory(Guid orderId)
		{
			return JsonConvert.DeserializeObject<List<OrderHistoryChapter>>(GetSerializedOrderHistory(orderId));
		}

		public string GetSerializedOrderHistory(Guid orderId)
		{
			var commands = InterAppCallFactory.CreateNew();
			commands.ReturnAddress = new ReturnAddress(element.DmaId, element.ElementId, 9000001);

			var message = new GetOrderHistoryRequest
			{
				OrderId = orderId,
			};

			commands.Messages.Add(message);

			var knownTypes = new List<Type> { typeof(IInterAppCall), typeof(AddOrUpdateOrderHistoryMessage), typeof(GetOrderHistoryRequest), typeof(GetOrderHistoryResponse) };

			var responses = commands.Send(Engine.SLNetRaw, element.DmaId, element.ElementId, 9000000, TimeSpan.FromSeconds(2), knownTypes);

			return (responses.Single() as GetOrderHistoryResponse).OrderHistory;
		}

		public bool IsDeviceAutomationEnabled(Service service)
		{
			if (service is null) throw new ArgumentNullException(nameof(service));

			return IsDeviceAutomationEnabled(service.Definition.Name, service.IntegrationType);
		}

		public bool IsDeviceAutomationEnabled(string serviceDefinitionName, IntegrationType integrationType)
		{
			serviceDefinitionName = serviceDefinitionName.Trim('_');

			deviceAutomationConfigurationTable = deviceAutomationConfigurationTable ?? DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.DeviceAutomationConfigurationTable.TablePid);

			if (deviceAutomationConfigurationTable is null || deviceAutomationConfigurationTable.Values.Any(row => row.Count() < 4))
			{
				Log(nameof(IsDeviceAutomationEnabled), $"Device Automation Configuration table has unexpected format.");
				return true;
			}

			var primaryMatchingRows = deviceAutomationConfigurationTable.Values.Where(row => Convert.ToString(row[1]) == serviceDefinitionName && Convert.ToInt32(row[2]) == 3 /*All integration types*/).ToList();

			if (primaryMatchingRows.Any()) return IsDeviceAutomationEnabled(serviceDefinitionName, primaryMatchingRows);

			var secundaryMatchingRows = deviceAutomationConfigurationTable.Values.Where(row => Convert.ToString(row[1]) == serviceDefinitionName && OrderManagerProtocol.DeviceAutomationConfigurationTable.DiscreetToIntegrationType[Convert.ToInt32(row[2])] == integrationType).ToList();

			if (secundaryMatchingRows.Any()) return IsDeviceAutomationEnabled(serviceDefinitionName, secundaryMatchingRows);

			Log(nameof(IsDeviceAutomationEnabled), $"No config found for {serviceDefinitionName} {integrationType.GetDescription()}");

			return true; // enabled by default
		}

		private bool IsDeviceAutomationEnabled(string serviceDefinitionName, List<object[]> matchingRows)
		{
			if (!int.TryParse(Convert.ToString(matchingRows[0][3]), out int statusDiscreet))
			{
				Log(nameof(IsDeviceAutomationEnabled), $"Unable to parse {Convert.ToString(matchingRows[0][3])} to an integer.");
				return true;
			}

			if (statusDiscreet != 0 && statusDiscreet != 1)
			{
				Log(nameof(IsDeviceAutomationEnabled), $"Unable to parse {statusDiscreet} to a boolean.");
				return true;
			}

			bool status = Convert.ToBoolean(statusDiscreet);

			Log(nameof(IsDeviceAutomationEnabled), $"{serviceDefinitionName} is {(status ? "enabled" : "disabled")} for all integration types");

			return status;
		}

		public TimeRangeUtc GetServiceResourceAllocationWindow(string serviceDefinitionName, DateTime serviceStart, DateTime serviceEnd)
		{
			if (serviceStart.Date != serviceEnd.Date)
			{
				Log(nameof(GetServiceResourceAllocationWindow), $"Service spans multiple days (Start: {serviceStart.ToString("O")}; End: {serviceEnd.ToString("O")})");
				return new TimeRangeUtc(serviceStart, serviceEnd, TimeZoneInfo.Utc);
			}

			if (TryGetConfiguredServiceResourceAllocationWindow(serviceDefinitionName, serviceStart, serviceEnd, out var window))
			{
				Log(nameof(GetServiceResourceAllocationWindow), $"Using configured service resource allocation window: {window}");
				return window;
			}

			if (TryGetDefaultConfiguredServiceResourceAllocationWindow(serviceStart, serviceEnd, out window))
			{
				Log(nameof(GetServiceResourceAllocationWindow), $"Using default service resource allocation window: {window}");
				return window;
			}

			window = new TimeRangeUtc(serviceStart.Date, serviceStart.Date.AddDays(1), TimeZoneInfo.Utc);
			Log(nameof(GetServiceResourceAllocationWindow), $"Using fallback service resource allocation window: {window}");
			return window;
		}

		public void DeleteRow(int tablePid, string primaryKey)
		{
			DataMinerInterface.Element.DeleteRow(helpers, element, tablePid, primaryKey);
		}

		public bool TriggerDeleteServices(Order order, List<Guid> bookedServicesToRemove)
		{
			if (order.Id == Guid.Empty) throw new ArgumentException($"Order ID cannot be an empty Guid", nameof(order));

			var reservationId = order.Id.ToString();

			try
			{
				var orderInfo = new OrderInfo
				{
					ReservationId = reservationId,
					Name = order.Name,
					StartTime = order.StartWithPreRoll,
					EndTime = order.EndWithPostRoll,
					BookServicesStatus = BookServicesStatus.NotApplicable,
					BookEventLevelReceptionServicesStatus = BookServicesStatus.NotApplicable,
					ServicesToRemoveReservationIds = bookedServicesToRemove.Select(id => id.ToString()).ToList(),
					IsHighPriority = false,
					AgentId = Engine.SLNetRaw.ServerDetails.AgentID,
					ProcessChronologically = false,
				};

				string message = orderInfo.Serialize();

				Log(nameof(TriggerDeleteServices), $"Sending message to order manager: {message}");

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.OrderUpdateParameterId, message);

				return DataMinerInterface.Element.TableContainsPrimaryKey(helpers, element, OrderManagerProtocol.OrdersTable.TablePid, reservationId);
			}
			catch (Exception e)
			{
				Log(nameof(TriggerDeleteServices), "Error while updating order manager reference: " + e);
				return false;
			}
		}

		/// <summary>
		/// Adds or updates the reference to this order in the order manager element.
		/// </summary>
		/// <param name="order">The order to reference.</param>
		/// <param name="bookedServicesToRemove"></param>
		/// <param name="isHighPriority"></param>
		/// <param name="processChronologically"></param>
		/// <returns>True in case the order could be referenced correctly.</returns>
		public bool UpdateOrderManagerReference(Order order, List<string> bookedServicesToRemove, bool isHighPriority, bool processChronologically)
		{
			var reservationId = order.Id.ToString();

			try
			{
				var orderInfo = new OrderInfo
				{
					ReservationId = reservationId,
					Name = order.Name,
					StartTime = order.StartWithPreRoll,
					EndTime = order.EndWithPostRoll,
					BookServicesStatus = BookServicesStatus.NotApplicable,
					BookEventLevelReceptionServicesStatus = BookServicesStatus.NotApplicable,
					ServicesToRemoveReservationIds = bookedServicesToRemove,
					IsHighPriority = isHighPriority,
					AgentId = Engine.SLNetRaw.ServerDetails.AgentID,
					ProcessChronologically = processChronologically,
				};

				if (order.Status == Status.ChangeRequested || order.Status == Status.Confirmed || order.Status == Status.Planned || order.Status == Status.Running)
				{
					// reset to To Be Scheduled which means the order manager element will evaluate when the service booking should be scheduled
					orderInfo.BookServicesStatus = BookServicesStatus.ToBeScheduled;
				}

				if (order.AllServices.Exists(s => s.IsSharedSource))
				{
					// reset to To Be Scheduled which means the order manager element will evaluate when the event level reception service booking should be scheduled
					orderInfo.BookEventLevelReceptionServicesStatus = BookServicesStatus.ToBeScheduled;
				}

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.OrderUpdateParameterId, orderInfo.Serialize());

				return DataMinerInterface.Element.TableContainsPrimaryKey(helpers, element, OrderManagerProtocol.OrdersTable.TablePid, reservationId);
			}
			catch (Exception e)
			{
				Log(nameof(UpdateOrderManagerReference), "Error while updating order manager reference: " + e);
				return false;
			}
		}

		public IDictionary<string, object[]> GetIntegrationOrdersTable()
		{
			return DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.IntegrationOrdersTable.TablePid);
		}

		public void ReprocessIntegrationOrders(params Guid[] orderIds)
		{
			if (orderIds == null) throw new ArgumentNullException(nameof(orderIds));

			var integrationOrdersTable = GetIntegrationOrdersTable();
			List<string> integrationOrderKeys = new List<string>();

			foreach (var kvp in integrationOrdersTable)
			{
				if (!Guid.TryParse(Convert.ToString(kvp.Value[OrderManagerProtocol.IntegrationOrdersTable.OrderIdIdx]), out var integrationOrderId)) continue;
				if (!orderIds.Contains(integrationOrderId)) continue;

				integrationOrderKeys.Add(kvp.Key);
			}

			if (!integrationOrderKeys.Any()) return;

			ReprocessIntegrationOrders(integrationOrderKeys.ToArray());
		}

		public void ReprocessIntegrationOrders(params string[] integrationOrderKeys)
		{
			if (integrationOrderKeys == null) throw new ArgumentNullException(nameof(integrationOrderKeys));
			DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.ReprocessIntegrationOrdersPid, String.Join(";", integrationOrderKeys));
		}

		public bool AddOrUpdateServiceConfigurations(Order order)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));
			if (order.Id == Guid.Empty) throw new ArgumentException($"Order {order.Name} ID is empty", nameof(order));

			return AddOrUpdateServiceConfigurations(order.Id, order.EndWithPostRoll, order.GetSerializedServiceConfigurations());
		}

		public bool AddOrUpdateServiceConfigurations(Guid orderId, DateTime orderEnd, string serializedServiceConfigurations)
		{
			if (orderId == Guid.Empty) throw new ArgumentException("Order ID is empty", nameof(orderId));
			if (orderEnd == default(DateTime)) throw new ArgumentException("Invalid order end", nameof(orderEnd));
			if (string.IsNullOrWhiteSpace(serializedServiceConfigurations)) throw new ArgumentNullException(nameof(serializedServiceConfigurations));

			LogMethodStart(nameof(AddOrUpdateServiceConfigurations), out var stopwatch);

			Log(nameof(AddOrUpdateServiceConfigurations), $"Saving service configurations: {serializedServiceConfigurations}");

			var serviceConfigurationRequest = new ServiceConfigurationRequest
			{
				RequestId = Guid.NewGuid().ToString(),
				RequestType = ServiceConfigurationRequestType.AddOrUpdate,
				OrderId = orderId.ToString(),
				OrderEnd = orderEnd,
				ServiceConfiguration = serializedServiceConfigurations
			};

			try
			{
				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.ExternalServiceConfigurationRequests.ExternalServiceConfigurationRequestPid, serviceConfigurationRequest.Serialize());

				var retries = 0;
				while (retries < 20)
				{
					var externalServiceConfigurationRequestIds = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, element, OrderManagerProtocol.ExternalServiceConfigurationRequests.ExternalServiceConfigurationRequestsTablePid);
					if (externalServiceConfigurationRequestIds.Contains(serviceConfigurationRequest.RequestId))
					{
						bool result = Convert.ToBoolean(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.ExternalServiceConfigurationRequests.ExternalServiceConfigurationRequestsTableRequestStatusColumnPid, serviceConfigurationRequest.RequestId));

						LogMethodCompleted(nameof(AddOrUpdateServiceConfigurations), stopwatch);

						return result;
					}

					Thread.Sleep(50);
					retries++;
				}
			}
			catch (Exception e)
			{
				Log(nameof(AddOrUpdateServiceConfigurations), $"Exception occurred: {e}");
			}

			LogMethodCompleted(nameof(AddOrUpdateServiceConfigurations), stopwatch);
			return false;
		}

		public bool TryGetServiceConfigurations(Guid orderId, out Dictionary<int, ServiceConfiguration> serviceConfigurations)
		{
			if (orderId == Guid.Empty) throw new ArgumentException($"Order ID is empty", nameof(orderId));

			LogMethodStart(nameof(TryGetServiceConfigurations), out var stopwatch);

			serviceConfigurations = new Dictionary<int, ServiceConfiguration>();

			try
			{
				var serializedServiceConfiguration = GetSerializedServiceConfigurations(orderId);

				if (string.IsNullOrWhiteSpace(serializedServiceConfiguration) || serializedServiceConfiguration == "-1" || serializedServiceConfiguration.Equals("Not found", StringComparison.InvariantCultureIgnoreCase))
				{
					Log(nameof(TryGetServiceConfigurations), $"Something went wrong with API");
					LogMethodCompleted(nameof(TryGetServiceConfigurations), stopwatch);
					return false;
				}

				serviceConfigurations = DeserializeServiceConfigurations(serializedServiceConfiguration);

				LogMethodCompleted(nameof(TryGetServiceConfigurations), stopwatch);
				return true;
			}
			catch (Exception e)
			{
				Log(nameof(TryGetServiceConfigurations), $"Exception occurred: {e}");
				LogMethodCompleted(nameof(TryGetServiceConfigurations), stopwatch);
				return false;
			}
		}

		private Dictionary<int, ServiceConfiguration> DeserializeServiceConfigurations(string serializedServiceConfiguration)
		{
			LogMethodStart(nameof(DeserializeServiceConfigurations), out var stopwatch);

			var result = JsonConvert.DeserializeObject<Dictionary<int, ServiceConfiguration>>(serializedServiceConfiguration);

			LogMethodCompleted(nameof(DeserializeServiceConfigurations), stopwatch);

			return result;
		}

		public string GetSerializedServiceConfigurations(Guid orderId)
		{
			var request = new ServiceConfigurationRequest
			{
				RequestId = Guid.NewGuid().ToString(),
				RequestType = ServiceConfigurationRequestType.Get,
				OrderId = orderId.ToString()
			};

			DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.ExternalServiceConfigurationRequests.ExternalServiceConfigurationRequestPid, request.Serialize());

			var serializedServiceConfiguration = String.Empty;

			var retries = 0;
			while (retries < 50)
			{
				var externalServiceConfigurationRequestIds = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, element, OrderManagerProtocol.ExternalServiceConfigurationRequests.ExternalServiceConfigurationRequestsTablePid);
				if (externalServiceConfigurationRequestIds.Contains(request.RequestId))
				{
					serializedServiceConfiguration = (string)DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.ExternalServiceConfigurationRequests.ExternalServiceConfigurationRequestsTableRequestStatusServiceConfigColumnPid, request.RequestId);
					break;
				}

				Thread.Sleep(50);
				retries++;
			}

			Log(nameof(GetSerializedServiceConfigurations), $"Retrieved service configuration: '{serializedServiceConfiguration}'");

			return serializedServiceConfiguration;
		}

		public bool DeleteServiceConfigurations(Guid orderId)
		{
			if (orderId == Guid.Empty) throw new ArgumentException($"Order ID is empty", nameof(orderId));

			LogMethodStart(nameof(DeleteServiceConfigurations), out var stopwatch);

			var serviceConfigurationRequest = new ServiceConfigurationRequest
			{
				RequestId = Guid.NewGuid().ToString(),
				RequestType = ServiceConfigurationRequestType.Delete,
				OrderId = orderId.ToString()
			};

			try
			{
				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.ExternalServiceConfigurationRequests.ExternalServiceConfigurationRequestPid, serviceConfigurationRequest.Serialize());

				LogMethodCompleted(nameof(DeleteServiceConfigurations), stopwatch);
				return true;
			}
			catch (Exception e)
			{
				Log(nameof(DeleteServiceConfigurations), $"Exception occurred: {e}");
				LogMethodCompleted(nameof(DeleteServiceConfigurations), stopwatch);
				return false;
			}
		}

		public void SetHandleRecurringOrderActionScriptIsRunningFlag(bool flagValueToSet, string recurringOrderInfosToProcessName = null)
		{
			if (flagValueToSet && recurringOrderInfosToProcessName == null) throw new ArgumentNullException(nameof(recurringOrderInfosToProcessName));

			if (flagValueToSet)
			{
				var now = DateTime.Now;

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.RecurringOrdersScriptStatusParameterId, $"Processing recurring order '{recurringOrderInfosToProcessName}'");

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.RecurringOrdersLatestScriptStartParameterId, now.ToOADate());
			}
			else
			{
				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.RecurringOrdersScriptStatusParameterId, "-1");

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.RecurringOrdersLatestScriptStartParameterId, -1);
			}
		}

		public List<RecurringSequenceInfo> GetRecurringOrdersTable()
		{
			var recurringOrdersTable = DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.RecurringOrdersTable.TablePid);

			var recurringOrderInfos = new List<RecurringSequenceInfo>();

			foreach (var recurringOrderRow in recurringOrdersTable.Values)
			{
				recurringOrderInfos.Add(RecurringSequenceInfo.FromTableRow(recurringOrderRow));
			}

			return recurringOrderInfos;
		}

		public TimeSpan GetRecurringOrdersSlidingWindowSize()
		{
			return TimeSpan.FromMinutes(Convert.ToDouble(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.RecurringOrdersSlidingWindowSizeParameterId)));
		}

		public int GetRecurringOrdersMaxBookingAmount()
		{
			return Convert.ToInt32(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.RecurringOrdersMaxBookingAmountParameterId));
		}

		public bool AddRecurringOrder(Order order, Guid templateId)
		{
			try
			{
				var request = new ExternalRecurringOrderRequest
				{
					Action = YLE.Order.Recurrence.Action.Add,
					RecurringSequenceInfo = order.RecurringSequenceInfo
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.RecurringOrderUpdateParameterId, request.Serialize());

				Log(nameof(AddRecurringOrder), $"Sent message to order manager {request.Serialize()}");

				return DataMinerInterface.Element.TableContainsPrimaryKey(helpers, element, OrderManagerProtocol.RecurringOrdersTable.TablePid, request.RecurringSequenceInfo.TemplateId.ToString());
			}
			catch (Exception e)
			{
				Log(nameof(AddRecurringOrder), $"Something went wrong: {e}");
				return false;
			}
		}

		public bool DeleteRecurringOrder(RecurringSequenceInfo recurringSequenceInfo)
		{
			try
			{
				var request = new ExternalRecurringOrderRequest
				{
					Action = YLE.Order.Recurrence.Action.Delete,
					RecurringSequenceInfo = recurringSequenceInfo
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.RecurringOrderUpdateParameterId, request.Serialize());

				return !DataMinerInterface.Element.TableContainsPrimaryKey(helpers, element, OrderManagerProtocol.RecurringOrdersTable.TablePid, request.RecurringSequenceInfo.TemplateId.ToString());
			}
			catch (Exception e)
			{
				Log(nameof(DeleteRecurringOrder), $"Something went wrong: {e}");
				return false;
			}
		}

		public void UpdateRecurringOrder(Order order, Guid templateId, bool templateIsUpdated)
		{
			try
			{
				order.RecurringSequenceInfo.TemplateId = templateId;
				order.RecurringSequenceInfo.TemplateIsUpdated = templateIsUpdated;
				order.RecurringSequenceInfo.EventId = order.Event?.Id ?? Guid.Empty;

				var request = new ExternalRecurringOrderRequest
				{
					Action = YLE.Order.Recurrence.Action.Update,
					RecurringSequenceInfo = order.RecurringSequenceInfo
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.RecurringOrderUpdateParameterId, request.Serialize());
			}
			catch (Exception e)
			{
				Log(nameof(UpdateRecurringOrder), $"Something went wrong: {e}");
			}
		}

		/// <summary>
		/// Forwards the services to remove from an order to the order manager to have them removed in the background.
		/// This is only needed for order changes via AddOrUpdate as this blocks the UI progress too long.
		/// BookServices method can remove the services in the method itself as this already runs in the background.
		/// </summary>
		/// <param name="order">The order to reference.</param>
		/// <param name="servicesToRemove">The list of services to remove.</param>
		/// <returns></returns>
		public bool UpdateOrderManagerServicesToRemove(Order order, List<Service> servicesToRemove)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));
			if (servicesToRemove == null) throw new ArgumentNullException(nameof(servicesToRemove));

			var reservationId = order.Id.ToString();

			try
			{
				var orderInfo = new OrderInfo
				{
					ReservationId = reservationId,
					ServicesToRemoveReservationIds = servicesToRemove.Select(s => s.Id.ToString()).ToList()
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.OrderUpdateParameterId, orderInfo.Serialize());

				return DataMinerInterface.Element.TableContainsPrimaryKey(helpers, element, OrderManagerProtocol.OrdersTable.TablePid, reservationId);
			}
			catch (Exception e)
			{
				Log(nameof(UpdateOrderManagerServicesToRemove), "Error updating order manager services to remove: " + e);
				return false;
			}
		}

		/// <summary>
		/// Update the name of an already existing order entry in the order manager element.
		/// </summary>
		/// <param name="order">The order for which services were booked.</param>
		public void UpdateOrderName(Order order)
		{
			try
			{
				var orderInfo = new OrderInfo
				{
					ReservationId = order.Id.ToString(),
					Name = order.Name
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.OrderUpdateParameterId, orderInfo.Serialize());
			}
			catch (Exception e)
			{
				Log(nameof(UpdateBookServiceStatus), "Error updating order manager order name: " + e);
			}
		}

		/// <summary>
		/// Update the name of an already existing order entry in the order manager element.
		/// </summary>
		/// <param name="orderReservation">The order reservation for which services were booked.</param>
		/// <param name="newName"></param>
		public void UpdateOrderName(ReservationInstance orderReservation, string newName)
		{
			try
			{
				var orderInfo = new OrderInfo
				{
					ReservationId = orderReservation.ID.ToString(),
					Name = newName
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.OrderUpdateParameterId, orderInfo.Serialize());
			}
			catch (Exception e)
			{
				Log(nameof(UpdateBookServiceStatus), "Error updating order manager order name: " + e);
			}
		}

		public void SendResponse(params IntegrationResponse[] responses)
		{
			try
			{
				// Get the processed at time of all integration updates for which the status is not Removed (these will be removed from the table)
				Dictionary<string, int> previousProcessedAtTimes = GetProcessedAtTimes(responses);

				string serializedResponse = JsonConvert.SerializeObject(responses);
				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.IntegrationResponseParameterId, serializedResponse);

				int retries = 0;
				do
				{
					if (UpdatesAreProcessed(responses, previousProcessedAtTimes))
					{
						Log(nameof(SendResponse), $"Waited {retries * 1000}ms for responses {String.Join(", ", responses.Select(x => x.Key))} to be handled by the Order Manager element");
						return;
					}

					Thread.Sleep(1000);
					retries++;
				}
				while (retries < 10);

				throw new InvalidOperationException($"The following integration updates were not handled in time by the Order Manager element: {String.Join(", ", responses.Select(x => x.Key))}");
			}
			catch (Exception e)
			{
				Log(nameof(SendResponse), "Exception sending response to order manager element: " + e);
			}
		}

		private Dictionary<string, int> GetProcessedAtTimes(IEnumerable<IntegrationResponse> integrationUpdates)
		{
			var updatesWithoutLastProcessedAtValues = new List<string>();

			var processedAtTimes = new Dictionary<string, int>();

			foreach (var update in integrationUpdates)
			{
				if (update.Status == UpdateStatus.Removed) continue;

				try
				{
					var lastProcessedAtParameterValue = DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.IntegrationOrdersTable.LastProcessedAtPid, update.Key);

					if (lastProcessedAtParameterValue is null)
					{
						updatesWithoutLastProcessedAtValues.Add(update.Key);
					}
					else
					{
						int processedAt = lastProcessedAtParameterValue.GetHashCode();
						processedAtTimes.Add(update.Key, processedAt);
					}
				}
				catch (Exception e)
				{
					Log(nameof(GetProcessedAtTimes), $"Exception while getting 'Last Processed At' datetime for integration update {update.Key}: {e}");
					continue;
				}
			}

			Log(nameof(GetProcessedAtTimes), $"Unable to get 'Last Processed At' datetimes for integration updates '{string.Join(", ", updatesWithoutLastProcessedAtValues)}'");

			return processedAtTimes;
		}

		private bool UpdatesAreProcessed(IntegrationResponse[] responses, Dictionary<string, int> previousProcessedAtTimes)
		{
			Dictionary<string, int> currentProcessedAtTimes = GetProcessedAtTimes(responses);
			bool allUpdatesHandled = true;
			foreach (string key in currentProcessedAtTimes.Keys)
			{
				// In case its a new order, no existing entry will be present in the table until it is handled
				if (!previousProcessedAtTimes.ContainsKey(key))
				{
					if (!currentProcessedAtTimes.ContainsKey(key))
					{
						allUpdatesHandled = false;
						break;
					}
				}
				else
				{
					if (previousProcessedAtTimes[key] == currentProcessedAtTimes[key])
					{
						allUpdatesHandled = false;
						break;
					}
				}
			}

			return allUpdatesHandled;
		}

		/// <summary>
		/// Update the book service status in the order manager element.
		/// </summary>
		/// <param name="order">The order for which services were booked.</param>
		/// <param name="succeeded">Indicates if services were successfully booked.</param>
		/// <param name="errorMessage">The error message in case of a failure.</param>
		public void UpdateBookServiceStatus(Order order, bool succeeded, string errorMessage = null)
		{
			try
			{
				var orderInfo = new OrderInfo
				{
					ReservationId = order.Id.ToString(),
					BookServicesStatus = succeeded ? BookServicesStatus.Ok : BookServicesStatus.Fail,
					BookServicesFailureReason = errorMessage,
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.OrderUpdateParameterId, orderInfo.Serialize());
			}
			catch (Exception e)
			{
				Log(nameof(UpdateBookServiceStatus), "Error updating order manager book services status: " + e);
			}
		}

		/// <summary>
		/// Update the book event level reception service status in the order manager element.
		/// </summary>
		/// <param name="order">The order for which services were booked.</param>
		/// <param name="succeeded">Indicates if services were successfully booked.</param>
		/// <param name="errorMessage">The error message in case of a failure.</param>
		public void UpdateBookEventLevelReceptionServiceStatus(Order order, bool succeeded, string errorMessage = null)
		{
			try
			{
				var orderInfo = new OrderInfo
				{
					ReservationId = order.Id.ToString(),
					BookEventLevelReceptionServicesStatus = succeeded ? BookServicesStatus.Ok : BookServicesStatus.Fail,
					BookEventLevelReceptionServicesFailureReason = errorMessage,
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.OrderUpdateParameterId, orderInfo.Serialize());
			}
			catch (Exception e)
			{
				Log(nameof(UpdateBookEventLevelReceptionServiceStatus), "Error updating order manager book services status: " + e);
			}
		}

		/// <summary>
		/// Adds or updates the reference to this event in the order manager element.
		/// </summary>
		/// <param name="event">The event to reference.</param>
		/// <returns>True in case the event could be referenced correctly.</returns>
		public bool UpdateOrderManagerReference(Event.Event @event)
		{
			var jobId = @event.Id.ToString();

			try
			{
				var eventInfo = new EventInfo
				{
					JobId = jobId,
					Name = @event.Name,
					StartTime = @event.Start,
					EndTime = @event.End
				};

				DataMinerInterface.Element.SetParameter(helpers, element, OrderManagerProtocol.EventUpdateParameterId, eventInfo.Serialize());

				Log(nameof(UpdateOrderManagerReference), "Sent message to Order Manager element: " + eventInfo.Serialize());

				return DataMinerInterface.Element.TableContainsPrimaryKey(helpers, element, OrderManagerProtocol.EventsTable.TablePid, jobId);
			}
			catch (Exception e)
			{
				Log(nameof(UpdateOrderManagerReference), "Error updating order manager reference: " + e);
				return false;
			}
		}

		/// <summary>
		/// Delete the reference to this order from the order manager element.
		/// </summary>
		/// <param name="orderId">The ID of the order to delete.</param>
		public void DeleteOrderManagerReference(Guid orderId)
		{
			try
			{
				DataMinerInterface.Element.SetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.OrdersTable.DeleteParameterId, orderId.ToString(), "Delete");
			}
			catch (Exception e)
			{
				Log(nameof(DeleteOrderManagerReference), "Error deleting order manager reference: " + e);
			}
		}

		/// <summary>
		/// Delete the reference to this event from the order manager element.
		/// </summary>
		/// <param name="event">The event to delete.</param>
		public void DeleteOrderManagerReference(Event.Event @event)
		{
			try
			{
				DataMinerInterface.Element.SetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.EventsTable.DeleteParameterId, @event.Id.ToString(), "Delete");
			}
			catch (Exception e)
			{
				Log(nameof(DeleteOrderManagerReference), "Error deleting order manager reference: " + e);
			}
		}

		/// <summary>
		/// Retrieve the name of the Eurovision element for a given user group from the Order Manager element.
		/// </summary>
		/// <param name="userGroupName">The name of the user group.</param>
		/// <returns>The name of the configured Eurovision element.</returns>
		public string GetEurovisionElementName(string userGroupName)
		{
			if (string.IsNullOrEmpty(userGroupName)) return null;

			try
			{
				var orderManagerUserGroups = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, element, OrderManagerProtocol.UserGroupsTable.TablePid);
				if (!orderManagerUserGroups.Contains(userGroupName)) return null;

				return Convert.ToString(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.UserGroupsTable.EurovisionElementParameterId, userGroupName));
			}
			catch (Exception e)
			{
				Log(nameof(GetEurovisionElementName), "Error retrieving eurovision element name from order manager: " + e);
				return null;
			}
		}

		/// <summary>
		/// Returns true if the order is referenced in the Order Manager element.
		/// </summary>
		/// <param name="orderId">The id of the order.</param>
		/// <returns>True if the order is referenced.</returns>
		public bool HasOrderReference(Guid orderId)
		{
			var orderIds = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, element, OrderManagerProtocol.OrdersTable.TablePid);

			return orderIds.Contains(orderId.ToString());
		}

		public bool TryGetBookServicesStatus(Guid orderId, out BookServicesStatus status)
		{
			status = BookServicesStatus.NotApplicable;

			try
			{
				status = GetBookServiceStatus(orderId);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Returns the book services status from the Order Manager element for the given order.
		/// </summary>
		/// <param name="orderId">The id of the order.</param>
		/// <returns>The book services status.</returns>
		public BookServicesStatus GetBookServiceStatus(Guid orderId)
		{
			return (BookServicesStatus)Convert.ToInt32(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.OrdersTable.BookServicesStatusParameterId, orderId.ToString()));
		}

		public int GetResourceOverbookedRetryCounter(Guid orderId)
		{
			try
			{
				return Convert.ToInt32(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.OrdersTable.ResourceOverbookedRetryCounterReadPid, orderId.ToString()));
			}
			catch (Exception e)
			{
				helpers.Log(nameof(OrderManagerElement), nameof(GetResourceOverbookedRetryCounter), $"Exception while getting value: {e}");
				return 0;
			}
		}

		public TimeSpan GetServiceDeletionDelayTime()
		{
			return TimeSpan.FromMinutes(Convert.ToDouble(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.OrderServiceDeletionDelayParameterId)));
		}

		public void IncreaseResourceOverbookedRetryCounter(params Guid[] orderIds)
		{
			foreach (var orderId in orderIds)
			{
				int currentCounterValue = GetResourceOverbookedRetryCounter(orderId);

				DataMinerInterface.Element.SetParameterByPrimaryKey(helpers, element, OrderManagerProtocol.OrdersTable.ResourceOverbookedRetryCounterReadPid, orderId.ToString(), currentCounterValue + 1);
			}
		}

		public bool CheckDoesOrderMatchAutoMessiHideConfiguration(Order order, object enabledStatus)
		{
			if (!int.TryParse(Convert.ToString(enabledStatus), out int statusDiscreet))
			{
				Log(nameof(CheckDoesOrderMatchAutoMessiHideConfiguration), $"Unable to parse {Convert.ToString(enabledStatus)} to an integer.");
				return false;
			}

			if (statusDiscreet != 0 && statusDiscreet != 1)
			{
				Log(nameof(CheckDoesOrderMatchAutoMessiHideConfiguration), $"Unable to parse {statusDiscreet} to a boolean.");
				return false;
			}

			bool status = Convert.ToBoolean(statusDiscreet);

			Log(nameof(CheckDoesOrderMatchAutoMessiHideConfiguration), $"{order.Name} is {(status ? "enabled" : "disabled")} for {order.IntegrationType.GetDescription()}");

			return status;
		}

		/// <summary>
		/// Will check if the given order need to be auto hidden or not, currently only news recording orders are allowed.
		/// </summary>
		/// <param name="order">Order to check on</param>
		/// <returns>True if the order name contains one of the hidden rule values.</returns>
		/// <exception cref="ArgumentNullException">Will be thrown when the order is null.</exception>
		public bool DoesOrderMatchAutoMessiHideConfiguration(Order order)
		{
			if (!order.AllServices.Exists(service => service.Definition?.Description != null && service.Definition.Description.Contains("News"))) return false;


			autoHiddenRecordingOrderConfigurationTable = autoHiddenRecordingOrderConfigurationTable ?? DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.AutoHideMessiOrdersConfigurationTable.TablePid);

			if (!autoHiddenRecordingOrderConfigurationTable.Any()) return true;

			var primaryMatchingRows = new List<object[]>();
			var secundaryMatchingRows = new List<object[]>();

			foreach (var row in autoHiddenRecordingOrderConfigurationTable.Values)
			{
				bool regexIsValid = Convert.ToBoolean(row[OrderManagerProtocol.AutoHideMessiOrdersConfigurationTable.RegexCheckIdx]);
				if (!regexIsValid)
					continue;

				bool regexisMatched = Regex.IsMatch(order.Name, Convert.ToString(row[OrderManagerProtocol.AutoHideMessiOrdersConfigurationTable.OrderDescriptionIdx]));
				if (!regexisMatched)
					continue;

				bool integrationTypeIsAll = Convert.ToInt16(row[OrderManagerProtocol.AutoHideMessiOrdersConfigurationTable.IntegrationIdx]) == 6;

				bool integrationTypeMatchesOrder = OrderManagerProtocol.AutoHideMessiOrdersConfigurationTable.DiscreetToIntegrationType.TryGetValue(Convert.ToInt32(row[OrderManagerProtocol.AutoHideMessiOrdersConfigurationTable.IntegrationIdx]), out var integrationType) && integrationType == order.IntegrationType;

				if (integrationTypeIsAll) primaryMatchingRows.Add(row);
				else if (integrationTypeMatchesOrder) secundaryMatchingRows.Add(row);
				else
				{
					//Nothing
				}
			}

			if (primaryMatchingRows.Any())
			{
				return CheckDoesOrderMatchAutoMessiHideConfiguration(order, primaryMatchingRows[0][2]);
			}


			if (secundaryMatchingRows.Any())
			{
				return CheckDoesOrderMatchAutoMessiHideConfiguration(order, secundaryMatchingRows[0][2]);
			}

			return false;
		}

		public List<EvsMessiNewsTarget> GetEvsMessiNewsTargets(YLE.ServiceDefinition.ServiceDefinition serviceDefinition)
		{
			if (serviceDefinition.Id != ServiceDefinitionGuids.RecordingMessiNews) return new List<EvsMessiNewsTarget>();

			var table = evsMessiNewsTargetsTable ?? DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.EvsMessiNewsTargetsTable.TablePid);
			if (table == null || table.Count == 0) throw new InvalidOperationException($"No targets defined in the EVS Messi News Targets table");

			return table.Values.Select(x =>
			{
				string destinationPath = Convert.ToString(x[OrderManagerProtocol.EvsMessiNewsTargetsTable.Idx.RecordingFileDestinationPath]);
				string target = Convert.ToString(x[OrderManagerProtocol.EvsMessiNewsTargetsTable.Idx.Target]);
				bool isDefault = Convert.ToBoolean(Convert.ToInt32(x[OrderManagerProtocol.EvsMessiNewsTargetsTable.Idx.DefaultState]));
				return new EvsMessiNewsTarget(destinationPath, target, isDefault);
			}).ToList();
		}

		private bool TryGetConfiguredServiceResourceAllocationWindow(string serviceDefinitionName, DateTime serviceStart, DateTime serviceEnd, out TimeRangeUtc window)
		{
			window = new TimeRangeUtc(serviceStart, serviceEnd, TimeZoneInfo.Local);

			try
			{
				serviceResourceAllocationWindowTable = serviceResourceAllocationWindowTable ??
					DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.ServiceResourceAllocationWindowTable.TablePid);

				var serviceResourceAllocationWindowRowsForService = serviceResourceAllocationWindowTable.Values.Where(w => Convert.ToString(w[1]) == serviceDefinitionName.Trim('_'));

				bool foundStartAndEnd = false;
				DateTime? firstWindowStart = null;
				DateTime? secondWindowEnd = null;
				foreach (var windowRow in serviceResourceAllocationWindowRowsForService)
				{
					if (foundStartAndEnd) break;

					var windowStart = serviceStart.Date.Add(TimeSpan.FromSeconds(Convert.ToInt32(windowRow[2])));
					var windowEnd = serviceEnd.Date.Add(TimeSpan.FromSeconds(Convert.ToInt32(windowRow[3])));

					bool serviceStartFallsWithinWindow = windowStart <= serviceStart && serviceStart <= windowEnd;
					if (serviceStartFallsWithinWindow)
					{
						firstWindowStart = windowStart;
					}

					bool serviceEndFallsWithinWindow = windowStart <= serviceEnd && serviceEnd <= windowEnd;
					if (serviceEndFallsWithinWindow)
					{
						secondWindowEnd = windowEnd;
					}

					foundStartAndEnd = firstWindowStart.HasValue && secondWindowEnd.HasValue;
				}

				if (foundStartAndEnd)
				{
					window = new TimeRangeUtc(firstWindowStart.Value, secondWindowEnd.Value, TimeZoneInfo.Local);
					return true;
				}
			}
			catch (Exception e)
			{
				Log(nameof(TryGetConfiguredServiceResourceAllocationWindow), $"Exception retrieving service resource allocation window: {e}");
			}

			return false;
		}

		private bool TryGetDefaultConfiguredServiceResourceAllocationWindow(DateTime serviceStart, DateTime serviceEnd, out TimeRangeUtc window)
		{
			window = new TimeRangeUtc(serviceStart, serviceEnd, TimeZoneInfo.Utc);

			try
			{
				var windowStart = serviceStart.Date.Add(TimeSpan.FromSeconds(Convert.ToInt32(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.DefaultServiceResourceAllocationWindowStartParameterId))));
				var windowEnd = serviceEnd.Date.Add(TimeSpan.FromSeconds(Convert.ToInt32(DataMinerInterface.Element.GetParameter(helpers, element, OrderManagerProtocol.DefaultServiceResourceAllocationWindowEndParameterId))));

				window = new TimeRangeUtc(windowStart, windowEnd, TimeZoneInfo.Utc);
				return true;
			}
			catch (Exception e)
			{
				Log(nameof(TryGetDefaultConfiguredServiceResourceAllocationWindow), $"Exception retrieving default service resource allocation window: {e}");
			}

			return false;
		}

		private bool GetStatusFromParameter(int pid)
		{
			try
			{
				var elementIdParameter = DataMinerInterface.Element.GetParameter(helpers, element, pid);
				if (elementIdParameter == null) return false;

				return Convert.ToBoolean(Convert.ToInt16(elementIdParameter));
			}
			catch (Exception e)
			{
				Log(nameof(GetStatusFromParameter), "Exception occurred: " + e);
				return false;
			}
		}

		private HashSet<string> GetIntegrationIds(IntegrationType integrationType)
		{
			try
			{
				var integrationUpdatesTable = DataMinerInterface.Element.GetTable(helpers, element, OrderManagerProtocol.IntegrationOrdersTable.TablePid);

				var integrationUpdateRows = new List<object[]>();

				foreach (var row in integrationUpdatesTable.Values)
				{
					if (Convert.ToInt16(row[OrderManagerProtocol.IntegrationOrdersTable.IntegrationTypeIdx]) == (int)integrationType)
					{
						integrationUpdateRows.Add(row);
					}
				}

				var integrationIds = new HashSet<string>();
				foreach (var integrationUpdateRow in integrationUpdateRows) integrationIds.Add((string)integrationUpdateRow[OrderManagerProtocol.IntegrationOrdersTable.IntegrationIdIdx]);

				return integrationIds;
			}
			catch (Exception e)
			{
				Log(nameof(GetIntegrationIds), "Exception occurred: " + e);
				return new HashSet<string>();
			}
		}

		private void Log(string nameOfMethod, string message)
		{
			helpers.Log(nameof(OrderManagerElement), nameOfMethod, message);
		}

		private void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch)
		{
			helpers.LogMethodStart(nameof(OrderManagerElement), nameOfMethod, out stopwatch);
		}

		private void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch)
		{
			helpers.LogMethodCompleted(nameof(OrderManagerElement), nameOfMethod, null, stopwatch);
		}
	}

	public class EvsMessiNewsTarget
	{
		internal EvsMessiNewsTarget(string destinationPath, string target, bool isDefault)
		{

			DestinationPath = destinationPath;
			Target = target;
			IsDefault = isDefault;
		}

		public string ID { get; private set; }

		public string DestinationPath { get; private set; }

		public string Target { get; private set; }

		public bool IsDefault { get; private set; }
	}
}