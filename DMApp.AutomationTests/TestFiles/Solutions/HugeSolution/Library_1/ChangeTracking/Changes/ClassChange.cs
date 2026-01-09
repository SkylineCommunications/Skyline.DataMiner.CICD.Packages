namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;

	public class ClassChange : Change
	{
		private readonly PropertyChangeSummary summary = new PropertyChangeSummary();

		public ClassChange(string className)
		{
			ClassName = className;
		}

		public string ClassName { get; set; }

		public HashSet<ClassChange> ClassChanges { get; set; } = new HashSet<ClassChange>();

		public HashSet<PropertyChange> PropertyChanges { get; set; } = new HashSet<PropertyChange>();
		
		public HashSet<CollectionChanges> CollectionChanges { get; set; } = new HashSet<CollectionChanges>();

		[JsonIgnore]
		public override ChangeSummary Summary => summary;

		public CollectionChanges GetCollectionChanges(string collectionName)
		{
			return CollectionChanges.SingleOrDefault(cc => cc.CollectionName == collectionName);
		}

		public PropertyChange GetPropertyChange(string propertyName)
		{
			return PropertyChanges.SingleOrDefault(cc => cc.PropertyName == propertyName);
		}

		public override Change GetActualChanges()
		{
			if (!Summary.IsChanged) return null;

			var change = new ClassChange(ClassName);

			change.TryAddChanges(ClassChanges.Select(c => c.GetActualChanges()).OfType<Change>().ToList());
			change.TryAddChanges(PropertyChanges.Select(c => c.GetActualChanges()).OfType<Change>().ToList());
			change.TryAddChanges(CollectionChanges.Select(c => c.GetActualChanges()).OfType<Change>().ToList());

			return change;
		}

		public override bool TryAddChange(Change changeToAdd)
		{
			summary.TryAddChangeSummary(changeToAdd.Summary);

			if (changeToAdd is PropertyChange propertyChangeToAdd)
			{
				PropertyChanges.Add(propertyChangeToAdd);
				return true;
			}
			else if(changeToAdd is ClassChange classChangeToAdd)
			{
				ClassChanges.Add(classChangeToAdd);
				return true;
			}
			else if(changeToAdd is CollectionChanges collectionChangesToAdd)
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

				return true;
			}

			return false;
		}

		public bool PropertyHasChange(string property)
		{
			var propertyChange = PropertyChanges.SingleOrDefault(x => x.PropertyName.Equals(property));

			if (propertyChange == null)
			{
				return false;
			}

			return propertyChange.Summary.IsChanged;
		}
	}
}
