namespace ShowPlasmaDetails_2.Sections
{
	using System;
	using Plasma;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class PlasmaProgramSeasonInformationSection : Section
	{
		private readonly Label titleLabel = new Label("SEASON INFORMATION") {Style = TextStyle.Bold};
		private readonly CollapseButton seasonInfoCollapseButton;

		private readonly Label seasonNumberLabel = new Label("SEASON NUMBER");
		private readonly Label seasonIdLabel = new Label("SEASON ID");
		private readonly Label finnishSeasonNameLabel = new Label("FINNISH SEASON NAME");
		private readonly Label swedishSeasonNameLabel = new Label("SWEDISH SEASON NAME");
		private readonly Label originalSeasonNameLabel = new Label("ORIGINAL SEASON NAME");
		private readonly Label episodeNumberLabel = new Label("EPISODE NUMBER");

		private readonly Label seasonNumberValue;
		private readonly Label seasonIdValue;
		private readonly Label finnishSeasonNameValue;
		private readonly Label swedishSeasonNameValue;
		private readonly Label originalSeasonNameValue;
		private readonly Label episodeNumberValue;

		public PlasmaProgramSeasonInformationSection(Engine engine, ParsedSeason plasmaSeasonInfo, int columnOffset = 0)
		{
			seasonNumberValue = new UIDetailValueLabel(plasmaSeasonInfo?.SeasonNumber ?? Constants.NotApplicable);
			seasonIdValue = new UIDetailValueLabel(Convert.ToString(plasmaSeasonInfo?.Id ?? Constants.NotApplicable));
			finnishSeasonNameValue = new UIDetailValueLabel(plasmaSeasonInfo?.FinnishPublicationName ?? Constants.NotApplicable);
			swedishSeasonNameValue = new UIDetailValueLabel(plasmaSeasonInfo?.SwedishPublicationName ?? Constants.NotApplicable );
			originalSeasonNameValue = new UIDetailValueLabel(plasmaSeasonInfo?.OriginalName ?? Constants.NotApplicable);
			episodeNumberValue = new UIDetailValueLabel(Convert.ToString(plasmaSeasonInfo?.SeriesId ?? Constants.NotApplicable));

			seasonInfoCollapseButton = new CollapseButton(new[] {seasonNumberLabel, seasonIdLabel, finnishSeasonNameLabel, swedishSeasonNameLabel, originalSeasonNameLabel, episodeNumberLabel, seasonNumberValue, seasonIdValue, finnishSeasonNameValue, swedishSeasonNameValue, originalSeasonNameValue, episodeNumberValue}, true) {CollapseText = "-", ExpandText = "+", Width = 44};

			GenerateUI(columnOffset);
		}

		private void GenerateUI(int columnOffset)
		{
			var row = 0;

			AddWidget(seasonInfoCollapseButton, new WidgetLayout(row, 0));
			AddWidget(titleLabel, new WidgetLayout(row, 1, 1, 5 + columnOffset));

			AddWidget(seasonNumberLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(seasonNumberValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(seasonIdLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(seasonIdValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(finnishSeasonNameLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(finnishSeasonNameValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(swedishSeasonNameLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(swedishSeasonNameValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(originalSeasonNameLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(originalSeasonNameValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(episodeNumberLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(episodeNumberValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));
		}

		public Label Title => titleLabel;

		public CollapseButton CollapseButton => seasonInfoCollapseButton;
	}
}