namespace Debug_2.Debug.NonLive
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class HowToSection : YleSection
	{
		private readonly YleCollapseButton howToCollapseButton;
		private readonly Label howToLabel = new Label("How To") { Style = TextStyle.Title };

		private readonly YleCollapseButton howToRetrieveTicketsForCertainNonLiveOrderTypeCollapseButton;
		private readonly Label howToRetrieveTicketsForCertainNonLiveOrderTypeHeaderLabel = new Label("Retrieve tickets for certain Non-Live order type") { Style = TextStyle.Heading };
		private readonly Label howToRetrieveTicketsForCertainNonLiveOrderTypeTextLabel = new Label($"1. Add an enum property value filter\n2. As property name, fill in 'Type'\n3. As property values, fill in one of these combinations:\n{string.Join("\n", Enum.GetValues(typeof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type)).Cast<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type>().Select(e => $"{e} , {(int)e}"))}");

		public HowToSection(Helpers helpers) : base(helpers)
		{
			howToRetrieveTicketsForCertainNonLiveOrderTypeCollapseButton = new YleCollapseButton(howToRetrieveTicketsForCertainNonLiveOrderTypeTextLabel.Yield(), true);

			howToCollapseButton = new YleCollapseButton(new Widget[] { howToRetrieveTicketsForCertainNonLiveOrderTypeCollapseButton, howToRetrieveTicketsForCertainNonLiveOrderTypeHeaderLabel }, true);

			GenerateUi();
		}

		public override void RegenerateUi()
		{
			Clear();
			GenerateUi();
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			// no actions required
		}

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(howToCollapseButton, ++row, 0);
			AddWidget(howToLabel, row, 1, 1, 5);

			AddWidget(howToRetrieveTicketsForCertainNonLiveOrderTypeCollapseButton, ++row, 1);
			AddWidget(howToRetrieveTicketsForCertainNonLiveOrderTypeHeaderLabel, row, 2, 1, 5);
			AddWidget(howToRetrieveTicketsForCertainNonLiveOrderTypeTextLabel, ++row, 2, 9, 1, verticalAlignment: VerticalAlignment.Top);
		}
	}
}
