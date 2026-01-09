namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;

	public class CollectionChanges : Change
	{
		private readonly CollectionChangesSummary summary = new CollectionChangesSummary();

		[JsonConstructor]
		public CollectionChanges(string collectionName)
		{
			CollectionName = collectionName;
		}

		public CollectionChanges(string collectionName, CollectionChangeType collectionChangeType, string itemIdentifier, string shortDescription)
		{
			CollectionName = collectionName;

			Changes.Add(new CollectionChange
			{
				ItemIdentifier = itemIdentifier,
				Type = collectionChangeType,
				DisplayName = shortDescription
			});

			if (collectionChangeType == CollectionChangeType.Add)
			{
				summary.MarkItemsAdded();
			}
			else if(collectionChangeType == CollectionChangeType.Remove)
			{
				summary.MarkItemsRemoved();
			}
		}

		public string CollectionName { get; set; }

		public HashSet<CollectionChange> Changes { get; set; } = new HashSet<CollectionChange>();

		[JsonIgnore]
		public override ChangeSummary Summary => summary;

		public void AddCollectionChange(CollectionChange collectionChangeToAdd)
		{
			Changes.Add(collectionChangeToAdd);

			if (collectionChangeToAdd.Type == CollectionChangeType.Add)
			{
				summary.MarkItemsAdded();
			}
			else if (collectionChangeToAdd.Type == CollectionChangeType.Remove)
			{
				summary.MarkItemsRemoved();
			}
		}

		public override Change GetActualChanges()
		{
			return this;
		}

		public override bool TryAddChange(Change changeToAdd)
		{
			if (changeToAdd is CollectionChanges collectionChangesToAdd && collectionChangesToAdd.CollectionName == CollectionName)
			{
				Changes.UnionWith(collectionChangesToAdd.Changes);
				summary.TryAddChangeSummary(collectionChangesToAdd.summary);
				return true;
			}

			return false;
		}
	}
}
