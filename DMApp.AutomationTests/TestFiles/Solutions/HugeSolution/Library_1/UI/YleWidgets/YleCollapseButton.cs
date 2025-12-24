namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class YleCollapseButton : CollapseButton, IYleWidget
	{
		public YleCollapseButton(bool isCollapsed = false)
			: base(isCollapsed)
		{
			CollapseText = "-";
			ExpandText = "+";
			Width = 44;

			Pressed += YleCollapseButton_Pressed;
		}

		public YleCollapseButton(IEnumerable<Widget> linkedWidgets, bool isCollapsed)
			: base(linkedWidgets, isCollapsed)
		{
			CollapseText = "-";
			ExpandText = "+";
			Width = 44;

			Pressed += YleCollapseButton_Pressed;
		}

		public Guid Id { get; set; }
		
		public string Name { get; set; }
		
		public Helpers Helpers { get; set; }

		private void YleCollapseButton_Pressed(object sender, EventArgs e)
		{
			Helpers?.Log(nameof(YleCollapseButton), nameof(YleCollapseButton_Pressed), $"USER INPUT: user pressed collapse button. CollapseButton Name='{Name}'. ID='{Id}'");
		}
	}
}