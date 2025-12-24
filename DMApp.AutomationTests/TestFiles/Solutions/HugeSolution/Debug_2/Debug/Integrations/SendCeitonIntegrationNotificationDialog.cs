namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations.Ceiton;

	public class SendCeitonIntegrationNotificationDialog : DebugDialog
	{
		private readonly Label ceitonLabel = new Label("Ceiton") { Style = TextStyle.Heading };

		private readonly Button addAdHocTaskButton = new Button("Add Ad Hoc Task") { Width = 200 };
		private readonly Button sendNotificationButton = new Button("Send Notification") { Width = 200 };
		private readonly Button deleteProjectInfoButton = new Button("Delete Project Info") { Width = 200 };
		private readonly CheckBox provideProductInfoCheckBox = new CheckBox("Provide Product Info?") { IsChecked = false };

		private readonly ProjectSection projectSection = new ProjectSection();
		private readonly List<CeitonTaskSection> adHocTaskSections = new List<CeitonTaskSection>();

		private ProductSection productSection = null;

		public SendCeitonIntegrationNotificationDialog(Utilities.Helpers helpers) : base(helpers)
		{
			Title = "Send Ceiton Integration Notification";
			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			projectSection.TriggerUiGeneration += (s, e) => GenerateUi();
			provideProductInfoCheckBox.Changed += (s, e) =>
			{
				if (e.IsChecked)
				{
					productSection = new ProductSection(projectSection);
					productSection.TriggerUiGeneration += (sec, args) => GenerateUi();
				}
				else
				{
					productSection = null;
				}

				GenerateUi();
			};

			addAdHocTaskButton.Pressed += (s, e) =>
			{
				var section = new CeitonTaskSection();
				section.DeleteTaskButton.Pressed += (sec, args) =>
				{
					adHocTaskSections.Remove(section);
					GenerateUi();
				};

				section.TriggerUiGeneration += (sec, args) => GenerateUi();
				adHocTaskSections.Add(section);

				GenerateUi();
			};

			sendNotificationButton.Pressed += (s, e) => SendNotification();
			deleteProjectInfoButton.Pressed += (s, e) => DeleteProject();
		}

		private void SendNotification()
		{
			string ceitonId = Guid.NewGuid().ToString();

			List<Data> data = new List<Data>();
			foreach (var projectData in projectSection.Data)
			{
				projectData.ProjectCeitonId = ceitonId;
				data.Add(projectData);
			}

			if (productSection != null)
			{
				foreach (var productData in productSection.Data)
				{
					productData.ProductCeitonId = ceitonId;
					data.Add(productData);
				}
			}

			foreach (var adHocData in adHocTaskSections.SelectMany(x => x.Data))
			{
				data.Add(adHocData);
			}

			OrderManagerElement orderManager = new OrderManagerElement(helpers);
			CeitonElement ceitonElement = new CeitonElement(orderManager.CeitonElement);

			Export export = new Export
			{
				SystemId = ceitonId,
				TimeStamp = DateTime.Now.Ticks,
				TaskData = data
			};

			ceitonElement.SendNotification(export);

			ShowRequestResult($"Notification {export.SystemId} sent", JsonConvert.SerializeObject(export));
			GenerateUi();
		}

		private void DeleteProject()
		{
			if (!projectSection.NumberTextBox.IsValid) return;

			OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);
			if (orderManagerElement.CeitonElement == null || orderManagerElement.CeitonElement.State != ElementState.Active)
			{
				ShowRequestResult($"Unable to remove Ceiton Project {projectSection.NumberTextBox.Text}", "Ceiton element was not linked with order manager or not active");
				return;
			}

			// Remove entry from Ceiton element
			var liveStreamOrdersTable = orderManagerElement.CeitonElement.GetTable(1000);
			if (liveStreamOrdersTable.RowExists(projectSection.NumberTextBox.Text))
			{
				liveStreamOrdersTable.DeleteRow(projectSection.NumberTextBox.Text);
			}

			// Trigger integration update
			orderManagerElement.Element.SetParameter(110, projectSection.NumberTextBox.Text);

			ShowRequestResult($"Removed Ceiton project {projectSection.NumberTextBox.Text}", "Removed");
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(ceitonLabel, ++row, 0);

			AddWidget(new Label("Project Information") { Style = TextStyle.Heading }, ++row, 0, 1, 2);
			AddSection(projectSection, new SectionLayout(++row, 0));
			row += projectSection.RowCount;
			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(new Label("Product Information") { Style = TextStyle.Heading }, ++row, 0, 1, 2);
			AddWidget(provideProductInfoCheckBox, row, 2);

			if (productSection != null)
			{
				AddSection(productSection, new SectionLayout(++row, 0));
				row += productSection.RowCount;
			}

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(new Label("Ad Hoc Tasks") { Style = TextStyle.Heading }, ++row, 0, 1, 2);
			foreach (var section in adHocTaskSections)
			{
				AddWidget(new Label("Ad Hoc Task") { Style = TextStyle.Heading }, ++row, 0, 1, 2);
				AddSection(section, new SectionLayout(++row, 0));
				row += section.RowCount;
				AddWidget(new WhiteSpace(), ++row, 0);
			}

			AddWidget(addAdHocTaskButton, ++row, 0);
			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(sendNotificationButton, ++row, 0);
			AddWidget(deleteProjectInfoButton, ++row, 0);

			AddResponseSections(row + 1);
		}

		private interface ICeitonSection
		{
			Data[] Data { get; }

			event EventHandler TriggerUiGeneration;
		}

		private class ProjectSection : Section, ICeitonSection
		{
			private static readonly string[] contentClassCaptionOptions = new string[]
			{
				"Ajankohtainen",
				"Asia",
				"Draama",
				"Hartaus",
				"Musiikki",
				"Lapset",
				"Opetus ja tiede",
				"Urheilu",
				"Uutiset",
				"Viihde"
			};

			private readonly Button addTaskButton = new Button("Add Project Task");
			private readonly Button randomProjectNumberButton = new Button("Enter Random Guid") { Width = 150 };
			private readonly List<CeitonTaskSection> projectTaskSections = new List<CeitonTaskSection>();

			public ProjectSection()
			{
				Initialize();
				GenerateUi();
			}

			public YleTextBox NumberTextBox { get; } = new YleTextBox(Guid.NewGuid().ToString());

			public TextBox NameTextBox { get; } = new TextBox();

			public TextBox CeitonIdTextBox { get; } = new TextBox();

			public TextBox DescriptionTextBox { get; } = new TextBox();

			public TextBox ContentOwnerTextBox { get; } = new TextBox();

			public DropDown ContentClassCaptionDropDown { get; } = new DropDown(contentClassCaptionOptions, contentClassCaptionOptions[0]);

			public Data[] Data
			{
				get
				{
					List<Data> data = new List<Data>()
					{
						new Data
						{
							ProjectNumber = NumberTextBox.Text,
							ProjectName = NameTextBox.Text,
							ProjectDescription = DescriptionTextBox.Text,
							ProjectContentOwnerName = ContentOwnerTextBox.Text,
							ProjectContentClassCaption = ContentClassCaptionDropDown.Selected,
						}
					};

					foreach (var task in projectTaskSections.SelectMany(x => x.Data))
					{
						task.ProjectCeitonId = CeitonIdTextBox.Text;
						task.ProjectNumber = NumberTextBox.Text;
						task.ProjectName = NameTextBox.Text;

						data.Add(task);
					}

					return data.ToArray();
				}
			}

			public event EventHandler TriggerUiGeneration;

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(new Label("Project Number"), ++row, 0);
				AddWidget(NumberTextBox, row, 1, 1, 2);
				AddWidget(randomProjectNumberButton, row, 3);

				AddWidget(new Label("Project Name"), ++row, 0);
				AddWidget(NameTextBox, row, 1, 1, 2);

				AddWidget(new Label("Description"), ++row, 0);
				AddWidget(DescriptionTextBox, row, 1, 1, 2);

				AddWidget(new Label("Content Owner"), ++row, 0);
				AddWidget(ContentOwnerTextBox, row, 1, 1, 2);

				AddWidget(new Label("Class Caption"), ++row, 0);
				AddWidget(ContentClassCaptionDropDown, row, 1, 1, 2);

				foreach (var section in projectTaskSections)
				{
					AddWidget(new Label("Project Task") { Style = TextStyle.Heading }, ++row, 1, 1, 2);
					AddSection(section, new SectionLayout(++row, 1));
					AddWidget(new WhiteSpace(), ++row, 0);
					row += section.RowCount;
				}

				AddWidget(addTaskButton, row + 1, 0);
			}

			private void Initialize()
			{
				randomProjectNumberButton.Pressed += (s, e) => NumberTextBox.Text = Guid.NewGuid().ToString();

				addTaskButton.Pressed += (s, e) =>
				{
					CeitonTaskSection section = new CeitonTaskSection();
					section.DeleteTaskButton.Pressed += (sec, args) =>
					{
						projectTaskSections.Remove(section);
						GenerateUi();
						TriggerUiGeneration?.Invoke(this, EventArgs.Empty);
					};

					section.TriggerUiGeneration += (sec, args) => TriggerUiGeneration?.Invoke(sec, args);
					projectTaskSections.Add(section);
					GenerateUi();

					TriggerUiGeneration?.Invoke(this, EventArgs.Empty);
				};
			}
		}

		private class ProductSection : Section, ICeitonSection
		{
			private readonly Button addTaskButton = new Button("Add Product Task");
			private readonly Button randomProductNumberButton = new Button("Enter Random Guid") { Width = 150 };
			private readonly List<CeitonTaskSection> productTaskSections = new List<CeitonTaskSection>();
			private readonly ProjectSection projectSection;

			public ProductSection(ProjectSection projectSection)
			{
				this.projectSection = projectSection;

				Initialize();
				GenerateUi();
			}

			public TextBox NumberTextBox { get; } = new TextBox(Guid.NewGuid().ToString());

			public TextBox NameTextBox { get; } = new TextBox();

			public TextBox DescriptionTextBox { get; } = new TextBox();

			public Data[] Data
			{
				get
				{
					List<Data> data = new List<Data>()
					{
						new Data
						{
							ProductNumber = NumberTextBox.Text,
							ProductName = NameTextBox.Text,
							ProductDescription = DescriptionTextBox.Text,
							ProjectNumber = projectSection.NumberTextBox.Text,
						}
					};

					foreach (var task in productTaskSections.SelectMany(x => x.Data))
					{
						task.ProductNumber = NumberTextBox.Text;
						task.ProductName = NameTextBox.Text;

						data.Add(task);
					}

					return data.ToArray();
				}
			}

			public event EventHandler TriggerUiGeneration;

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(new Label("Product Number"), ++row, 0);
				AddWidget(NumberTextBox, row, 1, 1, 2);
				AddWidget(randomProductNumberButton, row, 3);

				AddWidget(new Label("Product Name"), ++row, 0);
				AddWidget(NameTextBox, row, 1, 1, 2);

				AddWidget(new Label("Description"), ++row, 0);
				AddWidget(DescriptionTextBox, row, 1, 1, 2);

				foreach (var section in productTaskSections)
				{
					AddWidget(new Label("Product Task") { Style = TextStyle.Heading }, ++row, 1, 1, 2);
					AddSection(section, new SectionLayout(++row, 1));
					AddWidget(new WhiteSpace(), ++row, 0);
					row += section.RowCount;
				}

				AddWidget(addTaskButton, row + 1, 0);
			}

			private void Initialize()
			{
				randomProductNumberButton.Pressed += (s, e) => NumberTextBox.Text = Guid.NewGuid().ToString();

				addTaskButton.Pressed += (s, e) =>
				{
					CeitonTaskSection section = new CeitonTaskSection();
					section.DeleteTaskButton.Pressed += (sec, args) =>
					{
						productTaskSections.Remove(section);
						GenerateUi();
						TriggerUiGeneration?.Invoke(this, EventArgs.Empty);
					};

					section.TriggerUiGeneration += (sec, args) => TriggerUiGeneration?.Invoke(sec, args);
					productTaskSections.Add(section);
					GenerateUi();

					TriggerUiGeneration?.Invoke(this, EventArgs.Empty);
				};
			}
		}

		private class CeitonTaskSection : Section, ICeitonSection
		{
			private readonly Button randomIdButton = new Button("Enter Random Guid") { Width = 150 };

			public CeitonTaskSection()
			{
				Initialize();
				GenerateUi();
			}

			public TextBox IdTextBox = new TextBox(Guid.NewGuid().ToString());

			public DropDown StateDropDown { get; } = new DropDown(Enum.GetNames(typeof(TaskStates)), Enum.GetName(typeof(TaskStates), TaskStates.Confirmed));

			public CheckBox IsCanceledCheckBox { get; } = new CheckBox("Canceled?");

			public TextBox CommentTextBox { get; } = new TextBox();

			public YleDateTimePicker DatePicker { get; } = new YleDateTimePicker { DateTimeFormat = Automation.DateTimeFormat.LongDate };

			public TimePicker StartTimePicker { get; } = new TimePicker();

			public TimePicker EndTimePicker { get; } = new TimePicker();

			public TextBox ActivityTypeTextBox { get; } = new TextBox();

			public TextBox ActivityNameTextBox { get; } = new TextBox();

			public TextBox ResourceIdTextBox { get; } = new TextBox();

			public TextBox ResourceNameTextBox { get; } = new TextBox();

			public CheckBox IsFlexibleCheckBox { get; } = new CheckBox("Flexible?");

			public Button DeleteTaskButton { get; } = new Button("Delete Task");

			public Data[] Data {
				get
				{
					return new Data[]
					{
						new Data()
						{
							TaskId = IdTextBox.Text,
							TaskStatus = (int)Enum.Parse(typeof(TaskStates), StateDropDown.Selected),
							TaskIsCanceled = IsCanceledCheckBox.IsChecked.ToString(),
							TaskComment = CommentTextBox.Text,
							TaskIsFlexible = IsFlexibleCheckBox.IsChecked.ToString(),
							StartTime = StartTimePicker.Time.ToString(@"hh\:mm"),
							EndTime = EndTimePicker.Time.ToString(@"hh\:mm"),
							Date = DatePicker.DateTime.ToString("dd.MM.yyyy"),
							DurationAsString = Math.Abs(Math.Round(EndTimePicker.Time.Subtract(StartTimePicker.Time).TotalMinutes)).ToString(),
							ActivityTypeId = ActivityTypeTextBox.Text,
							ActivityTypeName = ActivityNameTextBox.Text,
							ResourceId = ResourceIdTextBox.Text,
							ResourceName = ResourceNameTextBox.Text,
						}
					};
				}
			}

			public event EventHandler TriggerUiGeneration;

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(new Label("Task ID"), ++row, 0);
				AddWidget(IdTextBox, row, 1);
				AddWidget(randomIdButton, row, 2);

				AddWidget(new Label("Task Status"), ++row, 0);
				AddWidget(StateDropDown, row, 1);

				AddWidget(new Label("Is Canceled"), ++row, 0);
				AddWidget(IsCanceledCheckBox, row, 1);

				AddWidget(new Label("Comments"), ++row, 0);
				AddWidget(CommentTextBox, row, 1);

				AddWidget(new Label("Is Flexible"), ++row, 0);
				AddWidget(IsFlexibleCheckBox, row, 1);

				AddWidget(new Label("Start Time"), ++row, 0);
				AddWidget(StartTimePicker, row, 1);

				AddWidget(new Label("End Time"), ++row, 0);
				AddWidget(EndTimePicker, row, 1);

				AddWidget(new Label("Date"), ++row, 0);
				AddWidget(DatePicker, row, 1);

				AddWidget(new Label("Activity Type"), ++row, 0);
				AddWidget(ActivityTypeTextBox, row, 1);

				AddWidget(new Label("Activity Name"), ++row, 0);
				AddWidget(ActivityNameTextBox, row, 1);

				AddWidget(new Label("Resource ID"), ++row, 0);
				AddWidget(ResourceIdTextBox, row, 1);

				AddWidget(new Label("Resource Name"), ++row, 0);
				AddWidget(ResourceNameTextBox, row, 1);

				AddWidget(DeleteTaskButton, row + 1, 0, 1, 2);
			}

			private void Initialize()
			{
				randomIdButton.Pressed += (s, e) => IdTextBox.Text = Guid.NewGuid().ToString();
			}
		}

		private enum TaskStates
		{
			Deleted = 0,
			Unactivated = 1,
			PrePlanned = 2,
			Planned = 3,
			Confirmed = 4,
			Completed = 5,
		}
	}
}
