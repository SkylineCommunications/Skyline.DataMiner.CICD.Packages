using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.History
{
	public class PropertyChangeSection : Section
	{
		private Label nameLabel;
		private Label oldValueLabel;
		private Label newValueLabel;

		public PropertyChangeSection(string propertyName, string oldValue, string newValue)
		{
			Initialize(propertyName, oldValue, newValue);
			GenerateUi();
		}

		public PropertyChangeSection(string propertyName, ValueChange change) : this(propertyName, change.OldValue, change.NewValue)
		{

		}

		public PropertyChangeSection(PropertyChange propertyChange) : this(propertyChange.PropertyName, propertyChange.Change)
		{

		}

		public static PropertyChangeSection FromCollectionChange(CollectionChanges collectionChange, string collectionNamePrefix = null)
		{
			string collectionName = collectionNamePrefix is null ? collectionChange.CollectionName : $"{collectionNamePrefix} {collectionChange.CollectionName}";

			var removedItems = collectionChange.Changes.Where(cc => cc.Type == CollectionChangeType.Remove).Select(cc => cc.DisplayName ?? cc.ItemIdentifier).ToList();
			var addedItems = collectionChange.Changes.Where(cc => cc.Type == CollectionChangeType.Add).Select(cc => cc.DisplayName ?? cc.ItemIdentifier).ToList();

			var newValue = new StringBuilder();
			foreach (var item in removedItems)
			{
				newValue.Append($"Removed {item}.\n");
			}

			foreach (var item in addedItems)
			{
				newValue.Append($"Added {item}.\n");
			}

			return new PropertyChangeSection(collectionName, string.Empty, newValue.ToString().Trim('\n'));
		}

		private void Initialize(string propertyName, string oldValue, string newValue)
		{
			nameLabel = new Label(propertyName.SplitCamelCase().Replace("  ", " "));
			oldValueLabel = new Label(oldValue);
			newValueLabel = new Label(newValue);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(nameLabel, ++row, 0);
			AddWidget(oldValueLabel, row, 1);
			AddWidget(newValueLabel, row, 2);
		}
	}
}
