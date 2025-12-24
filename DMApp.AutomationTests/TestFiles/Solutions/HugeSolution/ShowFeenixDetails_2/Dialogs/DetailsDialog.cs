namespace ShowFeenixDetails_2.Dialogs
{
	using Feenix;
	using ShowFeenixDetails_2.Sections;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DetailsDialog : Dialog
	{
		private readonly GeneralSection generalSection;
		private readonly SeriesSection seriesSection;
		private readonly MetadataSection metadataSection;
		private readonly VersionSection versionSection;

		public DetailsDialog(Engine engine, LiveStreamOrder order) : base(engine)
		{
			this.Title = "Feenix order details";

			OkButton = new Button("OK") { Width = 100, Style = ButtonStyle.CallToAction };
			generalSection = new GeneralSection(order.OrderGeneral);
			seriesSection = new SeriesSection(order.OrderSeriesInformation);
			metadataSection = new MetadataSection(order.OrderMetadata);
			versionSection = new VersionSection(order.OrderVersion);

			GenerateUI();
		}

		public Button OkButton { get; private set; }

		private void GenerateUI()
		{
			int row = 0;

			AddSection(generalSection, new SectionLayout(row, 0));
			row += generalSection.RowCount;

			AddSection(seriesSection, new SectionLayout(row, 0));
			row += seriesSection.RowCount;

			AddSection(metadataSection, new SectionLayout(row, 0));
			row += metadataSection.RowCount;

			AddSection(versionSection, new SectionLayout(row, 0));
			row += versionSection.RowCount;

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(OkButton, new WidgetLayout(++row, 0, 1, 3));

			SetColumnWidth(0, 50);
			SetColumnWidth(1, 50);
			SetColumnWidth(2, 50);

			for (int i = 3; i < 12; i++)
			{
				SetColumnWidth(i, 170);
			}
		}
	}

}