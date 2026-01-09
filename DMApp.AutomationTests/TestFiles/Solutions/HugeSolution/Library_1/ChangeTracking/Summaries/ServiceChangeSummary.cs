namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;

	public class ServiceChangeSummary : ChangeSummary
	{
		public override bool IsChanged
		{
			get
			{
				bool changed = false;
				changed |= IsNew;
				changed |= IsMissingResources;
				changed |= PropertyChangeSummary.IsChanged;
				changed |= TimingChangeSummary.IsChanged;
				changed |= FunctionChangeSummary.IsChanged;
				changed |= CollectionChangesSummary.IsChanged;
				changed |= SecurityViewIdsHaveChanged;
				return changed;
			}
		}

		public bool IsNew { get; set; }

		public bool IsMissingResources { get; set; }

		public bool SecurityViewIdsHaveChanged { get; set; }

		public bool ServiceDefinitionChanged { get; set; }

		public TimingChangeSummary TimingChangeSummary { get; } = new TimingChangeSummary();

		public PropertyChangeSummary PropertyChangeSummary { get; } = new PropertyChangeSummary();

		public FunctionChangeSummary FunctionChangeSummary { get; } = new FunctionChangeSummary();

		public CollectionChangesSummary CollectionChangesSummary { get; } = new CollectionChangesSummary();

		public bool OnlyResourcesHaveChanged 
		{
			get
			{
				bool changed = false;
				changed |= IsNew;
				changed |= PropertyChangeSummary.IsChanged;
				changed |= TimingChangeSummary.IsChanged;
				changed |= SecurityViewIdsHaveChanged;
				changed |= FunctionChangeSummary.ProfileParameterChangeSummary.IsChanged;

				return FunctionChangeSummary.ResourceChangeSummary.IsChanged && !changed;
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		public override bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd)
		{
			if (changeSummaryToAdd is FunctionChangeSummary functionChangeSummaryToAdd)
			{
				FunctionChangeSummary.TryAddChangeSummary(functionChangeSummaryToAdd);
				return true;
			}
			else if(changeSummaryToAdd is PropertyChangeSummary propertyChangeSummaryToAdd)
			{
				PropertyChangeSummary.TryAddChangeSummary(propertyChangeSummaryToAdd);
				return true;
			}
			else if(changeSummaryToAdd is TimingChangeSummary timingChangeSummaryToAdd)
			{
				TimingChangeSummary.TryAddChangeSummary(timingChangeSummaryToAdd);
				return true;
			}
			else if(changeSummaryToAdd is ServiceChangeSummary serviceChangeSummaryToAdd)
			{
				IsNew |= serviceChangeSummaryToAdd.IsNew;
				IsMissingResources |= serviceChangeSummaryToAdd.IsMissingResources;
				SecurityViewIdsHaveChanged |= serviceChangeSummaryToAdd.SecurityViewIdsHaveChanged;
				ServiceDefinitionChanged |= serviceChangeSummaryToAdd.ServiceDefinitionChanged;

				FunctionChangeSummary.TryAddChangeSummary(serviceChangeSummaryToAdd.FunctionChangeSummary);
				PropertyChangeSummary.TryAddChangeSummary(serviceChangeSummaryToAdd.PropertyChangeSummary);
				TimingChangeSummary.TryAddChangeSummary(serviceChangeSummaryToAdd.TimingChangeSummary);
				CollectionChangesSummary.TryAddChangeSummary(serviceChangeSummaryToAdd.CollectionChangesSummary);
			}
			else if(changeSummaryToAdd is CollectionChangesSummary collectionChangesSummaryToAdd)
			{
				CollectionChangesSummary.TryAddChangeSummary(collectionChangesSummaryToAdd);
			}

			return false;
		}
	}
}
