namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.WgTransfer
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public sealed class TransferGeneralInfoSection : YleSection
	{
        private readonly ISectionConfiguration configuration;

		private readonly Label generalOrderInformationTitle = new Label("General Order Information") { Style = TextStyle.Bold };

		private readonly Label nameOfTheOrderLabel = new Label("Name of the order");
		private readonly YleTextBox nameOfTheOrderTextBox = new YleTextBox();

		private readonly Label deadlineLabel = new Label("Deadline");
		private readonly YleDateTimePicker deadlineDateTimePicker = new YleDateTimePicker(DateTime.Now.AddDays(1)) { ValidationText = "The deadline cannot be in the past" };

		private bool ignoreTimingValidationWhenOrderExists;

		public TransferGeneralInfoSection(Helpers helpers, ISectionConfiguration configuration, Transfer transfer = null) : base(helpers)
		{
            this.configuration = configuration;

			InitializeTransfer(transfer);

            GenerateUi(out int row);
		}

        public override void RegenerateUi()
        {
			GenerateUi(out int row);
        }

		public void UpdateTransfer(Transfer transfer)
		{
			if (transfer is null) return;

			transfer.OrderDescription = nameOfTheOrderTextBox.Text;
			transfer.Deadline = deadlineDateTimePicker.DateTime;			
		}

		public bool IsValid(OrderAction action)
		{
			bool isOrderDescriptionValid = !String.IsNullOrWhiteSpace(nameOfTheOrderTextBox.Text);
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

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(generalOrderInformationTitle, new WidgetLayout(++row, 0));

			AddWidget(nameOfTheOrderLabel, new WidgetLayout(++row, 0));
			AddWidget(nameOfTheOrderTextBox, new WidgetLayout(row, 1, 1, 2));

			AddWidget(deadlineLabel, ++row, 0);
			AddWidget(deadlineDateTimePicker, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

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

		private void InitializeTransfer(Transfer transfer)
		{
			ignoreTimingValidationWhenOrderExists = transfer != null;

			if (transfer != null)
			{
				nameOfTheOrderTextBox.Text = transfer.OrderDescription;
				deadlineDateTimePicker.DateTime = transfer.Deadline;
			}
		}
	}
}
