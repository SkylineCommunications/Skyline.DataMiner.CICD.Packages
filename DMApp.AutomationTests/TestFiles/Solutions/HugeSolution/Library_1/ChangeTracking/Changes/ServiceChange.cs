namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	public class ServiceChange : ClassChange
	{
		private readonly ServiceChangeSummary summary = new ServiceChangeSummary();

		[JsonConstructor]
		public ServiceChange(string serviceName, string serviceDisplayName, HashSet<FunctionChange> functionChanges = null) : base(nameof(Service))
		{
			ServiceName = serviceName;
			ServiceDisplayName = serviceDisplayName;
			FunctionChanges = functionChanges ?? new HashSet<FunctionChange>();
		}

		public string ServiceName { get; }

		public string ServiceDisplayName { get; }

		public HashSet<FunctionChange> FunctionChanges { get; }

		[JsonIgnore]
		public override ChangeSummary Summary => summary;

		public override Change GetActualChanges()
		{
			if(!Summary.IsChanged || summary.IsNew) return null;

			var change = new ServiceChange(ServiceName, ServiceDisplayName);

			change.TryAddChanges(FunctionChanges.Select(c => c.GetActualChanges()).OfType<Change>().ToList());
			change.TryAddChanges(CollectionChanges.Select(s => s.GetActualChanges()).OfType<Change>().ToList());
			change.TryAddChanges(ClassChanges.Select(s => s.GetActualChanges()).OfType<Change>().ToList());
			change.TryAddChanges(PropertyChanges.Select(s => s.GetActualChanges()).OfType<Change>().ToList());

			return change;
		}

		public ServiceChange GetChangeForCreationHistory()
		{
			var synopsisFilesChange = GetCollectionChanges(nameof(Service.SynopsisFiles));
			if (synopsisFilesChange is null) return null;

			if (synopsisFilesChange.Summary.IsChanged)
			{
				var change = new ServiceChange(ServiceName, ServiceDisplayName);

				change.TryAddChange(synopsisFilesChange);

				return change;
			}
			else
			{
				return null;
			}
		}

		public override bool TryAddChange(Change changeToAdd)
		{
			bool successful = false;

			if (changeToAdd is PropertyChange propertyChangeToAdd)
			{
				PropertyChanges.Add(propertyChangeToAdd);
				successful = true;
			}
			else if (changeToAdd is ClassChange classChangeToAdd)
			{
				ClassChanges.Add(classChangeToAdd);
				successful = true;
			}
			else if (changeToAdd is CollectionChanges collectionChangesToAdd)
			{
				var existingCollectionChange = CollectionChanges.SingleOrDefault(c => c.CollectionName == collectionChangesToAdd.CollectionName);

				if (existingCollectionChange is null)
				{
					CollectionChanges.Add(collectionChangesToAdd);
				}
				else
				{
					existingCollectionChange.TryAddChange(collectionChangesToAdd);
				}

				successful = true;
			}
			else if (changeToAdd is FunctionChange functionChangeToAdd)
			{
				FunctionChanges.Add(functionChangeToAdd);
				successful = true;
			}

			UpdateSummary(changeToAdd);

			return successful;
		}

		public void UpdateSummaryWithServiceInfo(Service service)
		{
			summary.IsNew = !service.IsBooked;

			summary.IsMissingResources = service.IsMissingResources(null);

			foreach (var functionChange in FunctionChanges)
			{
				functionChange.UpdateSummaryWithServiceInfo(service.Definition);
				summary.TryAddChangeSummary(functionChange.Summary);
			}
		}

		private void UpdateSummary(Change changeToAdd)
		{
			if (!changeToAdd.Summary.IsChanged) return;

			summary.TryAddChangeSummary(changeToAdd.Summary);

			if (changeToAdd is PropertyChange propertyChangeToAdd)
			{
				switch (propertyChangeToAdd.PropertyName)
				{
					case nameof(Service.PreRoll):
						summary.TimingChangeSummary.MarkPrerollChanged();
						break;
					case nameof(Service.Start):
						summary.TimingChangeSummary.MarkStartTimingChanged();
						break;
					case nameof(Service.End):
						summary.TimingChangeSummary.MarkEndTimingChanged();
						break;
					case nameof(Service.PostRoll):
						summary.TimingChangeSummary.MarkPostrollChanged();
						break;
					case nameof(Service.RecordingConfiguration):
						summary.PropertyChangeSummary.RecordingConfigurationChanged = true;
						break;
					default:
						break;
				}
			}
			else if (changeToAdd is CollectionChanges collectionChangeToAdd)
			{
				if (collectionChangeToAdd.CollectionName == nameof(Service.Functions))
				{
					summary.ServiceDefinitionChanged = true;
				}
				else if(collectionChangeToAdd.CollectionName == nameof(Service.SecurityViewIds))
				{
					summary.SecurityViewIdsHaveChanged = true;
				}
				else
				{
					summary.PropertyChangeSummary.MarkIsChanged();
				}
			}
			else if(changeToAdd is ClassChange classChangeToAdd && classChangeToAdd.ClassName == nameof(Service.RecordingConfiguration))
			{
				summary.PropertyChangeSummary.RecordingConfigurationChanged = true;
			}
			else if(changeToAdd is FunctionChange)
			{
				// no actions (?)
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
