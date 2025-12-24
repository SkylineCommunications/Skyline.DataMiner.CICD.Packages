namespace ShowCeitonDetails_2.Sections
{
	using ShowCeitonDetails_2.Ceiton;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ProjectSection : Section
	{
		private readonly Label projectTitleLabel = new Label("Project") { Style = TextStyle.Bold };
		private readonly Label projectNumberLabel = new Label("PROJECT NUMBER");
		private readonly Label projectCeitonIdLabel = new Label("CEITON ID");
		private readonly Label projectNameLabel = new Label("PROJECT NAME");
		private readonly Label projectDescriptionLabel = new Label("PROJECT DESCRIPTION");
		private readonly Label projectContentOwnerLabel = new Label("PROJECT CONTENT OWNER");
		private readonly Label projectContentClassLabel = new Label("PROJECT CONTENT CLASS");

		private readonly CollapseButton projectTitleCollapseButton;
		private readonly Label projectNumberValue;
		private readonly Label projectCeitonIdValue;
		private readonly Label projectNameValue;
		private readonly Label projectDescriptionValue;
		private readonly Label projectContentOwnerValue;
		private readonly Label projectContentClassValue;

		public ProjectSection(Engine engine, Project project)
		{
			projectNumberValue = new Label(project.Number);
			projectCeitonIdValue = new Label(project.CeitonId);
			projectNameValue = new Label(project.Name);
			projectDescriptionValue = new Label(project.Description);
			projectContentOwnerValue = new Label(project.ContentOwnerName);
			projectContentClassValue = new Label(project.ContentClass);

			projectTitleCollapseButton = new CollapseButton(new[] { projectNumberLabel, projectNumberValue, projectCeitonIdLabel, projectCeitonIdValue, projectNameLabel, projectNameValue, projectDescriptionLabel, projectDescriptionValue, projectContentOwnerLabel, projectContentOwnerValue, projectContentClassLabel, projectContentClassValue }, isCollapsed: false) { CollapseText = "-", ExpandText = "+", Width = 44 };

			GenerateUI();
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(projectTitleCollapseButton, new WidgetLayout(row, 0));
			AddWidget(projectTitleLabel, new WidgetLayout(row, 1, 1, 20));

			AddWidget(projectNumberLabel, new WidgetLayout(++row, 1, 1, 2));
			AddWidget(projectNumberValue, new WidgetLayout(row, 3, 1, 20));

			AddWidget(projectCeitonIdLabel, new WidgetLayout(++row, 1, 1, 2));
			AddWidget(projectCeitonIdValue, new WidgetLayout(row, 3, 1, 20));

			AddWidget(projectNameLabel, new WidgetLayout(++row, 1, 1, 2));
			AddWidget(projectNameValue, new WidgetLayout(row, 3, 1, 20));

			AddWidget(projectDescriptionLabel, new WidgetLayout(++row, 1, 1, 2));
			AddWidget(projectDescriptionValue, new WidgetLayout(row, 3, 1, 20));

			AddWidget(projectContentOwnerLabel, new WidgetLayout(++row, 1, 1, 2));
			AddWidget(projectContentOwnerValue, new WidgetLayout(row, 3, 1, 20));

			AddWidget(projectContentClassLabel, new WidgetLayout(++row, 1, 1, 2));
			AddWidget(projectContentClassValue, new WidgetLayout(row, 3, 1, 20));
		}
	}
}