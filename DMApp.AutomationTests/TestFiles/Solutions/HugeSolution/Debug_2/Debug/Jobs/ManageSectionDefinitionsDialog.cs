namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Jobs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Correlation;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Section = Utils.InteractiveAutomationScript.Section;

	public class ManageSectionDefinitionsDialog : DebugDialog
	{
		private readonly Label titleLabel = new Label("Manage Section Definitions") { Style = TextStyle.Heading };
		private readonly Button listSectionDefinitionsButton = new Button("List Section Definitions");
		private readonly List<SectionDefinitionSection> sections = new List<SectionDefinitionSection>();
		private readonly Button removeSelectedFieldDescriptorsButton = new Button("Remove Selected Field Descriptors");

		public ManageSectionDefinitionsDialog(Helpers helpers) : base(helpers)
		{
			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			listSectionDefinitionsButton.Pressed += (s, e) => ListSectionDefinitions();
			removeSelectedFieldDescriptorsButton.Pressed += (s, e) =>
			{
				JobManagerHelper jobManager = new JobManagerHelper(msg => Automation.Engine.SLNet.SendMessages(msg));
				foreach (var section in sections)
				{
					if (!section.SelectedFieldDescriptors.Any()) continue;
					var retrievedSection = jobManager.SectionDefinitions.Read(SectionDefinitionExposers.ID.Equal(section.SectionDefinition.GetID())).FirstOrDefault();
					if (retrievedSection == null)
					{
						Engine.GenerateInformation($"Unable to retrieve section: {section.SectionDefinition.GetName()}");
						continue;
					}

					if (!(retrievedSection is CustomSectionDefinition customSectionDefinition))
					{
						Engine.GenerateInformation($"Section: {section.SectionDefinition.GetName()} is not a custom section");
						continue;
					}

					foreach (var fieldDescriptorId in section.SelectedFieldDescriptors.Select(x => x.ID))
					{
						customSectionDefinition.RemoveFieldDescriptor(fieldDescriptorId);
						customSectionDefinition = (CustomSectionDefinition)jobManager.SectionDefinitions.Update(customSectionDefinition);
						Engine.GenerateInformation($"section definition {customSectionDefinition.Name} contains {customSectionDefinition.GetAllFieldDescriptors().Count} field descriptors");
					}
				}

				GenerateUi();
			};
		}

		private void ListSectionDefinitions()
		{
			sections.Clear();
			sections.Add(new SectionDefinitionSection(helpers.EventManager.StaticEventSectionDefinition));
			sections.Add(new SectionDefinitionSection(helpers.EventManager.CustomEventSectionDefinition));
			sections.Add(new SectionDefinitionSection(helpers.EventManager.OrderSectionDefinition));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(titleLabel, ++row, 0);

			AddWidget(listSectionDefinitionsButton, ++row, 0);

			foreach (var section in sections)
			{
				AddSection(section, new SectionLayout(++row, 1));
				row += section.RowCount;
			}

			AddWidget(removeSelectedFieldDescriptorsButton, ++row, 0);
		}

		private sealed class SectionDefinitionSection : Section
		{
			private readonly SectionDefinition sectionDefinition;
			private readonly List<FieldDescriptorSection> sections = new List<FieldDescriptorSection>();
			private readonly CheckBox selectCheckBox = new CheckBox { IsChecked = false };

			public SectionDefinitionSection(SectionDefinition sectionDefinition)
			{
				this.sectionDefinition = sectionDefinition ?? throw new ArgumentNullException(nameof(sectionDefinition));

				Initialize();
				GenerateUi();

				if (!(sectionDefinition is CustomSectionDefinition))
				{
					this.IsEnabled = false;
				}
			}

			private void Initialize()
			{
				foreach (var fieldDescriptor in sectionDefinition.GetAllFieldDescriptors())
				{
					var fieldDescriptorSection = new FieldDescriptorSection(fieldDescriptor);
					fieldDescriptorSection.SelectCheckBox.Changed += (s, e) =>
					{
						selectCheckBox.IsChecked = sections.Where(x => x.IsEnabled).ToList().TrueForAll(x => x.SelectCheckBox.IsChecked);
					};

					sections.Add(fieldDescriptorSection);
				}

				selectCheckBox.Changed += (s, e) =>
				{
					foreach (var section in sections.Where(x => x.IsEnabled))
					{
						section.SelectCheckBox.IsChecked = e.IsChecked;
					}
				};
			}

			public SectionDefinition SectionDefinition => sectionDefinition;

			public IEnumerable<FieldDescriptor> SelectedFieldDescriptors => sections.Where(x => x.SelectCheckBox.IsChecked).Select(x => x.FieldDescriptor);

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(new Label($"Section Definition {sectionDefinition.GetName()} [{sections.Count}]") { Style = TextStyle.Bold }, ++row, 0);
				AddWidget(selectCheckBox, row, 1);

				foreach (var section in sections.OrderBy(x => x.Name))
				{
					AddSection(section, new SectionLayout(++row, 0));
					row += section.RowCount;
				}
			}
		}

		private sealed class FieldDescriptorSection : Section
		{
			private readonly FieldDescriptor fieldDescriptor;

			public FieldDescriptorSection(FieldDescriptor fieldDescriptor)
			{
				this.fieldDescriptor = fieldDescriptor ?? throw new ArgumentNullException(nameof(fieldDescriptor));

				GenerateUi();

				string[] fixedFieldDescriptors = new string[] { Event.OrderReservationFieldDescriptorName, Event.OrderIsIntegrationFieldDescriptorName };
				if (fixedFieldDescriptors.Contains(fieldDescriptor.Name) || Guid.TryParse(fieldDescriptor.Name, out _))
				{
					SelectCheckBox.IsChecked = false;
					this.IsEnabled = false;
				}
			}

			public string Name => fieldDescriptor.Name;

			public string ID => fieldDescriptor.ID.Id.ToString();

			public FieldDescriptor FieldDescriptor => fieldDescriptor;

			public CheckBox SelectCheckBox { get; private set; } = new CheckBox { IsChecked = false };

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(new Label($"{Name} [{ID}]"), ++row, 0);
				AddWidget(SelectCheckBox, row, 1);
			}
		}
	}
}
