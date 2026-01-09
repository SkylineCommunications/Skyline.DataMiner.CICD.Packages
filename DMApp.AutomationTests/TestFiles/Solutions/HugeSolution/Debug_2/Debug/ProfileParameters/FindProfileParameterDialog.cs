namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.ProfileParameters
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class FindProfileParameterDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label findParameterLabel = new Label("Find Profile Parameter") { Style = TextStyle.Heading };
		private readonly Label parameterIdLabel = new Label("GUID");
		private readonly Label parameterNameLabel = new Label(String.Empty);

		public FindProfileParameterDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Find Profile Parameter";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private TextBox ParameterIdTextBox { get; set; }

		private Button FindByParameterIdButton { get; set; }

		private void Initialize()
		{
			ParameterIdTextBox = new TextBox { PlaceHolder = "GUID", ValidationText = "Invalid GUID", Width = 400 };

			FindByParameterIdButton = new Button("Find By GUID") { Width = 150 };
			FindByParameterIdButton.Pressed += FindByParameterIdButton_Pressed;
		}

		private void FindByParameterIdButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(ParameterIdTextBox.Text, out var guid))
			{
				parameterNameLabel.Text = String.Empty;
				ParameterIdTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			try
			{
				var parameter = helpers.ProfileManager.GetProfileParameter(guid);
				parameterNameLabel.Text = parameter.Name;
			}
			catch (ProfileParameterNotFoundException)
			{
				parameterNameLabel.Text = "No profile parameter found with given ID";
			}

			ParameterIdTextBox.ValidationState = UIValidationState.Valid;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(findParameterLabel, ++row, 0, 1, 5);

			AddWidget(parameterIdLabel, ++row, 0, 1, 2);
			AddWidget(ParameterIdTextBox, row, 2);
			AddWidget(FindByParameterIdButton, row, 3);

			AddWidget(new WhiteSpace(), ++row, 1);

			AddWidget(parameterNameLabel, ++row, 0, 1, 3);
		}
	}
}