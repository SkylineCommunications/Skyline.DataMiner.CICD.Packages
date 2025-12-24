namespace LiveOrderForm_6.Dialogs
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class SelectFileToUploadDialog : Dialog
	{
		private readonly Label infoLabel = new Label("Please select a file containing order and source data in the predefined JSON format.");
		private readonly FileSelector fileSelector = new FileSelector { AllowMultipleFiles = false };
		private readonly Button cancelButton = new Button("Cancel") { Width = 150};
		private readonly Button confirmButton = new Button("Confirm") { Width = 150, Style = ButtonStyle.CallToAction };
		private readonly Label invalidLabel = new Label("Please select a file") { IsVisible = false};

		public SelectFileToUploadDialog(IEngine engine) : base(engine)
		{
			Title = "Select a File";

			cancelButton.Pressed += (o, e) => Cancelled?.Invoke(this, EventArgs.Empty);
			confirmButton.Pressed += ConfirmButton_Pressed;

			GenerateUi();
		}

		public string[] FilePaths => fileSelector.UploadedFilePaths;

		public string Info
		{
			get => infoLabel.Text;
			set => infoLabel.Text = value;
		}

		public bool AllowMultipleFiles
		{
			get => fileSelector.AllowMultipleFiles;
			set => fileSelector.AllowMultipleFiles = value;
		}

		public event EventHandler Cancelled;

		public event EventHandler<string[]> Confirmed;

		private bool IsValid => fileSelector.UploadedFilePaths.Any();

		private void ConfirmButton_Pressed(object sender, EventArgs e)
		{
			if (!IsValid)
			{
				invalidLabel.IsVisible = !IsValid;
				return;
			}

			Confirmed?.Invoke(this, FilePaths);
		}

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(infoLabel, new WidgetLayout(++row, 0, 1, 2));

			AddWidget(fileSelector, new WidgetLayout(++row, 0, 1, 2));

			AddWidget(cancelButton, new WidgetLayout(++row, 0));
			AddWidget(confirmButton, new WidgetLayout(row, 1));

			AddWidget(invalidLabel, new WidgetLayout(++row, 0, 1, 2));

			SetColumnWidth(0, cancelButton.Width);
			SetColumnWidth(1, confirmButton.Width);
		}
	}
}