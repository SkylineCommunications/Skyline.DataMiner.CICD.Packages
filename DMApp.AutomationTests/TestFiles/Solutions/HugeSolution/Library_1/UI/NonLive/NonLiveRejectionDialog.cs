namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

    public class NonLiveRejectionDialog : Dialog
	{
		private readonly Label rejectionLabel = new Label("Reason");

		public NonLiveRejectionDialog(IEngine engine) : base((Engine)engine)
		{
			this.Title = "Reject Order";

			ReasonOfRejectionTextBox = new YleTextBox() { IsMultiline = true, Height = 140, Width = 300 };
			RejectButton = new Button("Reject") { Width = 70, Style = ButtonStyle.CallToAction };
			CancelButton = new Button("Cancel") { Width = 70 };

			GenerateUI();
		}

		public YleTextBox ReasonOfRejectionTextBox { get; private set; }

		public Button RejectButton { get; private set; }

		public Button CancelButton { get; private set; }

		public void GenerateUI()
		{
			AddWidget(rejectionLabel, 0, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(ReasonOfRejectionTextBox, 0, 1);

			AddWidget(CancelButton, 1, 0);
			AddWidget(RejectButton, 2, 0);
		}

		public bool IsValid()
		{
			bool isReasonOfRejectionValid = !String.IsNullOrWhiteSpace(ReasonOfRejectionTextBox.Text);
			ReasonOfRejectionTextBox.ValidationState = isReasonOfRejectionValid ? UIValidationState.Valid : UIValidationState.Invalid;
			ReasonOfRejectionTextBox.ValidationText = "Provide a reason for rejection";

			return isReasonOfRejectionValid;
		}

		public void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			nonLiveOrder.ReasonOfRejection = ReasonOfRejectionTextBox.Text;
		}
	}
}
