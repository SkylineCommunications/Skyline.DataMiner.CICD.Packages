namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Aspera
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Aspera;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    public sealed class AsperaGeneralInfoSection : YleSection
    {
        private readonly ISectionConfiguration configuration;

		private readonly Label generalOrderInformationTitle = new Label("General Order Information") { Style = TextStyle.Bold };

		private readonly Label nameOfTheOrderLabel = new Label("Name of the order");
		private readonly YleTextBox nameOfTheOrderTextBox = new YleTextBox();

		private readonly Label deadlineLabel = new Label("Deadline");
		private readonly YleDateTimePicker deadlineDateTimePicker = new YleDateTimePicker(DateTime.Now.AddDays(1)) { ValidationText = "The deadline cannot be in the past" };

		private bool ignoreTimingValidationWhenOrderExists;

		public AsperaGeneralInfoSection(Helpers helpers, ISectionConfiguration configuration, Aspera aspera = null) : base(helpers)
        {
            this.configuration = configuration;

			InitializeAsperaOrder(aspera);

            GenerateUi(out int row);
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

			AddWidget(generalOrderInformationTitle, ++row, 0, 1, 5);

			AddWidget(nameOfTheOrderLabel, ++row, 0);
			AddWidget(nameOfTheOrderTextBox, row, 1, 1, 2);

			AddWidget(deadlineLabel, ++row, 0);
            AddWidget(deadlineDateTimePicker, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

        public void UpdateAsperaOrder(Aspera asperaOrder)
        {
            asperaOrder.OrderDescription = nameOfTheOrderTextBox.Text;
            asperaOrder.Deadline = deadlineDateTimePicker.DateTime;
        }

		private void InitializeAsperaOrder(Aspera aspera)
		{
			ignoreTimingValidationWhenOrderExists = aspera != null;

			if (aspera != null)
			{
				nameOfTheOrderTextBox.Text = aspera.OrderDescription;
				deadlineDateTimePicker.DateTime = aspera.Deadline;
			}
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
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
