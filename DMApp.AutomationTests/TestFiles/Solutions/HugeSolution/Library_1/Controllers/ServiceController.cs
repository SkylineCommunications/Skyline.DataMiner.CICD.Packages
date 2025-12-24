namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library_1.EventArguments;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DTR;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class ServiceController : HelpedObject, IDisableableUi
	{
		private readonly ResourceAssignmentHandler resourceAssignmentHandler;
		private readonly UserInfo userInfo;
		private readonly OrderController orderController;
		private readonly EurovisionController eurovisionController;

		/// <summary>
		/// Creates an instance of the ServiceController class.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="service">The service object this controller will control.</param>
		/// <param name="selectableCompanySecurityViewIds">A dictionary with company name as key and company security view ID as value. Contains all companies that should be selectable for visibility rights.</param>
		/// <param name="serviceSections">The service section this controller will subscribe on.</param>
		/// <param name="orderController">The order controller that will be used to get information from and update the order.</param>
		/// <param name="resourceAssignmentHandler"></param>
		/// <param name="userInfo"></param>
		/// <exception cref="ArgumentNullException"/>
		public ServiceController(Helpers helpers, Service service, OrderController orderController, Dictionary<string, int> selectableCompanySecurityViewIds, IEnumerable<ServiceSection> serviceSections, ResourceAssignmentHandler resourceAssignmentHandler, UserInfo userInfo) : base(helpers)
		{
			this.Service = service as DisplayedService ?? throw new ArgumentNullException(nameof(service));
			this.orderController = orderController ?? throw new ArgumentNullException(nameof(orderController));
			this.resourceAssignmentHandler = resourceAssignmentHandler;
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			if (resourceAssignmentHandler == null && !service.Definition.IsDummy) throw new ArgumentNullException(nameof(resourceAssignmentHandler));
			if (selectableCompanySecurityViewIds == null) throw new ArgumentNullException(nameof(selectableCompanySecurityViewIds));
			Sections = serviceSections.ToList();

			if (service.Definition.VirtualPlatformServiceName == VirtualPlatformName.Eurovision)
			{
				if(!serviceSections.Any())
				{
					var displayedService = service as DisplayedService;
					var configuration = new EurovisionSectionConfiguration();
					var eurovisionSection = new EurovisionSection(displayedService, configuration);

					eurovisionController = new EurovisionController(helpers, service, eurovisionSection, (NormalOrderSection)orderController.OrderSection);
				}
				else
				{
					eurovisionController = new EurovisionController(helpers, service, serviceSections.First().EurovisionSection, (NormalOrderSection)orderController.OrderSection);
				}

				eurovisionController.BookEurovisionService += (s, e) => BookEurovisionService(s, e);
				eurovisionController.ServiceTimingChanged += (s, e) => orderController.HandleServiceTimeUpdate();
			}

			service.ChangedByUpdateServiceScript = true; // Used in Book Services to skip resource assignment logic

			serviceSections.ForEach(ss => SubscribeToUi(ss));
			InitializeValues(selectableCompanySecurityViewIds);
			ValidateService();
		}

		public event EventHandler BookEurovisionService;

		public event EventHandler UploadJsonButtonPressed;

		public event EventHandler<ServiceEventArgs> UploadSynopsisButtonPressed;

		public event EventHandler AudioChannelConfigurationChanged;

		public event EventHandler<string> ServiceToRecordOrTransmitChanged;

		public event EventHandler OrderValidationRequired;

		/// <summary>
		/// The Service Section this controller is subscribed to.
		/// </summary>
		public List<ServiceSection> Sections { get; } = new List<ServiceSection>();

		public DisplayedService Service { get; }

		public void AddSection(ServiceSection section)
		{
			Sections.Add(section);

			Log($"Controller is now subscribed to {Sections.Count} sections");

			SubscribeToUi(section);

			ValidateService();
		}

		public void ReplaceSection(ServiceSection newSection)
		{
			Sections.Clear();
			Sections.Add(newSection);

			SubscribeToUi(newSection);

			ValidateService();
		}

		/// <summary>
		/// Checks if certain properties are valid and sets their corresponding ValidationInfo.
		/// </summary>
		/// <returns>A boolean indicating if the service is valid.</returns>
		public bool ValidateService()
		{
			var options = ServiceValidator.Options.SkipCheckOccupyingOrderLocks;

			if (helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.IsResourceChangeAction)
			{
				options |= ServiceValidator.Options.SkipEncryptionKeyValidation;
			}

			var validator = new ServiceValidator(helpers, Service, userInfo, options);

			bool isValid = validator.Validate();

			Service.ValidationMessages = validator.ValidationMessages;

			Log(nameof(ValidateService), $"Validation messages: '{string.Join("\n", Service.ValidationMessages)}");

			return isValid;
		}

		/// <summary>
		/// Initializes values on the Service object to use in the section.
		/// </summary>
		private void InitializeValues(Dictionary<string, int> selectableCompanySecurityViewIds)
		{
			if (Service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception)
			{
				Service.SelectableSecurityViewIds = selectableCompanySecurityViewIds;
			}
			else if (Service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Recording)
			{
				Service.RecordingConfiguration.SelectableRecordingFileDestinations = Service.Definition.Description.Contains("Live") ? new List<string> { FileDestination.ArchiveMetro.GetDescription() } : new List<string> { FileDestination.UaIplay.GetDescription() };
				Service.RecordingConfiguration.RecordingFileDestination = Service.Definition.Description.Contains("Live") ? FileDestination.ArchiveMetro : FileDestination.UaIplay;

				Service.RecordingConfiguration.SelectableEvsMessiNewsTargets = helpers.OrderManagerElement.GetEvsMessiNewsTargets(Service.Definition);
				Service.RecordingConfiguration.EvsMessiNewsTarget = Service.RecordingConfiguration.SelectedEvsMessiNewsTarget?.Target;
			}
			else
			{
				//Nothing
			}

            if (helpers.Context.Script == Scripts.UpdateService && Service.IsBooked)
            {
                InitializeValuesForUpdateServiceContext();
            }

			if (Service.Definition.IsDummy) return;

			UpdateAllSelectableAndSelectedResources();

			if (userInfo.IsMcrUser)
			{
				// DCP218203
				foreach (var function in Service.Functions)
				{
					function.EnforceSelectedResource = true;
				}
			}
		}

		private void InitializeValuesForUpdateServiceContext()
		{
			if (Service.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite)
			{
				var matrixInputAsiFunction = Service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.MatrixInputAsi) as DisplayedFunction ?? throw new FunctionNotFoundException(FunctionGuids.MatrixInputAsi);

				matrixInputAsiFunction.EnforceSelectedResource = false;

				var matrixOutputAsiFunction = Service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.MatrixOutputAsi) as DisplayedFunction ?? throw new FunctionNotFoundException(FunctionGuids.MatrixOutputAsi);

				matrixOutputAsiFunction.EnforceSelectedResource = false;

				if ((helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.IsResourceChangeAction))
				{
					ClearDownstreamDtrCapabilities(Service.Functions.Single(f => f.Id == FunctionGuids.MatrixOutputLband));
				}
			}
		}

		/// <summary>
		/// Subscribes to all applicable Widgets in the section.
		/// </summary>
		private void SubscribeToUi(ServiceSection serviceSection)
		{
			if (serviceSection is null)
			{
				return;
			}

			serviceSection.GeneralInfoSection.StartChanged += (o, e) => UpdateStartTime(e, true);
			serviceSection.GeneralInfoSection.PrerollChanged += PrerollTimePicker_Changed;
			serviceSection.GeneralInfoSection.EndChanged += (o, e) => UpdateEndTime(e, true);
			serviceSection.GeneralInfoSection.PostrollChanged += PostrollTimePicker_Changed;
			serviceSection.GeneralInfoSection.SecurityViewIdsChanged += SecurityViewIds_Changed;

			serviceSection.GeneralInfoSection.ServiceTypeChanged += ServiceTypeChanged;

			serviceSection.ServiceToRecordOrTransmitChanged += (s, serviceToRecordOrTransmit) =>
			{
				Service.NameOfServiceToTransmitOrRecord = serviceToRecordOrTransmit.Name;
				ServiceToRecordOrTransmitChanged?.Invoke(this, serviceToRecordOrTransmit.Name);
			};

			foreach (var functionSection in serviceSection.FunctionSections)
			{
				functionSection.ResourceSection.AutomaticCheckBox.Changed += FunctionSection_AutomaticCheckBox_Changed;
				functionSection.ResourceSection.ResourceDropDown.Changed += FunctionSection_ResourceDropDown_Changed;
				functionSection.ResourceSection.IncludeOccupiedResourcesCheckBox.Changed += IncludeOccupiedResourcesCheckBox_Changed;

				foreach (var profileParameterSection in functionSection.ProfileParameterSections)
				{
					profileParameterSection.Changed += ProfileParameter_Changed;
				}
			}

			if (serviceSection.AudioChannelConfigurationSection.AudioEmbeddingRequiredDropDown != null)
				serviceSection.AudioChannelConfigurationSection.AudioEmbeddingRequiredDropDown.Changed += ProfileParameter_Changed;
			if (serviceSection.AudioChannelConfigurationSection.AudioDeembeddingRequiredDropDown != null)
				serviceSection.AudioChannelConfigurationSection.AudioDeembeddingRequiredDropDown.Changed += ProfileParameter_Changed;
			if (serviceSection.AudioChannelConfigurationSection.AudioShufflingRequiredDropDown != null)
				serviceSection.AudioChannelConfigurationSection.AudioShufflingRequiredDropDown.Changed += ProfileParameter_Changed;

			serviceSection.AudioChannelConfigurationSection.AddAudioPairButton.Pressed += (s, e) =>
			{
				AddAudioPairButton_Pressed(s, e);
				AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
			};

			serviceSection.AudioChannelConfigurationSection.DeleteAudioPairButton.Pressed += (s, e) =>
			{
				DeleteAudioPairButton_Pressed(s, e);
				AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
			};

			serviceSection.AudioChannelConfigurationSection.CopyFromSourceCheckBox.Changed += (s, e) =>
			{
				Service.AudioChannelConfiguration.IsCopyFromSource = e.IsChecked;
				if (e.IsChecked) Service.AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.Value = "No";
				AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
			};

			foreach (var audioChannelPairSection in serviceSection.AudioChannelConfigurationSection.AudioChannelPairSections)
			{
				audioChannelPairSection.FirstChannelDropDown.Changed += (s, e) =>
				{
					FirstChannelDropDown_Changed(audioChannelPairSection, Convert.ToString(e.Value));
					AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
				};

				audioChannelPairSection.SecondChannelDropDown.Changed += (s, e) =>
				{
					SecondChannelDropDown_Changed(audioChannelPairSection, Convert.ToString(e.Value));
					AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
				};

				audioChannelPairSection.FirstChannelOtherTextBox.Changed += (s, e) =>
				{
					ProfileParameter_Changed(s, e);
					AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
				};

				audioChannelPairSection.SecondChannelOtherTextBox.Changed += (s, e) =>
				{
					ProfileParameter_Changed(s, e);
					AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
				};

				audioChannelPairSection.StereoCheckBox.Changed += (s, e) =>
				{
					AudioChannelStereoCheckBox_Changed(s, e);
					AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
				};

				audioChannelPairSection.DolbyDecodingCheckBox.Changed += (s, e) =>
				{
					DolbyDecodingCheckBox_Changed(s, e);
					AudioChannelConfigurationChanged?.Invoke(this, EventArgs.Empty);
				};
			}

			serviceSection.RecordingConfigurationSection.DisplayedPropertyChanged += RecordingConfigurationSection_DisplayedPropertyChanged;
			serviceSection.RecordingConfigurationSection.SubRecordingsNeededChanged += RecordingConfigurationSection_SubRecordingsNeededChanged;
			serviceSection.RecordingConfigurationSection.AddNewSubRecordingButtonPressed += AddNewSubRecordingButton_Pressed;
			serviceSection.RecordingConfigurationSection.SubRecordingSectionsAdded += RecordingConfigurationSection_SubRecordingSectionsAdded;
			serviceSection.RecordingConfigurationSection.CopyPlasmaIdFromOrder += RecordingConfigurationSection_CopyPlasmaIdFromOrder;

			foreach (var subRecordingSection in serviceSection.RecordingConfigurationSection.SubRecordingSections)
			{
				subRecordingSection.DisplayedPropertyChanged += SubRecordingSection_DisplayedPropertyChanged;
				subRecordingSection.DeleteButtonPressed += SubRecordingSection_DeleteButtonPressed;
			}

			serviceSection.LiveUContactInfoSection.DisplayedPropertyChanged += (o, e) => Service.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);

			serviceSection.DisplayedPropertyChanged += (o, e) => Service.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);

			serviceSection.TechnicalSpecificationSection.DisplayedPropertyChanged += (o, e) => Service.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);

			serviceSection.UploadJsonButtonPressed += (o, e) => UploadJsonButtonPressed?.Invoke(this, e);

			serviceSection.UploadSynopsisSection.DeleteSynopsisButtonPressed += (o, synopsisFile) => Service.SynopsisFiles.Remove(synopsisFile);
			serviceSection.UploadSynopsisSection.UploadSynopsisButtonPressed += (o, service) => UploadSynopsisButtonPressed?.Invoke(this, new ServiceEventArgs(service));
		}

		private void RecordingConfigurationSection_CopyPlasmaIdFromOrder(object sender, bool copyPlasmaId)
		{
			Service.RecordingConfiguration.CopyPlasmaIdFromOrder = copyPlasmaId;

			if (!copyPlasmaId)
			{
				// clear value if unchecked
				Service.RecordingConfiguration.PlasmaIdForArchive = string.Empty;
			}

			orderController.CopyOrderPlasmaIdToServices();
		}

		private void ServiceTypeChanged(object o, DropDown.DropDownChangedEventArgs e)
		{
			Log(nameof(ServiceTypeChanged), $"Service {Service.Name} type changed from {e.Previous} to {e.Selected}");

			var newService = orderController.ServiceTypeDropDown_Changed(o, e);
			if (Service.Equals(newService))
			{
				UpdateAllSelectableAndSelectedResources();
			}
		}

		private void RecordingConfigurationSection_DisplayedPropertyChanged(object sender, DisplayedPropertyEventArgs e)
		{
			Service.RecordingConfiguration.IsConfigured = true;
			Service.RecordingConfiguration.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);

			InvokeOrderValidationRequired();
		}

		private void IncludeOccupiedResourcesCheckBox_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				string functionDefinitionLabel = ((YleResourceCheckBox)sender).FunctionDefinitionLabel ?? throw new NotFoundException($"Unable to find function definition label");

				Log(nameof(IncludeOccupiedResourcesCheckBox_Changed), $"USER INPUT: Show unavailable resources checkbox set to {Convert.ToBoolean(e.Value)} for function {functionDefinitionLabel}");

				var functionOfWhichCheckBoxHasChanged = Service.Functions.SingleOrDefault(f => f != null && f.Definition?.Label == functionDefinitionLabel) ?? throw new FunctionNotFoundException(functionDefinitionLabel);

				bool useAvailableResources = !Convert.ToBoolean(e.Value);

				var useAvailableResourcesPerFunction = new Dictionary<string, bool> { { functionOfWhichCheckBoxHasChanged.Definition.Label, useAvailableResources } };

				UpdateAllSelectableAndSelectedResources(useAvailableResourcesPerFunction);

				ValidateService();
			}
		}

		private void SecurityViewIds_Changed(object sender, Dictionary<string, int> newSecurityViewIds)
		{
			Service.SecurityViewIds = new HashSet<int>(newSecurityViewIds.Values);
			Log(nameof(SecurityViewIds_Changed), $"USER INPUT: Security View IDs changed to {string.Join(",", newSecurityViewIds.Select(x => $"{x.Key}({x.Value})"))}");
		}

		public void UpdateStartTime(DateTime newServiceStartTime)
		{
			UpdateStartTime(newServiceStartTime, false);
		}

		/// <summary>
		/// Handles changes to the General Info Section Start DateTimePicker.
		/// </summary>
		private void UpdateStartTime(DateTime newServiceStartTime, bool userInput)
		{
			using (UiDisabler.StartNew(this))
			{
				if (Service.Start == newServiceStartTime) return;

				var previousServiceStartTime = Service.Start;
				Service.Start = newServiceStartTime;
				Log(nameof(UpdateStartTime), $"{(userInput ? "USER" : "ORDER CONTROLLER")} INPUT: Setting start time from {previousServiceStartTime} to {Service.Start}");

				ValidateService();
				if (!Service.StartValidation.IsValid)
				{
					Log(nameof(UpdateStartTime), $"Start time is no longer valid, skipping selectable resources update");
					return;
				}

				orderController.HandleServiceStartTimeUpdate(Service, previousServiceStartTime);

				UpdateAllSelectableAndSelectedResources();

				if (userInput && Service.IntegrationType == IntegrationType.Feenix)
				{
					// If an MCR user changes the start time of a Feenix service,
					// then the integration is no longer the master.
					Service.IntegrationIsMaster = false;
				}
			}
		}

		private void PrerollTimePicker_Changed(object sender, TimeSpan e)
		{
			using (UiDisabler.StartNew(this))
			{
				var previousPreroll = Service.PreRoll;
				Service.PreRoll = e;
				Log(nameof(PrerollTimePicker_Changed), $"USER INPUT: Setting preroll from {previousPreroll} to {Service.PreRoll}");

				UpdateAllSelectableAndSelectedResources();

				orderController.InvokeValidationRequired();
			}
		}

		private void PostrollTimePicker_Changed(object sender, TimeSpan e)
		{
			using (UiDisabler.StartNew(this))
			{
				var previousPostroll = Service.PostRoll;
				Service.PostRoll = e;
				Log(nameof(PostrollTimePicker_Changed), $"USER INPUT: Setting postroll from {previousPostroll} to {Service.PostRoll}");

				UpdateAllSelectableAndSelectedResources();

				orderController.InvokeValidationRequired();
			}
		}

		public void UpdateEndTime(DateTime newServiceEndTime)
		{
			UpdateEndTime(newServiceEndTime, false);
		}

		private void UpdateEndTime(DateTime newServiceEndTime, bool userInput)
		{
			using (UiDisabler.StartNew(this))
			{
				using (StartPerformanceLogging())
				{
					if (Service.End == newServiceEndTime) return;

					var previousServiceEndTime = Service.End;
					Service.End = newServiceEndTime;

					Log(nameof(UpdateEndTime), $"{(userInput ? "USER" : "ORDER CONTROLLER")} INPUT: Setting end time from {previousServiceEndTime} to {Service.End}");

					if (!ValidateService()) return;

					orderController.HandleServiceEndTimeUpdate(Service, previousServiceEndTime);

					UpdateAllSelectableAndSelectedResources(null, Service.End > previousServiceEndTime);

					if (userInput && Service.IntegrationType == IntegrationType.Feenix)
					{
						// If an MCR user changes the end time of a Feenix service,
						// then the integration is no longer the master.
						Service.IntegrationIsMaster = false;
					}
				}
			}
		}

		/// <summary>
		/// Handles changes to Function Section Profile Parameters.
		/// </summary>
		/// <param name="sender">The Widget that triggered this event.</param>
		/// <param name="e">Contains the ID of the Profile Parameter and its updated value.</param>
		private void ProfileParameter_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				var functionsToUpdate = new List<DisplayedFunction>();
				foreach (var function in Service.Functions)
				{
					foreach (var profileParameter in function.Parameters.Where(p => p.Id == e.Id))
					{
						profileParameter.Value = e.Value;
						Log(nameof(ProfileParameter_Changed), $"USER INPUT: Setting profile parameter " + profileParameter.Name + " on function " + function.Name + " to '" + profileParameter.StringValue + "'");

						if (profileParameter.IsCapability) functionsToUpdate.Add(function);

						orderController.HandleProfileParameterUpdate(Service, function, profileParameter);
					}
				}

				// Only update resources when all capabilities on all functions are set
				foreach (var function in functionsToUpdate.OrderBy(f => f.ConfigurationOrder)) resourceAssignmentHandler.ExecuteDtr(function);

				UpdateAllSelectableAndSelectedResources();

				ValidateService();

				orderController.InvokeValidationRequired();
			}
		}

		/// <summary>
		/// Handles changes to Function Section Resource DropDown.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">Contains the Label Definition of the Function and the name of the updated resource.</param>
		private void FunctionSection_ResourceDropDown_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				using (StartPerformanceLogging())
				{
					string functionDefinitionLabel = ((YleResourceDropDown)sender).FunctionDefinitionLabel ?? throw new NotFoundException($"Unable to find function definition label");

					var function = Service.Functions.SingleOrDefault(f => f.Definition?.Label == functionDefinitionLabel) as DisplayedFunction ?? throw new FunctionNotFoundException(functionDefinitionLabel);

					var previousResourceName = function.ResourceName;

					string selectedResourceName = Convert.ToString(e.Value);

					function.Resource = selectedResourceName == Constants.None ? null : function.DisplayedResources.FirstOrDefault(r => function.ResourceNameConverter.Invoke(r.Name) == selectedResourceName) ?? throw new NotFoundException($"Unable to find resource for dropdown value {selectedResourceName} in collection '{string.Join(", ", function.DisplayedResourceNames)}'");

					if (function.Resource != null)
					{
						// Especially for Audio processing: when we select a resource for a function that originally did not require a resource, we should set this property to true to make sure routing is properly regenerated.
						function.RequiresResource = true;
					}

					Log(nameof(FunctionSection_ResourceDropDown_Changed), $"USER INPUT: Changing resource from {previousResourceName} to {function.ResourceName} on function {function.Name}");

					ClearDownstreamDtrCapabilities(function);

					bool functionResourceIsPartOfFixedTieLine = function.Resource != null && !string.IsNullOrEmpty(function.Resource.GetResourcePropertyStringValue(ResourcePropertyNames.FixedTieLineSource));
					if (functionResourceIsPartOfFixedTieLine)
					{
						function.McrHasOverruledFixedTieLineLogic = true;
						Log(nameof(FunctionSection_ResourceDropDown_Changed), $"Set function {function.Definition.Label} property {nameof(function.McrHasOverruledFixedTieLineLogic)} to true, because resource {function.ResourceName} is part of a fixed tie line");
					}

					orderController?.HandleSelectedResourceUpdate(Service, function);

					resourceAssignmentHandler.ExecuteDtr(function);

					if (Service.IntegrationType == IntegrationType.Feenix)
					{
						// If an MCR user changes the resource of a Feenix service,
						// then the integration is no longer the master.
						Service.IntegrationIsMaster = false;
					}

					UpdateOccupyingOrders();

					ValidateService();
					InvokeOrderValidationRequired();
				}
			}
		}

		private void ClearDownstreamDtrCapabilities(Function function)
		{
			Log(nameof(ClearDownstreamDtrCapabilities), $"Resetting DTR capabilities downstream from {function.Definition.Label}");

			// reset ResourceInputConnections, ResourceOutputConnections and _Matrix capabilities for the functions with a higher ConfigurationOrder
			// this is needed because filtering by ResourceAssignmentHandlers will otherwise still contain old values

			var downstreamDtrParametersPerFunction = ResourceUpdateHandler.GetDownstreamDtrParameters(function, Service);

			foreach (var downstreamDtrParameterKvp in downstreamDtrParametersPerFunction)
			{
				foreach (var downstreamDtrParameter in downstreamDtrParameterKvp.Value)
				{
					Log(nameof(ClearDownstreamDtrCapabilities), $"Setting function {downstreamDtrParameterKvp.Key} parameter {downstreamDtrParameter.Name} value from {downstreamDtrParameter.StringValue} to null");

					downstreamDtrParameter.Value = null;
				}
			}
		}

		private void FunctionSection_AutomaticCheckBox_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				string functionDefinitionLabel = ((YleResourceCheckBox)sender).FunctionDefinitionLabel ?? throw new NotFoundException($"Unable to find function definition label");

				var functionOfWhichResourceHasChanged = Service.Functions.SingleOrDefault(f => f != null && f.Definition?.Label == functionDefinitionLabel) as DisplayedFunction ?? throw new FunctionNotFoundException(functionDefinitionLabel);

				functionOfWhichResourceHasChanged.EnforceSelectedResource = !Convert.ToBoolean(e.Value);

				Log(nameof(FunctionSection_AutomaticCheckBox_Changed), $"USER INPUT: Changing enforce resource selection to {functionOfWhichResourceHasChanged.EnforceSelectedResource} on function {functionOfWhichResourceHasChanged.Name}");

				UpdateAllSelectableAndSelectedResources();
			}
		}

		private void InvokeOrderValidationRequired()
		{
			OrderValidationRequired?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateOccupyingOrders()
		{
			using (StartPerformanceLogging())
			{
				foreach (var function in Service.Functions.Cast<DisplayedFunction>())
				{
					if (function.Resource is OccupiedResource occupiedResource)
					{
						if (!occupiedResource.OccupyingServicesAlreadyRetrieved)
						{
							occupiedResource.OccupyingServices = helpers.ResourceManager.GetOccupyingServices(occupiedResource, Service.StartWithPreRoll, Service.EndWithPostRoll, orderController.OrderId, Service.Name);
						}

						function.OccupyingServicesForCurrentResource = occupiedResource.OccupyingServices;
					}
					else
					{
						function.OccupyingServicesForCurrentResource = new List<OccupyingService>(); // explicitly use the property setter to trigger events
					}
				}
			}
		}

		/// <summary>
		/// Handles changes to Audio Channel Pair Section IsStereo checkbox.
		/// </summary>
		/// <param name="sender">The YleCheckBox that threw the event.</param>
		/// <param name="e">Contains ID of FirstChannelAudioProfileParameter and a boolean indicating if the stereo checkBox is checked or not.</param>
		/// <exception cref="AudioChannelPairNotFoundException"/>
		private void AudioChannelStereoCheckBox_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			AudioChannelPair audioChannelPair = Service.AudioChannelConfiguration.AudioChannelPairs.FirstOrDefault(pair => pair.FirstChannelProfileParameter.Id == e.Id);
			if (audioChannelPair == null) return;

			audioChannelPair.IsStereo = (bool)e.Value;

			// Set Dolby Decoding Parameter to false
			if ((!audioChannelPair.FirstChannel.IsDolby || !audioChannelPair.FirstChannel.IsStereo) && audioChannelPair.DolbyDecodingProfileParameter != null)
			{
				audioChannelPair.DolbyDecodingProfileParameter.Value = "No";
			}

			if (audioChannelPair.FirstChannel.OriginatesFromSource && (audioChannelPair.IsStereo && !audioChannelPair.FirstChannel.IsStereo) || (!audioChannelPair.IsStereo && audioChannelPair.FirstChannel.IsStereo))
			{
				// Clear values
				audioChannelPair.Clear();
			}

			// Copy values over from First Audio Channel to Second Audio Channel
			if (audioChannelPair.IsStereo)
			{
				audioChannelPair.SecondChannel = audioChannelPair.FirstChannel;

				// Clear description of second channel if value is not "Other"
				//if (audioChannelPair.SecondChannelProfileParameter.StringValue != "Other")
				//{
				//    audioChannelPair.SecondChannelDescriptionProfileParameter.Value = string.Empty;
				//}
			}
		}

		/// <summary>
		/// Handles changes to Audio Channel Pair Section First Channel DropDown.
		/// </summary>
		/// <param name="audioChannelPairSection">The Section that triggered the event.</param>
		/// <param name="displayValue">The displayed value of the selected AudioChannelOption.</param>
		/// <exception cref="AudioChannelPairNotFoundException"/>
		private void FirstChannelDropDown_Changed(AudioChannelPairSection audioChannelPairSection, string displayValue)
		{
			var audioChannelPair = audioChannelPairSection.AudioChannelPair;
			AudioChannelOption selectedOption = audioChannelPair.FirstChannelOptions.First(x => x.DisplayValue.Equals(displayValue));

			// Only update the IsStereo value if the option originated from another Service (e.g. copy from source)
			if (selectedOption.OriginatesFromSource) audioChannelPair.IsStereo = selectedOption.IsStereo;
			if (selectedOption.IsDolby) audioChannelPair.IsStereo = true;

			audioChannelPair.FirstChannel = selectedOption;

			// Copy value to second channel if stereo
			if (audioChannelPair.IsStereo) audioChannelPair.SecondChannel = audioChannelPair.FirstChannel;

			if ((!selectedOption.IsDolby || !audioChannelPair.IsStereo) && audioChannelPair.DolbyDecodingProfileParameter != null)
			{
				audioChannelPair.DolbyDecodingProfileParameter.Value = "No";
			}

			if (Service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Reception) Service.AudioChannelConfiguration.UpdateSelectableOptions();
		}

		/// <summary>
		/// Handles changes to Audio Channel Pair Section Second Channel DropDown.
		/// </summary>
		/// <param name="audioChannelPairSection">The Section that triggered the event.</param>
		/// <param name="displayValue">The displayed value of the selected AudioChannelOption.</param>
		/// <exception cref="AudioChannelPairNotFoundException"/>
		private void SecondChannelDropDown_Changed(AudioChannelPairSection audioChannelPairSection, string displayValue)
		{
			var audioChannelPair = audioChannelPairSection.AudioChannelPair;
			AudioChannelOption selectedOption = audioChannelPair.SecondChannelOptions.First(x => x.DisplayValue.Equals(displayValue));

			// Only update the IsStereo value if the option originated from another Service (e.g. copy from source)
			if (selectedOption.OriginatesFromSource) audioChannelPair.IsStereo = selectedOption.IsStereo;
			if (selectedOption.IsDolby) audioChannelPair.IsStereo = true;

			audioChannelPair.SecondChannel = selectedOption;

			// Copy value to second channel if stereo
			if (selectedOption.IsStereo) audioChannelPair.FirstChannel = audioChannelPair.SecondChannel;

			if (Service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Reception) Service.AudioChannelConfiguration.UpdateSelectableOptions();
		}

		/// <summary>
		/// Handles changes to Audio Channel Pair Section Dolby Decoding checkbox.
		/// </summary>
		/// <param name="sender">The YleCheckBox that threw the event.</param>
		/// <param name="e">Contains ID of FirstChannelAudioProfileParameter and a boolean indicating if the Dolby Decoding checkBox is checked or not.</param>
		/// <exception cref="AudioChannelPairNotFoundException"/>
		private void DolbyDecodingCheckBox_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			var audioChannelPair = Service.AudioChannelConfiguration.AudioChannelPairs.FirstOrDefault(x => x.FirstChannelProfileParameter.Id == e.Id);
			if (audioChannelPair != null && audioChannelPair.DolbyDecodingProfileParameter != null)
			{
				audioChannelPair.DolbyDecodingProfileParameter.Value = (bool)e.Value ? "Yes" : "No";
			}
		}

		private void AddAudioPairButton_Pressed(object sender, EventArgs e)
		{
			// Add Audio Pair - Set next Pair visible
			Service.AudioChannelConfiguration.AddAudioChannelPair();
		}

		private void DeleteAudioPairButton_Pressed(object sender, EventArgs e)
		{
			// Remove Audio Pair - Remove last Pair and clear its values
			int channelId = Service.AudioChannelConfiguration.RemoveAudioChannelPair();
			if (channelId < 0) return;

			// Clear values from removed pair
			var removedAudioChannelPair = Service.AudioChannelConfiguration.AudioChannelPairs.FirstOrDefault(x => x.Channel == channelId);
			if (removedAudioChannelPair == null) return;

			removedAudioChannelPair.Clear();
		}

		/// <summary>
		/// Handles changes to the SubRecording section subrecordings needed checkbox.
		/// </summary>
		private void RecordingConfigurationSection_SubRecordingsNeededChanged(object sender, bool subRecordingsNeeded)
		{
			Service.RecordingConfiguration.SubRecordingsNeeded = subRecordingsNeeded;

			if (!Service.RecordingConfiguration.SubRecordingsNeeded)
				Service.RecordingConfiguration.ClearSubRecordings();
		}

		/// <summary>
		/// Handles changes to the SubRecording section add new subrecording button.
		/// </summary>
		private void AddNewSubRecordingButton_Pressed(object sender, EventArgs e)
		{
			Service.RecordingConfiguration.AddSubRecording(new SubRecording());
		}

		/// <summary>
		/// Subscribes on newly generated subrecording sections.
		/// </summary>
		private void RecordingConfigurationSection_SubRecordingSectionsAdded(object sender, SubRecordingSection subRecordingSection)
		{
			subRecordingSection.DisplayedPropertyChanged += SubRecordingSection_DisplayedPropertyChanged;
			subRecordingSection.DeleteButtonPressed += SubRecordingSection_DeleteButtonPressed;
		}

		/// <summary>
		/// Handles changes to the SubRecording section name textbox.
		/// </summary>
		private void SubRecordingSection_DisplayedPropertyChanged(object sender, DisplayedPropertyEventArgs e)
		{
			var subRecordingId = ((SubRecordingSection)sender).SubRecordingId;

			var subRecording = Service.RecordingConfiguration.SubRecordings.SingleOrDefault(r => r.Id == subRecordingId);
			if (subRecording == null) return;

			subRecording.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);

			ValidateService();
		}

		/// <summary>
		/// Handles changes to the SubRecording section delete subrecording button.
		/// </summary>
		private void SubRecordingSection_DeleteButtonPressed(object sender, EventArgs e)
		{
			var subRecordingId = ((SubRecordingSection)sender).SubRecordingId;

			Service.RecordingConfiguration.DeleteSubRecording(subRecordingId);
		}

		private void UpdateAllSelectableAndSelectedResources(Dictionary<string, bool> useAvailableResourcesPerFunction = null, bool serviceExtension = false)
		{
			using (StartPerformanceLogging())
			{
				if (resourceAssignmentHandler is null) return;

				useAvailableResourcesPerFunction = useAvailableResourcesPerFunction ?? Service.Functions.ToDictionary(f => f.Definition.Label, f => true); // By default, use available resources

				foreach (var function in Service.Functions.OrderBy(f => f.ConfigurationOrder).Cast<DisplayedFunction>())
				{
					UpdateSelectableAndSelectedResourceForFunction(function, useAvailableResourcesPerFunction, serviceExtension);
				}

				if (Service.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite && helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.IsResourceChangeAction)
				{
					FilterMatrixOutputLbandResourcesConditionally();
				}

				UpdateOccupyingOrders();
			}
		}

		private void UpdateSelectableAndSelectedResourceForFunction(DisplayedFunction function, Dictionary<string, bool> useAvailableResourcesPerFunction, bool serviceExtension)
		{
			var previousResourceId = function.Resource?.ID ?? Guid.Empty;

			function.DisplayedResources = GetResourcesToDisplay(useAvailableResourcesPerFunction, function);

			bool resourceChanged = false;

			bool extendingNonRunningBookedReceptionCausesResourceToBeUnavailable = serviceExtension && Service.IsBooked && !Service.IsOrShouldBeRunning && Service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception && !function.DisplayedResources.Contains(function.Resource);

			if (extendingNonRunningBookedReceptionCausesResourceToBeUnavailable)
			{
				// DCP203568 Don't reselect source resource when extending

				Log(nameof(UpdateAllSelectableAndSelectedResources), $"Extending non-running booked reception causes the assigned resource '{function.ResourceName}' to become unavailable. An available resource will not be automatically reselected. [DCP203568]");

				if (!function.DisplayedResourceNames.Contains(Constants.None))
				{
					// Add a "None" option to the resource dropdown
					function.ResourceSelectionMandatory = false;
				}

				function.Resource = null;
				function.EnforceSelectedResource = true;
				resourceChanged = previousResourceId != Guid.Empty;
			}
			else
			{
				function.Resource = resourceAssignmentHandler.SelectCurrentOrNewResource(function, function.DisplayedResources, out resourceChanged);

				function.ResourceSelectionMandatory = function.DisplayedResources.Any(r => r.GetResourcePropertyBooleanValue(ResourcePropertyNames.ResourceSelectionAllowed));
			}

			if (function.Resource != null && function.ResourceSelectionMandatory)
			{
				function.EnforceSelectedResource = true; // will remove "none" from resource dropdown
			}

			resourceChanged = CheckToKeepResourceForRunningService(function, previousResourceId, resourceChanged);

			if (resourceChanged)
			{
				resourceAssignmentHandler.ExecuteDtr(function);

				orderController?.HandleSelectedResourceUpdate(Service, function);
			}
		}

		private bool CheckToKeepResourceForRunningService(DisplayedFunction function, Guid previousResourceId, bool resourceChanged)
		{
			bool previousResourceIsNone = previousResourceId == Guid.Empty;
			if (resourceChanged && Service.IsOrShouldBeRunning && !previousResourceIsNone)
			{
				// When extending a running order and resource becomes unavailable, try to keep it by selecting a new resource for the occupying order [DCP191769]
				// To enable this, assign an OccupiedResource to the function

				resourceChanged = !TryKeepPreviouslyAssignedResource(function, previousResourceId);
			}

			return resourceChanged;
		}

		private void FilterMatrixOutputLbandResourcesConditionally()
		{
			// Additional filtering required [DCP201965]

			var demodulatingFunction = Service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.Demodulating) as DisplayedFunction ?? throw new FunctionNotFoundException(FunctionGuids.Demodulating);
			var decodingFunction = Service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.Decoding) as DisplayedFunction ?? throw new FunctionNotFoundException(FunctionGuids.Decoding);

			bool demodulatingAndDecodingResourcesAreSameDevice = demodulatingFunction.Resource != null && decodingFunction.Resource != null && demodulatingFunction.Resource.MainDVEElementID == decodingFunction.Resource.MainDVEElementID;

			if (demodulatingAndDecodingResourcesAreSameDevice)
			{
				var matrixOutputLbandFunction = Service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.MatrixOutputLband) as DisplayedFunction ?? throw new FunctionNotFoundException(FunctionGuids.MatrixOutputLband);

				var matrixOutputLbandFunctionToDisplay = new HashSet<FunctionResource>();

				var resourceInputConnectionsLbandProfileParameter = helpers.ProfileManager.GetProfileParameter(ProfileParameterGuids.ResourceInputConnectionsLband);

				foreach (var matrixResource in matrixOutputLbandFunction.DisplayedResources)
				{
					resourceInputConnectionsLbandProfileParameter.Value = matrixResource.Name;

					var connectedDemodulating = demodulatingFunction.DisplayedResources.FirstOrDefault(demodResource => demodResource.MatchesProfileParameter(helpers, resourceInputConnectionsLbandProfileParameter));
					if (connectedDemodulating is null) continue;

					bool connectedIrdSupportsDemodulatingAndDecoding = decodingFunction.DisplayedResources.Any(decodResource => decodResource.MainDVEElementID == connectedDemodulating.MainDVEElementID);
					if (!connectedIrdSupportsDemodulatingAndDecoding) continue;

					matrixOutputLbandFunctionToDisplay.Add(matrixResource);
				}

				var removedResources = matrixOutputLbandFunction.DisplayedResources.Except(matrixOutputLbandFunctionToDisplay);

				Log(nameof(FilterMatrixOutputLbandResourcesConditionally), $"Removed matrix output lband resources {string.Join(", ", removedResources.Select(r => r.Name))}");

				matrixOutputLbandFunction.DisplayedResources = matrixOutputLbandFunctionToDisplay;
			}
		}

		private HashSet<FunctionResource> GetResourcesToDisplay(Dictionary<string, bool> useAvailableResourcesPerFunction, DisplayedFunction function)
		{
			if (!useAvailableResourcesPerFunction.TryGetValue(function.Definition.Label, out bool useAvailableResources)) useAvailableResources = true;

			var filterOptions = useAvailableResources ? FilterOptions.InterService : FilterOptions.None;
			// Only filter inter service when available resources are need to be shown.
			// No filtering allowed when all resources need to be shown.

			var resourcesToDisplay = resourceAssignmentHandler.GetSelectableResources(function, filterOptions, useAvailableResources);

			return resourcesToDisplay;
		}

		private bool TryKeepPreviouslyAssignedResource(DisplayedFunction function, Guid previousResourceId)
		{
			// When extending a running order and resource becomes unavailable, try to keep it by selecting a new resource for the occupying order [DCP191769]
			// To enable this, assign an OccupiedResource to the function

			Log(nameof(TryKeepPreviouslyAssignedResource), $"A resource on running service {Service.Name} has changed, we're going to try to keep the same resource...");

			var allResources = resourceAssignmentHandler.GetSelectableResources(function, FilterOptions.None, false);

			var previousResource = allResources.SingleOrDefault(r => r.ID == previousResourceId);

			if (previousResource != null)
			{
				function.IncludeUnavailableResources = true;
				function.EnforceSelectedResource = true;
				function.DisplayedResources = allResources;
				function.Resource = previousResource;

				Log(nameof(TryKeepPreviouslyAssignedResource), $"Successfully kept same resource {previousResource.Name} ({previousResource.ID})");

				return true;
			}
			else
			{
				Log(nameof(TryKeepPreviouslyAssignedResource), $"Unable keep same resource {previousResourceId}");

				return false;
			}
		}



		//private void SetAudioChannelPairDropDownOptions()
		//{
		//    bool serviceHasAudioChannelConfig = service.AudioChannelConfiguration.AudioChannelPairs.Any();
		//    if (!serviceHasAudioChannelConfig) return;

		//    List<AudioChannelOption> allAudioChannelOptions = new List<AudioChannelOption>(service.AudioChannelConfiguration.GetProfileParameterOptions());
		//    List<AudioChannelOption> dolbyOptions = allAudioChannelOptions.Where(x => x.Value.Contains("Dolby")).ToList();
		//    List<AudioChannelOption> dolbyDuoChannelOptions = dolbyOptions.Where(x => x.Value.Contains("&A")).ToList();

		//    List<AudioChannelOption> options;
		//    bool serviceIsReception = service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception;
		//    if (serviceIsReception)
		//    {
		//        options = allAudioChannelOptions.Except(dolbyDuoChannelOptions).ToList();
		//        options.Add(AudioChannelOption.None());
		//    }
		//    else
		//    {
		//        options = GetSelectableAudioChannelPairDropDownOptionsForNonReception();
		//    }

		//    helpers.Log(nameof(ServiceController), nameof(SetAudioChannelPairDropDownOptions), $"Audio Options: {String.Join(", ", options)}");
		//    service.AudioChannelConfiguration.SetProfileParameterOptions(options);
		//}

		/// <summary>
		/// Gets the Audio Channel options that should be available for non-reception services based on the source service.
		/// </summary>
		/// <returns>A collection of strings representing the selectable options.</returns>
		//private List<AudioChannelOption> GetSelectableAudioChannelPairDropDownOptionsForNonReception()
		//{
		//    List<AudioChannelOption> allAudioChannelOptions = new List<AudioChannelOption>(service.AudioChannelConfiguration.GetProfileParameterOptions());
		//    List<AudioChannelOption> dolbyOptions = allAudioChannelOptions.Where(x => x.Value.Contains("Dolby")).ToList();
		//    List<AudioChannelOption> dolbyDuoChannelOptions = dolbyOptions.Where(x => x.Value.Contains("&A")).ToList();

		//    var sourceAudioChannelConfiguration = orderController.GetSourceAudioChannelConfiguration();
		//    if (sourceAudioChannelConfiguration == null) return allAudioChannelOptions.Except(dolbyOptions).ToList();

		//    var allSourceAudioChannelConfigSelectedValues = new List<AudioChannelOption>(sourceAudioChannelConfiguration.GetSourceOptions());
		//    //foreach (var audioChannelPair in sourceAudioChannelConfiguration.AudioChannelPairs)
		//    //{
		//    //    var firstChannelOption = audioChannelPair.FirstChannelProfileParameter.StringValue == "Other" ? $"{audioChannelPair.FirstChannelProfileParameter.StringValue} - {audioChannelPair.FirstChannelDescriptionProfileParameter.StringValue}" : $"{audioChannelPair.FirstChannelProfileParameter.StringValue}";

		//    //    if (!allSourceAudioChannelConfigSelectedValues.Contains(firstChannelOption))
		//    //    {
		//    //        allSourceAudioChannelConfigSelectedValues.Add(firstChannelOption);
		//    //    }

		//    //    var secondChannelOption = audioChannelPair.SecondChannelProfileParameter.StringValue == "Other" ? $"{audioChannelPair.SecondChannelProfileParameter.StringValue} - {audioChannelPair.SecondChannelDescriptionProfileParameter.StringValue}" : $"{audioChannelPair.SecondChannelProfileParameter.StringValue}";

		//    //    if (!allSourceAudioChannelConfigSelectedValues.Contains(secondChannelOption))
		//    //    {
		//    //        allSourceAudioChannelConfigSelectedValues.Add(secondChannelOption);
		//    //    }
		//    //}

		//    List<AudioChannelOption> options = allSourceAudioChannelConfigSelectedValues.Except(dolbyOptions).ToList();
		//    options.Add(AudioChannelOption.None());

		//    bool dolbyAudioChannelIsSelectedInSource = allSourceAudioChannelConfigSelectedValues.Intersect(dolbyOptions).Any();
		//    bool dolbyDecodingIsRequired = sourceAudioChannelConfiguration.AudioDolbyDecodingRequiredProfileParameter != null && sourceAudioChannelConfiguration.AudioDolbyDecodingRequiredProfileParameter.StringValue == "Yes";
		//    if (dolbyAudioChannelIsSelectedInSource && dolbyDecodingIsRequired)
		//        options.AddRange(dolbyDuoChannelOptions);

		//    return options;
		//}

		public void DisableUi()
		{
			orderController?.DisableUi();
		}

		public void EnableUi()
		{
			orderController?.EnableUi();
		}
	}
}