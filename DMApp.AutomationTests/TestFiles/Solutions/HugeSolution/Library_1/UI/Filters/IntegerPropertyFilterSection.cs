namespace Library.UI.Filters
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class IntegerPropertyFilterSection<T> : FilterSectionTwoInputs<T>, IFilter<T>
	{
		protected readonly YleTextBox propertyNameTextBox = new YleTextBox();
		protected readonly YleNumeric propertyValueNumeric = new YleNumeric(0) { Decimals = 0, StepSize = 1 };

		public IntegerPropertyFilterSection(string filterName, Func<object, object, FilterElement<T>> emptyFilter) : base(filterName, emptyFilter)
		{
			GenerateUi();
		}

		public override bool IsValid => !string.IsNullOrEmpty(propertyNameTextBox.Text);

		public override object Value
		{
			get => propertyNameTextBox.Text;
			set => propertyNameTextBox.Text = (string)value;
		}

		public override object SecondValue
		{
			get => (int)propertyValueNumeric.Value;
			set => propertyValueNumeric.Value = (int)value;
		}

		protected override void GenerateUi()
		{
			base.GenerateUi();

			AddWidget(propertyNameTextBox, 0, 1);
			AddWidget(propertyValueNumeric, 0, 2);
		}

		protected override void HandleDefaultUpdate()
		{
			filterNameCheckBox.IsChecked = IsDefault;
			filterNameCheckBox.IsEnabled = !IsDefault;
			propertyNameTextBox.IsEnabled = !IsDefault;
			propertyValueNumeric.IsEnabled = !IsDefault;
		}
	}
}
