namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Automation;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.Events;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using DataMinerInterface = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface.DataMinerInterface;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using VirtualPlatform = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform;

	public class AutomationHandler
	{
		private const int GenericDveTableId = 65132;

		private readonly object locker = new object();

		private readonly Helpers helpers;
		private readonly Service service;

		private readonly IReadOnlyDictionary<VirtualPlatform, IAutomationConfiguration> automatedServiceConfigurations = new Dictionary<VirtualPlatform, IAutomationConfiguration>
		{
			{ VirtualPlatform.ReceptionSatellite, new SatellliteRxAutomationConfiguration() }
		};

		private AutomationHandler(Helpers helpers, Service service)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.service = service ?? throw new ArgumentNullException(nameof(service));
		}

		public static void ApplyProfiles(Helpers helpers, Service service)
		{
			var automationHandler = new AutomationHandler(helpers, service);

			automationHandler.ApplyProfiles();
		}

		/// <summary>
		/// Will launch profile load script for each service function node.
		/// </summary>
		private void ApplyProfiles()
		{
			helpers.LogMethodStart(nameof(AutomationHandler), nameof(ApplyProfiles), out var stopWatch);
			if (!automatedServiceConfigurations.TryGetValue(service.Definition.VirtualPlatform, out IAutomationConfiguration config))
			{
				helpers.Log(nameof(AutomationHandler), nameof(ApplyProfiles), $"No automation required for service: {service.Name} of type {service.Definition.VirtualPlatform.GetDescription()}");
				helpers.LogMethodCompleted(nameof(AutomationHandler), nameof(ApplyProfiles), null, stopWatch);
				return;
			}

			if (!ResourcesAllowAutomation(config.AutomatedFunctions))
			{
				helpers.Log(nameof(AutomationHandler), nameof(ApplyProfiles), $"No automation performed for service {service.Name} as none of its assigned resources allow automation '{String.Join(";", service.Functions.Select(f => f.Name + "|" + f.Resource?.Name))}'");
				helpers.LogMethodCompleted(nameof(AutomationHandler), nameof(ApplyProfiles), null, stopWatch);
				return;
			}

			if (ResourcesAreCurrentlyInUse(config.AutomatedFunctions))
			{
				Log(nameof(ApplyProfiles), $"No automation performed for service {service.Name} as the applied resources are currently in use. Function resources: '{String.Join(";", service.Functions.Select(f => f.Name + "|" + f.Resource?.Name))}'");
				helpers.LogMethodCompleted(nameof(AutomationHandler), nameof(ApplyProfiles), null, stopWatch);
				return;
			}

			EnableFunctionElements(config.AutomatedFunctions);
			CreateBookingConfiguration(out var enhancedAction, out var srmBookingConfiguration);
			HandleServiceOrchestration(enhancedAction, srmBookingConfiguration, config.AutomatedFunctions);

			helpers.LogMethodCompleted(nameof(AutomationHandler), nameof(ApplyProfiles), null, stopWatch);
		}

		private void CreateBookingConfiguration(out LsoEnhancedAction enhancedAction, out SrmBookingConfiguration srmBookingConfiguration)
		{
			var reservationInstance = helpers.ServiceManager.GetReservation(service.Id);

			var bookingManagerInfo = new BookingManagerInfo
			{
				Action = BookingOperationAction.New,
				Element = reservationInstance.FindBookingManagerName(),
				Reason = null,
				ServiceId = null,
				TableIndex = service.Id.ToString(),
			};

			enhancedAction = new LsoEnhancedAction { Event = SrmEvent.START_BOOKING_WITH_PREROLL };
			srmBookingConfiguration = new SrmBookingConfiguration(service.Id.ToString(), bookingManagerInfo, enhancedAction.Event, (Engine)helpers.Engine);
		}

		public void HandleServiceOrchestration(LsoEnhancedAction enhancedAction, SrmBookingConfiguration srmBookingConfiguration, IEnumerable<Guid> functionsToAutomate)
		{
			try
			{
				if (service.Definition.VirtualPlatform != VirtualPlatform.ReceptionSatellite)
				{
					return;
				}

				if (string.Equals(enhancedAction.PreviousServiceState, DefaultValue.StateFailed, StringComparison.InvariantCultureIgnoreCase))
				{
					throw new InvalidOperationException($"Booking {srmBookingConfiguration.Reservation.Name} is in Failed State, so orchestration will be done for Action: {enhancedAction}");
				}

				if (!srmBookingConfiguration.ShouldConfigureResources)
				{
					helpers.Log(nameof(AutomationHandler), nameof(HandleServiceOrchestration), $"Resource configuration isn't enabled for service: {service.Name}");
					return;
				}

				ExecuteServiceOrchestration(enhancedAction, srmBookingConfiguration);
				SetServiceProfileConfigDataToSucceeded();
			}
			catch (Exception e)
			{
				helpers.Log(nameof(AutomationHandler), nameof(HandleServiceOrchestration), $"Something went wrong during service orchestration processing: {e}");
				service.ProfileConfigurationFailReason = $"Service orchestration couldn't be executed to set the profile automated configuration due to:  {e.Message}";
				throw;
			}
		}

		[SuppressMessage("SonarCloud", "S2583", Justification = "False positive, nonConfiguredResources is incremented from ParallelForEach")]
		private void ExecuteServiceOrchestration(LsoEnhancedAction enhancedAction, SrmBookingConfiguration srmBookingConfiguration)
		{
			if (!automatedServiceConfigurations.TryGetValue(service.Definition.VirtualPlatform, out var automatedServiceConfiguration))
			{
				helpers.Log(nameof(AutomationHandler), nameof(ExecuteServiceOrchestration), $"No automation needed for service: {service.Name} of type {service.Definition.VirtualPlatform.GetDescription()}");
				return;
			}

			var resourcesToAutomate = srmBookingConfiguration.GetResources().Where(x => !x.IsContributingResource && automatedServiceConfiguration.AutomatedFunctions.Contains(x.FunctionDefinition.Id)).ToArray();

			var totalResources = resourcesToAutomate.Length;
			var configuredResources = 0;
			var nonConfiguredResources = 0;
			var resourceExceptionMessages = new List<string>();

			// Similar to a foreach but in parallel
			resourcesToAutomate.AsParallel().ForAll(resource =>
			{
				// Because this code will run in multiple threads at the same time we cannot do confiredResource++
				Interlocked.Increment(ref configuredResources);

				try
				{
					switch (enhancedAction.Event)
					{
						case SrmEvent.START_BOOKING_WITH_PREROLL:
							resource.ApplyProfile("APPLY");
							break;

						case SrmEvent.START:
						case SrmEvent.STOP:
						case SrmEvent.STOP_BOOKING_WITH_POSTROLL:
							break;

						default:
							// nothing to configure here
							break;
					}
				}
				catch (Exception e)
				{
					Interlocked.Increment(ref nonConfiguredResources);

					lock (locker)
					{
						string exceptionMessage = $"Booking function {resource.Identifier} ({configuredResources}/{totalResources}) could not be successfully configured:";
						helpers.Log(nameof(AutomationHandler), nameof(ExecuteServiceOrchestration), $"{exceptionMessage}\r\n{e}");

						resourceExceptionMessages.Add($"{exceptionMessage} {e.Message}");
					}
				}
			});

			// Notify the caller script that the booking configuration has failed
			if (nonConfiguredResources > 0)
			{
				throw new SrmConfigurationException($"Failed to configure {nonConfiguredResources} resources:\r\n{string.Join("\r\n", resourceExceptionMessages)}");
			}
		}

		/// <summary>
		/// Checks if at least one of the assigned resources allows automation.
		/// </summary>
		/// <param name="functionsToAutomate">GUIDs of the functions that allow automation.</param>
		/// <returns>True if at least one function has a resource assigned to it that has the IsAutomated property set to true.</returns>
		private bool ResourcesAllowAutomation(IEnumerable<Guid> functionsToAutomate)
		{
			return helpers.OrderManagerElement.IsDeviceAutomationEnabled(service) && service.Functions.Exists(x => functionsToAutomate.Contains(x.Id) && x.Resource?.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsAutomated) == true);
		}

		private bool ResourcesAreCurrentlyInUse(IEnumerable<Guid> functionsToAutomate)
		{
			foreach (var function in service.Functions)
			{
				if (function.Resource is null || !functionsToAutomate.Contains(function.Id)) continue;

				var occupyingServices = helpers.ResourceManager.GetOccupyingServices(function.Resource, DateTime.Now, service.StartWithPreRoll, service.OrderReferences.First(), service.Name);

				Log(nameof(ResourcesAreCurrentlyInUse), $"Function {function.Name} resource {function.Resource.Name} is currently used by: {string.Join("\n", occupyingServices.Select(os => os.ToString()))}");

				if (occupyingServices.Any())
				{
					return true;
				}
			}

			return false;
		}

		private void EnableFunctionElements(IEnumerable<Guid> functionsToAutomate)
		{
			foreach (var function in service.Functions)
			{
				try
				{
					if (!functionsToAutomate.Contains(function.Id))
					{
						Log(nameof(EnableFunctionElements), $"Skipping function: {function.Name} as no automation is required for this function.");
						continue;
					}

					if (function.Resource == null)
					{
						Log(nameof(EnableFunctionElements), $"Skipping function: {function.Name} as no resource is assigned.");
						continue;
					}

					if (!function.Resource.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsAutomated))
					{
						Log(nameof(EnableFunctionElements), $"Skipping function: {function.Name} as resource {function.Resource.Name} does not have the {ResourcePropertyNames.IsAutomated} property set to {true}.");
						continue;
					}

					EnableFunctionElement(function);
				}
				catch (Exception e)
				{
					Log(nameof(EnableFunctionElements), $"Something went wrong while enabling function element for function {function.Definition.Label}: {e}");
					service.ProfileConfigurationFailReason = $"Function DVE elements could not be enabled for this service while executing the profile automated configuration due to following reason: {e.Message}";
					throw;
				}
			}
		}

		private void EnableFunctionElement(YLE.Function.Function function)
		{
			var dms = Engine.SLNetRaw.GetDms();
			var mainElementId = new DmsElementId(function.Resource.MainDVEDmaID, function.Resource.MainDVEElementID);
			if (!dms.ElementExists(mainElementId))
			{
				Log(nameof(EnableFunctionElement), $"Skipping function: {function.Name} as the Main DVE Element with ID {mainElementId} could not be found.");
				return;
			}

			var mainElement = helpers.Engine.FindElementByKey(mainElementId.Value);
			var genericDveTable = DataMinerInterface.Element.GetTable(helpers, mainElement, GenericDveTableId);

			var functionElementId = new DmsElementId(function.Resource.DmaID, function.Resource.ElementID);

			// Order name should be visible in Visio when in use.
			mainElement.SetPropertyValue(FunctionElementPropertyNames.EventName, service.OrderName); // Event Name is used over the whole Visio.
			bool propertyGotUpdated = Retry(() => { return mainElement.GetPropertyValue(FunctionElementPropertyNames.EventName) == service.OrderName; }, TimeSpan.FromSeconds(5)); //Waiting until the property has the right value.

			Log(nameof(EnableFunctionElement), $"Property event name set succeeded: {propertyGotUpdated}| Order name property value: {service.OrderName}");

			if (dms.ElementExists(functionElementId))
			{
				Log(nameof(EnableFunctionElement), $"Function DVE element with ID {functionElementId} is available.");
				return;
			}

			string functionElementFullId = $"{functionElementId.AgentId}/{functionElementId.ElementId}";
			var matchingRow = genericDveTable.Values.FirstOrDefault(row => Convert.ToString(row[2]) == functionElementFullId);

			if (matchingRow == null)
			{
				Log(nameof(EnableFunctionElement), $"Skipping function {function.Name} because no row in the [Generic DVE Table] of Main DVE Element {mainElement.Name} [{mainElementId}] mentions DVE Element {functionElementId.AgentId}/{functionElementId.ElementId}");
				return;
			}

			// set DVE to enabled
			mainElement.SetParameterByPrimaryKey(65136, Convert.ToString(matchingRow[0]), 1);

			Log(nameof(EnableFunctionElement), $"Set column 'DVE State' to enabled for row {Convert.ToString(matchingRow[0])} on main resource element {mainElementId}");

			bool elementIsAvailable = Retry(() => { return ResourceElementIsCreated(dms, functionElementId); }, helpers.OrderManagerElement.EnableResourceElementTimeout); //Waiting until the function elements are available in the system & checking if the property has the right value

			if (!elementIsAvailable)
			{
				throw new InvalidOperationException($"Function DVE element with ID {functionElementId} is not available {helpers.OrderManagerElement.EnableResourceElementTimeout} after enabling it");
			}
		}

		private bool ResourceElementIsCreated(IDms dms, DmsElementId resourceElementId)
		{
			bool elementExists = dms.ElementExists(resourceElementId);

			Log(nameof(ResourceElementIsCreated), $"Resource element {resourceElementId} is {(elementExists ? string.Empty : "not ")}created");

			return elementExists;
		}

		private void SetServiceProfileConfigDataToSucceeded()
		{
			service.ProfileConfigurationFailReason = String.Empty;
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
					Thread.Sleep(100);
				}
			}
			while (!success && sw.Elapsed <= timeout);

			return success;
		}

		private void Log(string nameOfMethod, string message)
		{
			helpers?.Log(nameof(AutomationHandler), nameOfMethod, message);
		}
	}
}
