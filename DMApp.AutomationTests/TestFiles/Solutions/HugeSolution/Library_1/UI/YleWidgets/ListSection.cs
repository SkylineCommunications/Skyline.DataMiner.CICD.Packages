namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ListSection<T> : Section
	{
		private readonly Label nameLabel = new Label { Style = TextStyle.Heading };
		private readonly List<ListItemSection<T>> listItemSections = new List<ListItemSection<T>>();
		private readonly Button addItemButton = new Button("Add");

		public ListSection(string name)
		{
			Initialize(name);
			GenerateUi();
		}

		public List<T> GetValue()
		{
			return listItemSections.Select(s => s.Value).ToList();
		}

		public void SetValue(List<T> value)
		{
			listItemSections.Clear();

			foreach (var listItem in value)
			{
				AddListItemSection(listItem, false);
			}

			GenerateUi();
			RegenerateUi?.Invoke(this, EventArgs.Empty);
		}

		private void AddListItemSection(T listItem = default(T), bool regenerateUi = true)
		{
			var listItemsection = new ListItemSection<T>(listItem);

			listItemsection.Deleted += ListItemSection_Deleted;

			listItemSections.Add(listItemsection);

			if (regenerateUi)
			{
				GenerateUi();
				RegenerateUi?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler RegenerateUi;

		private void Initialize(string name)
		{
			nameLabel.Text = name;

			addItemButton.Pressed += (o, e) => AddListItemSection();
		}

		private void ListItemSection_Deleted(object sender, EventArgs e)
		{
			listItemSections.Remove(sender as ListItemSection<T>);

			GenerateUi();
			RegenerateUi?.Invoke(this, EventArgs.Empty);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(nameLabel, ++row, 0);

			foreach (var listItemSection in listItemSections)
			{
				AddSection(listItemSection, new SectionLayout(++row, 0));
				row += listItemSection.RowCount;
			}

			AddWidget(addItemButton, ++row, 0);
		}
	}
}
