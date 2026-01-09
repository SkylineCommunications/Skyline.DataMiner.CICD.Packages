namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using System.CodeDom;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class Mapping
	{
		public static readonly IReadOnlyDictionary<Type, Func<IYleInteractiveWidget>> TypeToWidget = new Dictionary<Type, Func<IYleInteractiveWidget>>
		{
			{ typeof(string), () => new YleTextBox() },
			{ typeof(int), () => new YleNumeric(0) {Decimals = 0, StepSize = 1 } },
			{ typeof(double), () => new YleNumeric(0)},
			{ typeof(bool), () => new YleCheckBox(String.Empty) },
			{ typeof(DateTime), () => new YleDateTimePicker() },
		};

		public static readonly IReadOnlyDictionary<Type, Func<object, IYleInteractiveWidget>> TypeToWidgetV2 = new Dictionary<Type, Func<object, IYleInteractiveWidget>>
		{
			{ typeof(string), (value) => new YleTextBox((string)value) },
			{ typeof(int), (value) => new YleNumeric((int)value) {Decimals = 0, StepSize = 1 } },
			{ typeof(double), (value) => new YleNumeric((double)value)},
			{ typeof(bool), (value) => new YleCheckBox(String.Empty){ IsChecked = (bool)value } },
			{ typeof(DateTime), (value) => new YleDateTimePicker((DateTime)value) },
			{ typeof(Enum), (value) => new YleDropDown(EnumExtensions.GetEnumDescriptions(value), ((Enum)value).GetDescription()) },
		};
	}
}
