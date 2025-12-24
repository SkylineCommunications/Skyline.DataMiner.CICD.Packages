namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public sealed class DisplayedFunction : Function
	{
		private bool includeUnavailableResources;
		private HashSet<FunctionResource> displayedResources = new HashSet<FunctionResource>();
		private HashSet<FunctionResource> selectableResources = new HashSet<FunctionResource>();

		private List<OccupyingService> occupyingServicesForCurrentResource = new List<OccupyingService>();
		private bool resourceSelectionMandatory;

		public DisplayedFunction(Helpers helpers, Net.ServiceManager.Objects.Node serviceDefinitionNode, FunctionDefinition functionDefinition) : base(helpers, serviceDefinitionNode, functionDefinition)
		{
			
		}

		public DisplayedFunction(Helpers helpers, ReservationInstance reservation, Net.ServiceManager.Objects.Node serviceDefinitionNode, FunctionDefinition functionDefinition) : base(helpers, reservation, serviceDefinitionNode, functionDefinition)
		{

		}

		public bool ResourceSelectionMandatory
		{
			get => resourceSelectionMandatory;
			set
			{
				resourceSelectionMandatory = value;
				ResourceSelectionMandatoryChanged?.Invoke(this, resourceSelectionMandatory);
			}
		}

		public event EventHandler<bool> ResourceSelectionMandatoryChanged;

		/// <summary>
		/// A collection of selectable Resources. Set by Controller and used by UI.
		/// </summary>
		public HashSet<FunctionResource> DisplayedResources
		{
			get => displayedResources;

			set
			{
				displayedResources = value;
				DisplayedResourcesChanged?.Invoke(this, new SelectableResourcesChangedEventArgs(DisplayedResourceNames));
			}
		}

		/// <summary>
		/// Property used by UI.
		/// </summary>
		public IEnumerable<string> DisplayedResourceNames
		{
			get
			{
				var displayedResourceNames = DisplayedResources.Select(r => r.Name).ToList();

				if (!ResourceSelectionMandatory) displayedResourceNames.Add(Constants.None);	
				
				return displayedResourceNames.Select(name => ResourceNameConverter.Invoke(name));
			}
		}

		public event EventHandler<SelectableResourcesChangedEventArgs> DisplayedResourcesChanged;

		/// <summary>
		/// A collection of selectable Resources. Set by Controller and used by UI.
		/// </summary>
		public HashSet<FunctionResource> SelectableResources
		{
			get => selectableResources;

			set
			{
				selectableResources = value;

				DisplayedResources = SelectableResources;
			}
		}

		public bool IncludeUnavailableResources
		{
			get => includeUnavailableResources;
			set
			{
				if (includeUnavailableResources == value) return;

				includeUnavailableResources = value;
				IncludeUnavailableResourcesChanged?.Invoke(this, includeUnavailableResources);
			}
		}

		public event EventHandler<bool> IncludeUnavailableResourcesChanged;

		public List<OccupyingService> OccupyingServicesForCurrentResource
		{
			get => occupyingServicesForCurrentResource;
			set
			{
				occupyingServicesForCurrentResource = value;
				OccupyingServicesForCurrentResourceChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler<EventArgs> OccupyingServicesForCurrentResourceChanged;
	}
}
