namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Aspera
{
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Aspera;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;

	public sealed class AsperaEmailSection : YleSection
    {
        private readonly Label emailAddressLabel = new Label();
        private readonly TextBox emailAddressTextBox = new TextBox();

        private readonly ISectionConfiguration configuration;

        public AsperaEmailSection(AsperaType type, Helpers helpers, ISectionConfiguration configuration, string text = null) : base(helpers)
        {
            emailAddressLabel.IsVisible = true;
            emailAddressTextBox.IsVisible = true;
            emailAddressLabel.Text = type == AsperaType.Faspex ? "Sender email address" : "Email address of participant";
            emailAddressTextBox.Text = string.IsNullOrEmpty(text) ? String.Empty : text;
            
            this.configuration = configuration;

            GenerateUi(out int row);
            IsValid();
        }

        protected override void GenerateUi(out int row)
        {
            base.GenerateUi(out row);

            AddWidget(emailAddressLabel, ++row, 0);
            AddWidget(emailAddressTextBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

        public string EmailAddress { get => emailAddressTextBox.Text; }

        public bool IsValid()
        {
            bool isValid = !string.IsNullOrWhiteSpace(emailAddressTextBox.Text);
            emailAddressTextBox.ValidationState = isValid ? Automation.UIValidationState.Valid : Automation.UIValidationState.Invalid;
            emailAddressTextBox.ValidationText = "Please provide any information";

            return isValid; 
        }

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			emailAddressLabel.IsVisible = IsVisible;
			emailAddressTextBox.IsVisible = IsVisible;
			emailAddressTextBox.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
