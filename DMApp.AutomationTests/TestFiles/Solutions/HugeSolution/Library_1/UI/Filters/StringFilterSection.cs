namespace Library.UI.Filters
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class StringFilterSection<T> : FilterSectionOneInput<T>, IFilter<T>
	{
		protected readonly YleTextBox filterContentTextBox = new YleTextBox();

		public StringFilterSection(string filterName, Func<object, FilterElement<T>> emptyFilter) : base(filterName, emptyFilter)
		{
			GenerateUi();
		}

		public override bool IsValid => true;

		public override object Value
		{
			get => filterContentTextBox.Text;
			set => filterContentTextBox.Text = (string)value;
		}

		protected override void GenerateUi()
		{
			base.GenerateUi();

			AddWidget(filterContentTextBox, 0, 1);
		}

		protected override void HandleDefaultUpdate()
		{
			filterNameCheckBox.IsChecked = IsDefault;
			filterNameCheckBox.IsEnabled = !IsDefault;
			filterContentTextBox.IsEnabled = !IsDefault;
		}
	}
}
