namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.History
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class OrderHistoryDialog : Dialog
	{
		private readonly List<OrderHistoryChapter> chapters;

		private List<OrderHistoryChapterSection> chapterSections;

		public OrderHistoryDialog(Helpers helpers, List<OrderHistoryChapter> chapters) : base(helpers.Engine)
		{
			Title = "Order History";

			this.chapters = chapters;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 200, Style = ButtonStyle.CallToAction };

		private void Initialize()
		{
			chapterSections = chapters.OrderBy(c => c.Timestamp).Select(c => new OrderHistoryChapterSection(c)).ToList();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 5);

			foreach (var chapterSection in chapterSections)
			{
				AddSection(chapterSection, new SectionLayout(++row, 0));
				row += chapterSection.RowCount;
			}
		}
	}
}
