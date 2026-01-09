namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class EditTemplateDialog : Dialog
	{
		public EditTemplateDialog(Engine engine) : base(engine)
		{
			Title = "Edit Template";

			int row = -1;
			AddWidget(new Label("What would you like to do?"), ++row, 0, 1, 2);

			AddWidget(new Label("Update the existing template"), ++row, 0);
			AddWidget(UpdateTemplateButton, row, 1);

			AddWidget(new Label("Save the updated template as a new template"), ++row, 0);
			AddWidget(CreateNewTemplateButton, row, 1);

			AddWidget(new Label("Delete the template"), ++row, 0);
			AddWidget(DeleteTemplateButton, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(BackButton, ++row, 0, 1, 2);

			SetColumnWidth(0, 300);
		}

		public Button UpdateTemplateButton { get; private set; } = new Button("Update Template") { Width = 150 };

		public Button CreateNewTemplateButton { get; private set; } = new Button("Create New Template") { Width = 150 };

		public Button DeleteTemplateButton { get; private set; } = new Button("Delete Template") { Width = 150 };

		public Button BackButton { get; private set; } = new Button("Back") { Width = 150 };
	}
}
