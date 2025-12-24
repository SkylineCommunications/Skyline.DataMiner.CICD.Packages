namespace ShowFeenixDetails_2.Sections
{
	using Feenix;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class SeriesSection : Section
	{
		private readonly CollapseButton seriesInfoCollapseButton;
		private readonly Label seriesInfoLabel = new Label("SERIES INFORMATION") { Style = TextStyle.Bold };

		private readonly Label finnishTitleLabel = new Label("FINNISH TITLE");
		private readonly Label swedishTitleLabel = new Label("SWEDISH TITLE");
		private readonly Label samiTitleLabel = new Label("SAMI TITLE");
		private readonly Label finnishDescLabel = new Label("FINNISH DESCRIPTION");
		private readonly Label swedishDescLabel = new Label("SWEDISH DESCRIPTION");
		private readonly Label samiDescLabel = new Label("SAMI DESCRIPTION");
		private readonly Label createdLabel = new Label("CREATED");
		private readonly Label modifiedLabel = new Label("MODIFIED");

		private readonly Label finnishTitleValue;
		private readonly Label swedishTitleValue;
		private readonly Label samiTitleValue;
		private readonly Label finnishDescValue;
		private readonly Label swedishDescValue;
		private readonly Label samiDescValue;
		private readonly Label createdValue;
		private readonly Label modifiedValue;

		public SeriesSection(OrderSeriesInformation orderSeriesInformation)
		{
			finnishTitleValue = new UIDetailValueLabel(orderSeriesInformation.MemberOfTitleMainFin);
			swedishTitleValue = new UIDetailValueLabel(orderSeriesInformation.MemberOfTitleMainSwe);
			samiTitleValue = new UIDetailValueLabel(orderSeriesInformation.MemberOfTitleMainSmi);
			finnishDescValue = new UIDetailValueLabel(orderSeriesInformation.MemberOfDescrFin);
			swedishDescValue = new UIDetailValueLabel(orderSeriesInformation.MemberOfDescrSwe);
			samiDescValue = new UIDetailValueLabel(orderSeriesInformation.MemberOfDescrSmi);
			createdValue = new UIDetailValueLabel("Not available");
			modifiedValue = new UIDetailValueLabel("Not available");

			seriesInfoCollapseButton = new CollapseButton(new[] { finnishTitleLabel, finnishTitleValue, swedishTitleLabel, swedishTitleValue, samiTitleLabel, samiTitleValue, finnishDescLabel, finnishDescValue, swedishDescLabel, swedishDescValue, samiDescLabel, samiDescValue, createdLabel, createdValue, modifiedLabel, modifiedValue }, isCollapsed: true) { CollapseText = "-", ExpandText = "+", Width = 44 };

			GenerateUI();
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(seriesInfoCollapseButton, new WidgetLayout(row, 0, 1, 1));
			AddWidget(seriesInfoLabel, new WidgetLayout(row, 1, 1, 4));

			AddWidget(finnishTitleLabel, new WidgetLayout(++row, 1, 1, 4));
			AddWidget(finnishTitleValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(swedishTitleLabel, new WidgetLayout(++row, 1, 1, 4));
			AddWidget(swedishTitleValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(samiTitleLabel, new WidgetLayout(++row, 1, 1, 4));
			AddWidget(samiTitleValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(finnishDescLabel, new WidgetLayout(++row, 1, 1, 4));
			AddWidget(finnishDescValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(swedishDescLabel, new WidgetLayout(++row, 1, 1, 4));
			AddWidget(swedishDescValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(samiDescLabel, new WidgetLayout(++row, 1, 1, 4));
			AddWidget(samiDescValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(createdLabel, new WidgetLayout(++row, 1, 1, 4));
			AddWidget(createdValue, new WidgetLayout(row, 5, 1, 5));

			AddWidget(modifiedLabel, new WidgetLayout(++row, 1, 1, 4));
			AddWidget(modifiedValue, new WidgetLayout(row, 5, 1, 5));
		}
	}

}