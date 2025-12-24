namespace Library.UI.Filters
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class IntegerFilterSection<T> : FilterSectionOneInput<T>, IFilter<T>
	{
		protected readonly Numeric filterContentNumeric = new Numeric();

		public IntegerFilterSection(string filterName, Func<object, FilterElement<T>> emptyFilter) : base(filterName, emptyFilter)
		{
			GenerateUi();
		}

		public override bool IsValid => true;

		public override object Value
		{
			get => filterContentNumeric.Value;
			set => filterContentNumeric.Value = (double)value;
		}

		protected override void GenerateUi()
		{
			base.GenerateUi();

			AddWidget(filterContentNumeric, 0, 1);
		}

		protected override void HandleDefaultUpdate()
		{
			filterNameCheckBox.IsChecked = IsDefault;
			filterNameCheckBox.IsEnabled = !IsDefault;
			filterContentNumeric.IsEnabled = !IsDefault;
		}
	}
}
