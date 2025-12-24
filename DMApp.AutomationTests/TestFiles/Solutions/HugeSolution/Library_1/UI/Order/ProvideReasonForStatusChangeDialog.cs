namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status;

	public class ProvideReasonForStatusChangeDialog : Dialog
	{
		private readonly Dictionary<Status, string> statusDescriptions = new Dictionary<Status, string>
		{
			{Status.Cancelled, "canceling"},
			{Status.Rejected, "rejecting"},
		};

		private readonly Label provideReasonLabel;
		private readonly YleTextBox reasonTextBox = new YleTextBox(string.Empty) { IsMultiline = true, MinWidth = 500, MinHeight = 300 };

		public ProvideReasonForStatusChangeDialog(IEngine engine, Status status) : base((Engine)engine)
		{
			if (!statusDescriptions.ContainsKey(status)) throw new NotImplementedException($"Dialog currently does not support status {status}");

			provideReasonLabel = new Label($"Provide a reason for {statusDescriptions[status]}:");
			OkButton = new Button("OK") { Width = 150, Style  = ButtonStyle.CallToAction };

			GenerateUi();
		}

		public Button OkButton { get; private set; }

		public string ReasonForStatusChange => reasonTextBox.Text;

        public bool IsValid()
        {
            bool isReasonTextValid = reasonTextBox.Text.Length <= Constants.MaximumAllowedCharacters;
            reasonTextBox.ValidationState = isReasonTextValid ? UIValidationState.Valid : UIValidationState.Invalid;
            reasonTextBox.ValidationText = $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters";

            return isReasonTextValid;
        }

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(provideReasonLabel, new WidgetLayout(++row, 0));
			AddWidget(reasonTextBox, new WidgetLayout(row, 1));

			AddWidget(OkButton, new WidgetLayout(++row, 0, 1, 2));
		}
	}
}
