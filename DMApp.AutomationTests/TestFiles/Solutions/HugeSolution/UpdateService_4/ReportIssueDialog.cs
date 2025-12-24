namespace UpdateService_4
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ReportIssueDialog : Dialog
	{
		private readonly Label rejectionLabel = new Label("Reason");

		public ReportIssueDialog(IEngine engine) : base(engine)
		{
			InitializeWidgets();
			GenerateUI();
		}

		public TextBox ReportIssueTextBox { get; private set; }

		public Button ConfirmButton { get; private set; }

		public Button CancelButton { get; private set; }

		public bool IsValid()
		{
			bool isReasonOfReportingIssueValid = !String.IsNullOrWhiteSpace(ReportIssueTextBox.Text);
			ReportIssueTextBox.ValidationState = isReasonOfReportingIssueValid ? UIValidationState.Valid : UIValidationState.Invalid;
			ReportIssueTextBox.ValidationText = "Provide some information about the issue that was happened.";

			return isReasonOfReportingIssueValid;
		}

		private void InitializeWidgets()
		{
			this.Title = "Report Issue";

			ReportIssueTextBox = new TextBox { IsMultiline = true, Height = 140, Width = 300 };
			ConfirmButton = new Button("Confirm") { Width = 100, Style = ButtonStyle.CallToAction };
			CancelButton = new Button("Cancel") { Width = 100 };
		}

		private void GenerateUI()
		{
			AddWidget(rejectionLabel, 0, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(ReportIssueTextBox, 0, 1);

			AddWidget(CancelButton, 1, 0);
			AddWidget(ConfirmButton, 2, 0);
		}
	}
}