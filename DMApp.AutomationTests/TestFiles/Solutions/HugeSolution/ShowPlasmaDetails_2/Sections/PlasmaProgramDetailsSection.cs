namespace ShowPlasmaDetails_2.Sections
{
	using System;
	using System.Collections.Generic;
	using Plasma;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class PlasmaProgramDetailsSection : Section
	{
		private readonly Label titleLabel = new Label("PROGRAM INFORMATION") {Style = TextStyle.Bold};
		private readonly CollapseButton programDetailsCollapseButton;

		private readonly Label programCategoryLabel = new Label("PROGRAM CATEGORY");
		private readonly Label programSubcategoryLabel = new Label("PROGRAM SUBCATEGORY");
		private readonly Label finnishSeriesNameLabel = new Label("FINNISH SERIES NAME");
		private readonly Label swedishSeriesNameLabel = new Label("SWEDISH SERIES NAME");
		private readonly Label originalSeriesNameLabel = new Label("ORIGINAL SERIES NAME");
		private readonly Label programDurationLabel = new Label("PROGRAM DURATION");
		private readonly Label plasmaIdLabel = new Label("PLASMA ID");
		private readonly Label seriesIdLabel = new Label("SERIES ID");
		private readonly Label workingTitleLabel = new Label("WORKING TITLE");
		private readonly Label productionTitleLabel = new Label("PRODUCTION TITLE");
		private readonly Label originalTitleLabel = new Label("ORIGINAL TITLE");
		private readonly Label finnishDescriptionLabel = new Label("FINNISH DESCRIPTION");
		private readonly Label swedishDescriptionLabel = new Label("SWEDISH DESCRIPTION");
		private readonly Label finnishPublicationNameLabel = new Label("FINNISH PUBLICATION NAME");
		private readonly Label swedishPublicationNameLabel = new Label("SWEDISH PUBLICATION NAME");
		private readonly Label finnishMainTitleLabel = new Label("FINNISH MAIN TITLE");
		private readonly Label swedishMainTitleLabel = new Label("SWEDISH MAIN TITLE");
		private readonly Label finnishEpisodeTitleLabel = new Label("FINNISH EPISODE TITLE");
		private readonly Label swedishEpisodeTitleLabel = new Label("SWEDISH EPISODE TITLE");
		private readonly Label finnishMainDescriptionLabel = new Label("FINNISH MAIN DESCRIPTION");
		private readonly Label swedishMainDescriptionLabel = new Label("SWEDISH MAIN DESCRIPTION");
		private readonly Label yleIdLabel = new Label("YLE ID");

		private readonly Label workingTitleValue;
		private readonly Label productionTitleValue;
		private readonly Label originalTitleValue;
		private readonly Label plasmaIdValue;
		private readonly Label finnishSeriesNameValue;
		private readonly Label swedishSeriesNameValue;
		private readonly Label originalSeriesNameValue;
		private readonly Label seriesIdValue;
		private readonly Label programDurationValue;
		private readonly Label programCategoryValue;
		private readonly Label programSubcategoryValue;
		private readonly Label finnishDescriptionValue;
		private readonly Label swedishDescriptionValue;
		private readonly Label finnishPublicationNameValue;
		private readonly Label swedishPublicationNameValue;
		private readonly Label finnishMainTitleValue;
		private readonly Label swedishMainTitleValue;
		private readonly Label finnishEpisodeTitleValue;
		private readonly Label swedishEpisodeTitleValue;
		private readonly Label finnishMainDescriptionValue;
		private readonly Label swedishMainDescriptionValue;
		private readonly Label yleIdValue;

		public PlasmaProgramDetailsSection(Engine engine, ParsedPlasmaOrder plasmaOrder)
		{
			workingTitleValue = new UIDetailValueLabel(plasmaOrder.Program.WorkingTitle);
			productionTitleValue = new UIDetailValueLabel(plasmaOrder.Program.ProductionTitle);
			originalTitleValue = new UIDetailValueLabel( plasmaOrder.Program.OriginalTitle);
			plasmaIdValue = new UIDetailValueLabel(plasmaOrder.Program.PlasmaId);
			finnishSeriesNameValue = new UIDetailValueLabel(plasmaOrder.Series?.FinnishPublicationName ?? Constants.NotApplicable);
			swedishSeriesNameValue = new UIDetailValueLabel(plasmaOrder.Series?.SwedishPublicationName ?? Constants.NotApplicable);
			originalSeriesNameValue = new UIDetailValueLabel(plasmaOrder.Series?.OriginalName ?? Constants.NotApplicable);
			seriesIdValue = new UIDetailValueLabel(plasmaOrder.Program.SeriesId);
			programDurationValue = new UIDetailValueLabel(Convert.ToString(plasmaOrder.Program.Duration));
			programCategoryValue = new UIDetailValueLabel(((ProgramCategory)plasmaOrder.Program.YleMainCategory).GetDescription());
			programSubcategoryValue = new UIDetailValueLabel(plasmaOrder.Program.YleSubCategory);
			finnishDescriptionValue = new UIDetailValueLabel(plasmaOrder.Program.FinnishDescription);
			swedishDescriptionValue = new UIDetailValueLabel(plasmaOrder.Program.SwedishDescription);
			finnishPublicationNameValue = new UIDetailValueLabel(plasmaOrder.Program.FinnishPublicationName);
			swedishPublicationNameValue = new UIDetailValueLabel(plasmaOrder.Program.SwedishPublicationName);
			finnishMainTitleValue = new UIDetailValueLabel(plasmaOrder.Seasons?.FinnishPublicationName ?? Constants.NotApplicable);
			swedishMainTitleValue = new UIDetailValueLabel(plasmaOrder.Seasons?.SwedishPublicationName ?? Constants.NotApplicable);
			finnishEpisodeTitleValue = new UIDetailValueLabel(plasmaOrder.Program.EpisodeOriginalTitle);
			swedishEpisodeTitleValue = new UIDetailValueLabel(plasmaOrder.Program.EpisodeOriginalTitle);
			finnishMainDescriptionValue = new UIDetailValueLabel(plasmaOrder.Program.FinnishDescription);
			swedishMainDescriptionValue = new UIDetailValueLabel(plasmaOrder.Program.SwedishDescription);
			yleIdValue = new UIDetailValueLabel(plasmaOrder.Program.YleId);

			PlasmaProgramSeasonInformationSection = new PlasmaProgramSeasonInformationSection(engine, plasmaOrder.Seasons, -1);
			PlasmaProgramCompanyAndPeoplesSection = new PlasmaProgramCompanyAndPeopleSection(engine, plasmaOrder, -1);
			PlasmaProgramTechnicalDetailsSection = new PlasmaProgramTechnicalDetailsSection(engine, plasmaOrder, -1);
			PlasmaProgramTranslationsAndAudioMixingSection = new PlasmaProgramTranslationsAndAudioMixingSection(engine, plasmaOrder, -1);

			var linkedWidgets = new List<Widget>
			{
				programCategoryLabel,
				programSubcategoryLabel,
				finnishSeriesNameLabel,
				swedishSeriesNameLabel,
				originalSeriesNameLabel,
				programDurationLabel,
				plasmaIdLabel,
				seriesIdLabel,
				workingTitleLabel,
				productionTitleLabel,
				originalTitleLabel,
				finnishDescriptionLabel,
				swedishDescriptionLabel,
				finnishPublicationNameLabel,
				swedishPublicationNameLabel,
				finnishMainTitleLabel,
				swedishMainTitleLabel,
				finnishEpisodeTitleLabel,
				swedishEpisodeTitleLabel,
				finnishMainDescriptionLabel,
				swedishMainDescriptionLabel,
				yleIdLabel,
				workingTitleValue,
				productionTitleValue,
				originalTitleValue,
				plasmaIdValue,
				finnishSeriesNameValue,
				swedishSeriesNameValue,
				originalSeriesNameValue,
				seriesIdValue,
				programDurationValue,
				programCategoryValue,
				programSubcategoryValue,
				finnishDescriptionValue,
				swedishDescriptionValue,
				finnishPublicationNameValue,
				swedishPublicationNameValue,
				finnishMainTitleValue,
				swedishMainTitleValue,
				finnishEpisodeTitleValue,
				swedishEpisodeTitleValue,
				finnishMainDescriptionValue,
				swedishMainDescriptionValue,
				yleIdValue,
				PlasmaProgramSeasonInformationSection.Title,
				PlasmaProgramSeasonInformationSection.CollapseButton,
				PlasmaProgramCompanyAndPeoplesSection.Title,
				PlasmaProgramCompanyAndPeoplesSection.CollapseButton,
				PlasmaProgramTechnicalDetailsSection.Title,
				PlasmaProgramTechnicalDetailsSection.CollapseButton,
				PlasmaProgramTranslationsAndAudioMixingSection.Title,
				PlasmaProgramTranslationsAndAudioMixingSection.CollapseButton
			};

			programDetailsCollapseButton = new CollapseButton(linkedWidgets, true) {CollapseText = "-", ExpandText = "+", Width = 44};

			GenerateUI();
		}

		private PlasmaProgramSeasonInformationSection PlasmaProgramSeasonInformationSection { get; set; }

		private PlasmaProgramCompanyAndPeopleSection PlasmaProgramCompanyAndPeoplesSection { get; set; }

		private PlasmaProgramTechnicalDetailsSection PlasmaProgramTechnicalDetailsSection { get; set; }

		private PlasmaProgramTranslationsAndAudioMixingSection PlasmaProgramTranslationsAndAudioMixingSection { get; set; }

		private void GenerateUI()
		{
			var row = 0;

			AddWidget(programDetailsCollapseButton, new WidgetLayout(row, 0));
			AddWidget(titleLabel, new WidgetLayout(row, 1, 1, 5));

			//AddWidget(workingTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			//AddWidget(workingTitleValue, new WidgetLayout(row, 5, 1, 1));

			//AddWidget(productionTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			//AddWidget(productionTitleValue, new WidgetLayout(row, 5, 1, 1));

			//AddWidget(originalTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			//AddWidget(originalTitleValue, new WidgetLayout(row, 5, 1, 1));

			//AddWidget(swedishDescriptionLabel, new WidgetLayout(++row, 1, 1, 3));
			//AddWidget(swedishDescriptionValue, new WidgetLayout(row, 5, 1, 1));

			//AddWidget(finnishDescriptionLabel, new WidgetLayout(++row, 1, 1, 3));
			//AddWidget(finnishDescriptionValue, new WidgetLayout(row, 5, 1, 1));

			//AddWidget(finnishSeriesNameLabel, new WidgetLayout(++row, 1, 1, 3));
			//AddWidget(finnishSeriesNameValue, new WidgetLayout(row, 5, 1, 1));

			//AddWidget(swedishSeriesNameLabel, new WidgetLayout(++row, 1, 1, 3));
			//AddWidget(swedishSeriesNameValue, new WidgetLayout(row, 5, 1, 1));

			//AddWidget(originalSeriesNameLabel, new WidgetLayout(++row, 1, 1, 3));
			//AddWidget(originalSeriesNameValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(workingTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(workingTitleValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(productionTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(productionTitleValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(originalTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(originalTitleValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(originalSeriesNameLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(originalSeriesNameValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(finnishMainTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(finnishMainTitleValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(finnishMainDescriptionLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(finnishMainDescriptionValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(finnishDescriptionLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(finnishDescriptionValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(finnishPublicationNameLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(finnishPublicationNameValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(finnishSeriesNameLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(finnishSeriesNameValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(finnishEpisodeTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(finnishEpisodeTitleValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(swedishMainTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(swedishMainTitleValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(swedishMainDescriptionLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(swedishMainDescriptionValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(swedishDescriptionLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(swedishDescriptionValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(swedishPublicationNameLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(swedishPublicationNameValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(swedishSeriesNameLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(swedishSeriesNameValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(swedishEpisodeTitleLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(swedishEpisodeTitleValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(programDurationLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(programDurationValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(programCategoryLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(programCategoryValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(programSubcategoryLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(programSubcategoryValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(seriesIdLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(seriesIdValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(plasmaIdLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(plasmaIdValue, new WidgetLayout(row, 5, 1, 1));

			AddWidget(yleIdLabel, new WidgetLayout(++row, 1, 1, 3));
			AddWidget(yleIdValue, new WidgetLayout(row, 5, 1, 1));

			AddSection(PlasmaProgramSeasonInformationSection, new SectionLayout(++row, 1));
			row += PlasmaProgramSeasonInformationSection.RowCount;

			AddSection(PlasmaProgramCompanyAndPeoplesSection, new SectionLayout(++row, 1));
			row += PlasmaProgramCompanyAndPeoplesSection.RowCount;

			AddSection(PlasmaProgramTechnicalDetailsSection, new SectionLayout(++row, 1));
			row += PlasmaProgramTechnicalDetailsSection.RowCount;

			AddSection(PlasmaProgramTranslationsAndAudioMixingSection, new SectionLayout(++row, 1));
		}
	}
}