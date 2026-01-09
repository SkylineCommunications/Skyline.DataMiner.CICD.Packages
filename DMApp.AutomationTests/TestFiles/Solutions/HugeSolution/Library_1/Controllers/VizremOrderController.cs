namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = Service.Service;

	public class VizremOrderController : OrderController
	{
        private VizremOrderSection orderSection; 

        private readonly IReadOnlyDictionary<VirtualPlatformType, List<List<DisplayedService>>> cachedSourceChildServices = new Dictionary<VirtualPlatformType, List<List<DisplayedService>>>
        {
            { VirtualPlatformType.VizremFarm, new List<List<DisplayedService>>() },
            { VirtualPlatformType.VizremStudio, new List<List<DisplayedService>>() },
        };

        public VizremOrderController(Helpers helpers, Order order, UserInfo userInfo, VizremOrderSection orderSection = null, IEnumerable<Service> controlledServicesWhereOrderRefersTo = null) : base(helpers, order, userInfo, orderSection, controlledServicesWhereOrderRefersTo)
		{
            this.orderSection = orderSection;

            Initialize();
		}

		public override OrderSection OrderSection => orderSection;

		protected override IEnumerable<DisplayedService> GetCachedServices()
		{
			IEnumerable<DisplayedService> services = new List<DisplayedService>(cachedReceptionServices.Select(x => x.Value));
			foreach (var kvp in CachedSourceChildServices)
			{
				foreach (var cachedService in kvp.Value)
				{
					services = services.Concat(cachedService);
				}
			}

			return services.ToList();
		}

		protected override IReadOnlyDictionary<VirtualPlatformType, List<List<DisplayedService>>> CachedSourceChildServices => cachedSourceChildServices;

		public override void AddOrReplaceSection(OrderSection orderSection)
		{
			this.orderSection = orderSection as VizremOrderSection;

			SubscribeToUi();
			AddAllServiceSectionsToServiceControllers();
		}

		protected override List<DisplayedService> GetCachedAlternativeServices(Service previousDisplayedService)
		{
            bool success = CachedSourceChildServices.TryGetValue(previousDisplayedService.Definition.VirtualPlatformServiceType, out var retrievedCachedServices);

            if (success)
			{
                return retrievedCachedServices.FirstOrDefault(x => x.Contains(previousDisplayedService)) ?? throw new ServiceNotFoundException($"Unable to find cached alternative services for service {previousDisplayedService.Name} with VP {previousDisplayedService.Definition.VirtualPlatform.GetDescription()}", true); ;
            }
            else
            {
                throw new ServiceNotFoundException($"Unable to find key {previousDisplayedService.Definition.VirtualPlatformServiceType} in cached source child services", true);
            }
        }
        
        private Service StudioAsDestination => order.AllServices.Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremStudio && !order.SourceService.Equals(s));

        public override void HandleSelectedResourceUpdate(Service service, Function function)
		{
            bool sourceStudioResourceHasChanged = service.Equals(order.SourceService) && order.SourceService.Definition.VirtualPlatform == VirtualPlatform.VizremStudio;

            if (sourceStudioResourceHasChanged)
			{
                // Set same resource on studio destination

                var vizremStudioAsDestination = order.AllServices.Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremStudio && !order.SourceService.Equals(s));

                vizremStudioAsDestination.Functions.Single().Resource = function.Resource;

                Log(nameof(HandleSelectedResourceUpdate), $"Set service {service.Name} selected resource {function.ResourceName} on service {vizremStudioAsDestination.Name}");
			}

            InvokeValidationRequired();
        }

        protected override void Section_SourceDescriptionChanged(object sender, string e)
		{
			base.Section_SourceDescriptionChanged(sender, e);

            var vizremFarmService = order.AllServices.Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremFarm);
            var vizremStudioAsDestination = OrderManager.FlattenServices(vizremFarmService.Children).Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremStudio);

            // Update the vizrem studio as destination

            ServiceSelectionSection_ServiceTypeChanged(vizremStudioAsDestination, order.SourceService.Definition.VirtualPlatformServiceName.GetDescription(), order.SourceService.Definition.Description);

            var studioAsDestinationDirectionProfileParam = StudioAsDestination.Functions.Single().Parameters.SingleOrDefault(pp => pp.Id == ProfileParameterGuids._Direction);
			if (studioAsDestinationDirectionProfileParam != null)
			{
                studioAsDestinationDirectionProfileParam.Value = "Backwards";
			}
        }

        protected override void GenerateNewChildService(Service previousDisplayedService, string virtualPlatformName, string description, out DisplayedService newDisplayedService, out ServiceDefinition newServiceDefinition)
        {
            newServiceDefinition = serviceDefinitions[previousDisplayedService.Definition.VirtualPlatformServiceType].First(x => x.VirtualPlatformServiceName.GetDescription() == virtualPlatformName && x.Description == description);
            newDisplayedService = new DisplayedService(helpers, newServiceDefinition)
            {
                Start = order.Start,
                End = order.End,
            };
        }

        private void Initialize()
        {
			using (StartPerformanceLogging())
			{
				InitializeServiceDefinitions();
				InitializeCachedMainServices();
				InitializeDisplayedOrder();
				InitializeServiceControllers();

				if (orderSection is null) return;

				AddOrReplaceSection(orderSection);
			}
        }

		protected override void SubscribeToUi()
		{
			base.SubscribeToUi();

			orderSection.UseGraphicsEngineAsInputChanged += OrderSection_UseGraphicsEngineAsInputChanged;
		}

		protected override void Section_ServiceDescriptionChanged(object sender, string newDescription)
		{
			base.Section_ServiceDescriptionChanged(sender, newDescription);

            bool vizremFarmEngineAsSourceAndNewStudioIsSt26 = newDescription.Contains("ST26") && order.SourceService.Definition.VirtualPlatform == VirtualPlatform.VizremFarm;

            if (vizremFarmEngineAsSourceAndNewStudioIsSt26)
			{
                //TODO ST26 can only have definition (ST26 -> Farm -> ST26)
                order.UseGraphicsEngineAsInput = false;

                OrderSection_UseGraphicsEngineAsInputChanged(this, false);
			}
		}

		private void OrderSection_UseGraphicsEngineAsInputChanged(object sender, bool useGraphicsEngineAsInput)
		{
            order.UseGraphicsEngineAsInput = useGraphicsEngineAsInput;

			if (useGraphicsEngineAsInput)
			{
                var studioSource = order.SourceService;
                var graphicsEngine = order.AllServices.Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremFarm) as DisplayedService;
                var studioDestination = OrderManager.FlattenServices(graphicsEngine.Children).Single(s => s.Definition.VirtualPlatform != VirtualPlatform.VizremNC2Converter) as DisplayedService;

                Log(nameof(OrderSection_UseGraphicsEngineAsInputChanged), $"Setting studio destination service {studioDestination.Name} as child of studio source service {studioSource.Name}...");

                studioSource.SetChildren(studioDestination.Yield());

                Log(nameof(OrderSection_UseGraphicsEngineAsInputChanged), $"Setting studio destination service {studioDestination.Name} as child of studio source service {studioSource.Name} completed");

                ReplaceService(studioSource, graphicsEngine); // graphicsEngine gets the children of studioSource
            }
            else
			{
                var graphicsEngine = order.SourceService as DisplayedService;
                var studioDestination = OrderManager.FlattenServices(graphicsEngine.Children).Single(s => s.Definition.VirtualPlatform != VirtualPlatform.VizremNC2Converter) as DisplayedService;

                var studioSourceServiceDefinition = helpers.ServiceDefinitionManager.GetServiceDefinition(studioDestination.Definition.Id);
                var studioSource = new DisplayedService(helpers, studioSourceServiceDefinition);
                studioSource.Start = order.Start;
                studioSource.End = order.End;
                studioSource.Functions.Single().Resource = studioDestination.Functions.Single().Resource;
                studioSource.AcceptChanges();

                ReplaceService(graphicsEngine, studioSource); // studioSource gets the children of graphicsEngine

                studioSource.SetChildren(graphicsEngine.Yield());   
            }
		}

		private void InitializeServiceDefinitions()
		{
            var allServiceDefinitions = helpers.ServiceDefinitionManager.ServiceDefinitionsForLiveOrderForm;

            var serviceDefinitions = new Dictionary<VirtualPlatformType, IReadOnlyList<ServiceDefinition>>
            {
                { VirtualPlatformType.VizremStudio, new List<ServiceDefinition>() },
                { VirtualPlatformType.VizremFarm, new List<ServiceDefinition>() },
            };

            var allowedServiceDefinitions = new Dictionary<VirtualPlatformType, IReadOnlyList<ServiceDefinition>>
            {
                { VirtualPlatformType.VizremStudio, new List<ServiceDefinition>() },
                { VirtualPlatformType.VizremFarm, new List<ServiceDefinition>() },
            };

            var vizremStudios = allServiceDefinitions.VizremStudios.ToList();
            serviceDefinitions[VirtualPlatformType.VizremStudio] = vizremStudios;
            allowedServiceDefinitions[VirtualPlatformType.VizremStudio] = vizremStudios;

            var vizremFarms = allServiceDefinitions.VizremFarms.ToList();
            serviceDefinitions[VirtualPlatformType.VizremFarm] = vizremFarms;
            allowedServiceDefinitions[VirtualPlatformType.VizremFarm] = vizremFarms;

            this.serviceDefinitions = serviceDefinitions;
            this.allowedServiceDefinitions = allowedServiceDefinitions;
        }
	}
}
