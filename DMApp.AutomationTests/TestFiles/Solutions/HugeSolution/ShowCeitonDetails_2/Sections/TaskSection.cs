namespace ShowCeitonDetails_2.Sections
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ShowCeitonDetails_2.Ceiton;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class TaskSection : Section
	{
		private readonly Label productNameLabel;
		private readonly Label status;
		private readonly Label isCancelled;
		private readonly Label isFlexible;
		private readonly Label comment;
		private readonly Label startTime;
		private readonly Label endTime;
		private readonly Label duration;
		private readonly Label activityType;
		private readonly Label resourceType;
		private readonly Label equipmentName;
		private readonly Label resourceName;
		private readonly Label resourceId;

		private readonly Widget[] subTaskLabels;
		private readonly bool includeProductName;

		public TaskSection(Engine engine, Task task, IEnumerable<Product> products = null)
		{
			Task = task;

			status = new Label(task.Status);
			isCancelled = new Label(task.IsCancelled);
			isFlexible = new Label(task.IsFlexible);
			comment = new Label(task.Comment);
			startTime = new Label(task.StartTime.ToString(CultureInfo.InvariantCulture));
			endTime = new Label(task.EndTime.ToString(CultureInfo.InvariantCulture));
			duration = new Label(GetDurationString(task.Duration));
			activityType = new Label(task.ActivityType);
			resourceType = new Label(task.ResourceType);
			equipmentName = new Label(task.EquipmentName);
			resourceName = new Label(task.ResourceName);
			resourceId = new Label(task.ResourceId);

			List<Widget> createdLabels = new List<Widget>();
			foreach (SubTask subTask in task.SubTasks)
			{
				createdLabels.Add(new Label(subTask.StartTime.ToString(CultureInfo.InvariantCulture)));
				createdLabels.Add(new Label(subTask.EndTime.ToString(CultureInfo.InvariantCulture)));
				createdLabels.Add(new Label(GetDurationString(subTask.Duration)));
			}

			subTaskLabels = createdLabels.ToArray();

			CollapseButton = new CollapseButton(subTaskLabels, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44, IsEnabled = subTaskLabels.Any() };
			CollapseableWidgets = new Widget[] { status, isCancelled, isFlexible, comment, startTime, endTime, duration, activityType, resourceType, resourceName, resourceId, equipmentName, CollapseButton };

			// Filtering the matching product for a specific task
			if (products != null)
			{
				includeProductName = true;
				Product matchingProduct = products.FirstOrDefault(p => p.Number == task.ProjectOrProductId);

				if (matchingProduct != null)
				{
					ProductName = matchingProduct.Name;
					productNameLabel = new Label(matchingProduct.Name);
					CollapseableWidgets = new[] { productNameLabel }.Concat(CollapseableWidgets).ToArray();
				}
				else
				{
					productNameLabel = new Label("NA");
				}
			}

			GenerateUI();
		}

		public Task Task { get; }

		public string ProductName { get; }

		/// <summary>
		/// Gets an array containing the widgets that should be linked to a collapsebutton on higher level.
		/// </summary>
		public Widget[] CollapseableWidgets { get; private set; }

		public CollapseButton CollapseButton { get; private set; }

		private void GenerateUI()
		{
			int row = 0;
			int column = 0;

			AddWidget(CollapseButton, new WidgetLayout(row, column, HorizontalAlignment.Left));

			if (includeProductName)
			{
				AddWidget(productNameLabel, new WidgetLayout(row, ++column));
			}

			AddWidget(status, new WidgetLayout(row, ++column));
			AddWidget(isCancelled, new WidgetLayout(row, ++column));
			AddWidget(isFlexible, new WidgetLayout(row, ++column));
			AddWidget(startTime, new WidgetLayout(row, ++column));
			for (int i = 0; i < subTaskLabels.Length; i++)
			{
				AddWidget(subTaskLabels[i], new WidgetLayout(row + (i / 3) + 1, column + (i % 3)));
			}

			AddWidget(endTime, new WidgetLayout(row, ++column));
			AddWidget(duration, new WidgetLayout(row, ++column));
			AddWidget(activityType, new WidgetLayout(row, ++column));
			AddWidget(resourceId, new WidgetLayout(row, ++column));
			AddWidget(resourceType, new WidgetLayout(row, ++column));
			AddWidget(resourceName, new WidgetLayout(row, ++column));
			AddWidget(equipmentName, new WidgetLayout(row, ++column));
			AddWidget(comment, new WidgetLayout(row, ++column));
		}

		private static string GetDurationString(TimeSpan duration)
		{
			return String.Format("{0}:{1}:00", duration.Hours, duration.Minutes);
		}
	}

}