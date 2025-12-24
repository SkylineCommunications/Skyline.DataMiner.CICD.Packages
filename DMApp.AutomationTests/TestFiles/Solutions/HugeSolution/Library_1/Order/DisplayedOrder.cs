namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ExternalSources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class DisplayedOrder : DisplayedObject
	{
		private ValidationInfo availableSharedSourcesValidation;

		protected DisplayedOrder()
		{

		}

		private IReadOnlyDictionary<string, int> selectableSecurityViewIds = new Dictionary<string, int>();

		/// <summary>
		/// Gets a collection of DataMiner Cube view IDs of views that are selected in the event of which this service is part of. Used by UI.
		/// </summary>
		public IReadOnlyDictionary<string, int> SelectableSecurityViewIds
		{
			get => selectableSecurityViewIds;
			set
			{
				selectableSecurityViewIds = value;
				SelectableSecurityViewIdsChanged?.Invoke(this, SelectableSecurityViewIds);
			}
		}

		public event EventHandler<IReadOnlyDictionary<string, int>> SelectableSecurityViewIdsChanged;

		/// <summary>
		/// Used in UI.
		/// </summary>
		public ObservableCollection<Contracts.UserGroup> SelectableUserGroups { get; private set; } = new ObservableCollection<Contracts.UserGroup>();

		public ObservableCollection<string> SelectableCompanies { get; private set; } = new ObservableCollection<string>();

		public ObservableCollection<ExternalSourceInfo> AvailableSharedSources { get; private set; } = new ObservableCollection<ExternalSourceInfo>();

		public void SetAvailableSharedSources(IEnumerable<ExternalSourceInfo> sharedSourceServices)
		{
			AvailableSharedSources.Clear();
			foreach (var sharedSourceService in sharedSourceServices) AvailableSharedSources.Add(sharedSourceService);
		}

		public ObservableCollection<ExternalSourceInfo> UnavailableSharedSources { get; private set; } = new ObservableCollection<ExternalSourceInfo>();

		public void SetUnavailableSharedSources(IEnumerable<ExternalSourceInfo> unavailableSharedSources)
		{
			UnavailableSharedSources.Clear();
			foreach (var sharedSourceService in unavailableSharedSources) UnavailableSharedSources.Add(sharedSourceService);
		}

		public ValidationInfo AvailableSharedSourcesValidation
		{
			get => availableSharedSourcesValidation ?? (availableSharedSourcesValidation = new ValidationInfo());
			set
			{
				availableSharedSourcesValidation = value;
				AvailableSharedSourcesValidationChanged?.Invoke(this, availableSharedSourcesValidation);
			}
		}

		public event EventHandler<ValidationInfo> AvailableSharedSourcesValidationChanged;

		private IReadOnlyList<string> availableSourceServices = new List<string>();

		public event EventHandler<IReadOnlyList<string>> AvailableSourceServicesChanged;

		public IReadOnlyList<string> AvailableSourceServices
		{
			get => availableSourceServices;
			set
			{
				availableSourceServices = value;
				AvailableSourceServicesChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<string> availableSourceServiceDescriptions = new List<string>();

		public event EventHandler<IReadOnlyList<string>> AvailableSourceServiceDescriptionsChanged;

		public IReadOnlyList<string> AvailableSourceServiceDescriptions
		{
			get => availableSourceServiceDescriptions;
			set
			{
				availableSourceServiceDescriptions = value;
				AvailableSourceServiceDescriptionsChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<string> availableBackupSourceServices = new List<string>();

		public event EventHandler<IReadOnlyList<string>> AvailableBackupSourceServicesChanged;

		public IReadOnlyList<string> AvailableBackupSourceServices
		{
			get => availableBackupSourceServices;
			set
			{
				availableBackupSourceServices = value;
				AvailableBackupSourceServicesChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<string> availableBackupSourceServiceDescriptions = new List<string>();

		public event EventHandler<IReadOnlyList<string>> AvailableBackupSourceServiceDescriptionsChanged;

		public IReadOnlyList<string> AvailableBackupSourceServiceDescriptions
		{
			get => availableBackupSourceServiceDescriptions;
			set
			{
				availableBackupSourceServiceDescriptions = value;
				AvailableBackupSourceServiceDescriptionsChanged?.Invoke(this, value);
			}
		}

		public void SetSelectableUserGroups(IEnumerable<Contracts.UserGroup> userGroups)
		{
			SelectableUserGroups.Clear();
			foreach (Contracts.UserGroup userGroup in userGroups.Distinct())
			{
				SelectableUserGroups.Add(userGroup);
			}
		}

		public void SetSelectableCompanies(IEnumerable<string> companies)
		{
			SelectableCompanies.Clear();
			foreach (string company in companies.Distinct())
			{
				SelectableCompanies.Add(company);
			}
		}
	}
}
