namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public sealed class ExportGeneralInfoSection : YleSection
	{
		private readonly Label generalOrderInformationTitle = new Label("General Order Information") { Style = TextStyle.Bold };

		private readonly Label nameOfTheOrderLabel = new Label("Name of the order");
		private readonly YleTextBox nameOfTheOrderTextBox = new YleTextBox();

		private readonly Label deadlineLabel = new Label("Deadline");
		private readonly YleDateTimePicker deadlineDateTimePicker = new YleDateTimePicker(DateTime.Now.AddDays(1)) { ValidationText = "The deadline cannot be in the past" };

		private readonly bool ignoreTimingValidationWhenOrderExists;
		private readonly ISectionConfiguration sectionConfiguration;

		public ExportGeneralInfoSection(Helpers helpers, ISectionConfiguration sectionConfiguration, NonLiveOrder nonLiveOrder = null) : base(helpers)
		{
			ignoreTimingValidationWhenOrderExists = nonLiveOrder != null;

			if (ignoreTimingValidationWhenOrderExists)
			{
				NameOfTheOrder = nonLiveOrder.OrderDescription;
				Deadline = nonLiveOrder.Deadline;
			}

			this.sectionConfiguration = sectionConfiguration;

			GenerateUi(out int row);
		}

		public string NameOfTheOrder
		{
			get
			{
				return nameOfTheOrderTextBox.Text;
			}
			protected set
			{
				nameOfTheOrderTextBox.Text = value;
			}
		}

		public DateTime Deadline
		{
			get
			{
				return deadlineDateTimePicker.DateTime;
			}
			protected set
			{
				deadlineDateTimePicker.DateTime = value;
			}
		}

		public bool IsValid(OrderAction action)
		{
			bool isOrderDescriptionValid = !String.IsNullOrWhiteSpace(NameOfTheOrder);
			nameOfTheOrderTextBox.ValidationState = isOrderDescriptionValid ? UIValidationState.Valid : UIValidationState.Invalid;
			nameOfTheOrderTextBox.ValidationText = "Provide a name for the order";

			bool isDeadLineValid = ignoreTimingValidationWhenOrderExists || deadlineDateTimePicker.IsValid;

			if (action == OrderAction.Save)
			{
				return isOrderDescriptionValid;
			}
			else
			{
				return isOrderDescriptionValid && isDeadLineValid;
			}
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}

		public void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (nonLiveOrder != null)
			{
				nonLiveOrder.OrderDescription = NameOfTheOrder;
				nonLiveOrder.Deadline = Deadline;
			}
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(generalOrderInformationTitle, new WidgetLayout(++row, 0));

			AddWidget(nameOfTheOrderLabel, new WidgetLayout(++row, 0));
			AddWidget(nameOfTheOrderTextBox, new WidgetLayout(row, 1, 1, 2));

			AddWidget(deadlineLabel, ++row, 0);
			AddWidget(deadlineDateTimePicker, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, sectionConfiguration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			generalOrderInformationTitle.IsVisible = IsVisible;

			nameOfTheOrderLabel.IsVisible = IsVisible;
			nameOfTheOrderTextBox.IsVisible = IsVisible;
			nameOfTheOrderTextBox.IsEnabled = IsEnabled;

			deadlineLabel.IsVisible = IsVisible;
			deadlineDateTimePicker.IsVisible = IsVisible;
			deadlineDateTimePicker.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
