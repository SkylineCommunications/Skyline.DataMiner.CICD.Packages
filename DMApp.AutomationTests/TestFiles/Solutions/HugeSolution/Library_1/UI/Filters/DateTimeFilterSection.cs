namespace Library.UI.Filters
{
	using System;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DateTimeFilterSection<T> : FilterSectionOneInput<T>, IFilter<T>
	{
		private readonly DateTimePicker dateTimePicker = new DateTimePicker(DateTime.Now);
		
		public DateTimeFilterSection(string filterName, Func<object, FilterElement<T>> filterFunction) : base(filterName, filterFunction)
		{
			GenerateUi();
		}

		public override bool IsValid => true;

		public override object Value
		{
			get => dateTimePicker.DateTime;
			set => dateTimePicker.DateTime = (DateTime)value;
		}

		protected override void GenerateUi()
		{
			base.GenerateUi();

			AddWidget(dateTimePicker, 0, 1);
		}

		protected override void HandleDefaultUpdate()
		{
			filterNameCheckBox.IsChecked = IsDefault;
			filterNameCheckBox.IsEnabled = !IsDefault;
			dateTimePicker.IsEnabled = !IsDefault;
		}
	}
}
