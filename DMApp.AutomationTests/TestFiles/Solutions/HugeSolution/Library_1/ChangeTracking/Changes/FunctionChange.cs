namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class FunctionChange : Change
	{
		private readonly FunctionChangeSummary summary = new FunctionChangeSummary();

		public FunctionChange()
		{

		}

		public FunctionChange(Function function, FunctionResource initialResource, FunctionResource newResource)
		{
			FunctionLabel = function.Definition.Label;
			FunctionId = function.Id;

			bool resourceWasAdded = initialResource == null && newResource != null;
			bool resourceWasRemoved = initialResource != null && newResource == null;
			bool resourceWasChanged = initialResource != null && newResource != null && !initialResource.Equals(newResource);

			if (resourceWasAdded || resourceWasRemoved || resourceWasChanged)
			{
				var oldValue = string.IsNullOrWhiteSpace(initialResource?.Name) ? Constants.None : initialResource.Name;
				var newValue = string.IsNullOrWhiteSpace(newResource?.Name) ? Constants.None : newResource.Name;

				ResourceChange = new ValueChange(oldValue, newValue);
			}

			if (resourceWasAdded || resourceWasChanged) summary.ResourceChangeSummary.MarkResourceAddedOrSwapped();
			else if (resourceWasRemoved) summary.ResourceChangeSummary.MarkResourceRemoved();	
		}

		public string FunctionLabel { get; set; }

		public Guid FunctionId { get; set; }

		public HashSet<ProfileParameterChange> ProfileParameterChanges { get; set; } = new HashSet<ProfileParameterChange>();

		public ValueChange ResourceChange { get; set; } = new ValueChange();

		[JsonIgnore]
		public override ChangeSummary Summary => summary;

		public override Change GetActualChanges()
		{
			if (!Summary.IsChanged) return null;
			
			var change = new FunctionChange
			{
				FunctionId = FunctionId,
				FunctionLabel = FunctionLabel,
				ResourceChange = ResourceChange.NewValue != ResourceChange.OldValue ? ResourceChange : null,
			};

			change.TryAddChanges(ProfileParameterChanges.Select(c => c.GetActualChanges()).OfType<Change>().ToList());

			return change;
		}

		public override bool TryAddChange(Change changeToAdd)
		{
			if (changeToAdd is null) return false;

			summary.TryAddChangeSummary(changeToAdd.Summary);

			if (changeToAdd is ProfileParameterChange profileParameterChangeToAdd)
			{
				ProfileParameterChanges.Add(profileParameterChangeToAdd); 
				return true;
			}

			return false;
		}

		public void UpdateSummaryWithServiceInfo(ServiceDefinition serviceDefinition)
		{
			if (!Summary.IsChanged) return;

			if (serviceDefinition.FunctionIsFirst(FunctionLabel))
			{
				if (summary.ResourceChangeSummary.ResourcesAddedOrSwapped)
				{
					summary.ResourceChangeSummary.MarkResourceAtBeginningOfServiceDefinitionAddedOrSwapped();
				}
				else if(summary.ResourceChangeSummary.ResourcesRemoved)
				{
					summary.ResourceChangeSummary.MarkResourceAtBeginningOfServiceDefinitionRemoved();
				}
			}
			else if (serviceDefinition.FunctionIsLast(FunctionLabel))
			{
				if (summary.ResourceChangeSummary.ResourcesAddedOrSwapped)
				{
					summary.ResourceChangeSummary.MarkResourceAtEndOfServiceDefinitionAddedOrSwapped();
				}
				else if (summary.ResourceChangeSummary.ResourcesRemoved)
				{
					summary.ResourceChangeSummary.MarkResourceAtEndOfServiceDefinitionRemoved();

				}
			}
		}
	}
}
