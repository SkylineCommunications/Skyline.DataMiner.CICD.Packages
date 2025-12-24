namespace ShowCeitonDetails_2.Sections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ShowCeitonDetails_2.Ceiton;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class TasksTableSection : Section
	{
		private readonly Engine engine;

		private static readonly List<string> columnHeaders = new List<string> {
			"STATUS",
			"IS CANCELLED",
			"IS FLEXIBLE",
			"START TIME",
			"END TIME",
			"DURATION",
			"ACTIVITY TYPE",
			"RESOURCE ID",
			"RESOURCE TYPE",
			"RESOURCE NAME",
			"EQUIPMENT NAME",
			"COMMENT",
		};
		private readonly List<Label> columnHeaderLabels = new List<Label>();

		private readonly Label title;
		private readonly CollapseButton collapseButton;
	
		private readonly Label noTasksLabel = new Label("No Ceiton task details available for this event");

		private readonly Label sortOnLabel = new Label("Sort On");
		private readonly DropDown sortOnDropdown;

		private readonly Label filterLabel = new Label("Filter");
		private readonly TextBox filterTextBox = new TextBox { PlaceHolder = "Filter..." };

		private readonly TaskSection[] taskSections;

		public TasksTableSection(Engine engine, IEnumerable<Task> tasks, string title, IEnumerable<Product> products = null)
		{
			this.engine = engine;
			this.title = new Label(title) { Style = TextStyle.Bold };
			taskSections = CreateSections(tasks, products);

			foreach (var columnHeader in columnHeaders)
			{
				Label labelToAdd = new Label(columnHeader);
				columnHeaderLabels.Add(labelToAdd);
			}

			List<string> sortOptions = columnHeaders;
			if (products != null)
			{
				sortOptions.Add("PRODUCT NAME");
				columnHeaderLabels.Insert(0, new Label("PRODUCT NAME"));
			}

			sortOnDropdown = new DropDown { Options = sortOptions.OrderBy(x => x).ToArray(), Selected = "START TIME", IsDisplayFilterShown = true };
			sortOnDropdown.Changed += (s, e) =>
			{
				GenerateUI();
				RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
			};

			filterTextBox.Changed += (s, e) =>
			{
				GenerateUI();
				RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
			};

			IEnumerable<Widget> widgetsToCollapse = taskSections.SelectMany(x => x.CollapseableWidgets);
			collapseButton = new CollapseButton(columnHeaderLabels.Concat(widgetsToCollapse), isCollapsed: false) { CollapseText = "-", ExpandText = "+", Width = 44 };
			collapseButton.Pressed += CollapseButton_Pressed;

			GenerateUI();
		}

		private static TaskSection[] SortTaskSections(TaskSection[] sections, string sortOn)
		{
			switch (sortOn)
			{
				case "STATUS":
					return sections.OrderBy(t => t.Task.Status).ToArray();
				case "IS CANCELLED":
					return sections.OrderBy(t => t.Task.IsCancelled).ToArray();
				case "IS FLEXIBLE":
					return sections.OrderBy(t => t.Task.IsFlexible).ToArray();
				case "START TIME":
					return sections.OrderBy(t => t.Task.StartTime).ToArray();
				case "END TIME":
					return sections.OrderBy(t => t.Task.EndTime).ToArray();
				case "DURATION":
					return sections.OrderBy(t => t.Task.Duration).ToArray();
				case "ACTIVITY TYPE":
					return sections.OrderBy(t => t.Task.ActivityType).ToArray();
				case "RESOURCE ID":
					return sections.OrderBy(t => t.Task.ResourceId).ToArray();
				case "RESOURCE TYPE":
					return sections.OrderBy(t => t.Task.ResourceType).ToArray();
				case "RESOURCE NAME":
					return sections.OrderBy(t => t.Task.ResourceName).ToArray();
				case "EQUIPMENT NAME":
					return sections.OrderBy(t => t.Task.EquipmentName).ToArray();
				case "COMMENT":
					return sections.OrderBy(t => t.Task.Comment).ToArray();
				case "PRODUCT NAME":
					return sections.OrderBy(t => t.ProductName).ToArray();
				default:
					return sections;
			}
		}

		private static TaskSection[] FilterTaskSections(TaskSection[] sections, string filter)
		{
			if (String.IsNullOrWhiteSpace(filter)) return sections;

			HashSet<TaskSection> result = new HashSet<TaskSection>();
			foreach (var section in sections)
			{
				var valuesToCheck = new List<string>
				{
					section.Task.Status,
					section.Task.IsCancelled,
					section.Task.IsFlexible,
					section.Task.StartTime.ToString(),
					section.Task.EndTime.ToString(),
					section.Task.Duration.ToString(),
					section.Task.ActivityType,
					section.Task.ResourceId,
					section.Task.ResourceType,
					section.Task.ResourceName,
					section.Task.EquipmentName,
					section.Task.Comment,
				};

				foreach (var valueToCheck in valuesToCheck)
				{
					if (valueToCheck.ToLower().Contains(filter.ToLower()))
					{
						result.Add(section);
						break;
					}
				}
			}

			return result.ToArray();
		}

		public event EventHandler RegenerateUiRequired;

		private void CollapseButton_Pressed(object sender, EventArgs e)
		{
			if (this.collapseButton.IsCollapsed)
			{
				foreach (TaskSection taskSection in taskSections)
				{
					taskSection.CollapseButton.IsCollapsed = this.collapseButton.IsCollapsed;
				}
			}
		}

		private TaskSection[] CreateSections(IEnumerable<Task> tasks, IEnumerable<Product> products = null)
		{
			List<TaskSection> createdTasksSections = new List<TaskSection>();
			foreach (Task task in tasks)
			{
				createdTasksSections.Add(new TaskSection(engine, task, products));
			}

			return createdTasksSections.ToArray();
		}

		private void GenerateUI()
		{
			Clear();

			int row = 0;
			int column = 0;

			AddWidget(collapseButton, new WidgetLayout(row, column));
			AddWidget(title, new WidgetLayout(row, ++column, 1, 2));

			if (taskSections.Any())
			{
				var filteredTaskSections = FilterTaskSections(taskSections, filterTextBox.Text);
				var filteredAndSortedTaskSections = SortTaskSections(filteredTaskSections, sortOnDropdown.Selected);

				AddWidget(sortOnLabel, ++row, column, HorizontalAlignment.Left, VerticalAlignment.Center);
				AddWidget(sortOnDropdown, row, ++column, HorizontalAlignment.Left, VerticalAlignment.Center);

				column -= 1;
				AddWidget(filterLabel, ++row, column, HorizontalAlignment.Left, VerticalAlignment.Center);
				AddWidget(filterTextBox, row, ++column, HorizontalAlignment.Left, VerticalAlignment.Center);

				++row;
				column = 1;

				foreach (var item in columnHeaderLabels)
				{
					AddWidget(item, new WidgetLayout(row, ++column));
				}

				foreach (TaskSection taskSection in filteredAndSortedTaskSections)
				{
					AddSection(taskSection, new SectionLayout(++row, 1));
					row = RowCount;
				}
			}
			else
			{
				AddWidget(noTasksLabel, new WidgetLayout(++row, 1, 1, 4));
			}
		}
	}

}