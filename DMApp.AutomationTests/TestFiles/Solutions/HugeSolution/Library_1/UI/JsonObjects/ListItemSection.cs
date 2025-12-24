namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.JsonObjects
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ListItemSection : Section
	{
		private readonly YleButton deleteButton = new YleButton("Delete");

		public ListItemSection(Section section)
		{
			deleteButton.Pressed += (s, e) => Deleted?.Invoke(this, section);

			AddSection(section, new SectionLayout(0, 0));
			AddWidget(deleteButton, RowCount, ColumnCount - 1);
		}

		public event EventHandler<Section> Deleted;
	}

}
