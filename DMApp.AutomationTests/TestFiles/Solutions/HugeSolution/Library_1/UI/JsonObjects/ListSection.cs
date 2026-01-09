namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.JsonObjects
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ListSection : Section
	{
		private Type typeOfListItems;
		private readonly Label nameLabel = new Label { Style = TextStyle.Heading };
		private readonly List<Section> sectionsToConvertToListItems = new List<Section>();
		private readonly List<ListItemSection> listItemSections = new List<ListItemSection>();
		private readonly YleButton addItemButton = new YleButton("Add");

		public ListSection(object listObject, string listName)
		{
			List = listObject as IList ?? throw new ArgumentException($"Type {listObject.GetType().Name} does not implement {nameof(IEnumerable)} interface", nameof(listObject));
			this.typeOfListItems = List.GetType().GetGenericArguments()[0];

			Initialize(listName);
			GenerateUi();
		}

		public IList List { get; }

		public event EventHandler RegenerateUi;

		public void UpdateListWithUiValues()
		{
			foreach (var section in sectionsToConvertToListItems)
			{
				if (section is LabelAndInputSection labelAndInputSection)
				{
					List.Add(labelAndInputSection.InputValue);
				}
				else if(section is JsonObjectSection jsonObjectSection)
				{
					jsonObjectSection.UpdateJsonObjectWithUiValues();
					List.Add(jsonObjectSection.JsonObject);
				}
			}
		}

		private void AddListItemSection()
		{
			Section sectionToAdd = null;
			if (Mapping.TypeToWidget.ContainsKey(typeOfListItems))
			{
				string labelvalue = $"Item {listItemSections.Count + 1}";

				sectionToAdd = new LabelAndInputSection(labelvalue, typeOfListItems);
			}
			else
			{
				var listItem = Activator.CreateInstance(typeOfListItems);

				var jsonObjectSection = new JsonObjectSection(listItem);

				jsonObjectSection.RegenerateUi += JsonObjectSection_RegenerateUi;

				sectionToAdd = jsonObjectSection;
			}

			sectionsToConvertToListItems.Add(sectionToAdd);

			InvokeRegenerateUi();
		}

		private void JsonObjectSection_RegenerateUi(object sender, EventArgs e)
		{
			InvokeRegenerateUi();
		}

		private void InvokeRegenerateUi()
		{
			listItemSections.Clear();

			foreach (var sectionToConvert in sectionsToConvertToListItems)
			{
				var listItemSection = new ListItemSection(sectionToConvert);

				listItemSection.Deleted += ListItemSection_Deleted;

				listItemSections.Add(listItemSection);
			}

			GenerateUi();
			RegenerateUi?.Invoke(this, EventArgs.Empty);
		}

		private void Initialize(string listName)
		{
			nameLabel.Text = listName;

			addItemButton.Name = $"Add to {listName}";

			addItemButton.Pressed += (o, e) => AddListItemSection();
		}

		private void ListItemSection_Deleted(object sender, Section deletedSection)
		{
			sectionsToConvertToListItems.Remove(deletedSection);

			InvokeRegenerateUi();
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

			AddWidget(addItemButton, ++row, 1);
		}
	}

}
