namespace ShowPebbleBeachDetails_2
{
	using System;
	using ShowPebbleBeachDetails_2.PebbleBeach;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class PebbleBeachEventDetails : Dialog
	{
		private readonly Button confirmButton = new Button("OK") { Width = 100, Style = ButtonStyle.CallToAction };

		public PebbleBeachEventDetails(Engine engine, PebbleBeachEvent pebbleBeachEvent) : base(engine)
		{
			this.Title = "Program Details";
			PebbleBeachGeneralSection = new PebbleBeachGeneralSection(engine, pebbleBeachEvent);

			confirmButton.Pressed += (sender, args) => engine.ExitSuccess(String.Empty);
			GenerateUI();
		}

		private PebbleBeachGeneralSection PebbleBeachGeneralSection { get; set; }

		private void GenerateUI()
		{
			int row = 0;

			AddSection(PebbleBeachGeneralSection, new SectionLayout(row, 0));
			row += PebbleBeachGeneralSection.RowCount;

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));
			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));
			AddWidget(confirmButton, new WidgetLayout(++row, 0, 1, 3));

			for (int i = 0; i < 2; i++)
			{
				SetColumnWidth(i, 200);
			}
		}
	}
}