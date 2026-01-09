namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DictionarySection<TKey, TValue> : Section
	{
		private readonly Label dictionaryNameLabel = new Label { Style = TextStyle.Heading };
		private readonly List<KeyValuePairSection<TKey, TValue>> keyValuePairSections = new List<KeyValuePairSection<TKey, TValue>>();
		private readonly Button addKeyValuePairButton = new Button("Add");

		public DictionarySection(string dictionaryName)
		{
			Initialize(dictionaryName);
			GenerateUi();
		}

		public Dictionary<TKey, TValue> Value
		{
			get => keyValuePairSections.ToDictionary(x => x.KeyValuePair.Key, x => x.KeyValuePair.Value);
			set
			{
				keyValuePairSections.Clear();

				foreach (var keyValuePair in value)
				{
					AddKeyValuePairSection(keyValuePair.Key, keyValuePair.Value, false);				
				}

				GenerateUi();
				RegenerateUi?.Invoke(this, EventArgs.Empty);
			}
		} 

		public event EventHandler RegenerateUi;

		private void Initialize(string dictionaryName)
		{
			dictionaryNameLabel.Text = dictionaryName;

			addKeyValuePairButton.Pressed += (o, e) => AddKeyValuePairSection();
		}

		private void AddKeyValuePairSection(TKey key = default(TKey), TValue value = default(TValue), bool regenerateUi = true)
		{
			var keyValuePairSection = new KeyValuePairSection<TKey, TValue>(key, value);

			keyValuePairSection.Deleted += KeyValuePairSection_Deleted;

			keyValuePairSections.Add(keyValuePairSection);

			if (regenerateUi)
			{
				GenerateUi();
				RegenerateUi?.Invoke(this, EventArgs.Empty);
			}
		}

		private void KeyValuePairSection_Deleted(object sender, EventArgs e)
		{
			keyValuePairSections.Remove(sender as KeyValuePairSection<TKey, TValue>);

			GenerateUi();
			RegenerateUi?.Invoke(this, EventArgs.Empty);
		}

		public void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(dictionaryNameLabel, ++row, 0);

			foreach (var keyValuePairSection in keyValuePairSections)
			{
				AddSection(keyValuePairSection, new SectionLayout(++row, 0));
				row += keyValuePairSection.RowCount;
			}

			AddWidget(addKeyValuePairButton, ++row, 0);
		}
	}
}
