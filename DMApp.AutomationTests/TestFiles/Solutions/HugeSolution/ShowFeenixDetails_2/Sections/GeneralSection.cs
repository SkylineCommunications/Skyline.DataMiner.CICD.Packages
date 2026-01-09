namespace ShowFeenixDetails_2.Sections
{
	using Feenix;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public class GeneralSection : Section
	{
		private readonly Label generalSectionLabel = new Label("GENERAL") { Style = TextStyle.Bold };

		private readonly Label titleMainLabel = new Label("MAIN TITLE");
		private readonly Label titlePromoLabel = new Label("PROMO TITLE");
		private readonly Label yleIdLabel = new Label("YLE ID");
		private readonly Label ageRestrictionLabel = new Label("AGE RESTRICTION");
		private readonly Label plasmaIdLabel = new Label("PLASMA ID");
		private readonly Label finnishMainDescLabel = new Label("FINNISH MAIN DESCRIPTION");
		private readonly Label swedishMainDescLabel = new Label("SWEDISH MAIN DESCRIPTION");
		private readonly Label samiMainDescLabel = new Label("SAMI MAIN DESCRIPTION");
		private readonly Label finnishShortDescLabel = new Label("FINNISH SHORT DESCRIPTION");
		private readonly Label swedishShortDescLabel = new Label("SWEDISH SHORT DESCRIPTION");
		private readonly Label samiShortDescLabel = new Label("SAMI SHORT DESCRIPTION");

		private readonly Label titleMainValue;
		private readonly Label titlePromoValue;
		private readonly Label yleIdValue;
		private readonly Label ageRestrictionValue;
		private readonly Label plasmaIdValue;
		private readonly Label finnishMainDescValue;
		private readonly Label swedishMainDescValue;
		private readonly Label samiMainDescValue;
		private readonly Label finnishShortDescValue;
		private readonly Label swedishShortDescValue;
		private readonly Label samiShortDescValue;

		public GeneralSection(OrderGeneral orderGeneral)
		{
			if (orderGeneral.TitleMainFin != Constants.NotFound)
			{
				titleMainValue = new UIDetailValueLabel(orderGeneral.TitleMainFin);

			}
			else if(orderGeneral.TitleMainSwe != Constants.NotFound)
			{
				titleMainValue = new UIDetailValueLabel(orderGeneral.TitleMainSwe);

			}
			else
			{
				titleMainValue = new UIDetailValueLabel(orderGeneral.TitleMainSmi);
			}

			titlePromoValue = new UIDetailValueLabel(orderGeneral.TitlePromoFin);
			yleIdValue = new UIDetailValueLabel(orderGeneral.YleId);
			ageRestrictionValue = new UIDetailValueLabel(orderGeneral.ContentRatingAgeRestriction);
			plasmaIdValue = new UIDetailValueLabel(orderGeneral.PlasmaId);
			finnishMainDescValue = new UIDetailValueLabel(orderGeneral.DescriptionMainFin);
			swedishMainDescValue = new UIDetailValueLabel(orderGeneral.DescriptionMainSwe);
			samiMainDescValue = new UIDetailValueLabel(orderGeneral.DescriptionMainSmi);
			finnishShortDescValue = new UIDetailValueLabel(orderGeneral.DescriptionShortFin);
			swedishShortDescValue = new UIDetailValueLabel(orderGeneral.DescriptionShortSwe);
			samiShortDescValue = new UIDetailValueLabel(orderGeneral.DescriptionShortSmi);

			GenerateUI();
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(generalSectionLabel, new WidgetLayout(row, 0, 1, 4));

			AddWidget(titleMainLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(titleMainValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(titlePromoLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(titlePromoValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(yleIdLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(yleIdValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(ageRestrictionLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(ageRestrictionValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(plasmaIdLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(plasmaIdValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(finnishMainDescLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(finnishMainDescValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(swedishMainDescLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(swedishMainDescValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(samiMainDescLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(samiMainDescValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(finnishShortDescLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(finnishShortDescValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(swedishShortDescLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(swedishShortDescValue, new WidgetLayout(row, 5, 1, 10));

			AddWidget(samiShortDescLabel, new WidgetLayout(++row, 0, 1, 4));
			AddWidget(samiShortDescValue, new WidgetLayout(row, 5, 1, 10));
		}
	}

}