namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
    using System.Collections.ObjectModel;

    public sealed class DisplayedService : Service
	{
		private ValidationInfo startValidation;
		private ValidationInfo availableVirtualPlatformNamesValidation;
		private Dictionary<string, int> selectableSecurityViewIds;
		private List<string> validationMessages = new List<string>();

		public DisplayedService()
		{

		}

		public DisplayedService(string name) : base(name)
		{

		}

		public DisplayedService(Helpers helpers, int nodeId, string nodeLabel, ServiceConfiguration configuration) : base(helpers, nodeId, nodeLabel, configuration)
		{

		}

		public DisplayedService(Helpers helpers, ServiceDefinition serviceDefinition) : this()
        {
			Definition = serviceDefinition ?? throw new ArgumentNullException(nameof(serviceDefinition));

            IsEurovisionService = Definition.VirtualPlatformServiceName == VirtualPlatformName.Eurovision;
            IsUnknownSourceService = Definition.VirtualPlatformServiceName == VirtualPlatformName.Unknown;
			Functions = helpers.ServiceManager.GetDefaultFunctions(serviceDefinition);
			AudioChannelConfiguration = InitializeAudioChannelConfiguration(Functions, serviceDefinition.VirtualPlatformServiceType == VirtualPlatformType.Reception);
			RecordingConfiguration = new RecordingConfiguration();
        }

		public bool IsDisplayed { get; set; } // currently only set and used in UpdateService script

		/// <summary>
		/// Property set by controller and used by UI for validation.
		/// </summary>
		public ValidationInfo StartValidation
		{
			get => startValidation ?? (startValidation = new ValidationInfo());
			set => startValidation = value;
		}

		/// <summary>
		/// Gets a collection of DataMiner Cube view IDs of views that are selected in the event of which this service is part of. Used by UI.
		/// </summary>
		public Dictionary<string, int> SelectableSecurityViewIds
		{
			get => selectableSecurityViewIds;
			set
			{
				selectableSecurityViewIds = value;
				SelectableSecurityViewIdsChanged?.Invoke(this, SelectableSecurityViewIds);
			}
		}

		public event EventHandler<Dictionary<string, int>> SelectableSecurityViewIdsChanged;

		public List<string> ValidationMessages
		{
			get => validationMessages;
			set
			{
				validationMessages = value;
				ValidationMessagesChanged?.Invoke(this, ValidationMessages);
			}
		}

		public event EventHandler<List<string>> ValidationMessagesChanged;

		public ObservableCollection<string> AvailableVirtualPlatformNames { get; private set; } = new ObservableCollection<string>();

		/// <summary>
		/// Property set by controller and used by UI for validation.
		/// </summary>
		public ValidationInfo AvailableVirtualPlatformNamesValidation
		{
			get => availableVirtualPlatformNamesValidation ?? (availableVirtualPlatformNamesValidation = new ValidationInfo());
			set
			{
				availableVirtualPlatformNamesValidation = value;
				AvailableVirtualPlatformNamesValidationChanged?.Invoke(this, availableVirtualPlatformNamesValidation);
			}
		}

		public event EventHandler<ValidationInfo> AvailableVirtualPlatformNamesValidationChanged;

		public void SetAvailableVirtualPlatformNames(IEnumerable<string> virtualPlatformNames)
		{
			AvailableVirtualPlatformNames.Clear();
			foreach (string virtualPlatformName in virtualPlatformNames.Distinct())
			{
				AvailableVirtualPlatformNames.Add(virtualPlatformName);
			}
		}

		public ObservableCollection<string> AvailableServiceDescriptions { get; private set; } = new ObservableCollection<string>();

		public void SetAvailableServiceDescriptions(IEnumerable<string> serviceDefinitionDescriptions)
        {
			AvailableServiceDescriptions.Clear();
			foreach (string serviceDefinitionDescription in serviceDefinitionDescriptions.Distinct())
            {
				AvailableServiceDescriptions.Add(serviceDefinitionDescription);
			}
        }
	}
}
