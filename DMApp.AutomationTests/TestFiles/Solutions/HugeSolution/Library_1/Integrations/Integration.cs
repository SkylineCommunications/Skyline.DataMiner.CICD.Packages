namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Helpers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/// <summary>
	/// Integration base class.
	/// </summary>
	public abstract class Integration : HelpedObject
	{
		public static readonly string Company = "YLE";
		
		protected readonly OrderManagerElement orderManagerElement;

		protected Integration(Utilities.Helpers helpers, OrderManagerElement orderManagerElement) : base(helpers)
		{
			this.orderManagerElement = orderManagerElement ?? throw new ArgumentNullException(nameof(orderManagerElement));
			CompanyDetails = helpers.ContractManager.RequestCompanyContractDetails(Company);
		}

		/// <summary>
		/// Gets the type of integration.
		/// </summary>
		public abstract IntegrationType IntegrationType { get; }

		/// <summary>
		/// Gets the text explaining what integration is responsible for the generation of the Order, Event and/or Service.
		/// </summary>
		protected abstract string IntegrationText { get; }

		/// <summary>
		/// The company details for the YLE company.
		/// </summary>
		protected ExternalCompanyResponse CompanyDetails { get; set; }

		/// <summary>
		/// Handle a specific update for this integration.
		/// </summary>
		/// <param name="id">The reference to the specific integration data.</param>
		/// <param name="integrationData">Integration data.</param>
		public abstract void HandleUpdate(string id, string integrationData);

		protected void SendResponseToOrderManagerElement(string id, UpdateStatus status, string additionalInfo, Guid? orderId = null, Guid? eventId = null)
		{
			var integrationResponse = new IntegrationResponse(IntegrationType, id)
			{
				Status = status,
				AdditionalInformation = additionalInfo,
				OrderId = orderId,
				EventId = eventId,
			};

			orderManagerElement.SendResponse(integrationResponse);

			Log(nameof(SendResponseToOrderManagerElement), $"Sent response: {integrationResponse.Serialize()}");
		}

		protected void RetrieveLock(Order order)
		{
			try
			{
				var lockInfo = helpers.LockManager.RequestOrderLock(order.Id);
				if (!lockInfo.IsLockGranted)
				{
					helpers.Log(nameof(Integration), "RetrieveLock", "Lock could not be retrieved");
					throw new LockNotGrantedException();
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Integration), nameof(RetrieveLock), $"Exception retrieving order lock: {e}");
				throw;
			}

			if (order.Event != null)
			{
				RetrieveLock(order.Event);
			}
		}

		protected void RetrieveLock(Event @event)
		{
			try
			{
				var lockInfo = helpers.LockManager.RequestEventLock(@event.Id);
				if (!lockInfo.IsLockGranted)
				{
					helpers.Log(nameof(Integration), "RetrieveLock", "Lock could not be retrieved");
					throw new LockNotGrantedException();
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Integration), nameof(RetrieveLock), $"Exception retrieving event lock: {e}");
				throw;
			}
		}

		protected void RecreatePreliminaryOrder(Order order)
		{
			helpers.Log(nameof(Integration), nameof(RecreatePreliminaryOrder), "Deleting existing order reservation in order to put it back to preliminary state");

			RemoveExistingOrder(order.Id);

			order.RemoveAutogenerateServices();

			foreach (var service in order.AllServices.Where(x => !x.IsSharedSource))
            {
                service.OrderReferences.Remove(order.Id); // Remove old order id from already existing integration/non-integration services.
				service.IsBooked = false;
            }

            order.Id = Guid.Empty;
			order.Status = YLE.Order.Status.Preliminary;
			order.Definition = new ServiceDefinition
			{
				BookingManagerElementName = SrmConfiguration.OrderBookingManagerElementName,
			};

			helpers.LogMethodCompleted(nameof(Integration), nameof(RecreatePreliminaryOrder));
		}

		protected void RemoveExistingOrder(Guid orderId)
		{
			// Release contributing resources
			Order existingOrder = helpers.OrderManager.GetOrder(orderId);

			// Remove order
			var tasks = helpers.OrderManager.DeleteOrder(existingOrder, existingOrder.AllServices.Where(x => x.IsSharedSource).Select(x => x.Id).ToList());
			if (tasks.Any(x => x.Status == Tasks.Status.Fail))
			{
				helpers.Log(nameof(Integration), nameof(RemoveExistingOrder), "Delete Order|Failed tasks: " + String.Join(", ", tasks.Where(x => x.Status == Tasks.Status.Fail).Select(x => x.Description)));
			}
		}

		/// <summary>
		/// Removes any Routing services that are not used inside the given order.
		/// </summary>
		protected static List<Service> RemoveUnusedRoutingServices(Utilities.Helpers helpers, Order order)
		{
			var removedRoutingServices = new List<Service>();

			var orderServices = order.AllServices;
			var routingServicesWithoutChildren = orderServices.Where(s => s.Definition.VirtualPlatform == VirtualPlatform.Routing && !s.Children.Any()).ToList();

			helpers?.Log(nameof(Integration), nameof(RemoveUnusedRoutingServices), $"Found routing services without children: '{string.Join(";", routingServicesWithoutChildren.Select(s => s?.Name))}'");

			while (routingServicesWithoutChildren.Any())
			{
				foreach (var routingService in routingServicesWithoutChildren)
				{
					var parent = orderServices.FirstOrDefault(s => s.Children.Contains(routingService));
					parent?.Children.Remove(routingService); // Will only remove link, routing service still remains at orderServices list

					helpers?.Log(nameof(Integration), nameof(RemoveUnusedRoutingServices), $"Remove routing service: {routingService.Name} from children of parent: {parent?.Name}");

					removedRoutingServices.Add(routingService);
				}

				// Regenerate all services to make sure we don't work with already detached routing services anymore
				routingServicesWithoutChildren = order.AllServices.Where(s => s.Definition.VirtualPlatform == VirtualPlatform.Routing && !s.Children.Any()).ToList();
			}
			

			helpers?.Log(nameof(Integration), nameof(RemoveUnusedRoutingServices), $"Routing services which are removed: '{string.Join(";", removedRoutingServices.Select(s => s?.Name))}'");

			return removedRoutingServices;
		}

		/// <summary>
		/// Generates a new Service with default values from the provided start to end time.
		/// </summary>
		/// <param name="serviceDefinition">Service definition of the new Service.</param>
		/// <param name="start">Start time of the Service.</param>
		/// <param name="end">End time of the Service.</param>
		/// <returns>New Service based on the ServiceDefinition, with default values for its function profile parameters.</returns>
		protected DisplayedService GenerateDefaultIntegrationService(ServiceDefinition serviceDefinition, DateTime start, DateTime end)
		{
			if (serviceDefinition == null) throw new ArgumentNullException(nameof(serviceDefinition));
			if (start > end) throw new ArgumentException($"Start time {start} cannot be greater than end time {end}");

			var newService = new DisplayedService(helpers, serviceDefinition)
			{
				Start = start,
				End = end,
				IntegrationType = IntegrationType,
				BackupType = BackupType.None,
				Comments = IntegrationText,
				RequiresRouting = true,
				IntegrationIsMaster = true
			};

			var integrationTypeProfileParams = newService.Functions.SelectMany(f => f.Parameters).Where(p => p.Id == ProfileParameterGuids._IntegrationType).ToList();

			integrationTypeProfileParams.ForEach(pp => pp.Value = IntegrationType.GetDescription());

			return newService;
		}

		/// <summary>
		/// Retrieves the profile parameters from the provided function and sets the value to their default value if provided.
		/// If no default value is provided, the value is determined based on the type of the parameter.
		/// If the parameter is a discreet value, the first option will be set.
		/// If the parameter is a number, the lowest number in its range will be set.
		/// iF the parameter is a text, an empty string will be set.
		/// </summary>
		/// <param name="functionDefinition">Function definition containing the profile parameters to be set.</param>
		/// <returns>Profile parameters with filled out values.</returns>
		protected static List<ProfileParameter> GetDefaultProfileParameters(FunctionDefinition functionDefinition)
		{
			List<ProfileParameter> profileParameters = functionDefinition.ProfileDefinition.ProfileParameters.ToList();
			foreach (ProfileParameter profileParameter in profileParameters)
			{
				// Skip parameters used for DTR
				if (profileParameter.Name.StartsWith("_")) continue;

				switch (profileParameter.Type)
				{
					case Library.Solutions.SRM.Model.ParameterType.Discrete:
						profileParameter.Value = profileParameter.Discreets.FirstOrDefault()?.InternalValue;
						break;

					case Library.Solutions.SRM.Model.ParameterType.Number:
						profileParameter.Value = profileParameter.RangeMin;
						break;

					default:
						profileParameter.Value = String.Empty;
						break;
				}

				if (profileParameter.DefaultValue != null)
				{
					if (profileParameter.DefaultValue.Type == Net.Profiles.ParameterValue.ValueType.Double && profileParameter.DefaultValue.DoubleValue >= profileParameter.RangeMin && profileParameter.DefaultValue.DoubleValue <= profileParameter.RangeMax)
					{
						profileParameter.Value = profileParameter.DefaultValue.DoubleValue;
					}
					else if (profileParameter.DefaultValue.Type == Net.Profiles.ParameterValue.ValueType.String && profileParameter.DefaultValue.StringValue != null)
					{
						profileParameter.Value = profileParameter.DefaultValue.StringValue;
					}
					else
					{
						// Do nothing
					}
				}
			}

			return profileParameters;
		}

		/// <summary>
		/// Checks if the timing, service definition, profile parameter values and assigned resources on both services match.
		/// </summary>
		/// <param name="service1">First service to compare.</param>
		/// <param name="service2">Second service to compare.</param>
		/// <param name="ignoredItems">Items to ignore when checking if both services match.</param>
		/// <returns></returns>
		public bool DoServicesMatch(Service service1, Service service2, IgnoredItems ignoredItems)
		{
			helpers.Log(nameof(Integration), nameof(DoServicesMatch), $"Comparing service {service1.Name} with service {service2.Name}");

			// Check start and end times
			bool startTimesMatch = ignoredItems.HasFlag(IgnoredItems.StartTime) || (service1.Start == service2.Start);
			if (!startTimesMatch) helpers.Log(nameof(Integration), nameof(DoServicesMatch), $"Start time changed from {service1.Start} to {service2.Start}");

			bool endTimesMatch = service1.End == service2.End;
			if (!endTimesMatch) helpers.Log(nameof(Integration), nameof(DoServicesMatch), $"End time changed from {service1.End} to {service2.End}");

			// Check Service Definition
			bool serviceDefinitionsMatch = service1.Definition.Id == service2.Definition.Id;
			if (!serviceDefinitionsMatch) helpers.Log(nameof(Integration), nameof(DoServicesMatch), $"Service Definition changed from {service1.Definition.Id} to {service2.Definition.Id}");

			// Check Functions and profile parameters
			bool functionCountMatch = service1.Functions.Count == service2.Functions.Count;

			bool functionMatch = true;
			foreach (Function function1 in service1.Functions)
			{
				Function function2 = service2.Functions.FirstOrDefault(x => x.Id.Equals(function1.Id));
				functionMatch = DoFunctionsMatch(helpers, function1, function2, ignoredItems);
				if (!functionMatch) break;
			}

			bool doesTimingMatch = startTimesMatch && endTimesMatch;
			bool doFunctionsMatch = functionCountMatch && functionMatch;

			return doesTimingMatch && doFunctionsMatch && serviceDefinitionsMatch;
		}

		/// <summary>
		/// Checks if the amount of profile parameters on the functions match, if the valus of those profile parameters match and if the assigned resources match.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="function1">First function to compare.</param>
		/// <param name="function2">Second function to compare.</param>
		/// <param name="ignoredItems">Items to ignore when checking if both functions match.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		private static bool DoFunctionsMatch(Utilities.Helpers helpers, Function function1, Function function2, IgnoredItems ignoredItems)
		{
			ArgumentNullCheck.ThrowIfNull(function1, nameof(function1));
			if (function2 == null) return false;

			// Check profile parameter count
			if (function1.Parameters.Count != function2.Parameters.Count) return false;

			// Check profile parameter values
			if(!AllParametersMatch(helpers, function1.Parameters, function2.Parameters, ignoredItems)) return false;

			// Check assigned resources
			if (function1.Resource == null && function2.Resource != null)
			{
				helpers.Log(nameof(Integration), nameof(DoFunctionsMatch), $"Function 1 doesn't have a resource, while function 2 has {function2.Resource.Name}");
				return false;
			}

			if (function1.Resource != null && function2.Resource == null)
			{
				helpers.Log(nameof(Integration), nameof(DoFunctionsMatch), $"Function 1 has resource {function1.Resource.Name}, while function 2 doesn't have a resource");
				return false;
			}

			if (function1.Resource != null && function2.Resource != null && !function1.Resource.Equals(function2.Resource))
			{
				helpers.Log(nameof(Integration), nameof(DoFunctionsMatch), $"Function 1 has resource {function1.Resource.Name}, while function 2 has resource {function2.Resource.Name}");
				return false;
			}

			return true;
		}

		private static bool AllParametersMatch(Utilities.Helpers helpers, List<ProfileParameter> firstCollection, List<ProfileParameter> secondCollection, IgnoredItems ignoredItems)
		{
			foreach (var profileParameter1 in firstCollection)
			{
				if (ignoredItems.HasFlag(IgnoredItems.AudioConfig) && SrmConfiguration.AudioProfileParameterNames.Contains(profileParameter1.Name)) continue;

				ProfileParameter profileParameter2 = secondCollection.FirstOrDefault(x => x.Id.Equals(profileParameter1.Id));
				if (profileParameter2 == null) return false;

				if (!String.Equals(profileParameter1.StringValue, profileParameter2.StringValue))
				{
					helpers.Log(nameof(Integration), nameof(DoFunctionsMatch), $"Profile parameter {profileParameter1.Name} value has changed from {profileParameter1.StringValue} to {profileParameter2.StringValue}");
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Used to update an existing service.
		/// </summary>
		/// <param name="existingService">The existing service to be updated.</param>
		/// <param name="newService">Service used to update the existing one.</param>
		/// <param name="itemsToIgnore">List of flags that indicate what items should not be update when the existing source has IntegrationIsMaster set to false.</param>
		/// <returns></returns>
		protected UpdatedService UpdateService(Service existingService, Service newService, IgnoredItems itemsToIgnore)
		{
			if (existingService == null) throw new ArgumentNullException(nameof(existingService));
			if (newService == null) throw new ArgumentNullException(nameof(newService));

			helpers.Log(nameof(Integration), nameof(UpdateService), $"Comparing services with resources: {string.Join(", ", existingService.Functions.Select(f => f.Resource?.Name))} and {string.Join(", ", newService.Functions.Select(f => f.Resource?.Name))}");

			if (DoServicesMatch(existingService, newService, itemsToIgnore))
			{
				helpers.Log(nameof(Integration), nameof(UpdateService), "No update required for existing Service: " + existingService.Name);
				return new UpdatedService
				{
					Service = existingService,
					UpdatePerformed = false,
				};
			}

			// Determine service to return
			Service serviceToReturn = DetermineServiceToReturn(existingService, newService);
			if (serviceToReturn.Equals(newService))
			{
				helpers.Log(nameof(Integration), nameof(DetermineServiceToReturn), $"Replacing existing service {existingService.Name} with new service {newService.Name}");
				return new UpdatedService
				{
					Service = newService,
					UpdatePerformed = true,
				};
			}

			existingService = UpdateExistingService(existingService, newService, itemsToIgnore);

			Log(nameof(UpdatedService), $"Updated service to {existingService.GetConfiguration().Serialize()}");

			return new UpdatedService
			{
				Service = existingService,
				UpdatePerformed = true,
			};
		}

		private Service DetermineServiceToReturn(Service existingService, Service newService)
		{
			Service result;
			if (existingService.Definition.Id.Equals(newService.Definition.Id))
			{
				helpers.Log(nameof(Integration), nameof(DetermineServiceToReturn), "Service definitions match");

				// If the service definitions match, update the existing service with the values from the new one.
				// When both of the services are dummy services - select the existing one.
				result = existingService;
			}
			else
			{
				helpers.Log(nameof(Integration), nameof(DetermineServiceToReturn), "Service definitions don't match");

				result = newService;
				result.NameOfServiceToTransmitOrRecord = existingService.NameOfServiceToTransmitOrRecord;
				result.SetChildren(existingService.Children);
			}

			return result;
		}

		private Service UpdateExistingService(Service existingService, Service newService, IgnoredItems itemsToIgnore)
		{
			// Update timing
			existingService = UpdateTiming(existingService, newService, itemsToIgnore);

			// Update profile parameter values
			existingService = UpdateProfileParameters(existingService, newService, itemsToIgnore);

			// Update resources
			existingService = UpdateResources(existingService, newService, itemsToIgnore);

			// Update integration type
			existingService.IntegrationType = newService.IntegrationType;

			existingService.RequiresRouting = newService.RequiresRouting;

			return existingService;
		}

		private static Service UpdateTiming(Service serviceToUpdate, Service newService, IgnoredItems itemsToIgnore)
		{
			serviceToUpdate.Start = (itemsToIgnore.HasFlag(IgnoredItems.StartTime) && !serviceToUpdate.IntegrationIsMaster) ? serviceToUpdate.Start : newService.Start;
			serviceToUpdate.End = newService.End;

			return serviceToUpdate;
		}

		private Service UpdateProfileParameters(Service serviceToUpdate, Service newService, IgnoredItems itemsToIgnore)
		{
			foreach (var functionToUpdate in serviceToUpdate.Functions)
			{
				var functionFromNewService = newService.Functions.SingleOrDefault(f => f.Definition.Label == functionToUpdate.Definition.Label) ?? throw new FunctionNotFoundException(functionToUpdate.Definition.Label);

				var newParameters = functionFromNewService.Parameters.Concat(functionFromNewService.InterfaceParameters).ToList();

				foreach (var parameterToUpdate in functionToUpdate.Parameters.Concat(functionToUpdate.InterfaceParameters))
				{
					var newParameter = newParameters.SingleOrDefault(p => p.Id == parameterToUpdate.Id) ?? throw new ProfileParameterNotFoundException(parameterToUpdate.Id);

					if (itemsToIgnore.HasFlag(IgnoredItems.AudioConfig) && SrmConfiguration.AudioConfigProfileParameterNames.Contains(parameterToUpdate.Name)) continue;
					if (itemsToIgnore.HasFlag(IgnoredItems.AudioChannels) && SrmConfiguration.AudioChannelsProfileParameterNames.Contains(parameterToUpdate.Name)) continue;
					if (itemsToIgnore.HasFlag(IgnoredItems.ServiceSelection) && SrmConfiguration.ServiceSelectionProfileParameterName.Equals(parameterToUpdate.Name))
					{
						Log(nameof(UpdateService), $"Skipping updating Service Selection Profile Parameter from {parameterToUpdate?.Value} to {newParameter.Value}");
						continue;
					}

					parameterToUpdate.Value = newParameter.Value;
				}
			}

			return serviceToUpdate;
		}

		private Service UpdateResources(Service serviceToUpdate, Service newService, IgnoredItems ignoredItems)
		{
			if (!ignoredItems.HasFlag(IgnoredItems.Resources) && newService.Definition.Id.Equals(serviceToUpdate.Definition.Id))
			{
				foreach (var function in newService.Functions)
				{
					if (ignoredItems.HasFlag(IgnoredItems.UnenforcedResources) && !function.EnforceSelectedResource)
					{
						Log(nameof(Integration), nameof(UpdateResources), $"Ignoring function {function.Definition.Label} because it has {nameof(Function.EnforceSelectedResource)}={function.EnforceSelectedResource} and option {IgnoredItems.UnenforcedResources} is set.");
						continue;
					}

					var functionToUpdate = serviceToUpdate.Functions.FirstOrDefault(x => x.Id.Equals(function.Id)) ?? throw new FunctionNotFoundException(function.Id);
					helpers.Log(nameof(Integration), nameof(UpdateResources), $"Updating resource of function {function.Name} to {function.Resource?.Name}");
					functionToUpdate.Resource = function.Resource;
				}
			}

			return serviceToUpdate;
		}

		/// <summary>
		/// Generates a new Ceiton Event.
		/// Used in the Ceiton and Feenix integrations.
		/// </summary>
		/// <param name="projectName">Name of the Ceiton project.</param>
		/// <param name="projectNumber">Number of the Ceiton project.</param>
		/// <param name="productNumbers">Numbers of the products that are part of the Ceiton project.</param>
		/// <param name="start">Start time of the project.</param>
		/// <param name="end">End time of the project.</param>
		/// <returns>New event with integration type set to Ceiton.</returns>
		protected Event GenerateNewCeitonEvent(string projectName, string projectNumber, IEnumerable<string> productNumbers, DateTime start, DateTime end)
		{
			return new Event(helpers)
			{
				Name = String.Format("{0}[{1}]", projectName, projectNumber),
				ProjectNumber = projectNumber,
				ProductNumbers = productNumbers.Distinct().ToList(),
				Start = start,
				End = end,
				Contract = "None",
				Company = Company,
				IntegrationType = IntegrationType.Ceiton,
				Status = YLE.Event.Status.Planned,
				Info = "Automatically created by Ceiton integration"
			};
		}

		protected void UpdateEventContractAndRights(Event eventInfo)
		{
			if (CompanyDetails == null || CompanyDetails.Contracts == null)
			{
				return;
			}

			var baseContract = CompanyDetails.Contracts.FirstOrDefault(c => c.Type == ContractType.BaseContract);
			if (baseContract != null)
			{
				eventInfo.Contract = baseContract.Name;
				eventInfo.Company = CompanyDetails.Company;
				eventInfo.SecurityViewIds = GetSecurityViewIds(baseContract); // All Ceiton events should be visible for whole YLE
			}
		}

		private HashSet<int> GetSecurityViewIds(Contract contract)
		{
			HashSet<int> viewIds = new HashSet<int>();

			// All orders should be visible to MCR users
			viewIds.Add(CompanyDetails.McrSecurityViewId);

			// All orders should be visible to the YLE users
			viewIds.Add(CompanyDetails.SecurityViewId);

			// If the company has linked orders, the orders should be visible to them as well
			foreach (Company linkedCompany in contract.LinkedCompanies)
			{
				viewIds.Add(linkedCompany.SecurityViewId);
			}

			return viewIds;
		}

		[Flags]
		public enum IgnoredItems
		{
			None = 0,
			StartTime = 1,
			AudioConfig = 2,
			ServiceSelection = 4, // Used for EBU multi-feed service updates
			Resources = 8,
			AudioChannels = 16,
			UnenforcedResources = 32, 
		}
	}
}