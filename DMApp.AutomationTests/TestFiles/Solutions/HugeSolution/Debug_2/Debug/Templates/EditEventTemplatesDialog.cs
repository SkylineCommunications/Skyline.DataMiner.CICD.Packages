namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Templates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class EditEventTemplatesDialog : Dialog
	{
		private readonly Helpers helpers;
		private readonly List<EventTemplateSection> sections = new List<EventTemplateSection>();

		public EditEventTemplatesDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Edit Event Templates";

			this.helpers = helpers;

			var eventTemplateNames = helpers.ContractManager.GetAllEventTemplates().OrderBy(x => x);
			foreach (var eventTemplateName in eventTemplateNames)
			{
				sections.Add(new EventTemplateSection(helpers, eventTemplateName));
			}

			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private void GenerateUi()
		{
			Clear();

			int row = -1;
			AddWidget(BackButton, ++row, 0, 1, 2);

			foreach (var eventTemplateSection in sections)
			{
				AddSection(eventTemplateSection, ++row, 0);
				row += eventTemplateSection.RowCount;
			}
		}

		private sealed class EventTemplateSection : Section
		{
			private readonly Helpers helpers;
			private readonly string name;

			private readonly TextBox textBox = new TextBox(String.Empty) { IsMultiline = true, Height = 800, Width = 800 };
			private readonly Label templateNameLabel = new Label { Style = TextStyle.Bold };
			private readonly Label statusLabel = new Label(String.Empty);
			private readonly Button updateButton = new Button("Update Template") { Width = 150 };
			private readonly CollapseButton collapseButton = new CollapseButton { CollapseText = "-", ExpandText = "+", Width = 44 };

			public EventTemplateSection(Helpers helpers, string name)
			{
				this.helpers = helpers;
				this.name = name;

				templateNameLabel.Text = name;

				collapseButton.LinkedWidgets.Add(textBox);
				collapseButton.LinkedWidgets.Add(updateButton);
				collapseButton.LinkedWidgets.Add(statusLabel);
				collapseButton.IsCollapsed = true;

				collapseButton.Pressed += (s, e) => RetrieveTemplate();
				updateButton.Pressed += (s, e) => UpdateTemplate();

				GenerateUi();
			}

			private void GenerateUi()
			{
				Clear();

				AddWidget(collapseButton, 0, 0);
				AddWidget(templateNameLabel, 0, 1);

				AddWidget(textBox, 1, 1);

				AddWidget(updateButton, 2, 1);

				AddWidget(statusLabel, 3, 1);
			}

			private void RetrieveTemplate()
			{
				if (!String.IsNullOrEmpty(textBox.Text)) return;

				if (!helpers.ContractManager.TryGetEventTemplate(name, out EventTemplate template))
				{
					statusLabel.Text = $"Unable to retrieve Event Template with name {name}";
					textBox.IsEnabled = false;
					updateButton.IsEnabled = false;
					return;
				}

				textBox.Text = JsonConvert.SerializeObject(template, Formatting.Indented);
			}

			private void UpdateTemplate()
			{
				EventTemplate template;
				try
				{
					template = JsonConvert.DeserializeObject<EventTemplate>(textBox.Text);
				}
				catch (Exception)
				{
					statusLabel.Text = $"Unable to serialize Event Template, template was not updated";
					return;
				}

				if (!helpers.ContractManager.TryEditEventTemplate(template))
				{
					statusLabel.Text = $"Unable to edit Event Template in Contract Manager";
					return;
				}

				statusLabel.Text = $"Event Template was updated";
			}
		}
	}
}
