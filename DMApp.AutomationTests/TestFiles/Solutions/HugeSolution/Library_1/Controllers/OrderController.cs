namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library_1.EventArguments;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.EventArguments;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers.NormalOrderController;
	using Service = Service.Service;

	public abstract class OrderController : HelpedObject, IDisableableUi
	{
		protected readonly Order order;
		protected readonly UserInfo userInfo;
		protected readonly IEnumerable<Service> controlledServicesWhereOrderRefersTo;
		private LiveVideoOrder liveVideoOrderr;

		protected readonly Dictionary<string, DisplayedService> cachedReceptionServices = new Dictionary<string, DisplayedService>(); // Cached by Service Definition Name
		protected readonly Dictionary<string, DisplayedService> cachedBackupReceptionServices = new Dictionary<string, DisplayedService>(); // Cached by Service Definition Name
		protected readonly Dictionary<Guid, DisplayedService> cachedSharedSourceServices = new Dictionary<Guid, DisplayedService>(); // Cached by Service ID

		protected IReadOnlyDictionary<VirtualPlatformType, IReadOnlyList<ServiceDefinition>> serviceDefinitions;
		protected IReadOnlyDictionary<VirtualPlatformType, IReadOnlyList<ServiceDefinition>> allowedServiceDefinitions;

		protected readonly Dictionary<Service, ServiceController> serviceControllers = new Dictionary<Service, ServiceController>();

		protected OrderController(Helpers helpers, Order order, UserInfo userInfo, OrderSection orderSection = null, IEnumerable<Service> controlledServicesWhereOrderRefersTo = null) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.controlledServicesWhereOrderRefersTo = controlledServicesWhereOrderRefersTo ?? new Service[0];

			ConfirmSharedRoutingServicesAreValid(this.controlledServicesWhereOrderRefersTo.ToList());
		}

		public abstract OrderSection OrderSection { get; }

		public Guid OrderId => order.Id;

		public event EventHandler UploadJsonButtonPressed;

		public event EventHandler<ServiceEventArgs> UploadSynopsisButtonPressed;

		public event EventHandler<ServiceReplacedEventArgs> ServiceReplaced;

		public event EventHandler SourceChanged;

		public event EventHandler ValidationRequired;

		public event EventHandler UiDisableRequired;

		public event EventHandler UiEnableRequired;

		public event EventHandler<ServiceEventArgs> BookEurovisionService;

		public event EventHandler<ServiceEventArgs> SharedSourceUnavailableDueToOrderTimingChange;

		protected LiveVideoOrder LiveVideoOrder => liveVideoOrderr ?? (liveVideoOrderr = new LiveVideoOrder(helpers, order));

		protected abstract IReadOnlyDictionary<VirtualPlatformType, List<List<DisplayedService>>> CachedSourceChildServices { get; }

		public abstract void AddOrReplaceSection(OrderSection orderSection);

		protected abstract IEnumerable<DisplayedService> GetCachedServices();

		protected void OnUploadSynopsisButtonPressed(Service service)
		{
			UploadSynopsisButtonPressed?.Invoke(this, new ServiceEventArgs(service));
		}

		public void InvokeValidationRequired()
		{
			Log(nameof(InvokeValidationRequired), "Invoking order validation");
			ValidationRequired?.Invoke(this, EventArgs.Empty);
		}

		protected void OnUploadJsonButtonPressed()
		{
			UploadJsonButtonPressed?.Invoke(this, EventArgs.Empty);
		}

		protected void OnBookEurovisionService(Service service)
		{
			BookEurovisionService?.Invoke(this, new ServiceEventArgs(service));
		}

		protected void OnSharedSourceUnavailableDueToOrderTimingChange()
		{
			SharedSourceUnavailableDueToOrderTimingChange?.Invoke(this, new ServiceEventArgs(order.SourceService));
		}

		/// <summary>
		/// Loops over all services in the Order and updates the Order start and end time accordingly.
		/// </summary>
		public virtual void HandleServiceTimeUpdate()
		{
			var endpointServices = order.AllServices.Where(x => !x.Children.Any());
			foreach (var endpointService in endpointServices)
			{
				order.UpdateAutoGeneratedServiceTimings(helpers, endpointService);
			}

			if (order.AllServices.Any(x => !x.IsSharedSource))
			{
				order.Start = order.AllServices.Where(s => !s.IsSharedSource).Select(s => s.Start).Min();
				Log(nameof(HandleServiceTimeUpdate), "Setting order start time to " + order.Start);

				order.End = order.AllServices.Where(s => !s.IsSharedSource).Select(s => s.End).Max();
				Log(nameof(HandleServiceTimeUpdate), "Setting order end time to " + order.End);
			}
		}

		public void HandleServiceStartTimeUpdate(Service service, DateTime previousStartTime)
		{
			HandleServiceTimeUpdate();

			if (helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.Action == EditOrderFlows.EditTimingForService_FromRecordingApp && service.Start < previousStartTime)
			{
				var source = LiveVideoOrder.GetSource(service).Service;

				if (!source.IsSharedSource && service.Start < source.Start)
				{
					Log(nameof(HandleServiceStartTimeUpdate), $"Bringing the source forward to {service.Start} to match service {service.Name}");

					// When the recording is brought forward, bring the non-shared source forward to match the recording. [DCP217547]
					if (serviceControllers.TryGetValue(source, out var sourceController))
					{
						sourceController.UpdateStartTime(service.Start);
					}
					else
					{
						throw new NotFoundException($"Unable to find controller for service {source.Name}");
					}
				}
			}
		}

		public void HandleServiceEndTimeUpdate(Service service, DateTime previousEndTime)
		{
			HandleServiceTimeUpdate();

			if (helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.Action == EditOrderFlows.EditTimingForService_FromRecordingApp && previousEndTime < service.End)
			{
				var source = LiveVideoOrder.GetSource(service).Service;

				if (!source.IsSharedSource && source.End < service.End)
				{
					Log(nameof(HandleServiceStartTimeUpdate), $"Extending the source to {service.End} to match service {service.Name}");

					// When the recording is extended, extend the non-shared source to match the recording. [DCP217547]
					if (serviceControllers.TryGetValue(source, out var sourceController))
					{
						sourceController.UpdateEndTime(service.End);
					}
					else
					{
						throw new NotFoundException($"Unable to find controller for service {source.Name}");
					}
				}
			}
		}

		public void CopyOrderPlasmaIdToServices()
		{
			foreach (var recordingConfiguration in order.AllServices.Select(s => s.RecordingConfiguration))
			{
				if (recordingConfiguration.CopyPlasmaIdFromOrder)
				{
					recordingConfiguration.PlasmaIdForArchive = order.PlasmaId;
				}
			}

			InvokeValidationRequired();
		}

		public void UnsubscribeFromUi()
		{
			OrderSection.RemoveAllSubscribers();
		}

		public abstract void HandleSelectedResourceUpdate(Service service, Function function);

		protected virtual void InitializeDisplayedOrder()
		{
			order.SelectableSecurityViewIds = GetSelectableCompanyViewIds();
			order.SetSelectableUserGroups(userInfo.AllUserGroups.Where(u => u.Company == userInfo.Contract.Company));
			order.SetSelectableCompanies(userInfo.AllCompanies);

			order.AvailableSourceServices = allowedServiceDefinitions[order.SourceService.Definition.VirtualPlatformServiceType].Select(x => x.VirtualPlatformServiceName.GetDescription()).Distinct().ToList();
			order.AvailableSourceServiceDescriptions = allowedServiceDefinitions[order.SourceService.Definition.VirtualPlatformServiceType].Where(x => x.VirtualPlatform == order.SourceService.Definition.VirtualPlatform).Select(x => x.Description).ToList();

			order.AvailableBackupSourceServices = allowedServiceDefinitions[order.SourceService.Definition.VirtualPlatformServiceType].Select(x => x.VirtualPlatformServiceName.GetDescription()).Distinct().ToList();
			var availableBackupSourceServiceDescriptions = order.BackupSourceService == null ? new List<string>() : allowedServiceDefinitions[order.SourceService.Definition.VirtualPlatformServiceType].Where(x => x.VirtualPlatform == order.BackupSourceService.Definition.VirtualPlatform).Select(x => x.Description).ToList();
			order.AvailableBackupSourceServiceDescriptions = availableBackupSourceServiceDescriptions;

			if (!order.RecurringSequenceInfo.Recurrence.IsConfigured)
			{
				order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.Day = (DaysOfTheWeek)(1 << ((int)order.Start.DayOfWeek + 6) % 7);
				order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.SelectableUmpteenthDayOfTheMonthOption = new RecurrenceRepeat.SelectableOption
				{
					DisplayValue = $"Monthly on day {order.Start.Day}",
					UmpteethDay = order.Start.Day
				};

				order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption = new RecurrenceRepeat.SelectableOption
				{
					DisplayValue = GetUmpteenthWeekDayOfTheMonthOption(),
					Day = order.Start.DayOfWeek.ToString().GetEnumValue<DaysOfTheWeek>(),
					UmpteethDay = (int)Math.Floor(order.Start.Day / 7.0) + 1
				};
			}
		}

		protected void InitializeCachedMainServices()
		{
			InitializeCachedReceptionService(order.SourceService);

			foreach (var service in OrderManager.FlattenServices(order.SourceService.Children))
			{
				switch (service.Definition.VirtualPlatformServiceType)
				{
					case VirtualPlatformType.Reception:
						throw new InvalidOperationException("VP Reception should already have been handled");

					case VirtualPlatformType.Recording:
					case VirtualPlatformType.Destination:
					case VirtualPlatformType.Transmission:
					case VirtualPlatformType.VizremFarm:
					case VirtualPlatformType.VizremStudio:
						if (service.BackupType == BackupType.None)
						{
							CachedSourceChildServices[service.Definition.VirtualPlatformServiceType].Add(new List<DisplayedService> { service as DisplayedService });
						}
						break;
					default:
						// No cache required as the service is not being displayed.
						break;
				}
			}
		}

		protected void InitializeCachedReceptionService(Service service)
		{
			if (service.BackupType == BackupType.None)
			{
				if (service.IsSharedSource)
				{
					cachedSharedSourceServices.Add(service.Id, service as DisplayedService);
				}
				else
				{
					cachedReceptionServices.Add(service.Definition.Name, service as DisplayedService);
				}
			}
			else
			{
				cachedBackupReceptionServices.Add(service.Definition.Name, service as DisplayedService);
			}
		}

		private string GetUmpteenthWeekDayOfTheMonthOption()
		{
			var umpteenthOccurrenceOfWeekDayOfTheMonth = (int)Math.Floor(order.Start.Day / 7.0) + 1;

			string postfix;
			switch (umpteenthOccurrenceOfWeekDayOfTheMonth)
			{
				case 1:
					postfix = "st";
					break;
				case 2:
					postfix = "nd";
					break;
				case 3:
					postfix = "rd";
					break;
				case 4:
				case 5:
					postfix = "th";
					break;
				default:
					postfix = "error";
					break;
			}

			return $"Monthly on the {umpteenthOccurrenceOfWeekDayOfTheMonth}{postfix} {order.Start.DayOfWeek.ToString()}";
		}

		internal Service ServiceTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			var newService = controlledServicesWhereOrderRefersTo.Distinct().SingleOrDefault(x => x.Definition?.Description == e.Selected);
			var existingSourceService = order.Sources.SingleOrDefault(s => s.BackupType == BackupType.None) ?? throw new ServiceNotFoundException($"Unable to find an main source service in order {order.Name}", true);

			if (newService != null && newService.IsSharedSource)
			{
				if (!newService.IsBooked) Service.CopyProfileParameterValuesFromPreviousToNewFunction(existingSourceService, newService);
				order.ChangeOrderMainSourceService(existingSourceService, newService);
			}
			else
			{
				var previousExistingService = controlledServicesWhereOrderRefersTo.Distinct().SingleOrDefault(x => x.Definition?.Description == e.Previous);
				if (newService == null || previousExistingService == null) return null;

				if (!newService.IsBooked) Service.CopyProfileParameterValuesFromPreviousToNewFunction(previousExistingService, newService);

				if (newService.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception) order.ChangeOrderMainSourceService(previousExistingService, newService);
				else order.ChangeOrderService(previousExistingService, newService);
			}

			return newService;
		}

		public void HandleProfileParameterUpdate(Service service, Function function, ProfileParameter changedProfileParameter)
		{
			// Cover all actions that need to happen accross the order when a service profile parameter value changes

			if (service.Equals(order.SourceService) && (changedProfileParameter.Id == ProfileParameterGuids.VideoFormat || ProfileParameterGuids.IsAudioChannelProfileParameter(changedProfileParameter.Id)))
			{
				foreach (var child in OrderManager.FlattenServices(service.Children))
				{
					SetValuesBasedOnSourceService(child, order.SourceService);
				}
			}

			bool remoteGraphicsChangedOnGraphicsProcessingService = changedProfileParameter.Id == ProfileParameterGuids.RemoteGraphics && service.Definition.VirtualPlatform == VirtualPlatform.GraphicsProcessing;
			if (remoteGraphicsChangedOnGraphicsProcessingService)
			{
				// If the value of the Remote Graphics profile parameter on the graphics processing service is manually changed by a user, we should update the child endpoint services of the gfx proc service accordingly

				foreach (var child in OrderManager.FlattenServices(service.Children))
				{
					var remoteGraphicsParameter = child.Functions.SelectMany(f => f.Parameters).SingleOrDefault(p => p.Id == ProfileParameterGuids.RemoteGraphics);
					if (remoteGraphicsParameter is null) continue;

					remoteGraphicsParameter.Value = changedProfileParameter.Value;
				}
			}

			if (ProfileParameterGuids.AllProcessingParameterGuids.Contains(changedProfileParameter.Id))
			{
				LiveVideoOrder.AddOrUpdateProcessingServices();
			}
		}

		protected virtual void SubscribeToUi()
		{
			if (OrderSection == null) return;

			OrderSection.SourceChanged += Section_SourceChanged;
			OrderSection.SourceDescriptionChanged += Section_SourceDescriptionChanged;
			OrderSection.SourceServiceSectionAdded += (s, addedSourceServiceSection) =>
			{
				RegisterServiceController(addedSourceServiceSection.Service);
				AddServiceSectionToServiceController(addedSourceServiceSection);
			};

			// General information section subscriptions
			OrderSection.GeneralInfoSection.NameFocusLost += (s, e) => OrderNameFocusLost(e);
			OrderSection.GeneralInfoSection.StartChanged += (s, e) => OrderStartTimeChanged(e);
			OrderSection.GeneralInfoSection.StartNowChanged += (s, e) => OrderStartNowChanged(e);
			OrderSection.GeneralInfoSection.EndChanged += (s, e) => OrderEndTimeChanged(e);
			OrderSection.GeneralInfoSection.SecurityViewIdsChanged += (s, selectedSecurityViewIds) => order.SetSecurityViewIds(selectedSecurityViewIds);
			OrderSection.GeneralInfoSection.DisplayedPropertyChanged += (s, e) => order.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);
			OrderSection.GeneralInfoSection.BillableCompanyChanged += (s, e) => order.BillingInfo.BillableCompany = e;
			OrderSection.GeneralInfoSection.CustomerCompanyChanged += (s, e) => order.BillingInfo.CustomerCompany = e;
			OrderSection.GeneralInfoSection.PlasmaIdChanged += (s, plasmaIdToSet) =>
			{
				order.PlasmaId = plasmaIdToSet;
				CopyOrderPlasmaIdToServices();
				InvokeValidationRequired();
			};

			// General information recurrence section subscriptions
			OrderSection.GeneralInfoSection.RecurrenceSection.RecurrenceCheckBoxChanged += RecurrenceSection_RecurrenceCheckBoxChanged;
			OrderSection.GeneralInfoSection.RecurrenceSection.RepeatEveryAmountChanged += (s, frequency) => order.RecurringSequenceInfo.Recurrence.RecurrenceFrequency.Frequency = frequency;
			OrderSection.GeneralInfoSection.RecurrenceSection.RepeatEveryUnitChanged += (s, unit) => order.RecurringSequenceInfo.Recurrence.RecurrenceFrequency.FrequencyUnit = unit;
			OrderSection.GeneralInfoSection.RecurrenceSection.RepeatTypeChanged += (s, repeatType) => order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.RepeatType = repeatType;
			OrderSection.GeneralInfoSection.RecurrenceSection.UmpteenthDayOfTheMonthChanged += (s, umpteenthDayOfTheMonth) => order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.UmpteenthDayOfTheMonth = umpteenthDayOfTheMonth;
			OrderSection.GeneralInfoSection.RecurrenceSection.DayOfTheWeekChanged += (s, dayOfTheWeek) => order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.Day = dayOfTheWeek;
			OrderSection.GeneralInfoSection.RecurrenceSection.UmpteenthOccurrenceOfWeekDayOfTheMonthChanged += (s, umpteenthOccurrenceOfWeekDayOfTheMonthChanged) => order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.UmpteenthOccurrenceOfWeekDayOfTheMonth = umpteenthOccurrenceOfWeekDayOfTheMonthChanged;
			OrderSection.GeneralInfoSection.RecurrenceSection.EndingTypeChanged += (s, endingType) => order.RecurringSequenceInfo.Recurrence.RecurrenceEnding.EndingType = endingType;
			OrderSection.GeneralInfoSection.RecurrenceSection.EndingDateTimeChanged += (s, endingDateTime) => order.RecurringSequenceInfo.Recurrence.RecurrenceEnding.EndingDateTime = endingDateTime;
			OrderSection.GeneralInfoSection.RecurrenceSection.AmountOfRepeatsChanged += (s, amountOfRepeats) => order.RecurringSequenceInfo.Recurrence.RecurrenceEnding.AmountOfRepeats = amountOfRepeats;

			foreach (var serviceSelectionSection in OrderSection.SourceChildSections)
			{
				SubscribeToServiceSelectionSection(serviceSelectionSection);
			}

			OrderSection.ServiceSelectionSectionAdded += (s, e) => RegisterServiceSelectionSection(e);
		}

		protected void RecurrenceSection_RecurrenceCheckBoxChanged(object sender, bool isChecked)
		{
			order.NamePostFix = isChecked ? $" [{order.Start.ToFinnishDateString()}]" : string.Empty;

			order.RecurringSequenceInfo.Recurrence.IsConfigured = isChecked;
			order.RecurringSequenceInfo.Name = order.ManualName;
			order.RecurringSequenceInfo.Recurrence.StartTime = order.Start;
		}

		protected virtual void InitializeServiceControllers()
		{
			using (StartPerformanceLogging())
			{
				if (order.SourceService is null) return;

				RegisterServiceController(order.SourceService);

				foreach (var childService in order.SourceService.Descendants)
				{
					RegisterServiceController(childService);
				}
			}
		}

		protected void AddAllServiceSectionsToServiceControllers()
		{
			using (StartPerformanceLogging())
			{
				foreach (var service in order.AllServices)
				{
					var serviceSections = OrderSection.GetServiceSections(service);

					foreach (var section in serviceSections)
					{
						if (section is ServiceSection serviceSection)
						{
							AddServiceSectionToServiceController(serviceSection);
						}
						else if (section is ServiceSelectionSection serviceSelectionSection)
						{
							AddServiceSectionToServiceController(serviceSelectionSection.ServiceSection);
						}
					}
				}
			}
		}

		protected virtual void AddServiceSectionToServiceController(ServiceSection serviceSection)
		{
			var service = serviceSection.Service;

			if (serviceControllers.TryGetValue(serviceSection.Service, out var serviceController))
			{
				if (service.IsAutogenerated)
				{
					Log(nameof(RegisterServiceController), $"Adding new section {serviceSection} to service controller");

					serviceController.AddSection(serviceSection);
				}
				else
				{
					Log(nameof(RegisterServiceController), $"Replacing section '{serviceController.Sections.SingleOrDefault()}' with new section {serviceSection} in service controller");

					serviceController.ReplaceSection(serviceSection);
				}
			}
			else
			{
				throw new NotFoundException($"Unable to find service controller for {serviceSection.Service.Name}");
			}
		}

		protected virtual void RegisterServiceController(Service service)
		{
			using (UiDisabler.StartNew(this))
			{
				using (StartPerformanceLogging())
				{
					var displayedService = service as DisplayedService;

					if (serviceControllers.TryGetValue(displayedService, out var serviceController))
					{
						Log(nameof(RegisterServiceController), $"Service Controller for Service {displayedService.Name} ({displayedService.Definition.VirtualPlatform}.{displayedService.Definition.Description}) BackupType {displayedService.BackupType} already exists.");
					}
					else
					{
						Log(nameof(RegisterServiceController), $"Creating new Service Controller for Service: {displayedService.Name} ({displayedService.Definition.VirtualPlatform}.{displayedService.Definition.Description}) BackupType {displayedService.BackupType}");

						var serviceResourceAssignmentHandler = ResourceAssignmentHandler.Factory(helpers, displayedService, order);
						serviceController = new ServiceController(helpers, displayedService, this, GetSelectableCompanyViewIds(), new List<ServiceSection>(), serviceResourceAssignmentHandler, userInfo);
						serviceControllers.Add(displayedService, serviceController);

						displayedService.SetAvailableVirtualPlatformNames(allowedServiceDefinitions[displayedService.Definition.VirtualPlatformServiceType].Select(x => x.VirtualPlatformServiceName.GetDescription()));
						displayedService.SetAvailableServiceDescriptions(allowedServiceDefinitions[displayedService.Definition.VirtualPlatformServiceType].Where(x => x.VirtualPlatform == displayedService.Definition.VirtualPlatform).Select(x => x.Description));

						if (service.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite && helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.IsResourceChangeAction)
						{
							var matrixOutputLbandFunction = service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.MatrixOutputLband) as DisplayedFunction ?? throw new FunctionNotFoundException(FunctionGuids.MatrixOutputLband);

							matrixOutputLbandFunction.ResourceNameConverter = name => name.Split('.').Last();
						}
					}
				}
			}
		}

		protected void RegisterServiceSelectionSection(ServiceSelectionSection section)
		{
			SubscribeToServiceSelectionSection(section);

			RegisterServiceController(section.Service);
			AddServiceSectionToServiceController(section.ServiceSection);
		}

		private void SubscribeToServiceSelectionSection(ServiceSelectionSection section)
		{
			section.ServiceVirtualPlatformNameChanged += Section_ServiceVirtualPlatformNameChanged;
			section.ServiceDescriptionChanged += Section_ServiceDescriptionChanged;
			section.DeleteButtonPressed += ServiceSelectionSection_DeleteButtonPressed;
		}

		private void OrderNameFocusLost(string name)
		{
			order.ManualName = name;
			order.RecurringSequenceInfo.Name = name;

			foreach (var recordingService in order.AllServices.Where(x => x.Definition.VirtualPlatform == VirtualPlatform.Recording))
			{
				if (!String.IsNullOrWhiteSpace(recordingService.RecordingConfiguration.RecordingName)) continue;
				recordingService.RecordingConfiguration.RecordingName = name;
			}

			InvokeValidationRequired();
		}

		protected void OrderStartNowChanged(bool startNow)
		{
			order.StartNow = startNow;
			OrderStartTimeChanged(DateTime.Now.RoundToMinutes());
		}

		protected virtual void OrderStartTimeChanged(DateTime orderStartTime)
		{
			using (UiDisabler.StartNew(this))
			{
				TimeSpan startTimeDifference = orderStartTime.Subtract(order.Start);
				DateTime previousOrderStartTime = order.Start;

				order.Start = orderStartTime;
				if (order.End < orderStartTime) order.End = orderStartTime;

				foreach (var service in order.AllServices.Concat(GetCachedServices()))
				{
					if (service.IsSharedSource) continue;

					// DCP 206356 (no service start time change required if service start time is still within order time slot)
					if (service.Start != previousOrderStartTime && orderStartTime < service.Start) continue;

					UpdateServiceStartTime(orderStartTime, service);
				}

				order.NamePostFix = order.RecurringSequenceInfo.Recurrence.IsConfigured ? $" [{order.Start.ToFinnishDateString()}]" : order.NamePostFix;

				UpdateRecurrence(startTimeDifference);

				InvokeValidationRequired();
			}
		}

		private void UpdateServiceStartTime(DateTime orderStartTime, Service service)
		{
			if (serviceControllers.TryGetValue(service, out var serviceController))
			{
				serviceController.UpdateStartTime(orderStartTime);
				if (service.End < service.Start) serviceController.UpdateEndTime(orderStartTime);
			}
			else
			{
				service.Start = orderStartTime;
				if (service.End < service.Start) service.End = orderStartTime;
			}
		}

		protected virtual void OrderEndTimeChanged(DateTime orderEndTime)
		{
			using (UiDisabler.StartNew(this))
			{
				var previousOrderEnd = order.End;

				order.End = orderEndTime;
				if (order.Start > orderEndTime) order.Start = orderEndTime;

				foreach (var service in order.AllServices.Concat(GetCachedServices()))
				{
					if (service.IsSharedSource || service.EurovisionBookingDetails?.Type == Integrations.Eurovision.Type.NewsEvent || service.EurovisionBookingDetails?.Type == Integrations.Eurovision.Type.ProgramEvent) continue;

					UpdateServiceEndTime(orderEndTime, previousOrderEnd, service);
				}

				InvokeValidationRequired();
			}
		}

		private void UpdateServiceEndTime(DateTime orderEndTime, DateTime previousOrderEnd, Service service)
		{
			// DCP 206356 (no service end time change required if service end time is still within order time slot)
			if (service.End == previousOrderEnd || orderEndTime < service.End)
			{
				if (serviceControllers.TryGetValue(service, out var serviceController))
				{
					serviceController.UpdateEndTime(orderEndTime);
					if (service.Start > service.End && !service.ShouldBeRunning) serviceController.UpdateStartTime(orderEndTime);
				}
				else
				{
					service.End = orderEndTime;
					if (service.Start > service.End && !service.ShouldBeRunning) service.Start = orderEndTime;

					Log(nameof(OrderEndTimeChanged), $"Set service {service.Name} end time to {service.End.ToFullDetailString()} based on order end time");
				}
			}

			bool routingServiceIsRunning = service.IsOrShouldBeRunning && service.Definition.VirtualPlatform == VirtualPlatform.Routing;
			if (routingServiceIsRunning)
			{
				var matrixOutputFunction = service.Functions.Single(f => f.Id == FunctionGuids.MatrixOutputSdi);

				matrixOutputFunction.Resource = OccupiedResource.WrapIfOccupied(helpers, matrixOutputFunction.Resource, service.StartWithPreRoll, service.EndWithPostRoll, order.Id, service.Name);
			}
		}

		private void UpdateRecurrence(TimeSpan startTimeDifference)
		{
			if (order.RecurringSequenceInfo.RecurrenceAction == RecurrenceAction.New)
			{
				order.RecurringSequenceInfo.Recurrence.StartTime = order.Start;
			}

			if (order.RecurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly)
			{
				order.RecurringSequenceInfo.Recurrence.StartTime += startTimeDifference;
				order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.Day |= (DaysOfTheWeek)(1 << ((int)order.Start.DayOfWeek + 6) % 7);
				order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.SelectableUmpteenthDayOfTheMonthOption = new RecurrenceRepeat.SelectableOption
				{
					DisplayValue = $"Monthly on day {order.Start.Day}",
					UmpteethDay = order.Start.Day
				};

				order.RecurringSequenceInfo.Recurrence.RecurrenceRepeat.SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption = new RecurrenceRepeat.SelectableOption
				{
					DisplayValue = GetUmpteenthWeekDayOfTheMonthOption(),
					Day = order.Start.DayOfWeek.ToString().GetEnumValue<DaysOfTheWeek>(),
					UmpteethDay = (int)Math.Floor(order.Start.Day / 7.0) + 1
				};
			}
		}

		protected void UpdateLiveVideoOrder()
		{
			LiveVideoOrder?.Update();
		}

		private void Section_ServiceVirtualPlatformNameChanged(object sender, string newVirtualPlatformName)
		{
			var serviceSelectionSection = (ServiceSelectionSection)sender;

			var virtualPlatformType = serviceSelectionSection.Service.Definition.VirtualPlatformServiceType;

			var possibleServiceDefinitions = allowedServiceDefinitions[virtualPlatformType].Where(x => x.VirtualPlatformServiceName.GetDescription() == newVirtualPlatformName);

			var newServiceDefinition = possibleServiceDefinitions.FirstOrDefault(x => x.IsDefault) ?? possibleServiceDefinitions.FirstOrDefault() ?? throw new NotFoundException("Could not find a service definition");

			Log(nameof(Section_ServiceVirtualPlatformNameChanged), $"Selected Service Definition: {newServiceDefinition.Name}");

			ServiceSelectionSection_ServiceTypeChanged(serviceSelectionSection.Service, newVirtualPlatformName, newServiceDefinition.Description);
		}

		protected virtual void Section_ServiceDescriptionChanged(object sender, string newDescription)
		{
			ServiceSelectionSection serviceSelectionSection = (ServiceSelectionSection)sender;
			ServiceSelectionSection_ServiceTypeChanged(serviceSelectionSection.Service, serviceSelectionSection.SelectedVirtualPlatformName, newDescription);
		}

		protected abstract List<DisplayedService> GetCachedAlternativeServices(Service previousDisplayedService);

		protected void ServiceSelectionSection_ServiceTypeChanged(Service previousDisplayedService, string virtualPlatformName, string description)
		{
			Log(nameof(ServiceSelectionSection_ServiceTypeChanged), $"Virtual platform name {virtualPlatformName}, description {description}");

			var cachedAlternativeServices = GetCachedAlternativeServices(previousDisplayedService);

			var newDisplayedService = cachedAlternativeServices.FirstOrDefault(x => x.Definition.VirtualPlatformServiceName.GetDescription() == virtualPlatformName && x.Definition.Description == description);

			if (newDisplayedService == null)
			{
				GenerateNewChildService(previousDisplayedService, virtualPlatformName, description, out newDisplayedService, out var newServiceDefinition);

				InitializeServiceBeforeBeingAdded(newDisplayedService, previousDisplayedService);

				cachedAlternativeServices.Add(newDisplayedService);

				Log(nameof(ServiceSelectionSection_ServiceTypeChanged), $"Created new service {newDisplayedService.Name}");
			}
			else
			{
				Log($"Found existing cached service {newDisplayedService.Name}");
			}

			ReplaceService(previousDisplayedService, newDisplayedService);

			UpdateLiveVideoOrder();

			Log(nameof(ServiceSelectionSection_ServiceTypeChanged), $"Service {previousDisplayedService.Name} ({previousDisplayedService.Definition.VirtualPlatform}) was replaced with {newDisplayedService.Name} ({newDisplayedService.Definition.VirtualPlatform})");
		}

		protected abstract void GenerateNewChildService(Service previousDisplayedService, string virtualPlatformName, string description, out DisplayedService newDisplayedService, out ServiceDefinition newServiceDefinition);

		protected virtual void ReplaceService(Service existingService, DisplayedService newService)
		{
			if (existingService.Equals(newService)) return;

			using (StartPerformanceLogging())
			{
				ServiceReplaced?.Invoke(this, new ServiceReplacedEventArgs(existingService, newService)); // fire event before actually replacing

				order.ReplaceService(existingService, newService);

				UpdateLiveVideoOrder();

				LiveVideoOrder.AddOrUpdateProcessingServices();
			}
		}

		private void ServiceSelectionSection_DeleteButtonPressed(object sender, EventArgs e)
		{
			ServiceSelectionSection section = (ServiceSelectionSection)sender;
			var endpointService = section.Service as Service;

			DeleteEndpointService(endpointService);
		}

		protected virtual void DeleteEndpointService(Service sectionService)
		{
			using (UiDisabler.StartNew(this))
			{
				List<DisplayedService> cachedAlternativeServices = CachedSourceChildServices[sectionService.Definition.VirtualPlatformServiceType].First(x => x.Contains(sectionService));
				Log(nameof(DeleteEndpointService), $"Removing cached source services: {String.Join(", ", cachedAlternativeServices.Select(x => (x.Name + " " + x.Definition.Name)))}");

				CachedSourceChildServices[sectionService.Definition.VirtualPlatformServiceType].Remove(cachedAlternativeServices);

				var parentService = order.AllServices.FirstOrDefault(x => x.Children.Contains(sectionService)) ?? throw new ServiceNotFoundException($"Unable to find parent of {sectionService.Name}", true);
				parentService.Children.Remove(sectionService);

				Log(nameof(DeleteEndpointService), $"Removed {sectionService.Name} from the children of {parentService.Name}");

				// recursively remove all parents without children (e.g.: routing and processing)
				var serviceToBeChecked = parentService;
				while (!serviceToBeChecked.Children.Any())
				{
					parentService = order.AllServices.SingleOrDefault(x => x.Children.Contains(serviceToBeChecked));
					if (parentService is null) break;

					parentService.Children.Remove(serviceToBeChecked);

					Log(nameof(DeleteEndpointService), $"Removed {serviceToBeChecked.Name} from the children of {parentService.Name}, because {parentService.Name} has no children");

					serviceToBeChecked = parentService;
				}

				foreach (var serviceToRemove in cachedAlternativeServices)
				{
					serviceControllers.Remove(serviceToRemove);
				}

				UpdateLiveVideoOrder();

				LiveVideoOrder.AddOrUpdateProcessingServices();
			}
		}

		protected void Section_SourceChanged(object sender, string newSourceVirtualPlatformName)
		{
			using (UiDisabler.StartNew(this))
			{
				Log(nameof(Section_SourceChanged), $"Source Dropdown changed: {newSourceVirtualPlatformName}");

				var sourceServiceDefinitions = allowedServiceDefinitions[VirtualPlatformType.Reception].Where(x => x.VirtualPlatformServiceName.GetDescription() == newSourceVirtualPlatformName).ToList();

				var defaultServiceDefinition = sourceServiceDefinitions.FirstOrDefault(x => x.IsDefault) ?? sourceServiceDefinitions[0];

				Log(nameof(Section_SourceChanged), $"Default Service Definition: {defaultServiceDefinition.Name}");

				UpdateOrderSourceService(OrderSection.Source, defaultServiceDefinition.Description);

				Log(nameof(Section_SourceChanged), $"Setting allowed Source Service Descriptions to: {String.Join(", ", sourceServiceDefinitions.Select(s => s.Description).OrderBy(s => s))}");

				order.AvailableSourceServiceDescriptions = sourceServiceDefinitions.Select(s => s.Description).OrderBy(s => s).ToList();

				SourceChanged?.Invoke(this, EventArgs.Empty);

				InvokeValidationRequired();
			}
		}

		protected virtual void Section_SourceDescriptionChanged(object sender, string e)
		{
			using (UiDisabler.StartNew(this))
			{
				Log(nameof(Section_SourceDescriptionChanged), $"Source Description Dropdown changed: {e}");

				UpdateOrderSourceService(OrderSection.Source, e);
			}
		}

		protected void UpdateOrderSourceService(string source, string sourceDescription)
		{
			Log(nameof(UpdateOrderSourceService), $"Updating Source Service to {source} - {sourceDescription}");

			var serviceDefinition = allowedServiceDefinitions[order.SourceService.Definition.VirtualPlatformServiceType].FirstOrDefault(x => x.VirtualPlatformServiceName.GetDescription() == source && x.Description == sourceDescription) ?? throw new ServiceDefinitionNotFoundException($"Unable to find SD with VP name '{source}' and description '{sourceDescription}' between '{string.Join(", ", allowedServiceDefinitions[order.SourceService.Definition.VirtualPlatformServiceType].Select(sd => $"{sd.VirtualPlatformServiceName}({sd.Description})"))}'");

			if (!cachedReceptionServices.TryGetValue(serviceDefinition.Name, out var service))
			{
				service = new DisplayedService(helpers, serviceDefinition)
				{
					Start = order.Start,
					End = order.End,
				};

				if (order.IntegrationType != IntegrationType.None)
				{
					service.PreRoll = ServiceManager.GetPostRollDuration(serviceDefinition, order.IntegrationType);
					service.PostRoll = ServiceManager.GetPostRollDuration(serviceDefinition, order.IntegrationType);
				}

				service.AcceptChanges();

				cachedReceptionServices.Add(serviceDefinition.Name, service);
			}

			Log(nameof(UpdateOrderSourceService), $"Updating Source Service to {service.Name}");

			ReplaceService(order.SourceService, service);

			UpdateLiveVideoOrder();

			foreach (var child in OrderManager.FlattenServices(order.SourceService.Children))
			{
				SetValuesBasedOnSourceService(child, order.SourceService);
			}

			InvokeValidationRequired();
		}

		protected void SetValuesBasedOnSourceService(Service serviceToUpdate, Service sourceService)
		{
			if (!userInfo.Contract.IsVideoProcessingAllowed())
			{
				// If user contract does not allow video processing, video format profile parameter value should be copied to all relevant children to avoid generation of video processing services.

				Log(nameof(SetValuesBasedOnSourceService), $"User {userInfo.User.Name} contract {userInfo.Contract.Name} does not allow video processing. Copying video format from source to {serviceToUpdate.Name} required");

				var sourceVideoFormatProfileParameter = sourceService.Functions.SelectMany(f => f.Parameters).SingleOrDefault(pp => pp.Id == ProfileParameterGuids.VideoFormat);
				var childVideoFormatProfileParameter = serviceToUpdate.Functions.SelectMany(f => f.Parameters).SingleOrDefault(pp => pp.Id == ProfileParameterGuids.VideoFormat);

				if (sourceVideoFormatProfileParameter is null)
				{
					Log(nameof(SetValuesBasedOnSourceService), $"WARNING: Could not find profile parameter {ProfileParameterGuids.VideoFormat} in source service {sourceService.Name} to copy its value to service {serviceToUpdate.Name}");
				}
				else if (childVideoFormatProfileParameter is null)
				{
					Log(nameof(SetValuesBasedOnSourceService), $"WARNING: Could not find profile parameter {ProfileParameterGuids.VideoFormat} in service {serviceToUpdate.Name} to copy its value from source service {sourceService.Name}");
				}
				else
				{
					childVideoFormatProfileParameter.Value = sourceVideoFormatProfileParameter.Value;

					Log(nameof(SetValuesBasedOnSourceService), $"Copied source service {sourceService} profile parameter {sourceVideoFormatProfileParameter.Name} value {sourceVideoFormatProfileParameter.StringValue} to service {serviceToUpdate.Name}");
				}
			}

			if (!userInfo.Contract.IsAudioProcessingAllowed())
			{
				// If user contract does not allow audio processing, audio channel profile parameter values should be copied to all relevant children to avoid generation of audio processing services.
				serviceToUpdate.AudioChannelConfiguration.CopyFromSource(sourceService.AudioChannelConfiguration);
				serviceToUpdate.AudioChannelConfiguration.SetSourceOptions(sourceService.AudioChannelConfiguration.GetSourceOptions());
			}

			if (serviceToUpdate.Definition.Description.Equals("messi news", StringComparison.OrdinalIgnoreCase) && serviceToUpdate.IntegrationType == IntegrationType.None)
			{
				// Recording Messi News connected to a IP RX Vidigo source does not require routing

				bool sourceIsIpVidigoReception = sourceService.Definition.VirtualPlatform == VirtualPlatform.ReceptionIp && sourceService.Definition.Description == "Vidigo";

				serviceToUpdate.RequiresRouting = !sourceIsIpVidigoReception;

				Log(nameof(SetValuesBasedOnSourceService), $"Service {serviceToUpdate.Name} is a manual Messi News recording service, RequiresRouting property set to {serviceToUpdate.RequiresRouting}, based on if the source is IP Vidigo");
			}
		}

		public virtual void AddChildService(DisplayedService serviceToAdd)
		{
			if (serviceToAdd is null) throw new ArgumentNullException(nameof(serviceToAdd));

			InitializeServiceBeforeBeingAdded(serviceToAdd);

			CachedSourceChildServices[serviceToAdd.Definition.VirtualPlatformServiceType].Add(new List<DisplayedService> { serviceToAdd });
			order.SourceService.Children.Add(serviceToAdd);

			UpdateLiveVideoOrder();
		}

		protected virtual void InitializeServiceBeforeBeingAdded(DisplayedService serviceToAdd, Service replacedService = null)
		{
			serviceToAdd.RecordingConfiguration.RecordingName = order.Name;

			int amountOfMainServicesWithSameVirtualPlatformType = order.GetAllMainServices().Count(s => s.Definition.VirtualPlatformServiceType == serviceToAdd.Definition.VirtualPlatformServiceType) + 1;

			for (int i = 1; i <= amountOfMainServicesWithSameVirtualPlatformType; i++)
			{
				string potentialDisplayName = $"{serviceToAdd.Definition.VirtualPlatformServiceType.GetDescription()} {i}";

				bool displayNameIsAlreadyUsed = order.AllServices.Exists(s => s.LofDisplayName == potentialDisplayName);
				bool displayNameIsUsedByReplacedService = replacedService != null && replacedService.LofDisplayName == potentialDisplayName;

				if (displayNameIsUsedByReplacedService || !displayNameIsAlreadyUsed)
				{
					serviceToAdd.LofDisplayName = potentialDisplayName;
					break;
				}
			}

			if (order.IntegrationType != IntegrationType.None)
			{
				serviceToAdd.PreRoll = ServiceManager.GetPreRollDuration(serviceToAdd.Definition, order.IntegrationType);
				serviceToAdd.PostRoll = ServiceManager.GetPostRollDuration(serviceToAdd.Definition, order.IntegrationType);
			}

			serviceToAdd.SetAvailableVirtualPlatformNames(allowedServiceDefinitions[serviceToAdd.Definition.VirtualPlatformServiceType].Select(x => x.VirtualPlatformServiceName.GetDescription()));
			serviceToAdd.SetAvailableServiceDescriptions(allowedServiceDefinitions[serviceToAdd.Definition.VirtualPlatformServiceType].Where(x => x.VirtualPlatform == serviceToAdd.Definition.VirtualPlatform).Select(x => x.Description));

			serviceToAdd.AcceptChanges();
		}

		protected void Section_AddChildService(VirtualPlatformType type)
		{
			if (order.SourceService == null || !allowedServiceDefinitions[type].Any()) return;

			// Description based
			var serviceDefinition = allowedServiceDefinitions[type].FirstOrDefault(x => x.IsDefault) ?? allowedServiceDefinitions[type][0];

			Log(nameof(Section_AddChildService), $"Adding child service with SD: {serviceDefinition.Name}");

			DisplayedService service = new DisplayedService(helpers, serviceDefinition)
			{
				Start = order.Start,
				End = order.End,
			};

			AddChildService(service);

			InvokeValidationRequired();
		}

		private Dictionary<string, int> GetSelectableCompanyViewIds()
		{
			var companyViewIds = new Dictionary<string, int> { { "MCR", userInfo.McrSecurityViewId } };

			foreach (var company in userInfo.AllCompanies.Distinct())
			{
				int companyViewId = userInfo.AllUserGroups.First(u => u.Company == company).CompanySecurityViewId;

				bool eventHasVisibilityRightsForThisCompany = order.Event != null && order.Event.SecurityViewIds.Contains(companyViewId);
				if (eventHasVisibilityRightsForThisCompany)
				{
					companyViewIds.Add(company, companyViewId);
				}
			}

			return companyViewIds;
		}

		private void ConfirmSharedRoutingServicesAreValid(List<Service> servicesToExcludeFromConfirmation)
		{
			using (StartPerformanceLogging())
			{
				if (!servicesToExcludeFromConfirmation.Any()) return;

				var serviceShownInUpdateService = order.AllServices.Intersect(servicesToExcludeFromConfirmation).Single(); // find the one service that is currently in the order

				foreach (var sharedRoutingService in LiveVideoOrder.GetRoutingServicesUsedByMultipleChains())
				{
					if (servicesToExcludeFromConfirmation.Contains(sharedRoutingService.Service)) continue;

					if (sharedRoutingService.Parent.Service.Equals(serviceShownInUpdateService))
					{
						// If the parent of the shared routing service is the service shown in the form,
						// do not validate the input of the shared routing as this can be changed by swapping the output of service shown in the form
						sharedRoutingService.MatrixOutputSdiIsValid = true;
					}
					else if (sharedRoutingService.Service.Children.Contains(serviceShownInUpdateService))
					{
						// If the child of the shared routing service is the service shown in the form,
						// do not validate the output of the shared routing as this can be changed by swapping the input of service shown in the form
						sharedRoutingService.MatrixInputSdiIsValid = true;
					}
					else
					{
						sharedRoutingService.MatrixInputSdiIsValid = true;
						sharedRoutingService.MatrixOutputSdiIsValid = true;
					}
				}
			}
		}

		public void DisableUi()
		{
			UiDisableRequired?.Invoke(this, EventArgs.Empty);
		}

		public void EnableUi()
		{
			UiEnableRequired?.Invoke(this, EventArgs.Empty);
		}
	}
}