namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class EditSharedSourceDialog : YleDialog
	{
		private readonly Guid sharedSourceServiceId;
		private readonly IEnumerable<Order> ordersUsingSharedService;
		private readonly UserInfo userInfo;
		private readonly IEnumerable<LockInfo> lockInfos;
		private readonly UpdateServiceDialogOptions options;

		private List<ServiceDefinition> sharedServiceDefinitions;

		private readonly List<ServiceController> serviceControllers = new List<ServiceController>();
		private readonly List<OrderController> orderControllers = new List<OrderController>();
		private readonly List<SharedSourceSection> AllSharedSourceSectionsIncludingOtherServiceTypes = new List<SharedSourceSection>();

		public EditSharedSourceDialog(Helpers helpers, Guid sharedServiceId, IEnumerable<Order> ordersUsingSharedService, IEnumerable<LockInfo> lockInfos, UserInfo userInfo, UpdateServiceDialogOptions options = UpdateServiceDialogOptions.None) : base(helpers)
		{
			this.sharedSourceServiceId = sharedServiceId.Equals(Guid.Empty) ? throw new ArgumentException(nameof(sharedServiceId)) : sharedServiceId;
			this.ordersUsingSharedService = ordersUsingSharedService ?? throw new ArgumentNullException(nameof(ordersUsingSharedService));
			this.lockInfos = lockInfos ?? throw new ArgumentNullException(nameof(lockInfos));
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.options = options;

			InitializeSharedServiceDefinitions();
			Initialize();
			GenerateUI();
		}

		public List<Service> SharedServices { get; private set; } = new List<Service>();

		public Button ConfirmButton { get; private set; }

		// public Button StopEventLevelReceptionButton { get; private set; } Stop button code is commented out, stopping a single Shared Source will be needed in v1.9

		public bool IsValid
		{
			get
			{
				bool allServicesValid = true;
				foreach (var serviceController in serviceControllers) allServicesValid &= serviceController.ValidateService();

				return allServicesValid;
			}
		}

		/// <summary>
		/// Gets and executes the necessary SRM tasks to do the changes made in the UI.
		/// </summary>
		public UpdateResult Finish()
		{
			var results = new List<UpdateResult>();

			results.AddRange(UpdateSharedServices(out var updatedSharedServices));

			var updatedSharedServiceIds = updatedSharedServices.Select(s => s.Id).ToList();

			var ordersWithUpdatedSharedService = ordersUsingSharedService.Where(o => o.AllServices.Exists(s => updatedSharedServiceIds.Contains(s.Id)) && o.Status != YLE.Order.Status.Completed && o.Status != YLE.Order.Status.Cancelled).ToList();

			foreach (var order in ordersWithUpdatedSharedService)
			{
				var addOrUpdateOptions = order.GetAddOrUpdateOptions(helpers, out OrderChangeSummary orderChangeInfo);

				// Update LastUpdatedBy properties on the affected orders
				order.LastUpdatedBy = Engine.UserLoginName;
				order.LastUpdatedByEmail = userInfo.User?.Email ?? String.Empty;
				order.LastUpdatedByPhone = userInfo.User?.Phone ?? String.Empty;
				order.SetMcrLateChangeRequired(helpers, userInfo.IsMcrUser, orderChangeInfo);

				var result = order.AddOrUpdate(helpers, userInfo.IsMcrUser, addOrUpdateOptions);

				results.Add(result);
			}

			return new UpdateResult(results);
		}

		private IEnumerable<UpdateResult> UpdateSharedServices(out List<Service> changedSharedServices)
		{
			// We only update distinct Shared Sources because each Shared Source reservation might be represented by multiple service objects (depending on how many orders use the Shared Source).
			// As a result the 'out' argument contains only distinct Shared Sources.

			var updateResults = new List<UpdateResult>();

			var distinctSharedServices = SharedServices.Distinct();

			changedSharedServices = new List<Service>();
			foreach (var service in distinctSharedServices)
			{
				helpers.Log(nameof(EditSharedSourceDialog), nameof(UpdateSharedServices), $"Trying to update Shared Service {service.Name}");
				helpers.Log(nameof(EditSharedSourceDialog), nameof(UpdateSharedServices), $"Change tracking enabled: {service.ChangeTrackingStarted}");

				var matchingOrder = ordersUsingSharedService.FirstOrDefault(o => o.AllServices.Exists(s => s.Id == service.Id));
				if (matchingOrder == null) continue;

				if (service.Change.Summary.IsChanged && service.IsBooked)
				{
					// Use case: user edited existing Shared Source (without changing the service definition)
					// The existing service reservation needs to be updated with its new values.
					changedSharedServices.Add(service);
					updateResults.Add(service.TryUpdateIfThisServiceIsSharedSource(helpers, matchingOrder));

					helpers.Log(nameof(EditSharedSourceDialog), nameof(UpdateSharedServices), $"Booked Shared Source {service.Name} has changed");
				}
				else if (!service.IsBooked)
				{
					// Use case: user selected new service definition for Shared Source
					// The service object has already been added to the order object. Updating the order object will book the service.
					changedSharedServices.Add(service);

					helpers.Log(nameof(EditSharedSourceDialog), nameof(UpdateSharedServices), $"Non-booked Shared Source {service.Name} was selected for its service definition {service.Definition.Name}");
				}
				else
				{
					// Use case: user did not change anything
					helpers.Log(nameof(EditSharedSourceDialog), nameof(UpdateSharedServices), $"Booked Shared Source {service.Name} has not changed");
				}
			}

			return updateResults;
		}

		private void Initialize()
		{
			Title = "Edit Shared Source";
			ConfirmButton = new Button("Confirm") { Width = 150, Style = ButtonStyle.CallToAction };

			SharedServices.Clear();
			var newServicesWithSameVirtualPlatform = new Dictionary<Guid, List<Service>>();

			foreach (var order in ordersUsingSharedService)
			{
				// See documentation for more info about how MVC works in the Shared Source case

				var sharedSourceService = order.AllServices.Single(s => s.Id.Equals(sharedSourceServiceId));

				bool orderServiceConfigurationDoesNotMatchWithServiceReservation = !sharedSourceService.IsSharedSource;
				if (orderServiceConfigurationDoesNotMatchWithServiceReservation)
				{
					/*	We know that each order in orders list contains a Shared Source. So if we can't find one based on IsSharedSource property,
						that means the ServiceConfig property on order reservation says that IsGlobalEventLevelReception = false. 
						We can rectify this by assuming the main source service should be a Shared Source and setting its property to true. 
						When finishing the script the ServiceConfig property on order will be updated and will be back in sync with the service reservation. */

					//sharedSourceService = order.Sources.SingleOrDefault(s => s.BackupType == BackupType.None) ?? throw new ServiceNotFoundException($"Unable to find an main source service in order {order.Name}", true);
					sharedSourceService.IsSharedSource = true; // Correction mechanism in case order service config property is not up to date

					helpers.Log(nameof(EditSharedSourceDialog), nameof(Initialize), $"Unable to find a Shared Source between services {string.Join(",", order.AllServices.Select(s => s.Name))} in order {order.Name}. Found main source service {sharedSourceService.Name} and considered it as ELR.");
				}

				order.ServiceChanged += (o, e) => RegenerateUI();

				bool serviceAlreadyHasSection = AllSharedSourceSectionsIncludingOtherServiceTypes.Any(x => x.Service.Id == sharedSourceService.Id);

				var newServices = sharedSourceService.GetOtherServicesWithSameVirtualPlatform(helpers, sharedServiceDefinitions);
				newServices.ForEach(s => s.AcceptChanges());

				if (!serviceAlreadyHasSection)
				{
					newServicesWithSameVirtualPlatform[sharedSourceService.Id] = newServices;
				}
				else
				{
					// To make sure that the same copies of the same Event Level Reception has matching Ids and Names.
					var alreadyExistingCopiedServices = newServicesWithSameVirtualPlatform[sharedSourceService.Id];
					for (int i = 0; i < alreadyExistingCopiedServices.Count; i++)
					{
						var newServiceWithSameType = newServices.FirstOrDefault(newService => newService.Definition.VirtualPlatform == alreadyExistingCopiedServices[i].Definition.VirtualPlatform && newService.Definition.Description == alreadyExistingCopiedServices[i].Definition.Description);
						if (newServiceWithSameType != null)
						{
							newServiceWithSameType.Id = alreadyExistingCopiedServices[i].Id;
							newServiceWithSameType.Name = alreadyExistingCopiedServices[i].Name;
						}
					}

					newServicesWithSameVirtualPlatform[sharedSourceService.Id].AddRange(newServices);
				}

				order.AcceptChanges();
				sharedSourceService.AcceptChanges();

				SharedServices.Add(sharedSourceService);
				SharedServices.AddRange(newServices);

				CreateServiceSectionsAndControllers(order, sharedSourceService, newServices, serviceAlreadyHasSection);
			}
		}

		private void CreateServiceSectionsAndControllers(Order order, Service alreadyBookedSharedSource, List<Service> newServicesWithSameVirtualPlatform, bool serviceAlreadyHasSection)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));
			if (!order.AllServices.Contains(alreadyBookedSharedSource)) throw new ArgumentException("The service is not part of the order", nameof(alreadyBookedSharedSource));

			var orderController = new NormalOrderController(helpers, order, userInfo, null, newServicesWithSameVirtualPlatform.Concat(new Service[] { alreadyBookedSharedSource }));
			orderControllers.Add(orderController);

			if (!serviceAlreadyHasSection)
			{
				var newServiceTypeSelectionDropDown = GenerateServiceTypeSelectionDropDown(alreadyBookedSharedSource);

				foreach (var newServiceWithSameVirtualPlatform in newServicesWithSameVirtualPlatform)
				{
					var newServiceTypeSection = CreateSection(newServiceWithSameVirtualPlatform, newServiceTypeSelectionDropDown, creatingNewServiceTypeSection: true);

					AllSharedSourceSectionsIncludingOtherServiceTypes.Add(newServiceTypeSection);
				}

				var activeSharedSourceSection = CreateSection(alreadyBookedSharedSource, newServiceTypeSelectionDropDown);
				AllSharedSourceSectionsIncludingOtherServiceTypes.Add(activeSharedSourceSection);
			}

			CreateController(order, alreadyBookedSharedSource, orderController);

			foreach (var newServiceSameVirtualPlatform in newServicesWithSameVirtualPlatform)
			{
				CreateController(order, newServiceSameVirtualPlatform, orderController);
			}
		}

		/// <summary>
		/// Creates a ServiceController with accompanying OrderController and adds it to the list.
		/// </summary>
		/// <param name="order">The Order containing the service.</param>
		/// <param name="sharedSourceService">The service that is part of the order.</param>
		/// <param name="orderController"></param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		private void CreateController(Order order, Service sharedSourceService, OrderController orderController)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));
			if (sharedSourceService == null) throw new ArgumentNullException(nameof(sharedSourceService));

			var serviceSection = AllSharedSourceSectionsIncludingOtherServiceTypes.SingleOrDefault(s => s.Service.Id == sharedSourceService.Id)?.ServiceSection;
			if (serviceSection == null) throw new SectionNotFoundException(sharedSourceService.Id);

			var resourceAssignmentHandler = ResourceAssignmentHandler.Factory(helpers, sharedSourceService, order);

			if (sharedSourceService.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite && helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.IsResourceChangeAction)
			{
				var matrixOutputLbandFunction = sharedSourceService.Functions.SingleOrDefault(f => f.Id == FunctionGuids.MatrixOutputLband) as DisplayedFunction ?? throw new FunctionNotFoundException(FunctionGuids.MatrixOutputLband);

				matrixOutputLbandFunction.ResourceNameConverter = name => name.Split('.').Last();
			}

			var serviceController = new ServiceController(helpers, sharedSourceService, orderController, GetSelectableCompanyViewIds(), serviceSection.Yield(), resourceAssignmentHandler, userInfo);
			serviceControllers.Add(serviceController);
		}

		/// <summary>
		/// Creates a section for the given service and adds it to the list.
		/// </summary>
		/// <exception cref="ArgumentNullException"/>
		private SharedSourceSection CreateSection(Service sharedSourceService, DropDown serviceTypeSelectionDropDown, bool creatingNewServiceTypeSection = false)
		{
			if (sharedSourceService == null) throw new ArgumentNullException(nameof(sharedSourceService));

			var configuration = CreateConfiguration(sharedSourceService, creatingNewServiceTypeSection, options == UpdateServiceDialogOptions.OnlyResourceChange);

			var sharedSourceSection = new SharedSourceSection(helpers, sharedSourceService, ordersUsingSharedService, configuration, userInfo, serviceTypeSelectionDropDown);
			sharedSourceSection.ServiceSection.GeneralInfoSection.RegenerateDialog += (o, e) => RegenerateUI();

			return sharedSourceSection;
		}

		private ServiceSectionConfiguration CreateConfiguration(Service service, bool creatingNewServiceTypeSection, bool showResourceSelectionOnly = false)
		{
			var orderIdsContainingThisService = service.OrderReferences;
			bool allOrderLocksGranted = lockInfos.Where(l => orderIdsContainingThisService.Contains(Guid.Parse(l.ObjectId))).All(l => l.IsLockGranted);
			var userCompanies = userInfo.UserGroups?.Select(userGroup => userGroup.Company).ToList();

			ServiceSectionConfiguration serviceSectionConfiguration;
			if (showResourceSelectionOnly)
			{
				serviceSectionConfiguration = ServiceSectionConfiguration.CreateConfigurationForResourceSelectionOnly(helpers, service, userInfo, allOrderLocksGranted, null, userCompanies);
				serviceSectionConfiguration.PromoteToSharedSourceIsVisible = false;
			}
			else
			{
				serviceSectionConfiguration = new ServiceSectionConfiguration(helpers, service, userInfo, allOrderLocksGranted, null, userCompanies);
				serviceSectionConfiguration.PromoteToSharedSourceIsVisible = false;
			}

			serviceSectionConfiguration.HideFunctions(service.Definition.FunctionDefinitions.Where(f => f.IsHidden).Select(f => f.Label).ToArray());

			return serviceSectionConfiguration;
		}

		private DropDown GenerateServiceTypeSelectionDropDown(Service sharedSourceService)
		{
			var newDropDownServiceTypeOptions = sharedServiceDefinitions.Where(x => x.VirtualPlatform == sharedSourceService.Definition?.VirtualPlatform).Select(x => x.Description).ToList();
			bool noNewServiceTypeAvailable = newDropDownServiceTypeOptions.Count == 1 && newDropDownServiceTypeOptions.Any(x => string.IsNullOrEmpty(x));

			DropDown newServiceTypeSelectionDropDown = new DropDown
			{
				Options = !noNewServiceTypeAvailable ? newDropDownServiceTypeOptions : new List<string>() { EnumExtensions.GetDescriptionFromEnumValue(sharedSourceService.Definition.VirtualPlatformServiceName) },
				Selected = !noNewServiceTypeAvailable ? newDropDownServiceTypeOptions.Single(x => x == sharedSourceService.Definition?.Description) : EnumExtensions.GetDescriptionFromEnumValue(sharedSourceService.Definition.VirtualPlatformServiceName),
			};

			if (sharedSourceService.Definition != null && sharedSourceService.Definition.VirtualPlatform == VirtualPlatform.ReceptionLiveU)
			{
				newServiceTypeSelectionDropDown.Options = new List<string> { sharedSourceService.Definition.Description };
				newServiceTypeSelectionDropDown.Selected = newServiceTypeSelectionDropDown.Options.First();
			}

			return newServiceTypeSelectionDropDown;
		}

		private Dictionary<string, int> GetSelectableCompanyViewIds()
		{
			var companyViewIds = new Dictionary<string, int>();
			companyViewIds.Add("MCR", userInfo.McrSecurityViewId);
			foreach (var company in userInfo.AllCompanies.Distinct())
			{
				int companyViewId = userInfo.AllUserGroups.First(u => u.Company == company).CompanySecurityViewId;
				companyViewIds.Add(company, companyViewId);
			}

			return companyViewIds;
		}

		private void GenerateUI()
		{
			Clear();

			int row = -1;

			foreach (var sharedSourceSection in AllSharedSourceSectionsIncludingOtherServiceTypes)
			{
				if (sharedSourceSection.ServiceSection.GeneralInfoSection.ServiceDefinitionTypeSelectionDropDown.Selected == sharedSourceSection.Service.Definition.Description || string.IsNullOrEmpty(sharedSourceSection.Service.Definition.Description))
				{
					AddSection(sharedSourceSection, new SectionLayout(++row, 0));
					row += sharedSourceSection.RowCount;
				}
			}

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));
			AddWidget(ConfirmButton, row + 1, 0, 1, 3);

			//AddWidget(StopEventLevelReceptionButton, ++row, 0, 1, 3);
			// AddWidget(selectEventLevelReceptionToStopLabel, ++row, 0, 1, 3);

			// Set indentation column widths
			SetColumnWidth(0, 20);
		}

		protected override void HandleEnabledUpdate()
		{
			AllSharedSourceSectionsIncludingOtherServiceTypes.ForEach(section => section.IsEnabled = true);
		}

		private void RegenerateUI()
		{
			Clear();
			foreach (var sharedSourceSection in AllSharedSourceSectionsIncludingOtherServiceTypes)
			{
				sharedSourceSection.RegenerateUi();
			}

			GenerateUI();
		}

		private void InitializeSharedServiceDefinitions()
		{
			var allServiceDefinitions = helpers.ServiceDefinitionManager.ServiceDefinitionsForLiveOrderForm;

			sharedServiceDefinitions = new List<ServiceDefinition>();
			sharedServiceDefinitions.AddRange(allServiceDefinitions.ReceptionServiceDefinitions);
			sharedServiceDefinitions.Add(ServiceDefinition.GenerateDummyReceptionServiceDefinition());
			sharedServiceDefinitions.Add(ServiceDefinition.GenerateEurovisionReceptionServiceDefinition());

			sharedServiceDefinitions.Add(helpers.ServiceDefinitionManager.GetServiceDefinition(ServiceDefinitionGuids.RoutingServiceDefinitionId));
		}

		// Will be needed in a later stage.
		//private void StopEventLevelReceptionButton_Pressed(object sender, EventArgs e)
		//{
		//    selectEventLevelReceptionToStopLabel.IsVisible = !EventLevelReceptions.Any(elr => elr != null && elr.StopNow);
		//}
	}
}