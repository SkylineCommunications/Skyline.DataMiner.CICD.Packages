namespace ShowPlasmaDetails_2.Sections
{
	using System;
	using System.Linq;
	using Plasma;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class PlasmaProgramTechnicalDetailsSection : Section
	{
		private readonly Label titleLabel = new Label("Technical Details") {Style = TextStyle.Bold};
		private readonly CollapseButton technicalDetailsButton;
		private readonly Label productionYearLabel = new Label("PRODUCTION YEAR");
		private readonly Label projectNumberLabel = new Label("PROJECT NUMBER");
		private readonly Label productNumberLabel = new Label("PRODUCT NUMBER");
		private readonly Label productionNumberLabel = new Label("PRODUCTION NUMBER");
		private readonly Label programProductionFormLabel = new Label("PROGRAM PRODUCTIONFORM");
		private readonly Label originalLanguageLabel = new Label("ORIGINAL LANGUAGE");
		private readonly Label audioTracksLabel = new Label("AUDIO TRACKS");
		private readonly Label seriesEpisodeOriginalTitleLabel = new Label("SERIES EPISODE ORIGINAL TITLE");

		private readonly Label productionYearValue;
		private readonly Label projectNumberValue;
		private readonly Label productNumberValue;
		private readonly Label productionNumberValue;
		private readonly Label programProductionFormValue;
		private readonly Label audioTracksValue;
		private readonly Label originalLanguageValue;
		private readonly Label seriesEpisodeOriginalTitleValue;

		public PlasmaProgramTechnicalDetailsSection(Engine engine, ParsedPlasmaOrder plasmaOrder, int columnOffset = 0)
		{
			productionYearValue = new UIDetailValueLabel(Convert.ToString(plasmaOrder.Program.ProductionYear));
			projectNumberValue = new UIDetailValueLabel(Convert.ToString(plasmaOrder.Program.ProjectNumber));
			productNumberValue = new UIDetailValueLabel(plasmaOrder.Program.ProductNumber);
			productionNumberValue = new UIDetailValueLabel(plasmaOrder.Program.ProductionNumber);
			programProductionFormValue = new UIDetailValueLabel(plasmaOrder.Program.ProductionForm);
			audioTracksValue = new UIDetailValueLabel(Constants.NotApplicable);
			originalLanguageValue = new UIDetailValueLabel(plasmaOrder.Program.OriginalLanguage);
			seriesEpisodeOriginalTitleValue = new UIDetailValueLabel(plasmaOrder.Program.EpisodeOriginalTitle);

			technicalDetailsButton = new CollapseButton(new[]
				{
					productionYearLabel, projectNumberLabel, productNumberLabel, productionNumberLabel, programProductionFormLabel, originalLanguageLabel,
					audioTracksLabel, seriesEpisodeOriginalTitleLabel, productionYearValue, projectNumberValue, productNumberValue, productionNumberValue, programProductionFormValue, audioTracksValue, originalLanguageValue, seriesEpisodeOriginalTitleValue
				}, true)
				{CollapseText = "-", ExpandText = "+", Width = 44};

			GenerateUI(columnOffset);
		}

		public Label Title => titleLabel;

		public CollapseButton CollapseButton => technicalDetailsButton;

		private void GenerateUI(int columnOffset)
		{
			var row = 0;

			AddWidget(technicalDetailsButton, new WidgetLayout(row, 0));
			AddWidget(titleLabel, new WidgetLayout(row, 1, 1, 5 + columnOffset));

			AddWidget(productionYearLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(productionYearValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(projectNumberLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(projectNumberValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(productNumberLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(productNumberValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(productionNumberLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(productionNumberValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(programProductionFormLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(programProductionFormValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(audioTracksLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(audioTracksValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(originalLanguageLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(originalLanguageValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(seriesEpisodeOriginalTitleLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(seriesEpisodeOriginalTitleValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));
		}
	}
}