namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class OrderSectionConfiguration : ISectionConfiguration
	{
		private readonly UserInfo userInfo;
		private readonly bool isReadOnly;
		private readonly Scripts script;
		private readonly EditOrderFlows flow;

		private readonly LiveVideoOrder liveVideoOrder;

		public OrderSectionConfiguration(Helpers helpers, Order order, Event @event, UserInfo userInfo, bool isReadOnly, LockInfo lockInfo, Scripts script, EditOrderFlows flow)
		{
			this.userInfo = userInfo;
			this.isReadOnly = isReadOnly;
			this.script = script;
			this.flow = flow;
			this.liveVideoOrder = new LiveVideoOrder(helpers, order);

			UserCompanies = userInfo.UserGroups?.Select(userGroup => userGroup.Company).ToList();
			EventOwnerCompany = @event?.CompanyOfCreator;

			OrderTypeIsEnabled = !order.IsBooked;

			GeneralInfoSectionConfiguration = new GeneralInfoSectionConfiguration(order, @event, userInfo);
			AdditionalInformationSectionConfiguration = new AdditionalInformationSectionConfiguration(order, userInfo);
			SportsPlanningSectionConfiguration = new SportsPlanningSectionConfiguration(userInfo);
			NewsInformationSectionConfiguration = new NewsInformationSectionConfiguration(userInfo);

			UseSharedSourceIsEnabled = order.SourceService == null || order.SourceService.IntegrationType == IntegrationType.None || (order.SourceService.IntegrationType == IntegrationType.Feenix && userInfo.IsMcrUser);
			SourceDropDownIsEnabled = order.SourceService == null || order.SourceService.IntegrationType == IntegrationType.None || (order.SourceService.IntegrationType == IntegrationType.Feenix && userInfo.IsMcrUser);
			SourceDescriptionDropDownIsEnabled = order.SourceService == null || order.SourceService.IntegrationType == IntegrationType.None || (order.SourceService.IntegrationType == IntegrationType.Feenix && userInfo.IsMcrUser);

			ConfigureServiceConfigurations(helpers, order, lockInfo);

			ConfigureBasedOnOrderSubType(helpers, order);

			ConfigureBasedOnFlow();

			if (isReadOnly) SetToReadOnly();

			helpers.Log(nameof(OrderSectionConfiguration), "Constructor", $"Configuration: {JsonConvert.SerializeObject(this)}");
		}

		private void ConfigureServiceConfigurations(Helpers helpers, Order order, LockInfo lockInfo)
		{
			foreach (var service in order.AllServices)
			{
				switch (service.Definition.VirtualPlatformServiceType)
				{
					case VirtualPlatformType.Reception when service.BackupType == BackupType.None:
						if (flow == EditOrderFlows.ChangeResourcesForService || flow == EditOrderFlows.ChangeResourcesForService_FromRecordingApp)
						{
							MainSourceServiceSectionConfigurations.Add(service.Id, ServiceSectionConfiguration.CreateConfigurationForResourceSelectionOnly(helpers, service, userInfo, lockInfo.IsLockGranted, EventOwnerCompany, UserCompanies.ToList()));
						}
						else if (flow == EditOrderFlows.EditTimingForService_FromRecordingApp)
						{
							MainSourceServiceSectionConfigurations.Add(service.Id, ServiceSectionConfiguration.CreateConfigurationForTimingSelectionOnly(helpers, service, userInfo, lockInfo.IsLockGranted, EventOwnerCompany, UserCompanies.ToList()));
						}
						else
						{
							AddNewMainSourceServiceSectionConfiguration(helpers, service, order);
						}

						break;
					case VirtualPlatformType.Reception when service.BackupType != BackupType.None:
						BackupSourceServiceSectionConfigurations.Add(service.Id, ServiceSectionConfiguration.CreateLiveOrderFormConfiguration(helpers, service, userInfo, EventOwnerCompany, UserCompanies));
						break;
					case VirtualPlatformType.VizremStudio:
						SourceDropDownIsVisible = false;
						MainSourceServiceSectionConfigurations.Add(service.Id, ServiceSectionConfiguration.CreateLiveOrderFormConfiguration(helpers, service, userInfo, EventOwnerCompany, UserCompanies));
						break;
					case VirtualPlatformType.Destination:
					case VirtualPlatformType.Transmission:
					case VirtualPlatformType.Recording:
					case VirtualPlatformType.Routing:
					case VirtualPlatformType.GraphicsProcessing:
					case VirtualPlatformType.VideoProcessing:
					case VirtualPlatformType.AudioProcessing:
						AddNewCollapsableServiceSelectionSectionConfiguration(helpers, service, order);
						break;
					default:
						// No need to create service sections configurations for other services
						continue;
				}
			}
		}

		private void ConfigureBasedOnOrderSubType(Helpers helpers, Order order)
		{
			switch (order.Subtype)
			{
				case OrderSubType.Normal:
					LabelColumn = 2;
					LabelSpan = 2;
					InputColumn = 4;
					InputSpan = 3;
					break;
				case OrderSubType.Vizrem:
					LabelColumn = 0;
					LabelSpan = 1;
					InputColumn = 1;
					InputSpan = 3;

					var vizremFarmService = order.AllServices.Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremFarm);
					var vizremFarmConfiguration = new ServiceSelectionSectionConfiguration(helpers, vizremFarmService, EventOwnerCompany, isReadOnly, userInfo, flow)
					{
						IsCollapsed = false,
						DeleteButtonIsVisible = false,
					};

					CollapsableServiceSelectionSectionConfigurations[vizremFarmService.Id] = vizremFarmConfiguration;

					var vizremStudioHelsinkiService = OrderManager.FlattenServices(vizremFarmService.Children).Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremStudio);
					var vizremStudioConfiguration = new ServiceSelectionSectionConfiguration(helpers, vizremStudioHelsinkiService, EventOwnerCompany, isReadOnly, userInfo, flow)
					{
						IsCollapsed = false,
						DeleteButtonIsVisible = false,
					};

					CollapsableServiceSelectionSectionConfigurations[vizremStudioHelsinkiService.Id] = vizremStudioConfiguration;
					break;
				default:
					// no other types
					break;
			}
		}

		private void ConfigureBasedOnFlow()
		{
			switch (flow)
			{
				case EditOrderFlows.AddOrder:
					MainSignalIsCollapsed = false;
					SourceIsCollapsed = false;
					break;
				case EditOrderFlows.EditOrder:
				case EditOrderFlows.DeleteOrder:
				case EditOrderFlows.DuplicateOrder:
				case EditOrderFlows.MergeOrders:
				case EditOrderFlows.AddOrderFromTemplate:
				case EditOrderFlows.ViewOrder:
					MainSignalIsCollapsed = true;
					SourceIsCollapsed = true;
					break;
				case EditOrderFlows.AddDestinationToOrder:
					MainSignalIsCollapsed = false;
					SourceIsCollapsed = true;
					OrderTypeIsVisible = false;
					SourceIsVisible = false;
					RecordingsIsVisible = false;
					TransmissionsIsVisible = false;
					MainSignalTitleIsVisible = false;
					DestinationsAreCollapsed = false;
					DestinationsTitleIsVisible = false;
					AddDestinationButtonIsVisible = false;
					BackupSignalIsVisible = false;
					GeneralInfoSectionConfiguration.IsVisible = false;
					AdditionalInformationSectionConfiguration.IsVisible = false;
					NewsInformationSectionConfiguration.IsVisible = false;
					SportsPlanningSectionConfiguration.IsVisible = false;
					HideAllServiceSections();
					break;
				case EditOrderFlows.AddTransmissionToOrder:
					MainSignalIsCollapsed = false;
					SourceIsCollapsed = true;
					OrderTypeIsVisible = false;
					SourceIsVisible = false;
					RecordingsIsVisible = false;
					DestinationsIsVisible = false;
					MainSignalTitleIsVisible = false;
					TransmissionsAreCollapsed = false;
					TransmissionsTitleIsVisible = false;
					AddTransmissionButtonIsVisible = false;
					BackupSignalIsVisible = false;
					GeneralInfoSectionConfiguration.IsVisible = false;
					AdditionalInformationSectionConfiguration.IsVisible = false;
					NewsInformationSectionConfiguration.IsVisible = false;
					SportsPlanningSectionConfiguration.IsVisible = false;
					HideAllServiceSections();
					break;
				case EditOrderFlows.EditService:
				case EditOrderFlows.ChangeResourcesForService:
				case EditOrderFlows.ViewService:
				case EditOrderFlows.ChangeResourcesForService_FromRecordingApp:
				case EditOrderFlows.EditTimingForService_FromRecordingApp:
					MainSignalIsCollapsed = false;
					SourceIsCollapsed = false;
					DestinationsAreCollapsed = false;
					RecordingsAreCollapsed = false;
					TransmissionsAreCollapsed = false;
					break;
				case EditOrderFlows.UseSharedSource:
					HideAllServiceSections();
					MainSignalIsCollapsed = false;
					SourceIsCollapsed = false;
					OrderTypeIsVisible = false;
					SourceIsVisible = true;
					SourceLabelIsVisible = false;
					SourceTitleIsVisible = false;
					SharedSourceOptionsAreVisible = false;
					RecordingsIsVisible = false;
					TransmissionsIsVisible = false;
					MainSignalTitleIsVisible = false;
					DestinationsAreCollapsed = false;
					DestinationsTitleIsVisible = false;
					AddDestinationButtonIsVisible = false;
					BackupSignalIsVisible = false;
					GeneralInfoSectionConfiguration.IsVisible = false;
					AdditionalInformationSectionConfiguration.IsVisible = false;
					NewsInformationSectionConfiguration.IsVisible = false;
					SportsPlanningSectionConfiguration.IsVisible = false;
					break;
				default:
					// Nothing to do
					break;
			}
		}

		private void SetToReadOnly()
		{
			SetIsEnabledPropertyValues(false);
			IsEnabled = true;
			CollapseButtonsAreEnabled = true;
			GeneralInfoSectionConfiguration.IsEnabled = true;
			GeneralInfoSectionConfiguration.EditOrderInformationIsEnabled = true;
			NewsInformationSectionConfiguration.IsEnabled = true;
			NewsInformationSectionConfiguration.CollapseButtonsAreEnabled = true;
			SportsPlanningSectionConfiguration.IsEnabled = true;
			SportsPlanningSectionConfiguration.CollapseButtonsAreEnabled = true;
			AdditionalInformationSectionConfiguration.IsEnabled = true;
			AdditionalInformationSectionConfiguration.CollapseButtonsAreEnabled = true;
		}

		[IsIsVisibleProperty]
		public bool IsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; } = true;

		[IsIsEnabledProperty]
		public bool CollapseButtonsAreEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool OrderTypeIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool OrderTypeIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool MainSignalIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool MainSignalTitleIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool SourceIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool SourceLabelIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool SourceTitleIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool SharedSourceOptionsAreVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool DestinationsIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool DestinationsTitleIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool AddDestinationButtonIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool RecordingsIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool RecordingsTitleIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool AddRecordingButtonIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool TransmissionsIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool TransmissionsTitleIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool AddTransmissionButtonIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool BackupSignalIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool BackupSignalTitleIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool UseSharedSourceIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool SourceDropDownIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool SourceDropDownIsEnabled { get; set; } = true;

		[IsIsEnabledProperty]
		public bool SourceDescriptionDropDownIsEnabled { get; set; } = true;

		[IsIsEnabledProperty]
		public bool AddDestinationButtonIsEnabled { get; set; } = true;

		[IsIsEnabledProperty]
		public bool AddRecordingButtonIsEnabled { get; set; } = true;

		[IsIsEnabledProperty]
		public bool AddTransmissionButtonIsEnabled { get; set; } = true;

		public bool MainSignalIsCollapsed { get; set; } = true;

		public bool SourceIsCollapsed { get; set; } = true;

		public bool DestinationsAreCollapsed { get; set; } = true;

		public bool RecordingsAreCollapsed { get; set; } = true;

		public bool TransmissionsAreCollapsed { get; set; } = true;

		public bool BackupSignalIsCollapsed { get; set; } = true;

		public bool BackupSourceIsCollapsed { get; set; } = true;

		public bool BackupDestinationsAreCollapsed { get; set; } = true;

		public bool BackupRecordingsAreCollapsed { get; set; } = true;

		public bool BackupTransmissionsAreCollapsed { get; set; } = true;

		public GeneralInfoSectionConfiguration GeneralInfoSectionConfiguration { get; private set; }

		public Dictionary<Guid, ServiceSectionConfiguration> MainSourceServiceSectionConfigurations { get; } = new Dictionary<Guid, ServiceSectionConfiguration>();

		public Dictionary<Guid, ServiceSectionConfiguration> BackupSourceServiceSectionConfigurations { get; } = new Dictionary<Guid, ServiceSectionConfiguration>();

		public Dictionary<Guid, ServiceSelectionSectionConfiguration> CollapsableServiceSelectionSectionConfigurations { get; set; } = new Dictionary<Guid, ServiceSelectionSectionConfiguration>();

		public AdditionalInformationSectionConfiguration AdditionalInformationSectionConfiguration { get; private set; }

		public SportsPlanningSectionConfiguration SportsPlanningSectionConfiguration { get; private set; }

		public NewsInformationSectionConfiguration NewsInformationSectionConfiguration { get; private set; }

		public IReadOnlyList<string> UserCompanies { get; private set; }

		public string EventOwnerCompany { get; private set; }

		public int LabelColumn { get; set; }

		public int LabelSpan { get; set; }

		public int InputColumn { get; set; }

		public int InputSpan { get; set; }

		[JsonIgnore]
		public Dictionary<string, string> ToolTip { get; set; } = ReflectionHandler.ReadTooltipFile();

		public void AddNewCollapsableServiceSelectionSectionConfiguration(Helpers helpers, Service service, Order order)
		{
			if (CollapsableServiceSelectionSectionConfigurations.ContainsKey(service.Id)) return;

			var collabsableServiceSectionConfig = new ServiceSelectionSectionConfiguration(helpers, service, EventOwnerCompany, isReadOnly, userInfo, flow);

			if (service.IntegrationType != IntegrationType.None)
			{
				collabsableServiceSectionConfig.ServiceSectionConfiguration.ReasonForBeingDisabled = $"Automatically generated by {service.IntegrationType.GetDescription()}";

				if (!userInfo.IsMcrUser)
				{
					collabsableServiceSectionConfig.DisableForIntegration(service.IntegrationType, service.BackupType);
				}
			}

			if (!IsEnabled || isReadOnly || service.Definition.VirtualPlatformServiceName == VirtualPlatformName.Eurovision)
			{
				collabsableServiceSectionConfig.SetIsEnabledPropertyValues(false);
				collabsableServiceSectionConfig.IsEnabled = isReadOnly;
				collabsableServiceSectionConfig.ServiceSectionConfiguration.AdjustDetailsIsEnabled = isReadOnly;
			}

			switch (script)
			{
				case Scripts.UpdateService:
					// default settings for Update Service
					collabsableServiceSectionConfig.IsCollapsed = false;
					collabsableServiceSectionConfig.CollapseButtonIsVisible = helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.ServiceId != service.Id;
					collabsableServiceSectionConfig.DeleteButtonIsVisible = false;
					break;

				default:
					collabsableServiceSectionConfig.IsCollapsed = service.IsBooked;
					break;
			}

			switch (flow)
			{
				case EditOrderFlows.AddDestinationToOrder:
				case EditOrderFlows.AddTransmissionToOrder:
					collabsableServiceSectionConfig.IsCollapsed = false;
					collabsableServiceSectionConfig.CollapseButtonIsVisible = false;
					collabsableServiceSectionConfig.DeleteButtonIsVisible = false;
					break;
				case EditOrderFlows.ChangeResourcesForService:
				case EditOrderFlows.ChangeResourcesForService_FromRecordingApp:
				case EditOrderFlows.EditTimingForService_FromRecordingApp:
					collabsableServiceSectionConfig.ServiceDefinitionDescriptionLabelIsVisible = false;
					collabsableServiceSectionConfig.ServiceDefinitionDescriptionDropDownIsVisible = false;
					collabsableServiceSectionConfig.VirtualPlatformDropDownIsVisible = false;
					break;
				case EditOrderFlows.ViewService:
					// additional settings 
					collabsableServiceSectionConfig.SetIsEnabledPropertyValues(false);
					collabsableServiceSectionConfig.IsEnabled = true;
					collabsableServiceSectionConfig.ServiceSectionConfiguration.AdjustDetailsIsEnabled = true;
					break;
				case EditOrderFlows.EditOrder:
					collabsableServiceSectionConfig.IsCollapsed = false;
					break;
				default:
					break;
			}

			CheckParentAndChildServices(order, service, collabsableServiceSectionConfig.ServiceSectionConfiguration);
			CheckRoutingServiceChainInputAndOutput(service, collabsableServiceSectionConfig.ServiceSectionConfiguration);

			CollapsableServiceSelectionSectionConfigurations.Add(service.Id, collabsableServiceSectionConfig);
		}

		public void AddNewMainSourceServiceSectionConfiguration(Helpers helpers, Service service, Order order)
		{
			var newSourceConfig = ServiceSectionConfiguration.CreateLiveOrderFormConfiguration(helpers, service, userInfo, EventOwnerCompany, UserCompanies);

			if (!IsEnabled || service.IsSharedSource)
			{
				newSourceConfig.DisableAll();
			}
			else if (service.IntegrationType != IntegrationType.None)
			{
				newSourceConfig.ReasonForBeingDisabled = $"Automatically generated by {service.IntegrationType.GetDescription()}";

				if (!userInfo.IsMcrUser)
				{
					newSourceConfig.DisableForIntegration(service.IntegrationType, service.BackupType);
				}
			}

			MainSourceServiceSectionConfigurations.Add(service.Id, newSourceConfig);
		}

		private void CheckParentAndChildServices(Order order, Service service, ServiceSectionConfiguration configuration)
		{
			if (service.Definition.VirtualPlatform != VirtualPlatform.Routing) return;

			var parentService = order.AllServices.SingleOrDefault(s => s.Children.Contains(service));
			bool parentServiceIsAudioProcessing = parentService != null && parentService.Definition.VirtualPlatform == VirtualPlatform.AudioProcessing;
			if (parentServiceIsAudioProcessing)
			{
				var firstFunction = service.Functions.Single(f => service.Definition.FunctionIsFirst(f));
				configuration.DisableFunctionResourceSelection(firstFunction.Definition.Label, "Swapping routing input resource is not supported for this Service.");
			}

			foreach (var childService in service.Children)
			{
				bool childServiceIsAudioProcessing = childService != null && childService.Definition.VirtualPlatform == VirtualPlatform.AudioProcessing;
				if (childServiceIsAudioProcessing)
				{
					var lastFunction = service.Functions.Single(f => service.Definition.FunctionIsLast(f));
					configuration.DisableFunctionResourceSelection(lastFunction.Definition.Label, "Swapping routing output resource is not supported for this Service.");
				}
			}
		}

		private void CheckRoutingServiceChainInputAndOutput(Service service, ServiceSectionConfiguration configuration)
		{
			if (service.Definition.VirtualPlatform != VirtualPlatform.Routing) return;

			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			var routingServiceChains = liveVideoOrder.GetRoutingServiceChainsForService(service.Id);

			foreach (var routingServiceChain in routingServiceChains)
			{
				bool serviceIsRoutingWithoutInputResource = routingServiceChain.InputService != null && routingServiceChain.InputService.OutputResource == null;
				bool serviceIsRoutingWithoutOutputResource = routingServiceChain.OutputService != null && routingServiceChain.OutputService.InputResource == null;
				if (serviceIsRoutingWithoutInputResource || serviceIsRoutingWithoutOutputResource)
				{
					foreach (var function in service.Functions)
					{
						configuration.DisableFunctionResourceSelection(function.Definition.Label, "Please make sure the routing chain is connected to an input and output.");
					}
				}
			}
		}

		public void HideAllServiceSections()
		{
			foreach (var sourceServiceSectionConfig in MainSourceServiceSectionConfigurations.Values.Concat(BackupSourceServiceSectionConfigurations.Values))
			{
				sourceServiceSectionConfig.IsVisible = false;
			}

			foreach (var serviceSelectionSectionConfig in CollapsableServiceSelectionSectionConfigurations.Values)
			{
				serviceSelectionSectionConfig.IsVisible = false;
			}
		}

		public void SetIsEnabledPropertyValues(bool valueToSet)
		{
			ConfigurationHelper.SetIsEnabledPropertyValues(this, valueToSet);
		}

		public void SetIsVisiblePropertyValues(bool valueToSet)
		{
			ConfigurationHelper.SetIsVisiblePropertyValues(this, valueToSet);
		}
	}
}