using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.History
{
	public class OrderHistoryChapterSection : Section
	{
		private readonly OrderHistoryChapter chapter;

		private CollapseButton collapseButton;
		private Label header;

		ClassChangeSection orderClassSection;

		List<ServiceChangeSection> serviceChangeSections = new List<ServiceChangeSection>();

		public OrderHistoryChapterSection(OrderHistoryChapter chapter)
		{
			this.chapter = chapter;

			Initialize();
			GenerateUi();
			UpdateWidgetVisibility();
		}

		private void Initialize()
		{
			header = new Label($"{chapter.UserName} on {chapter.Timestamp} using {chapter.ScriptName}") { Style = TextStyle.Bold };

			collapseButton = new YleCollapseButton(true);
			collapseButton.Pressed += (s, a) => UpdateWidgetVisibility();

			var classChangeSectionConfiguration = new ClassChangeSectionConfiguration
			{
				HiddenSections = new List<string> {
					nameof(LiteOrder.SportsPlanning),
					nameof(LiteOrder.NewsInformation),
					nameof(LiteOrder.BillingInfo),
					nameof(LiteOrder.UserGroupIds)}
			};

			orderClassSection = new ClassChangeSection(chapter.OrderChange, classChangeSectionConfiguration);

			serviceChangeSections = chapter.OrderChange.ServiceChanges.Select(sc => new ServiceChangeSection(sc)).ToList();
		}

		private void UpdateWidgetVisibility()
		{
			orderClassSection.IsVisible = !collapseButton.IsCollapsed;

			foreach (var serviceChangeSection in serviceChangeSections)
			{
				serviceChangeSection.IsVisible = !collapseButton.IsCollapsed;
			}
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(collapseButton, ++row, 0);
			AddWidget(header, row, 1, 1, 5);

			AddSection(orderClassSection, new SectionLayout(++row, 2));
			row += orderClassSection.RowCount;

			foreach (var serviceChangeSection in serviceChangeSections)
			{
				AddSection(serviceChangeSection, new SectionLayout(++row, 1));
				row += serviceChangeSection.RowCount;
			}
		}
	}
}
