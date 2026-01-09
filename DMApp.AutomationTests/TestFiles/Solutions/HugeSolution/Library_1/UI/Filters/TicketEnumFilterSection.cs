namespace Library.UI.Filters
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class TicketEnumFilterSection<T> : FilterSectionThreeInputs<T>, IFilter<T>
	{
		protected readonly YleTextBox propertyNameTextBox = new YleTextBox();
		protected readonly YleTextBox propertyValueTextBox = new YleTextBox();
		protected readonly YleNumeric propertyValueNumeric = new YleNumeric(0) { Decimals = 0, StepSize = 1 };

		public TicketEnumFilterSection(string filterName, Func<object, object, object, FilterElement<T>> emptyFilter) : base(filterName, emptyFilter)
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
			get => propertyValueTextBox.Text;
			set => propertyValueTextBox.Text = (string)value;
		}

		public override object ThirdValue
		{
			get => propertyValueNumeric.Value;
			set => propertyValueNumeric.Value = value;
		}

		protected override void GenerateUi()
		{
			base.GenerateUi();

			AddWidget(propertyNameTextBox, 0, 1);
			AddWidget(propertyValueTextBox, 0, 2);
			AddWidget(propertyValueNumeric, 1, 2);
		}

		protected override void HandleDefaultUpdate()
		{
			filterNameCheckBox.IsChecked = IsDefault;
			filterNameCheckBox.IsEnabled = !IsDefault;
			propertyNameTextBox.IsEnabled = !IsDefault;
			propertyValueTextBox.IsEnabled = !IsDefault;
			propertyValueNumeric.IsEnabled = !IsDefault;
		}
	}
}
