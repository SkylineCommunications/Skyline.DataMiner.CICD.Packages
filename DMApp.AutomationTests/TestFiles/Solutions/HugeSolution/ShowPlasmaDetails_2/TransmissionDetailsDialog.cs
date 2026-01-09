namespace ShowPlasmaDetails_2
{
	using System.Collections.Generic;
	using System.Linq;
	using Plasma;
	using Sections;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class TransmissionDetailsDialog : Dialog
	{
		private readonly Button confirmButton = new Button("OK") { Width = 100 };

		public TransmissionDetailsDialog(Engine engine, ParsedPlasmaOrder plasmaOrder) : base(engine)
		{
			Title = "Transmission Details";

			PlasmaTransmissionGeneralSection = new PlasmaTransmissionGeneralSection(engine, plasmaOrder.PublicationEvents.FirstOrDefault(pe => pe.Live == 1));

			PlasmaProgramDetailsSection = new PlasmaProgramDetailsSection(engine, plasmaOrder);

			PlasmaTransmissionAudioSection = new PlasmaTransmissionAudioSection(engine, plasmaOrder.AudioResources);

			PlasmaTransmissionSubtitlesSection = new PlasmaTransmissionSubtitlesSection(engine, plasmaOrder.SubtitleResources);

			confirmButton.Pressed += (sender, args) => engine.ExitSuccess(string.Empty);
			GenerateUI();
		}

		private PlasmaTransmissionGeneralSection PlasmaTransmissionGeneralSection { get; set; }

		private PlasmaProgramDetailsSection PlasmaProgramDetailsSection { get; set; }

		private PlasmaTransmissionAudioSection PlasmaTransmissionAudioSection { get; set; }

		private PlasmaTransmissionSubtitlesSection PlasmaTransmissionSubtitlesSection { get; set; }

		private void GenerateUI()
		{
			var row = 0;

			AddSection(PlasmaTransmissionGeneralSection, new SectionLayout(row, 0));
			row += PlasmaTransmissionGeneralSection.RowCount;

			AddSection(PlasmaProgramDetailsSection, new SectionLayout(row, 0));
			row += PlasmaProgramDetailsSection.RowCount;

			AddSection(PlasmaTransmissionAudioSection, new SectionLayout(++row, 0));
			row += PlasmaTransmissionAudioSection.RowCount;

			AddSection(PlasmaTransmissionSubtitlesSection, new SectionLayout(++row, 0));
			row += PlasmaTransmissionSubtitlesSection.RowCount;

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));
			AddWidget(confirmButton, new WidgetLayout(++row, 0, 1, 3));

			for (var i = 0; i < 2; i++) SetColumnWidth(i, 50);

			for (var i = 2; i < 5; i++) SetColumnWidth(i, 150);

			SetColumnWidth(5, 180);
		}
	}
}