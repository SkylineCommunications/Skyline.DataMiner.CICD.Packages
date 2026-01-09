namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Debug.Services
{
	using System;
	using System.IO;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ProfileLoadLoggingDialog : DebugDialog
	{
		private readonly Label header = new Label("Profile Load Logging") { Style = TextStyle.Heading };
		private readonly Label serviceIdLabel = new Label("Service ID");
		private readonly TextBox serviceIdTextBox = new TextBox();
		private readonly Button enterCurrentIdButton = new Button("Enter Current ID");
		private readonly Button getProfileLoadLoggingButton = new Button("Get Profile Load Logging") { Style = ButtonStyle.CallToAction };

		public ProfileLoadLoggingDialog(Helpers helpers) : base(helpers)
		{
			Title = "Profile Load Logging";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			enterCurrentIdButton.Pressed += (sender, args) => serviceIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;

			getProfileLoadLoggingButton.Pressed += GetProfileLoadLoggingButton_Pressed;
		}

		private void GetProfileLoadLoggingButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(serviceIdTextBox.Text, out Guid serviceId))
			{
				ShowRequestResult($"Service ID {serviceIdTextBox.Text} is not a Guid", string.Empty);
				GenerateUi();
				return;
			}

			string path = $@"C:\Skyline_Data\OrderLogging\{serviceId}.txt";

			if (!File.Exists(path))
			{
				ShowRequestResult($"File {path} does not exist", string.Empty);
				GenerateUi();
				return;
			}

			var text = File.ReadAllText(path);

			ShowRequestResult($"Profile Load Logging for {serviceId}", text);
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(header, ++row, 0, 1, 5);
			AddWidget(serviceIdLabel, ++row, 0);
			AddWidget(serviceIdTextBox, row, 1);
			AddWidget(enterCurrentIdButton, row, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(getProfileLoadLoggingButton, ++row, 0);

			AddResponseSections(row);
		}
	}
}
