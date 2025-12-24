namespace Library.UI.Filters
{
	using System;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class FilterSection<T> : Section, IFilter<T>
	{
		private bool isDefault;

		protected CheckBox filterNameCheckBox;

		protected FilterSection(string filterName)
		{
			this.filterNameCheckBox = new CheckBox(filterName);
		}

		public bool IsActive
		{
			get => filterNameCheckBox.IsChecked;
			protected set => filterNameCheckBox.IsChecked = value;
		}

		public new bool IsEnabled
		{
			get => base.IsEnabled;
			set
			{
				base.IsEnabled = value;
				HandleEnabledUpdate();
			}
		}

		public bool IsDefault
		{
			get => isDefault;
			protected set
			{
				isDefault = value;
				HandleDefaultUpdate();
			}
		}

		public abstract bool IsValid { get; }

		public abstract object Value { get; set; }

		public abstract FilterElement<T> Filter { get; }

		protected virtual void GenerateUi()
		{
			Clear();

			AddWidget(filterNameCheckBox, 0, 0);
		}

		protected abstract void HandleDefaultUpdate();

		private void HandleEnabledUpdate()
		{
			bool filterIsChecked = IsActive;

			HandleDefaultUpdate();

			filterNameCheckBox.IsChecked = filterIsChecked || IsDefault;
		}
	}

	public abstract class FilterSectionOneInput<T> : FilterSection<T>
	{
		private readonly Func<object, FilterElement<T>> filterFunctionWithOneInput;

		protected FilterSectionOneInput(string filterName, Func<object, FilterElement<T>> emptyFilter) : base(filterName)
		{
			this.filterFunctionWithOneInput = emptyFilter;
		}

		public override FilterElement<T> Filter => filterFunctionWithOneInput(Value);

		public void SetDefault(object value)
		{
			IsDefault = true;

			Value = value;
		}
	}

	public abstract class FilterSectionTwoInputs<T> : FilterSection<T>
	{
		private readonly Func<object, object, FilterElement<T>> filterFunctionWithTwoInputs;

		protected FilterSectionTwoInputs(string filterName, Func<object, object, FilterElement<T>> emptyFilter) : base(filterName)
		{
			this.filterFunctionWithTwoInputs = emptyFilter;
		}

		public override FilterElement<T> Filter => filterFunctionWithTwoInputs(Value, SecondValue);

		public abstract object SecondValue { get; set; }

		public void SetDefault(object value, object secondValue)
		{
			IsDefault = true;

			Value = value;
			SecondValue = secondValue;
		}
	}

	public abstract class FilterSectionThreeInputs<T> : FilterSection<T>
	{
		private readonly Func<object, object, object, FilterElement<T>> filterFunctionWithThreeInputs;

		protected FilterSectionThreeInputs(string filterName, Func<object, object, object, FilterElement<T>> emptyFilter) : base(filterName)
		{
			this.filterFunctionWithThreeInputs = emptyFilter;
		}

		public override FilterElement<T> Filter => filterFunctionWithThreeInputs(Value, SecondValue, ThirdValue);

		public abstract object SecondValue { get; set; }

		public abstract object ThirdValue { get; set; }

		public void SetDefault(object value, object secondValue, object thirdValue)
		{
			IsDefault = true;

			Value = value;
			SecondValue = secondValue;
			ThirdValue = thirdValue;
		}
	}

}
