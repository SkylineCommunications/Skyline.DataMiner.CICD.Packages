using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.History
{
	public class ClassChangeSection : Section
	{
		private readonly Label propertyChangeName = new Label("[Property]") { Style = TextStyle.Heading };
		private readonly Label propertyChangeOldValue = new Label("[Old Value]") { Style = TextStyle.Heading };
		private readonly Label propertyChangeNewValue = new Label("[New Value]") { Style = TextStyle.Heading };

		private readonly ClassChangeSectionConfiguration configuration;

		private List<PropertyChangeSection> propertyChangeSections = new List<PropertyChangeSection>();

		public ClassChangeSection(ClassChange classChange, ClassChangeSectionConfiguration configuration)
		{
			this.configuration = configuration;

			Initialize(classChange);
			GenerateUi();
		}

		private void Initialize(ClassChange classChange)
		{
			propertyChangeSections.AddRange(classChange.PropertyChanges.Where(pc => !configuration.PropertyShouldBeHidden(pc.PropertyName)).Select(pc => new PropertyChangeSection(pc)).ToList());
			propertyChangeSections.AddRange(classChange.CollectionChanges.Where(cc => !configuration.PropertyShouldBeHidden(cc.CollectionName)).Select(cc => PropertyChangeSection.FromCollectionChange(cc)).ToList());
			propertyChangeSections.AddRange(classChange.ClassChanges.Where(cc => !configuration.PropertyShouldBeHidden(cc.ClassName)).SelectMany(cc => GetPropertyChangeSectionFromClassChange(cc)));
		}

		private List<PropertyChangeSection> GetPropertyChangeSectionFromClassChange(ClassChange classChange, string higherClassName = null)
		{
			var propertyChangeSections = new List<PropertyChangeSection>();

			string className = higherClassName is null ? classChange.ClassName : $"{higherClassName} {classChange.ClassName}";

			foreach (var propertyChange in classChange.PropertyChanges)
			{
				propertyChangeSections.Add(new PropertyChangeSection($"{className} {propertyChange.PropertyName}", propertyChange.Change));
			}

			foreach (var collectionChange in classChange.CollectionChanges)
			{
				propertyChangeSections.Add(PropertyChangeSection.FromCollectionChange(collectionChange, className));
			}

			foreach (var deeperClassChange in classChange.ClassChanges)
			{
				propertyChangeSections.AddRange(GetPropertyChangeSectionFromClassChange(deeperClassChange, className));
			}

			return propertyChangeSections;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			if (!propertyChangeSections.Any())
			{
				AddWidget(new Label("No changes relevant to the user were made."), ++row, 0);
				return;
			}
			
			AddWidget(propertyChangeName, ++row, 0);
			AddWidget(propertyChangeOldValue, row, 1);
			AddWidget(propertyChangeNewValue, row, 2);

			foreach (var propertyChangeSection in propertyChangeSections)
			{
				AddSection(propertyChangeSection, new SectionLayout(++row, 0));
				row += propertyChangeSection.RowCount;
			}
		}
	}
}
