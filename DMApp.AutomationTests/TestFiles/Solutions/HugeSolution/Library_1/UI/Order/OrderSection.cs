namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class OrderSection : YleSection
	{
		protected readonly Order order;
		protected readonly OrderSectionConfiguration configuration;
		protected readonly UserInfo userInfo;
		protected readonly ServiceSectionCollection serviceSections;

		protected readonly Label orderTypeLabel = new Label("Order Type");
		protected readonly YleCheckBox orderTypeCheckbox = new YleCheckBox(OrderSubType.Vizrem.GetDescription());
		protected readonly YleDropDown sourceDropDown = new YleDropDown { Name = nameof(sourceDropDown), IsSorted = true };
		protected readonly YleDropDown sourceDescriptionDropDown = new YleDropDown { Name = nameof(sourceDescriptionDropDown), IsSorted = true };

		protected OrderSection(Helpers helpers, Order order, OrderSectionConfiguration configuration, UserInfo userInfo) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.serviceSections = new ServiceSectionCollection();
		}

		/// <summary>
		/// Gets the subsection that displays general order information.
		/// </summary>
		public GeneralInfoSection GeneralInfoSection { get; private set; }

		public event EventHandler<bool> OrderTypeChanged;

		public event EventHandler<string> SourceChanged;

		public event EventHandler<string> SourceDescriptionChanged;

		public event EventHandler<ServiceSection> SourceServiceSectionAdded;

		public event EventHandler<ServiceSelectionSection> ServiceSelectionSectionAdded;

		public ServiceSection SourceServiceSection => serviceSections.SourceServiceSection;

		public IEnumerable<ServiceSelectionSection> SourceChildSections => serviceSections.EndpointSections.SelectMany(x => x.Value).Select(x => x.DisplayedSection).Concat(serviceSections.SubSections.SelectMany(x => x.Value));

		public string Source => sourceDropDown.Selected;

		public string SourceDescription => sourceDescriptionDropDown.Selected;

		public override void RegenerateUi()
		{
			using (StartPerformanceLogging())
			{
				Clear();
				SourceServiceSection?.RegenerateUi();
				foreach (var childSection in SourceChildSections) childSection.RegenerateUi();
				GenerateUi();
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public void RemoveAllSubscribers()
		{
			foreach (var subscriber in SourceServiceSectionAdded.GetInvocationList())
			{
				SourceServiceSectionAdded -= subscriber as EventHandler<ServiceSection>;
			}

			foreach (var subscriber in ServiceSelectionSectionAdded.GetInvocationList())
			{
				ServiceSelectionSectionAdded -= subscriber as EventHandler<ServiceSelectionSection>;
			}

			UnsubscribeFromOrder();
		}


		public abstract List<Section> GetServiceSections(Service service);

		protected void EnableLogging()
		{
			var fields = typeof(OrderSection).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			var yleInteractiveWidgets = fields.Where(f => typeof(IYleInteractiveWidget).IsAssignableFrom(f.FieldType)).Select(f => f.GetValue(this)).Cast<IYleInteractiveWidget>().ToList();

			foreach (var yleWidget in yleInteractiveWidgets)
			{
				yleWidget.Helpers = helpers;
			}
		}

		protected virtual void InitializeWidgets()
		{
			orderTypeCheckbox.IsChecked = order.Subtype.GetDescription() == orderTypeCheckbox.Text;

			sourceDropDown.SetOptions(order.AvailableSourceServices);

			var options = new HashSet<string>(order.AvailableSourceServiceDescriptions);
			options.Add(order.SourceService.Definition.Description);

			sourceDescriptionDropDown.Options = options;

			sourceDropDown.Selected = order.SourceService.Definition.VirtualPlatformServiceName.GetDescription();
			sourceDescriptionDropDown.Selected = order.SourceService.Definition.Description;
		}

		protected void InitializeSections()
		{
			InitializeOtherSections();

			InitializeServiceSections();
		}

		protected virtual void InitializeOtherSections()
		{
			GeneralInfoSection = new GeneralInfoSection(helpers, order, configuration.GeneralInfoSectionConfiguration);
		}

		protected virtual void InitializeServiceSections()
		{
			UpdateSourceServiceSection(this, order.SourceService);

			foreach (var service in order.SourceService.Descendants)
			{
				AddOrReplaceChildServiceSection(service);
			}
		}

		protected virtual void SubscribeToWidgets()
		{
			orderTypeCheckbox.Changed += (s, e) => OrderTypeChanged?.Invoke(this, orderTypeCheckbox.IsChecked);

			sourceDropDown.Changed += (s, e) => SourceChanged?.Invoke(this, sourceDropDown.Selected);
			sourceDescriptionDropDown.Changed += (s, e) => SourceDescriptionChanged?.Invoke(this, sourceDescriptionDropDown.Selected);
		}

		protected virtual void SubscribeToOrder()
		{
			order.AvailableSourceServicesChanged += (s, e) => Order_AvailableSourceServicesChanged(e);
			order.AvailableSourceServiceDescriptionsChanged += (s, e) => Order_AvailableSourceServiceDescriptionsChanged(e);

			order.SourceServiceChanged += UpdateSourceServiceSection;

			foreach (var service in order.AllServices)
			{
				service.Children.CollectionChanged += ChildrenChanged;
			}
		}

		protected void UnsubscribeFromOrder()
		{
			order.SourceServiceChanged -= UpdateSourceServiceSection;

			foreach (var service in order.AllServices)
			{
				for (int i = 0; i < 5; i++)
				{
					service.Children.CollectionChanged -= ChildrenChanged;
				}
			}
		}

		protected void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var oldServices = e.OldItems == null ? new List<Service>() : e.OldItems.Cast<Service>().ToList();
			var newServices = e.NewItems == null ? new List<Service>() : e.NewItems.Cast<Service>().ToList();

			Log(nameof(ChildrenChanged), $"Children changed from '{string.Join(", ", oldServices.Select(s => s.Name))}' to '{string.Join(", ", newServices.Select(s => s.Name))}'");

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var addedService = newServices.Except(oldServices).Single();
					AddOrReplaceChildServiceSection(addedService);
					break;
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Reset:
					ServicesRemoved(oldServices.Except(newServices).ToList());
					break;
				case NotifyCollectionChangedAction.Replace:
					AddOrReplaceChildServiceSection(newServices.Except(oldServices).Single(), oldServices.Except(newServices).Single());
					break;
				default:
					return;
			}
		}

		protected void AddOrReplaceChildServiceSection(Service addedService, Service oldService = null)
		{
			using (StartPerformanceLogging())
			{
				if (serviceSections.ChildServiceIsAlreadyAdded(addedService))
				{
					Log(nameof(AddOrReplaceChildServiceSection), $"A section for service {addedService.Name} is already added");
					var collection = serviceSections.GetLinkedServiceSectionCollectionForService(addedService); // can be null for autogenerated services
					collection?.SetDisplayedSection(addedService.Id);
					InvokeRegenerateUi();
					return;
				}

				Log(nameof(AddOrReplaceChildServiceSection), $"Service {addedService.Name} ({addedService.Definition.Description}) (backupType={addedService.BackupType}) was added to the order and needs a new collapsable service selection section");

				if (oldService != null) oldService.Children.CollectionChanged -= ChildrenChanged; // unsubscribe from removed service
				addedService.Children.CollectionChanged += ChildrenChanged; // subscribe on added service

				var section = CreateNewCollapsableServiceSelectionSection(addedService);

				section.RegenerateUiRequired += (s, e) => InvokeRegenerateUi();

				if (addedService.IsAutogenerated || addedService.IsRecordingAfterDestination(order))
				{
					// Step 1: Save the new section
					serviceSections.AddSubServiceSection(section);
				}
				else
				{
					// Step 1: Save the new section
					if (oldService is null)
					{
						var collection = new LinkedServiceSectionCollection(section);
						serviceSections.AddCollection(collection);
					}
					else
					{
						var collection = serviceSections.GetLinkedServiceSectionCollectionForService(oldService);
						collection.AddSection(addedService.Id, section);
						collection.SetDisplayedSection(addedService.Id);
					}
				}

				// Step 2: Rebuild the SubSections dictionary
				var newlyGeneratedSections = serviceSections.RebuildSubSectionsDictionary(helpers, order);

				// Step 3: Register controllers for new sub sections 
				newlyGeneratedSections.ForEach(s => ServiceSelectionSectionAdded?.Invoke(this, section));

				// Step 4: Set the sub sections on all endpoint services
				var allEndpointServices = order.AllServices.Where(s => s.Definition.IsEndPointService && !s.IsRecordingAfterDestination(order)).ToList();

				foreach (var endpointService in allEndpointServices)
				{
					var subServiceSections = serviceSections.GetSubServiceSections(endpointService.Id); // find all routings and processings linked to the the endpoint service

					var serviceSectionCollection = serviceSections.GetLinkedServiceSectionCollectionForService(endpointService);

					if (serviceSectionCollection != null)
					{
						serviceSectionCollection.DisplayedSection.SetSubServiceSections(subServiceSections);
					}
				}

				ServiceSelectionSectionAdded?.Invoke(this, section);

				InvokeRegenerateUi();
			}
		}

		private ServiceSelectionSection CreateNewCollapsableServiceSelectionSection(Service addedService)
		{
			configuration.AddNewCollapsableServiceSelectionSectionConfiguration(helpers, addedService, order);

			var serviceSectionConfig = configuration.CollapsableServiceSelectionSectionConfigurations[addedService.Id];

			var section = new ServiceSelectionSection(helpers, addedService as DisplayedService, serviceSectionConfig, userInfo);

			section.ServiceSection.GeneralInfoSection.RegenerateDialog += (sender, args) => InvokeRegenerateUi();
			section.ServiceSection.UploadSynopsisSection.RegenerateDialog += (sender, args) => InvokeRegenerateUi();
			section.ServiceSection.RecordingConfigurationSection.RegenerateDialog += (sender, args) => InvokeRegenerateUi();

			return section;
		}

		private void ServicesRemoved(List<Service> removedServices)
		{
			Log(nameof(ServicesRemoved), $"Services for which to remove sections: '{String.Join(", ", removedServices.Select(x => x.Name))}'");

			foreach (var removedService in removedServices)
			{
				removedService.Children.CollectionChanged -= ChildrenChanged; // unsubscribe from removed service

				// Remove subsections
				foreach (var serviceSection in serviceSections.EndpointSections.Values.SelectMany(x => x).Select(x => x.DisplayedSection))
				{
					int amountOfRemovedSections = serviceSection.SubServiceSections.RemoveAll(x => x.Service.Equals(removedService));

					if (amountOfRemovedSections > 0)
					{
						Log(nameof(ServicesRemoved), $"Removed {amountOfRemovedSections} sections for service {removedService.Name} from the section of service {serviceSection.Service.Name}");
					}
				}

				serviceSections.RemoveCollectionContainingService(removedService);
			}

			InvokeRegenerateUi();
		}

		private void Order_AvailableSourceServicesChanged(IReadOnlyList<string> availableSourceServices)
		{
			// In case the current user is not allowed to select the service type of the existing service
			var options = new List<string>(availableSourceServices);
			if (order.SourceService != null && !availableSourceServices.Contains(order.SourceService.Definition.VirtualPlatformServiceName.GetDescription())) options.Add(order.SourceService.Definition.VirtualPlatformServiceName.GetDescription());

			sourceDropDown.Options = options;
			sourceDropDown.Selected = order.SourceService.Definition.VirtualPlatformServiceName.GetDescription();

			HandleVisibilityAndEnabledUpdate();
		}

		private void Order_AvailableSourceServiceDescriptionsChanged(IReadOnlyList<string> availableSourceServiceDescriptions)
		{
			// In case the current user is not allowed to select the service type of the existing service
			var options = new HashSet<string>(availableSourceServiceDescriptions);
			options.Add(order.SourceService.Definition.Description);

			sourceDescriptionDropDown.Options = options;
			sourceDescriptionDropDown.Selected = order.SourceService.Definition.Description;

			HandleVisibilityAndEnabledUpdate();
		}

		protected virtual void UpdateSourceServiceSection(object sender, Service service)
		{
			using (StartPerformanceLogging())
			{
				if (!serviceSections.CachedSourceServiceSections.TryGetValue(service.Id, out ServiceSection cachedServiceSection))
				{
					Log(nameof(UpdateSourceServiceSection), $"No cached source service section found for service {service.Name}, creating new one...");

					service.Children.CollectionChanged += ChildrenChanged;

					if (!configuration.MainSourceServiceSectionConfigurations.TryGetValue(service.Id, out var serviceSectionConfiguration))
					{
						configuration.AddNewMainSourceServiceSectionConfiguration(helpers, service, order);

						serviceSectionConfiguration = configuration.MainSourceServiceSectionConfigurations[service.Id];
					}

					cachedServiceSection = new ServiceSection(helpers, service as DisplayedService, serviceSectionConfiguration, userInfo, null);

					cachedServiceSection.GeneralInfoSection.RegenerateDialog += (s, args) => InvokeRegenerateUi();
					cachedServiceSection.UploadSynopsisSection.RegenerateDialog += (s, args) => InvokeRegenerateUi();
					cachedServiceSection.RecordingConfigurationSection.RegenerateDialog += (s, args) => InvokeRegenerateUi();

					serviceSections.CachedSourceServiceSections[service.Id] = cachedServiceSection;
					SourceServiceSectionAdded?.Invoke(this, cachedServiceSection);
				}

				serviceSections.SourceServiceSection = cachedServiceSection;

				sourceDropDown.Selected = cachedServiceSection.Service.Definition.VirtualPlatformServiceName.GetDescription();
				sourceDescriptionDropDown.Selected = cachedServiceSection.Service.Definition.Description;

				InvokeRegenerateUi();
			}
		}

		/// <summary>
		/// Updates the visibility of the widgets and underlying sections.
		/// </summary>
		protected void HandleMainVisibilityAndEnabledUpdate(bool sourceDropDownIsVisible, bool sourceServiceSectionIsVisible)
		{
			orderTypeLabel.IsVisible = IsVisible && configuration.OrderTypeIsVisible;
			orderTypeCheckbox.IsVisible = IsVisible && configuration.OrderTypeIsVisible;
			orderTypeCheckbox.IsEnabled = IsEnabled && configuration.OrderTypeIsEnabled;

			GeneralInfoSection.IsVisible = IsVisible && configuration.GeneralInfoSectionConfiguration.IsVisible;
			GeneralInfoSection.IsEnabled = IsEnabled && configuration.GeneralInfoSectionConfiguration.IsEnabled;

			sourceDropDown.IsVisible = IsVisible && configuration.MainSignalIsVisible && configuration.SourceIsVisible && configuration.SourceDropDownIsVisible && sourceDropDownIsVisible;
			sourceDropDown.IsEnabled = IsEnabled && configuration.SourceDropDownIsEnabled;

			sourceDescriptionDropDown.IsVisible = IsVisible && configuration.MainSignalIsVisible && sourceDropDownIsVisible && sourceDescriptionDropDown.Options.Count() > 1 && configuration.SourceIsVisible;
			sourceDescriptionDropDown.IsEnabled = IsEnabled && configuration.SourceDescriptionDropDownIsEnabled;

			SourceServiceSection.IsVisible = IsVisible && configuration.MainSignalIsVisible && configuration.SourceIsVisible && sourceServiceSectionIsVisible;
			SourceServiceSection.IsEnabled = IsEnabled;
		}

		protected abstract void GenerateUi();

		protected void GenerateSourceUi(ref int row)
		{
			AddWidget(sourceDropDown, ++row, configuration.InputColumn, 1, configuration.InputSpan);
			AddWidget(sourceDescriptionDropDown, ++row, configuration.InputColumn, 1, configuration.InputSpan);

			if (serviceSections.SourceServiceSection != null)
			{
				AddSection(serviceSections.SourceServiceSection, new SectionLayout(++row, configuration.LabelColumn));
				row += serviceSections.SourceServiceSection.RowCount;
			}
		}

		protected void GenerateHeaderUi(ref int row)
		{
			AddWidget(orderTypeLabel, ++row, 0);
			AddWidget(orderTypeCheckbox, row, 1, 1, 8);

			AddSection(GeneralInfoSection, new SectionLayout(++row, 0));
			row += GeneralInfoSection.RowCount;
		}

		protected void Log(string nameOfMethod, string message)
		{
			helpers?.Log(this.GetType().Name, nameOfMethod, message);
		}

		protected void LogMethodStart(string nameOfMethod)
		{
			helpers?.LogMethodStart(this.GetType().Name, nameOfMethod, out var stopwatch);
		}

		protected void LogMethodCompleted(string nameOfMethod)
		{
			helpers?.LogMethodCompleted(this.GetType().Name, nameOfMethod);
		}
	}
}