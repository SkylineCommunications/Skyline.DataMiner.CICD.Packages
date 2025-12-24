namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Functions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts;
	using Skyline.DataMiner.Utils.YLE.Integrations;

    public class ServiceSectionConfiguration : ISectionConfiguration
	{
		private ServiceSectionConfiguration(Helpers helpers, Service service, UserInfo userInfo, string eventOwnerCompany = null, IReadOnlyList<string> userCompanies = null)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (userInfo == null) throw new ArgumentNullException(nameof(userInfo));

			PromoteToSharedSourceIsVisible = userInfo.CanPromoteToSharedSource && service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception;
			// Hidden "Use Source from File" button on Satellite section (DCP task: 218236)
			// UploadJsonButtonIsVisible = service.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite && service.BackupType == BackupType.None && (userInfo.IsMcrUser || userInfo.UserGroups.Any(c => c.Name.Contains("NEP") || c.Name.Contains("NENT")));
			UploadJsonButtonIsVisible = false;
			UnsuccessfulResourceAssignmentWarningIsVisible = userInfo.IsMcrUser;

			InitGeneralInfoSectionConfiguration(service, userInfo, eventOwnerCompany, userCompanies);
			InitAudioChannelSectionConfiguration(service, userInfo);
			RecordingConfigurationSectionConfiguration = new RecordingConfigurationSectionConfiguration(service.IntegrationType != IntegrationType.None, service.Definition.Description);

			InitFunctionConfigurations(helpers, service, userInfo);

			FunctionsInDisplayOrder = service.Functions.OrderBy(f => service.Definition.GetFunctionPosition(f)).Select(f => f.Definition.Label).ToList();
		}

		public ServiceSectionConfiguration(Helpers helpers, Service service, UserInfo userInfo, bool lockGranted, string eventOwnerCompany = null, IReadOnlyList<string> userCompanies = null)
			: this(helpers, service, userInfo, eventOwnerCompany, userCompanies)
		{
			if (!lockGranted) DisableAll("The Order that uses this service is locked.");
			else if (service.IsOrShouldBeRunning)
			{
				GeneralInfoSectionConfiguration.StartDateIsEnabled = false;
				GeneralInfoSectionConfiguration.PrerollIsEnabled = false;
				ReasonForBeingDisabled = "Start time can not be changed as the service is currently running";
			}
			else EnableAll(service);
		}

		public static ServiceSectionConfiguration CreateConfigurationForResourceSelectionOnly(Helpers helpers, Service service, UserInfo userInfo, bool lockGranted, string eventOwnerCompany = null, List<string> userCompanies = null)
		{
			var configuration = new ServiceSectionConfiguration(helpers, service, userInfo, lockGranted, eventOwnerCompany, userCompanies);

			configuration.SetIsVisiblePropertyValues(false);

			foreach (var functionSectionConfiguration in configuration.FunctionSectionConfigurations)
			{
				functionSectionConfiguration.Value.IsVisible = true;
				functionSectionConfiguration.Value.HideAllProfileParameters(service.Functions.Single(f => f.Definition.Label == functionSectionConfiguration.Key));
				functionSectionConfiguration.Value.ResourceSectionConfiguration.IsVisible = true;
			}

			if (service.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite && helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.IsResourceChangeAction)
			{
				ApplyConfigurationFor_UpdateService_ResourceChange_SatelliteReception(service, configuration);
			}

			return configuration;
		}

		public static ServiceSectionConfiguration CreateConfigurationForTimingSelectionOnly(Helpers helpers, Service service, UserInfo userInfo, bool lockGranted, string eventOwnerCompany = null, List<string> userCompanies = null)
		{
			var configuration = new ServiceSectionConfiguration(helpers, service, userInfo, lockGranted, eventOwnerCompany, userCompanies);

			configuration.SetIsVisiblePropertyValues(false);

			configuration.IsVisible = true;
			configuration.GeneralInfoSectionConfiguration.IsVisible	= true;
			configuration.GeneralInfoSectionConfiguration.PrerollIsVisible = userInfo.IsMcrUser;
			configuration.GeneralInfoSectionConfiguration.PrerollIsEnabled = !service.IsOrShouldBeRunning;
			configuration.GeneralInfoSectionConfiguration.StartDateIsVisible = true;
			configuration.GeneralInfoSectionConfiguration.StartDateIsEnabled = !service.IsOrShouldBeRunning;
			configuration.GeneralInfoSectionConfiguration.EndDateIsVisible = true;
			configuration.GeneralInfoSectionConfiguration.PostrollIsVisible = userInfo.IsMcrUser;

			return configuration;
		}

		public static ServiceSectionConfiguration CreateLiveOrderFormConfiguration(Helpers helpers, Service service, UserInfo userInfo, string eventOwnerCompany, IReadOnlyList<string> userCompanies)
		{
			ServiceSectionConfiguration config;
			switch (service.Definition.VirtualPlatformServiceType)
			{
				case VirtualPlatformType.VizremStudio:
				case VirtualPlatformType.VizremFarm:
					config = CreateVizremConfiguration(helpers, service, userInfo);
					config.LabelColumn = 0;
					config.LabelSpan = 1;
					config.InputColumn = 1;
					config.InputSpan = 2;
					break;
				case VirtualPlatformType.Reception:
					config = CreateNormalConfiguration(helpers, service, userInfo, eventOwnerCompany, userCompanies);
					config.LabelColumn = 0;
					config.LabelSpan = 2;
					config.InputColumn = 2;
					config.InputSpan = 2;
					break;
				default:
					config = CreateNormalConfiguration(helpers, service, userInfo, eventOwnerCompany, userCompanies);
					config.LabelColumn = 0;
					config.LabelSpan = 1;
					config.InputColumn = 1;
					config.InputSpan = 3;
					break;
			}

			config.SetLabelSpan(config.LabelSpan);
			config.SetInputColumn(config.InputColumn);
			config.SetInputSpan(config.InputSpan);

			return config;
		}

		private static ServiceSectionConfiguration CreateNormalConfiguration(Helpers helpers, Service service, UserInfo userInfo, string eventOwnerCompany, IReadOnlyList<string> userCompanies)
        {
			var configuration = new ServiceSectionConfiguration(helpers, service, userInfo, eventOwnerCompany, userCompanies);

			if (service.Definition.VirtualPlatformServiceName == VirtualPlatformName.None)
            {
				configuration.SetIsVisiblePropertyValues(false);
            }
			else if (service.Definition.VirtualPlatform == VirtualPlatform.ReceptionCommentaryAudio)
            {
				configuration.AdjustDetailsIsVisible = true;
				configuration.PromoteToSharedSourceIsVisible = false;
			}
			else if (service.Definition.VirtualPlatform == VirtualPlatform.ReceptionUnknown)
            {
				configuration.SetIsVisiblePropertyValues(false);
				configuration.TechnicalSpecificationSectionIsVisible = true;
			}
            else
            {
				bool serviceIsManualSharedSource = service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception && service.IsSharedSource && service.BackupType == BackupType.None;

				configuration.AdjustDetailsIsVisible = true;
				configuration.PromoteToSharedSourceIsVisible = service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception && !service.IsSharedSource && userInfo.CanPromoteToSharedSource && service.BackupType == BackupType.None;
				configuration.PromoteToSharedSourceIsEnabled = service.BackupType == BackupType.None;
				configuration.GeneralInfoSectionConfiguration.StartDateIsEnabled = !serviceIsManualSharedSource;
				configuration.GeneralInfoSectionConfiguration.EndDateIsEnabled = !serviceIsManualSharedSource;

				if (service.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite)
                {
					configuration.UploadSynopsisSectionConfiguration.IsVisible = true;
					configuration.AudioChannelSectionConfiguration.IsVisible = userInfo.IsMcrUser;
					foreach (var functionSectionConfiguration in configuration.FunctionSectionConfigurations) functionSectionConfiguration.Value.IsVisible = userInfo.IsMcrUser;
				}
			}

			return configuration;
		}

		private static ServiceSectionConfiguration CreateVizremConfiguration(Helpers helpers, Service service, UserInfo userInfo)
		{
			var configuration = new ServiceSectionConfiguration(helpers, service, userInfo);

			configuration.AdjustDetailsIsVisible = false;
			configuration.AudioChannelSectionConfiguration.IsVisible = false;
			configuration.AdditionalInfoVisible = false;
			configuration.GeneralInfoSectionConfiguration.IsVisible = false;

			configuration.FunctionSectionConfigurations.Values.ToList().ForEach(fsc => fsc.ResourceSectionConfiguration.OccupiedResourceSelectionIsVisible = false);

			return configuration;
		}

		[IsIsVisibleProperty]
		public bool IsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; } = true;

		public string ReasonForBeingDisabled { get; set; }

		public GeneralInfoSectionConfiguration GeneralInfoSectionConfiguration { get; private set; }

		public AudioChannelConfigurationSectionConfiguration AudioChannelSectionConfiguration { get; private set; }

		public RecordingConfigurationSectionConfiguration RecordingConfigurationSectionConfiguration { get; private set; } = new RecordingConfigurationSectionConfiguration(false);

		public LiveUContactInfoSectionConfiguration LiveUContactInfoSectionConfiguration { get; private set; } = new LiveUContactInfoSectionConfiguration();

		public TechnicalSpecificationSectionConfiguration TechnicalSpecificationSectionConfiguration { get; private set; } = new TechnicalSpecificationSectionConfiguration();

		public AdditionalInformationSectionConfiguration AdditionalInformationSectionConfiguration { get; private set; } = new AdditionalInformationSectionConfiguration();

		public UploadSynopsisSectionConfiguration UploadSynopsisSectionConfiguration { get; private set; } = new UploadSynopsisSectionConfiguration();

		public EurovisionSectionConfiguration EurovisionSectionConfiguration { get; private set; } = new EurovisionSectionConfiguration();

		[IsIsVisibleProperty]
		public bool UnsuccessfulResourceAssignmentWarningIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool AdditionalInfoVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool AdditionalInfoEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool VidigoStreamSourceLinkIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool VidigoStreamSourceLinkEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool IpRecorderSectionIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IpRecorderSectionEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool TechnicalSpecificationSectionIsVisible { get; set; } = false;

		[IsIsEnabledProperty]
		public bool TechnicalSpecificationSectionEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool LiveUContactInfoIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool LiveUContactInfoEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool LiveUDeviceNameIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool LiveUDeviceNameEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool AudioReturnInfoIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool AudioReturnInfoEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool EurovisionIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool EurovisionIsEnabled { get; set; } = true;

		[IsIsEnabledProperty]
		public bool ServiceToRecordOrTransmitDropDownEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool ServiceToRecordOrTransmitDropDownIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool PromoteToSharedSourceIsVisible { get; set; } = false;

		[IsIsEnabledProperty]
		public bool PromoteToSharedSourceIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool AdjustDetailsIsVisible { get; set; } = false;

		[IsIsEnabledProperty]
		public bool AdjustDetailsIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool UploadJsonButtonIsVisible { get; set; } = false;

		[IsIsEnabledProperty]
		public bool UploadJsonButtonIsEnabled { get; set; } = true;

		[IsIsEnabledProperty]
		public bool RecordingFileDetailsIsEnabled { get; set; } = true;

		public int LabelColumn { get; private set; } = 0;
		
		public int LabelSpan { get; private set; } = 2;

		public int InputSpan { get; private set; } = 2;

		public int InputColumn { get; private set; } = 2;

		public List<string> FunctionsInDisplayOrder { get; set; } = new List<string>();

		/// <summary>
		/// A dictionary with the function Guid as key and the configuration for its section as value.
		/// </summary>
		public Dictionary<string, FunctionSectionConfiguration> FunctionSectionConfigurations { get; private set; } = new Dictionary<string, FunctionSectionConfiguration>();

		[JsonIgnore]
		public Dictionary<string, string> ToolTip { get; set; } = ReflectionHandler.ReadTooltipFile();

		/// <summary>
		/// Required for setting properties using reflection.
		/// </summary>
		public IEnumerable<FunctionSectionConfiguration> AllFunctionSections => FunctionSectionConfigurations.Values;

		public void SetLabelSpan(int span)
        {
			LabelSpan = span;
			GeneralInfoSectionConfiguration.LabelSpan = span;
			AudioChannelSectionConfiguration.LabelSpan = span;
			RecordingConfigurationSectionConfiguration.LabelSpan = span;
			AudioChannelSectionConfiguration.LabelSpan = span;
			AudioChannelSectionConfiguration.AudioChannelPairSectionConfiguration.LabelSpan = span;
			LiveUContactInfoSectionConfiguration.LabelSpan = span;
			AdditionalInformationSectionConfiguration.LabelSpan = span;
			TechnicalSpecificationSectionConfiguration.LabelSpan = span;
			EurovisionSectionConfiguration.LabelSpan = span;
			EurovisionSectionConfiguration.NewsEventSectionConfiguration.LabelSpan = span;
			EurovisionSectionConfiguration.ProgramEventSectionConfiguration.LabelSpan = span;
			EurovisionSectionConfiguration.SatelliteCapacitySectionConfiguration.LabelSpan = span;
			EurovisionSectionConfiguration.UnilateralTransmissionSectionConfiguration.LabelSpan = span;
			EurovisionSectionConfiguration.OsslateralTransmissionSectionConfiguration.LabelSpan = span;
			UploadSynopsisSectionConfiguration.LabelSpan = span;
			foreach (var functionConfiguration in FunctionSectionConfigurations.Values)
			{
				functionConfiguration.LabelSpan = span;
				foreach (var profileParameterConfiguration in functionConfiguration.ProfileParameterSectionConfigurations.Values)
				{
					profileParameterConfiguration.LabelSpan = span;
				}

				functionConfiguration.ResourceSectionConfiguration.LabelSpan = span;
			}
		}

		public void SetInputSpan(int span)
        {
			InputSpan = span;
			GeneralInfoSectionConfiguration.InputWidgetSpan = span;
			AudioChannelSectionConfiguration.InputWidgetSpan = span;
			RecordingConfigurationSectionConfiguration.InputWidgetSpan = span;
			AudioChannelSectionConfiguration.InputWidgetSpan = span;
			AudioChannelSectionConfiguration.AudioChannelPairSectionConfiguration.InputWidgetSpan = span;
			LiveUContactInfoSectionConfiguration.InputWidgetSpan = span;
			AdditionalInformationSectionConfiguration.InputWidgetSpan = span;
			TechnicalSpecificationSectionConfiguration.InputWidgetSpan = span;
			EurovisionSectionConfiguration.InputWidgetSpan = span;
			EurovisionSectionConfiguration.NewsEventSectionConfiguration.InputWidgetSpan = span;
			EurovisionSectionConfiguration.ProgramEventSectionConfiguration.InputWidgetSpan = span;
			EurovisionSectionConfiguration.SatelliteCapacitySectionConfiguration.InputWidgetSpan = span;
			EurovisionSectionConfiguration.UnilateralTransmissionSectionConfiguration.InputWidgetSpan = span;
			EurovisionSectionConfiguration.OsslateralTransmissionSectionConfiguration.InputWidgetSpan = span;
			UploadSynopsisSectionConfiguration.InputWidgetSpan = span;
			foreach (var functionConfiguration in FunctionSectionConfigurations.Values)
			{
				functionConfiguration.InputSpan = span;
				foreach (var profileParameterConfiguration in functionConfiguration.ProfileParameterSectionConfigurations.Values)
				{
					profileParameterConfiguration.InputWidgetSpan = span;
				}

				functionConfiguration.ResourceSectionConfiguration.InputWidgetSpan = span;
			}
		}

		public void SetInputColumn(int column)
        {
			InputColumn = column;
			GeneralInfoSectionConfiguration.InputWidgetColumn = column;
			AudioChannelSectionConfiguration.InputWidgetColumn = column;
			RecordingConfigurationSectionConfiguration.InputWidgetColumn = column;
			AudioChannelSectionConfiguration.InputWidgetColumn = column;
			AudioChannelSectionConfiguration.AudioChannelPairSectionConfiguration.InputWidgetColumn = column;
			LiveUContactInfoSectionConfiguration.InputWidgetColumn = column;
			AdditionalInformationSectionConfiguration.InputWidgetColumn = column;
			TechnicalSpecificationSectionConfiguration.InputWidgetColumn = column;
			EurovisionSectionConfiguration.InputWidgetColumn = column;
			EurovisionSectionConfiguration.NewsEventSectionConfiguration.InputWidgetColumn = column;
			EurovisionSectionConfiguration.ProgramEventSectionConfiguration.InputWidgetColumn = column;
			EurovisionSectionConfiguration.SatelliteCapacitySectionConfiguration.InputWidgetColumn = column;
			EurovisionSectionConfiguration.UnilateralTransmissionSectionConfiguration.InputWidgetColumn = column;
			EurovisionSectionConfiguration.OsslateralTransmissionSectionConfiguration.InputWidgetColumn = column;
			UploadSynopsisSectionConfiguration.InputWidgetColumn = column;
			foreach (var functionConfiguration in FunctionSectionConfigurations.Values)
			{
				functionConfiguration.InputColumn = column;
				foreach (var profileParameterConfiguration in functionConfiguration.ProfileParameterSectionConfigurations.Values)
				{
					profileParameterConfiguration.InputWidgetColumn = column;
				}

				functionConfiguration.ResourceSectionConfiguration.InputWidgetColumn = column;
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

		public void DisableForIntegration(IntegrationType integrationType, BackupType backupType) 
		{
			ReasonForBeingDisabled = $"Automatically generated by {integrationType.GetDescription()}";
            SetIsEnabledPropertyValues(false);

			PromoteToSharedSourceIsEnabled = backupType == BackupType.None;

			foreach (var functionSectionConfiguration in FunctionSectionConfigurations.Values)
			{
				// remote graphics and video format should always be enabled for integrations

				bool hasRemoteGraphics = functionSectionConfiguration.ProfileParameterSectionConfigurations.TryGetValue(ProfileParameterGuids.RemoteGraphics, out var remoteGraphicsSectionConfiguration);

				if (hasRemoteGraphics)
				{
					functionSectionConfiguration.IsEnabled = true;
					remoteGraphicsSectionConfiguration.IsEnabled = true;
				}

				bool hasVideoFormat = functionSectionConfiguration.ProfileParameterSectionConfigurations.TryGetValue(ProfileParameterGuids.VideoFormat, out var videoFormatSectionConfiguration);

				if (hasVideoFormat)
				{
					functionSectionConfiguration.IsEnabled = true;
					videoFormatSectionConfiguration.IsEnabled = true;
				}
			}

			// Audio configuration should always be enabled for integrations
			AudioChannelSectionConfiguration.SetIsEnabledPropertyValues(true);		
		}

		public void DisableAll(string reasonForDisabledState = null)
		{
			ReasonForBeingDisabled = String.IsNullOrEmpty(reasonForDisabledState) ? String.Empty : reasonForDisabledState;

			ConfigurationHelper.SetIsEnabledPropertyValues(this, false);
		}

		public void EnableAll(Service service)
		{
			ReasonForBeingDisabled = String.Empty;

			ConfigurationHelper.SetIsEnabledPropertyValues(this, true);
		}

		public void HideAllFunctions()
		{
			HideFunctions(FunctionSectionConfigurations.Keys.ToArray());
		}

		public void HideFunctions(params string[] functionLabels)
		{
			foreach (var functionLabel in functionLabels)
			{
				if(FunctionSectionConfigurations.TryGetValue(functionLabel, out var config))
				{
					config.SetIsVisiblePropertyValues(false);
				}
				else
				{
					throw new FunctionNotFoundException($"Unable to find function with label {functionLabel}");
				}
			}
		}

		public void DisableFunctionResourceSelection(string functionDefinitionLabel, string reasonForDisabledState)
		{
			var functionConfig = FunctionSectionConfigurations[functionDefinitionLabel];
			functionConfig.ResourceSectionConfiguration.ResourceSelectionEnabled = false;
			functionConfig.ResourceSectionConfiguration.ReasonForResourceSelectionBeingDisabled = reasonForDisabledState;
		}

		public void InitFunctionConfigurations(Helpers helpers, Service service, UserInfo userInfo)
		{
			foreach (var function in service.Functions)
			{
				FunctionSectionConfiguration functionConfig = InitializeFunctionConfiguration(helpers, service.Definition, userInfo, function, service.Id);

				FunctionSectionConfigurations.Add(function.Definition.Label, functionConfig);
			}
		}

		private static FunctionSectionConfiguration InitializeFunctionConfiguration(Helpers helpers,ServiceDefinition serviceDefinition, UserInfo userInfo, Function function, Guid serviceId)
		{
			var functionConfig = new FunctionSectionConfiguration(helpers, function, serviceDefinition, userInfo, serviceId);

			switch (function.Id.ToString())
			{
				case FunctionGuids.DummyString:
					functionConfig.IsVisible = false;
					break;
				case FunctionGuids.SatelliteString:
					functionConfig.ResourceSelectionPosition = Utils.InteractiveAutomationScript.HorizontalAlignment.Left;
					functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.Remove(ProfileParameterGuids.OtherSatelliteName);
					functionConfig.ProfileParametersInDisplayOrderBelowResourceSelection.AddLast(ProfileParameterGuids.OtherSatelliteName);
					break;
				case FunctionGuids.AntennaString:
					functionConfig.HideProfileParameter(ProfileParameterGuids.DownlinkFrequency);
					break;
				case FunctionGuids.RecordingString:
					functionConfig.HideProfileParameter(ProfileParameterGuids.RemoteGraphics);
					break;
				case FunctionGuids.DecodingString when serviceDefinition.VirtualPlatform == VirtualPlatform.ReceptionFixedService:
					functionConfig.IsVisible = false;
					break;
				case FunctionGuids.DecodingString:
				case FunctionGuids.GenericModulatingString:
					// Move Encryption key below Encryption type
					bool containsEncryptionKey = functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.Contains(ProfileParameterGuids.EncryptionKey);
					var containsEncryptionTypeNode = functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.Find(ProfileParameterGuids.EncryptionType);
					if (containsEncryptionKey && containsEncryptionTypeNode != null)
					{
						functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.Remove(ProfileParameterGuids.EncryptionKey);
						functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.AddAfter(containsEncryptionTypeNode, ProfileParameterGuids.EncryptionKey);
					}
					break;
				case FunctionGuids.DemodulatingString:
					//Move Roll-off below Polarization
					bool containsRollOff = functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.Contains(ProfileParameterGuids.RollOff);
					var containsPolarizationNode = functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.Find(ProfileParameterGuids.Polarization);
					if (containsRollOff && containsPolarizationNode != null)
					{
						functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.Remove(ProfileParameterGuids.RollOff);
						functionConfig.ProfileParametersInDisplayOrderAboveResourceSelection.AddAfter(containsPolarizationNode, ProfileParameterGuids.RollOff);
					}
					functionConfig.HideProfileParameter(ProfileParameterGuids.EncryptionType);
					functionConfig.HideProfileParameter(ProfileParameterGuids.EncryptionKey);
					break;
				default:
					// nothing
					break;
			}

			bool functionIsMatrix = FunctionGuids.AllMatrixGuids.Contains(function.Id);
			if (functionIsMatrix) functionConfig.HideAllProfileParameters(function);

			if (!ServiceDefinitionGuids.AllVizremServiceDefinitions.Contains(serviceDefinition.Id) && !userInfo.IsMcrUser && (!function.Parameters.Any() || !functionConfig.AllProfileParameterSections.Any(ppsc => ppsc.IsVisible)))
			{
				// Hide function sections for normal orders for non-MCR users if they don't have any (visible) profile parameters
				functionConfig.IsVisible = false;
			}

			if (helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.IsResourceChangeAction)
			{
				functionConfig.ResourceSelectionPosition = Utils.InteractiveAutomationScript.HorizontalAlignment.Left;
			}

			return functionConfig;
		}

		private void InitGeneralInfoSectionConfiguration(Service service, UserInfo userInfo, string eventOwnerCompany = null, IReadOnlyList<string> userCompanies = null)
		{
			bool serviceIsReception = service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception;
			bool serviceIsTransmission = service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission;

			GeneralInfoSectionConfiguration = new GeneralInfoSectionConfiguration(userInfo) { TimeZoneIsVisible = serviceIsReception || serviceIsTransmission };
			if (!String.IsNullOrWhiteSpace(eventOwnerCompany)) GeneralInfoSectionConfiguration.EventOwnerCompany = eventOwnerCompany;
			if (userCompanies != null)
			{
				GeneralInfoSectionConfiguration.UserCompanies.Clear();
				foreach (string userCompany in userCompanies) GeneralInfoSectionConfiguration.UserCompanies.Add(userCompany);
			}
		}

		private void InitAudioChannelSectionConfiguration(Service service, UserInfo userInfo)
		{
			bool serviceIsReception = service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception;
			AudioChannelSectionConfiguration = new AudioChannelConfigurationSectionConfiguration(service, userInfo)
			{
				AudioChannelPairSectionConfiguration = new AudioChannelPairSectionConfiguration 
				{ 
					DecodeDolbyECheckBoxIsVisible = serviceIsReception 
				} 
			};
		}

		private static void ApplyConfigurationFor_UpdateService_ResourceChange_SatelliteReception(Service service, ServiceSectionConfiguration configuration)
		{
			// special UI specs for SAT RX [DCP201965]

			var demodulatingFunction = service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.Demodulating) ?? throw new FunctionNotFoundException(FunctionGuids.Demodulating);
			var decodingFunction = service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.Decoding) ?? throw new FunctionNotFoundException(FunctionGuids.Decoding);

			bool demodulatingAndDecodingResourcesAreSameDevice = demodulatingFunction.Resource != null && decodingFunction.Resource != null && demodulatingFunction.Resource.MainDVEElementID == decodingFunction.Resource.MainDVEElementID;

			var matrixOutputLbandFunction = service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.MatrixOutputLband) as DisplayedFunction ?? throw new FunctionNotFoundException(FunctionGuids.MatrixOutputLband);

			List<Guid> functionToHideIds;

			if (demodulatingAndDecodingResourcesAreSameDevice)
			{
				// Consider the matrix output lband function to be demodulating and decoding combined
				// Hide all function sections except matrix output lband

				configuration.FunctionSectionConfigurations[matrixOutputLbandFunction.Definition.Label].DisplayedFunctionLabel = $"{demodulatingFunction.Name} & {decodingFunction.Name}";

				functionToHideIds = service.Functions.Except(matrixOutputLbandFunction.Yield()).Select(f => f.Id).ToList();
			}
			else
			{
				// Consider the matrix output lband function to be demodulating
				// Hide all function sections coming except matrix output lband and decoding

				configuration.FunctionSectionConfigurations[matrixOutputLbandFunction.Definition.Label].DisplayedFunctionLabel = demodulatingFunction.Name;

				functionToHideIds = new List<Guid> { FunctionGuids.Satellite, FunctionGuids.Antenna, FunctionGuids.MatrixInputLband, FunctionGuids.Demodulating, FunctionGuids.MatrixInputAsi, FunctionGuids.MatrixOutputAsi };
			}

			foreach (var function in service.Functions)
			{
				if (!functionToHideIds.Contains(function.Id)) continue;

				configuration.FunctionSectionConfigurations[function.Definition.Label].IsVisible = false;
			}
		}

	}
}