namespace ShowPebbleBeachDetails_2
{
	using System;
	using ShowPebbleBeachDetails_2.PebbleBeach;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class PebbleBeachGeneralSection : Section
	{
		private readonly Label typeLabel = new Label("TYPE");
		private readonly Label startLabel = new Label("START");
		private readonly Label sourceLabel = new Label("SOURCE");
		private readonly Label backupSourceLabel = new Label("BACKUP SOURCE");

		private readonly Label typeValue;
		private readonly Label startValue;
		private readonly Label sourceValue;
		private readonly Label backupSourceValue;

		public PebbleBeachGeneralSection(Engine engine, PebbleBeachEvent pebbleBeachEvent)
		{
			typeValue = new UIDetailValueLabel(pebbleBeachEvent.Type);
			startValue = new UIDetailValueLabel(Convert.ToString(pebbleBeachEvent.Start));
			sourceValue = new UIDetailValueLabel(pebbleBeachEvent.Source);
			backupSourceValue = new UIDetailValueLabel(pebbleBeachEvent.BackupSource);

			GenerateUI();
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(typeLabel, new WidgetLayout(++row, 0, 1, 1));
			AddWidget(typeValue, new WidgetLayout(row, 1, 1, 1));

			AddWidget(startLabel, new WidgetLayout(++row, 0, 1, 1));
			AddWidget(startValue, new WidgetLayout(row, 1, 1, 1));

			AddWidget(sourceLabel, new WidgetLayout(++row, 0, 1, 1));
			AddWidget(sourceValue, new WidgetLayout(row, 1, 1, 1));

			AddWidget(backupSourceLabel, new WidgetLayout(++row, 0, 1, 1));
			AddWidget(backupSourceValue, new WidgetLayout(row, 1, 1, 1));
		}
	}
}