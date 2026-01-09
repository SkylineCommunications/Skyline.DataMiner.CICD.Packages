namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.JsonObjects
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

	public class JsonObjectSection : Section
	{
		private Label headerLabel;

		private readonly List<LabelAndInputSection> labelAndInputSections = new List<LabelAndInputSection>();

		private readonly List<Section> sections = new List<Section>();

		public JsonObjectSection(object jsonObject)
		{
			JsonObject = jsonObject;

			Initialize();
			GenerateUi();
		}

		public object JsonObject { get; }

		public event EventHandler RegenerateUi;

		public void UpdateJsonObjectWithUiValues()
		{
			var properties = JsonObject.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

			foreach (var labelAndInputSection in labelAndInputSections)
			{
				var property = properties.SingleOrDefault(p => ((JsonPropertyAttribute)p.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault())?.PropertyName == labelAndInputSection.LabelValue) ?? throw new InvalidOperationException($"Unknown section with label text '{labelAndInputSection.LabelValue}'");

				object valueToSet;

				if (property.PropertyType == typeof(int))
				{
					// numeric widget returns double so we need to convert specifically here
					valueToSet = Convert.ToInt32(labelAndInputSection.InputValue);
				}
				else
				{
					valueToSet = labelAndInputSection.InputValue;
				}

				property.SetValue(JsonObject, valueToSet);
			}

			foreach (var listSection in sections.OfType<ListSection>())
			{
				listSection.UpdateListWithUiValues();
			}

			foreach (var jsonSubObjectSection in sections.OfType<JsonObjectSection>())
			{
				jsonSubObjectSection.UpdateJsonObjectWithUiValues();
			}
		}

		private void Initialize()
		{
			headerLabel = new Label(JsonObject.GetType().Name) { Style = TextStyle.Heading };
			
			var properties = JsonObject.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

			foreach (var property in properties)
			{
				var jsonPropertyAttribute = (JsonPropertyAttribute)property.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault();
				if (jsonPropertyAttribute is null) continue;

				var jsonPropertyType = property.PropertyType;
				if (Mapping.TypeToWidget.ContainsKey(jsonPropertyType))
				{
					string labelvalue = jsonPropertyAttribute.PropertyName;

					labelAndInputSections.Add(new LabelAndInputSection(labelvalue, jsonPropertyType));
				}
				else if (typeof(IEnumerable).IsAssignableFrom(jsonPropertyType))
				{
					var listObject = property.GetValue(JsonObject) ?? Activator.CreateInstance(jsonPropertyType);

					property.SetValue(JsonObject, listObject);

					var listSection = new ListSection(listObject, $"Collection of {jsonPropertyAttribute.PropertyName}");

					listSection.RegenerateUi += InvokeRegenerateUi;

					sections.Add(listSection);
				}
				else
				{
					var jsonSubObject = property.GetValue(JsonObject) ?? Activator.CreateInstance(jsonPropertyType);

					property.SetValue(JsonObject, jsonSubObject);

					var jsonSubObjectSection = new JsonObjectSection(jsonSubObject);

					jsonSubObjectSection.RegenerateUi += InvokeRegenerateUi;

					sections.Add(jsonSubObjectSection);
				}
			}
		}

		private void InvokeRegenerateUi(object sender, EventArgs e)
		{
			GenerateUi();
			RegenerateUi?.Invoke(this, EventArgs.Empty);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(headerLabel, ++row, 0, 1, 5);

			foreach (var labelAndInputSection in labelAndInputSections.OrderBy(x => x.LabelValue))
			{
				AddSection(labelAndInputSection, new SectionLayout(++row, 0));
				row += labelAndInputSection.RowCount;
			}

			foreach (var section in sections)
			{
				AddSection(section, new SectionLayout(++row, 0));
				row += section.RowCount;
			}
		}
	}
}
